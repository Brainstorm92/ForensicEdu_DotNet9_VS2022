using FluentValidation;
using ForensicEdu.Application.Common.Behaviors;
using ForensicEdu.Application.Common.Exceptions;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Infrastructure.Persistence;
using ForensicEdu.Infrastructure.Resilience;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;

void Check(bool ok,string message){if(!ok)throw new Exception(message);Console.WriteLine("OK: "+message);}
async Task Throws<T>(Func<Task> operation,string message) where T:Exception
{
    try{await operation();}catch(T){Console.WriteLine("OK: "+message);return;}
    throw new Exception(message);
}
var classifier=new NpgsqlTransientFailureClassifier();
NpgsqlException Transient()=>new("Timeout de prueba",new TimeoutException());
Check(classifier.IsTransient(Transient()),"Clasifica fallos transitorios de conexión");
Check(classifier.IsTransient(new InvalidOperationException("wrapper",Transient())),"Clasifica excepciones PostgreSQL envueltas");
Check(!classifier.IsTransient(new PostgresException("duplicado","ERROR","ERROR","23505")),"Unicidad no abre el circuito");
Check(!classifier.IsTransient(new ValidationException("entrada inválida")),"Validación no abre el circuito");
Check(!classifier.IsTransient(new OperationCanceledException()),"Cancelación no abre el circuito");

PostgreSqlCircuitBreaker NewCircuit(TestClock clock)=>new(Options.Create(new DatabaseResilienceOptions {
    FailureRatio=1,MinimumThroughput=2,SamplingDurationSeconds=30,BreakDurationSeconds=15
}),classifier,clock,NullLogger<PostgreSqlCircuitBreaker>.Instance);
var clock=new TestClock();var circuit=NewCircuit(clock);int attempts=0;
Task<int> Fail(CancellationToken ct){attempts++;return Task.FromException<int>(Transient());}
await Throws<NpgsqlException>(()=>circuit.ExecuteAsync(Fail),"Primer fallo se propaga sin replay");
await Throws<NpgsqlException>(()=>circuit.ExecuteAsync(Fail),"Segundo fallo abre el circuito");
Check(attempts==2,"No hay reintentos automáticos de comandos");
await Throws<DatabaseCircuitOpenException>(()=>circuit.ExecuteAsync(Fail),"Circuito abierto falla antes de ejecutar otra operación");
Check(attempts==2,"Circuito abierto no accede a la base");
try {await circuit.ExecuteAsync(Fail);}catch(DatabaseCircuitOpenException e){Check(e.RetryAfter>TimeSpan.Zero,"Circuito informa Retry-After");}
using(var canceled=new CancellationTokenSource())
{
    canceled.Cancel();await Throws<OperationCanceledException>(()=>circuit.ExecuteAsync(Fail,canceled.Token),"Cancelación tiene prioridad incluso con circuito abierto");
}

clock.Advance(TimeSpan.FromSeconds(16));
var entered=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
var release=new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
var probe=circuit.ExecuteAsync(async ct=>{attempts++;entered.SetResult();return await release.Task.WaitAsync(ct);});
await entered.Task;
await Throws<DatabaseCircuitOpenException>(()=>circuit.ExecuteAsync(_=>Task.FromResult(5)),"Half-open permite una sola prueba concurrente");
release.SetResult(42);Check(await probe==42,"Prueba de recuperación exitosa");
Check(await circuit.ExecuteAsync(_=>Task.FromResult(7))==7,"Éxito cierra el circuito y permite tráfico");

var secondClock=new TestClock();var failingProbe=NewCircuit(secondClock);
await Throws<NpgsqlException>(()=>failingProbe.ExecuteAsync(Fail),"Fallo de apertura 1");
await Throws<NpgsqlException>(()=>failingProbe.ExecuteAsync(Fail),"Fallo de apertura 2");
secondClock.Advance(TimeSpan.FromSeconds(16));
await Throws<NpgsqlException>(()=>failingProbe.ExecuteAsync(Fail),"Prueba fallida vuelve a abrir el circuito");
await Throws<DatabaseCircuitOpenException>(()=>failingProbe.ExecuteAsync(Fail),"Reapertura bloquea llamadas posteriores");

var businessCircuit=NewCircuit(new TestClock());
for(var i=0;i<3;i++)await Throws<InvalidOperationException>(()=>businessCircuit.ExecuteAsync<int>(_=>Task.FromException<int>(new InvalidOperationException("regla"))),"Error de negocio conserva su tipo");
Check(await businessCircuit.ExecuteAsync(_=>Task.FromResult(1))==1,"Errores de negocio no bloquean tráfico");

var counting=new CountingResilience();var validation=new ValidationBehavior<ProbeRequest,int>(new[]{new ProbeValidator()});
var resilience=new DatabaseResilienceBehavior<ProbeRequest,int>(counting);
await Throws<ValidationException>(()=>validation.Handle(new ProbeRequest(""),()=>resilience.Handle(new ProbeRequest(""),()=>Task.FromResult(1),default),default),"Validación se ejecuta antes de resiliencia");
Check(counting.Executions==0,"Entrada inválida no consume circuito");

var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{
    ["DatabaseProvider"]="PostgreSQL",["ConnectionStrings:DefaultConnection"]="Host=127.0.0.1;Port=9;Database=unused;Username=test"
}).Build();
var services=new ServiceCollection();services.AddLogging();services.AddSingleton<TimeProvider>(new TestClock());
services.AddDatabaseResilience(config);services.AddPostgreSqlPersistence(config);
using(var provider=services.BuildServiceProvider(new ServiceProviderOptions{ValidateScopes=true,ValidateOnBuild=true}))
{
    using var a=provider.CreateScope();using var b=provider.CreateScope();
    Check(ReferenceEquals(a.ServiceProvider.GetRequiredService<IDatabaseResilience>(),b.ServiceProvider.GetRequiredService<IDatabaseResilience>()),"Circuito singleton compartido entre requests");
    var db=a.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    Check(ReferenceEquals(db,a.ServiceProvider.GetRequiredService<IApplicationDbContext>()) && ReferenceEquals(db,a.ServiceProvider.GetRequiredService<IUnitOfWork>()),"Contexto y unidad de trabajo scoped comparten instancia");
    Check(!ReferenceEquals(db,b.ServiceProvider.GetRequiredService<ApplicationDbContext>()),"Cada scope obtiene su propio DbContext");
    Check(!db.Database.CreateExecutionStrategy().RetriesOnFailure,"EF no reintenta escrituras implícitamente");
}
var invalidServices=new ServiceCollection();invalidServices.AddLogging();
invalidServices.AddDatabaseResilience(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{["Resilience:PostgreSQL:MinimumThroughput"]="1"}).Build());
using(var invalidProvider=invalidServices.BuildServiceProvider())
{
    await Throws<OptionsValidationException>(()=>Task.FromResult(invalidProvider.GetRequiredService<IOptions<DatabaseResilienceOptions>>().Value),"Configuración inválida se rechaza");
}

var directory=new DirectoryInfo(Environment.CurrentDirectory);
while(directory!=null && !File.Exists(Path.Combine(directory.FullName,"ForensicEdu.sln")))directory=directory.Parent;
Check(directory!=null,"Encuentra la raíz de la solución");
var badFiles=new List<string>();
foreach(var file in Directory.EnumerateFiles(Path.Combine(directory!.FullName,"src"),"*.cs",SearchOption.AllDirectories))
{
    if(file.Split(Path.DirectorySeparatorChar).Any(x=>x is "obj" or "bin"))continue;
    if(System.Text.RegularExpressions.Regex.Matches(await File.ReadAllTextAsync(file),@"^public\s+(?:(?:sealed|static|abstract|partial|readonly)\s+)*(?:class|record|interface|enum)\s+\w+",System.Text.RegularExpressions.RegexOptions.Multiline).Count>1)badFiles.Add(file);
}
Check(badFiles.Count==0,"Un tipo público por archivo en las capas del backend");
var assembly=typeof(ForensicEdu.Application.DependencyInjection).Assembly;
var requests=assembly.GetTypes().Where(t=>!t.IsAbstract && typeof(IRequest<ApplicationResult>).IsAssignableFrom(t)).ToArray();
foreach(var request in requests)
{
    var files=Directory.GetFiles(Path.Combine(directory.FullName,"src","ForensicEdu.Application","Handlers"),request.Name+".cs",SearchOption.AllDirectories);
    Check(files.Length==1 && File.Exists(Path.Combine(Path.GetDirectoryName(files[0])!,request.Name+"Handler.cs")),"Request y handler separados: "+request.Name);
    var hasValidator=assembly.GetTypes().Any(t=>!t.IsAbstract && typeof(IValidator<>).MakeGenericType(request).IsAssignableFrom(t));
    if(hasValidator)Check(File.Exists(Path.Combine(Path.GetDirectoryName(files[0])!,request.Name+"Validator.cs")),"Validador separado: "+request.Name);
}
Console.WriteLine("Resiliencia, recuperación, DI y segmentación verificadas sin PostgreSQL remoto ni esperas reales.");

sealed class TestClock : TimeProvider
{
    private long ticks;
    public override DateTimeOffset GetUtcNow()=>DateTimeOffset.UnixEpoch.AddTicks(Interlocked.Read(ref ticks));
    public override long GetTimestamp()=>Interlocked.Read(ref ticks);
    public override long TimestampFrequency=>TimeSpan.TicksPerSecond;
    public void Advance(TimeSpan duration)=>Interlocked.Add(ref ticks,duration.Ticks);
}
sealed class CountingResilience : IDatabaseResilience
{
    public int Executions;
    public Task<T> ExecuteAsync<T>(Func<CancellationToken,Task<T>> action,CancellationToken ct=default){Executions++;return action(ct);}
}
sealed record ProbeRequest(string Value);
sealed class ProbeValidator : AbstractValidator<ProbeRequest>
{
    public ProbeValidator()=>RuleFor(x=>x.Value).NotEmpty();
}

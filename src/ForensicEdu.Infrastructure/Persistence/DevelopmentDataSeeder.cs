using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Infrastructure.Persistence;

public sealed class DevelopmentDataSeeder(IApplicationDbContext db)
{
    public async Task<int> SeedAsync(CancellationToken ct=default)
    {
        var added=0;
        foreach(var (id,title,slug,price) in new[] {
            ("seed-course-balistica","[PRUEBA] Balística básica","prueba-balistica-basica",10m),
            ("seed-course-escena","[PRUEBA] Escena del crimen","prueba-escena-crimen",15m)})
        {
            if(await db.Courses.AnyAsync(x=>x.Id==id || x.Slug==slug,ct))continue;
            var course=new Course {Id=id,Code="TEST-"+id,ShortCode="TEST",Title=title,Slug=slug,Price=price,AcademicHours=2,DurationHours=2,
                Category="Forense",Instructor="Docente de prueba",InstructorTitle="Contenido de demostración académica",
                ShortDescription="Datos de prueba persistidos mediante Entity Framework. No representa una certificación profesional.",
                FullDescription="Curso para verificar compra, validación administrativa, progreso, evaluación y emisión de un certificado de prueba.",
                ImageUrl="/assets/aistudio/image_91aace7f.png",IsActive=true,IsPublished=true,
                QuestionsJson=JsonSerializer.Serialize(new[] {
                    new {id="seed-q1",question="[PRUEBA] ¿Qué confirma el portal de verificación QR?",options=new[]{"Una compra bancaria","El estado del certificado"},correctOptionIndex=1,explanation="La verificación consulta el estado persistido."},
                    new {id="seed-q2",question="[PRUEBA] ¿Cuándo se habilita una matrícula de pago?",options=new[]{"Después de aprobar el pago","Al cerrar el navegador"},correctOptionIndex=0,explanation="La aprobación administrativa habilita la matrícula."},
                    new {id="seed-q3",question="[PRUEBA] ¿Cuál es la nota mínima aprobatoria?",options=new[]{"10/20","13/20"},correctOptionIndex=1,explanation="La regla académica requiere 13/20."}
                })};
            course.Modules.Add(new CourseModule {Id=id+"-m1",CourseId=id,Title="[PRUEBA] Lectura de las reglas de acceso",OrderIndex=0,Duration="5 minutos"});
            course.Modules.Add(new CourseModule {Id=id+"-m2",CourseId=id,Title="[PRUEBA] Repaso y preparación de la evaluación",OrderIndex=1,Duration="5 minutos"});
            db.Courses.Add(course);added++;
        }
        if(!await db.LiveEvents.AnyAsync(x=>x.Id=="seed-event",ct)){
            db.LiveEvents.Add(new LiveEvent {Id="seed-event",Code="TEST-EVENT",Title="[PRUEBA] Seminario de ciencias forenses",Description="Evento de prueba; sala aún no configurada.",Price=5,EventDate=DateTime.UtcNow.AddDays(7),Speaker="Ponente de prueba"});added++;
        }
        if(!await db.AdvisoryServices.AnyAsync(x=>x.Id=="seed-advisory",ct)){
            db.AdvisoryServices.Add(new AdvisoryService {Id="seed-advisory",Title="[PRUEBA] Consulta académica",Specialist="Consultor de prueba",Specialty="Orientación académica",Description="Servicio de prueba sin canal de contacto configurado.",Price=5,WhatsappContact=""});added++;
        }
        if(added>0){db.AuditLogs.Add(new AuditLog {UserId="seed-cli",Action="TEST_CATALOG_SEEDED",EntityType="Catalog",Details=$"Se añadieron {added} productos de prueba sin sobrescribir registros."});await db.SaveChangesAsync(ct);}
        return added;
    }
}

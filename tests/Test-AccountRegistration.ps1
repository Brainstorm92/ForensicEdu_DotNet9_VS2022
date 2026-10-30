param([string]$BaseUrl = 'http://localhost:5088')
$ErrorActionPreference = 'Stop'
$testId = [guid]::NewGuid().ToString('N')
$testEmail = "integration-$testId@example.invalid"
$testPassword = [guid]::NewGuid().ToString('N') + 'A!9'
$registration = @{
    fullName = 'Integración Temporal'; nombres = 'Integración'; apellidos = 'Temporal'
    email = $testEmail; dni = 'TEST' + $testId.Substring(0, 12); phone = '000000000'
    password = $testPassword; tipoDocumento = 'Pasaporte'; paisResidencia = 'Perú'
    nivelEducativo = 'Licenciatura'; institucionEducativa = 'Prueba automatizada'; profesion = 'Prueba de software'
    acceptTerms = $true; acceptPrivacy = $true
}
$created = $false
try {
    $registered = Invoke-RestMethod "$BaseUrl/api/auth/register" -Method Post -ContentType 'application/json; charset=utf-8' -Body ($registration | ConvertTo-Json)
    $created = $true
    if (-not $registered.userId) { throw 'El registro no devolvió ID.' }
    Write-Output 'OK: registro mediante API desde el mismo contrato usado por el formulario.'
    $duplicate = Invoke-WebRequest "$BaseUrl/api/auth/register" -Method Post -ContentType 'application/json; charset=utf-8' -Body ($registration | ConvertTo-Json) -SkipHttpErrorCheck
    if ($duplicate.StatusCode -ne 400 -and $duplicate.StatusCode -ne 409) { throw 'Se aceptó un registro duplicado.' }
    Write-Output 'OK: correo/documento duplicados rechazados.'
    $badLogin = Invoke-WebRequest "$BaseUrl/api/auth/login" -Method Post -ContentType 'application/json' -Body (@{ email = $testEmail; password = 'incorrecta' } | ConvertTo-Json) -SkipHttpErrorCheck
    if ($badLogin.StatusCode -ne 401) { throw 'Se aceptó una contraseña incorrecta.' }
    Write-Output 'OK: contraseña incorrecta rechazada.'
    $login = Invoke-RestMethod "$BaseUrl/api/auth/login" -Method Post -ContentType 'application/json' -Body (@{ email = $testEmail; password = $testPassword } | ConvertTo-Json)
    $profile = Invoke-RestMethod "$BaseUrl/api/auth/me" -Headers @{ Authorization = "Bearer $($login.token)" }
    if ($profile.id -ne $registered.userId -or $profile.email -ne $testEmail -or $profile.profesion -ne $registration.profesion) { throw 'Los datos consultados no coinciden con el registro.' }
    Write-Output 'OK: sesión JWT y perfil leído desde PostgreSQL.'
    $guest = Invoke-WebRequest "$BaseUrl/api/auth/me" -SkipHttpErrorCheck
    if ($guest.StatusCode -ne 401) { throw 'Se expuso el perfil sin autenticación.' }
    Write-Output 'OK: perfil protegido para visitantes sin sesión.'
} finally {
    if ($created) {
        dotnet run --project "$PSScriptRoot/ForensicEdu.BusinessChecks" -- --verify-remove-integration-user $testEmail
        if ($LASTEXITCODE -ne 0) { throw 'No se pudo verificar o retirar la cuenta temporal de integración.' }
    }
}

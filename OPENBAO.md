# Configuración desde OpenBao

La API carga el JSON KV v2 de `forense/produccion/api` al arrancar, antes de registrar PostgreSQL, RustFS y Google. AppRole solo requiere `read` en `forense/data/produccion/api`. No usa la cuenta administradora ni el token raíz. No cambia el secreto ni la base de datos.

## Desarrollo en Visual Studio

En el proyecto ForensicEdu.WebApi, selecciona **Administrar secretos de usuario**. Conserva los valores existentes y agrega esta sección con tus credenciales reales únicamente en ese archivo privado:

```json
{
  "OpenBao": {
    "Enabled": true,
    "Address": "https://forense9-openbao.wv6a0u.easypanel.host",
    "Mount": "forense",
    "SecretPath": "produccion/api",
    "AuthMount": "approle",
    "RoleId": "REEMPLAZAR_LOCALMENTE",
    "SecretId": "REEMPLAZAR_LOCALMENTE"
  }
}
```

No publiques ese archivo. User Secrets evita incluirlo en Git, pero no cifra su contenido local. No pegues las credenciales en chats o capturas.

## Easypanel: entorno del servicio de la API

```text
OpenBao__Enabled=true
OpenBao__Address=https://forense9-openbao.wv6a0u.easypanel.host
OpenBao__Mount=forense
OpenBao__SecretPath=produccion/api
OpenBao__AuthMount=approle
OpenBao__RoleId=REEMPLAZAR_EN_EASYPANEL
OpenBao__SecretId=REEMPLAZAR_EN_EASYPANEL
```

Estas credenciales de arranque deben permanecer fuera de OpenBao para poder autenticarse. No son las credenciales de PostgreSQL. Nunca deben llevar prefijo VITE_. Mantén `JwtSettings__Secret` configurado en producción o agrega `JwtSettings.Secret` al JSON privado del secreto: la API exige una clave de firma de al menos 32 caracteres.

La configuración JSON mantiene sus nombres originales (`ConnectionStrings.DefaultConnection`, `ObjectStorage`, `Authentication.Google`). El secreto remoto tiene prioridad sobre appsettings y User Secrets; variables de entorno y argumentos de inicio tienen prioridad final. Retira los overrides antiguos de conexión/almacenamiento del despliegue si quieres que prevalezca OpenBao.

## Operación

- Activación optativa: `OpenBao.Enabled=false` conserva el arranque actual. No se activó en tu entorno automáticamente.
- HTTPS obligatorio con validación normal de certificados, sin redirecciones. Tiempo de espera de 20 segundos por petición; sin reintentos automáticos de generación de tokens.
- Si falla la autenticación, el secreto no existe, OpenBao está sellado o el JSON es inválido, la API no arranca con credenciales locales de respaldo. Los errores no incluyen cuerpos de respuesta ni tokens.
- Carga una vez al arrancar. Los cambios del secreto requieren reiniciar la API. El token AppRole se usa solo para la lectura inicial; no hace falta renovarlo durante la ejecución.
- El SecretID del rol configurado vence a los 30 días. Genera un reemplazo antes del vencimiento, actualiza el entorno o User Secrets y reinicia; luego revoca el anterior mediante su accessor. Un SecretID vencido impide el próximo arranque, aunque no interrumpe un proceso ya iniciado.
- Después de reiniciar OpenBao con Shamir, se requieren dos claves privadas para desbloquearlo antes de arrancar la API. El volumen /bao/data conserva datos, pero no sustituye las copias de seguridad.
- Las credenciales de PostgreSQL y RustFS expuestas antes deben rotarse por separado: cambiar las claves de desbloqueo de OpenBao no las cambia.

## Validación

`dotnet run --project tests/ForensicEdu.OpenBaoChecks` verifica autenticación AppRole, ruta KV v2, JSON anidado, errores 403/404, rechazo de HTTP, configuración incompleta, datos inválidos y ausencia de credenciales en errores mediante un transporte simulado. No verifica el servidor real sin configurar RoleID/SecretID privados.

# Publicar solo la WebAPI en Easypanel

## Fuente y compilación

- Repositorio: `git@github.com:Brainstorm92/ForensicEdu_DotNet9_VS2022.git`
- Rama: `develop`.
- Ruta de compilación: `/` (las bibliotecas referenciadas también deben estar dentro del contexto).
- Constructor: Dockerfile. Ruta del archivo: `Dockerfile.webapi`.
- Puerto de destino del dominio: `8080`. Activar HTTPS en el dominio mediante Easypanel.

Los archivos de despliegue deben estar publicados en develop antes de desplegar desde Git.
La imagen contiene solo la API: no necesita Node ni publica la aplicación React.
El puerto local 5000 de Visual Studio no se utiliza dentro del contenedor.

## Variables del servicio

Agregar los valores reales únicamente en Entorno del servicio, nunca en Git:

```text
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_HTTP_PORTS=8080
DatabaseProvider=PostgreSQL
ConnectionStrings__DefaultConnection=Host=HOST_INTERNO;Port=5432;Database=BASE;Username=USUARIO;Password=CONTRASEÑA
JwtSettings__Secret=CLAVE_ALEATORIA_PRIVADA_DE_AL_MENOS_32_CARACTERES
JwtSettings__Issuer=ForensicEduApi
JwtSettings__Audience=ForensicEduApp
```

Para PostgreSQL usar el host interno que muestra el servicio de base de datos cuando ambos están en la misma red. No copiar el ejemplo con los marcadores sin reemplazarlos.

Si ya usas OpenBao, configurar las variables privadas `OpenBao__Enabled`, `OpenBao__Address`, `OpenBao__Mount`, `OpenBao__SecretPath`, `OpenBao__AuthMount`, `OpenBao__RoleId` y `OpenBao__SecretId` según OPENBAO.md. Las variables de entorno explícitas prevalecen sobre los valores cargados desde OpenBao; evitar duplicar conexiones antiguas. Con OpenBao habilitado, un error de autenticación impide iniciar la API.

Las opciones ObjectStorage y Authentication.Google también deben configurarse para utilizar RustFS y Google; consultar ALMACENAMIENTO_IMAGENES.md, DOCUMENTOS_PRIVADOS.md y ACCESO_GOOGLE.md.

## Persistencia y comprobación

- Montar almacenamiento persistente en `/app/App_Data` para los comprobantes privados locales. El usuario del contenedor (`app`, UID 1654) debe poder escribir en ese directorio.
- La publicación no crea ni migra automáticamente el esquema PostgreSQL. Aplicar las actualizaciones de esquema correspondientes antes de usar las nuevas funcionalidades.
- Desplegar y revisar los registros: debe aparecer `Now listening on: http://[::]:8080` o un mensaje equivalente.
- Probar un endpoint público existente, por ejemplo `GET /api/events?page=1&pageSize=1`.
- Scalar y `/api/health/database` están habilitados únicamente en Development. En Production, que `/scalar/v1` o `/` no abra la documentación no significa que la API esté detenida.
- El frontend publicado en otro dominio necesitará incorporar ese origen a la política CORS de Program.cs: actualmente solo se permiten orígenes localhost.

## Comprobación de publicación sin Docker

```powershell
dotnet publish src/ForensicEdu.WebApi/ForensicEdu.WebApi.csproj -c Release -o .artifacts/webapi-publish -p:SkipInteractiveClient=true -p:UseAppHost=false
```

La construcción completa de la imagen debe verificarse en Docker/Easypanel. No se verificó localmente si Docker no está instalado.

# Publicar el frontend en Easypanel

En el servicio webforense, configurar:

| Campo | Valor |
|---|---|
| Fuente Git | https://github.com/Brainstorm92/ForensicEdu_DotNet9_VS2022.git |
| Rama | develop (o la rama donde se suban estos archivos) |
| Ruta de compilación | / |
| Constructor | Dockerfile |
| Archivo | Dockerfile.frontend |
| Dominio: protocolo de destino | HTTP |
| Dominio: puerto de destino | 8081 |
| Dominio: ruta de origen y destino | / |

Activar HTTPS en el dominio público. Los archivos deben estar subidos a la rama elegida antes de desplegar desde Git.

## Conexión con la API

El Dockerfile compila React con VITE_API_URL=/api. Nginx recibe /api/... en el dominio del frontend y lo reenvía por HTTPS a:

https://forense9-webackend.wv6a0u.easypanel.host

La ruta /api se conserva. Por ejemplo: https://DOMINIO-FRONTEND/api/courses se reenvía a https://forense9-webackend.wv6a0u.easypanel.host/api/courses.

El navegador se comunica con el mismo dominio del sitio, por lo que no necesita un permiso CORS entre el dominio del frontend y el de la API. La API mantiene sus controles de autenticación y autorización. El proxy verifica el certificado TLS del backend.

Esta configuración del contenedor tiene prioridad sobre .env.production y .env.local. Esos archivos no entran en el contexto Docker. No hace falta agregar VITE_API_URL en Entorno del servicio para esta imagen. Las credenciales de PostgreSQL, JWT y OpenBao permanecen en el servicio de la API.

Si cambia el host del backend, actualizar las dos apariciones de ese host en deploy/nginx.frontend.conf y reconstruir la imagen.

## Comprobar después de desplegar

1. Abrir https://DOMINIO-FRONTEND/healthz: debe responder ok.
2. Abrir https://DOMINIO-FRONTEND/: debe mostrar React.
3. Abrir https://DOMINIO-FRONTEND/api/courses: debe responder JSON del backend.
4. Comprobar el catálogo y el inicio de sesión desde el sitio.

Para construir y ejecutar en una máquina con Docker:

```sh
docker build -f Dockerfile.frontend -t forensicedu-web .
docker run --rm -p 8081:8081 forensicedu-web
```

Abrir http://localhost:8081. La imagen Docker debe verificarse en Docker/Easypanel; la compilación local de Vite por sí sola no prueba el contenedor ni el proxy.

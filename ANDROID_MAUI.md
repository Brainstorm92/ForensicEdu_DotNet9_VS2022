# FORENSIQ — Android

App nativa .NET MAUI 10 para Android 8 o superior. Rama `feature/BS-4243`.
Diseño oscuro/claro adaptado de los bocetos; contratos y reglas de negocio de la API existente. La API sigue en .NET 9. Los proyectos móviles están en `src`, sus pruebas en `tests` y se incluyen tanto en `ForensicEdu.sln` como en la solución móvil independiente `ForensicEdu.Mobile.slnx`.

## Probar en el emulador

1. Comprobar acceso a Internet. La app usa el backend de producción de Contabo.
2. Iniciar un emulador Android desde Visual Studio Device Manager.
3. Instalar el APK firmado de depuración: `adb install -r .artifacts/mobile-android/bin/ForensicEdu.Mobile/debug_net10.0-android/pe.forense.academia-Signed.apk`.
4. Abrir **FORENSIQ** e iniciar sesión con una cuenta existente o registrarse.

Las compilaciones Debug y Release usan `https://forense9-webackend.wv6a0u.easypanel.host/api/`. El APK del script se entrega en `.artifacts/apk/FORENSIQ.apk`. No incluye credenciales de PostgreSQL, RustFS ni OpenBao.

## Compilar

Ejecutar `tools/Build-Android.ps1`. Reutiliza Android SDK/JDK existentes y el SDK .NET aislado de `.artifacts/maui-toolchain/dotnet`; cachés, temporales y artefactos nuevos se guardan en D dentro del repositorio. No cambia las instalaciones globales. El APK es para pruebas, firmado con clave de depuración, no para Play Store.

## Funciones

- Sesión con usuario/correo y contraseña, registro y token en SecureStorage.
- Catálogo, detalle, matrícula, comprobante y consulta de pagos. La aprobación continúa en el panel web.
- Mis cursos, progreso y módulos; reproducción de clase según el enlace autorizado del backend.
- PDF mediante PdfRenderer y Word mediante la vista HTML saneada del servidor; acceso autenticado y sin enlaces públicos al archivo.
- Pantalla de materiales con FLAG_SECURE en Android, sin botones de exportación. Esto limita capturas y grabación compatibles del sistema; requiere verificarlo en el dispositivo y no evita una cámara externa o un sistema modificado.
- Examen con plazo del servidor, guardado de respuestas, recuperación del intento y envío al vencer.
- Certificados con QR, validación y diploma PDF compartible; el diploma es distinto del material protegido.
- Eventos, asesoría aprobada y creación/consulta de solicitudes de soporte.

## Pendiente antes de producción

La app se ha instalado y abierto en el emulador Android. Validar navegación, compras y lectura contra una cuenta de pruebas. El acceso Android con Google usa Credential Manager y la configuración pública de la API (Client ID web y nonce), con validación en el backend, vinculación por contraseña y alta de perfil. Requiere crear en el mismo proyecto Google Cloud un cliente OAuth Android para pe.forense.academia y SHA-1 BA:4A:95:A9:48:1B:57:7F:9A:4A:AD:D2:13:E1:48:44:22:82:B3:38 (firma Debug actual; Release usará otra). No se ha completado la autenticación Google con una cuenta real durante estas verificaciones. La lectura de QR admite pegar el enlace/código, sin cámara todavía.

La distribución Release exige firma de distribución y pruebas en dispositivos. iOS se habilitará en una segunda fase con Mac/Xcode; el lector PDF iOS aún está pendiente.

Para ver el código en Visual Studio, recargar ForensicEdu.sln. La compilación del script usa el workload aislado en D; Visual Studio puede solicitar su propio workload MAUI para depuración integrada. No iniciar esa instalación con sus rutas predeterminadas en C si falta espacio.

## Identidad y contenido

Logo suministrado por el usuario: copias locales en AppIcon, Splash e Images/logo.png para disponibilidad sin conexión. Se retiraron el robot de plantilla y los SVG de marca reemplazados. Cursos, profesores, portadas y materiales se consultan por API; no se incluyen catálogos de ejemplo en el cliente móvil.

## Icono adaptativo actualizado

El manifiesto referencia explícitamente @mipmap/appicon y @mipmap/appicon_round. Fondo azul oscuro y primer plano PNG transparente (Resources/AppIcon/appiconfg.png), escala 0.65 para respetar la máscara del launcher. El escudo transparente también se usa en Resources/Images/logo.png y Resources/Splash/splash.png.

Edición realizada con la herramienta integrada image_gen a partir del PNG original. Prompt: extraer únicamente el escudo turquesa con huella, balanza y mira; conservar su identidad; retirar fondo, circuitos, esquinas blancas y letras; salida PNG transparente centrada con margen para Android.

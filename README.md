# SistemadeRenta

Proyecto universitario de un sistema web para gestionar la renta de vehículos.

## Tecnologías

- C# y ASP.NET Core MVC.
- .NET 10.
- Entity Framework Core.
- SQL Server.
- Razor, HTML, CSS y JavaScript.
- Tabler para las pantallas de administración.
- Cloudinary para almacenar las imágenes de los vehículos.
- CloudinaryDotNet 1.29.3 para la integración con Cloudinary.

## Organización

La solución contiene cinco proyectos:

- **Domain:** definición de las entidades del sistema.
- **Application:** operaciones y reglas del programa.
- **Persistence:** acceso y configuración de la base de datos.
- **Infrastructure:** herramientas e integración con servicios externos.
- **Presentation:** pantallas, modelos de formulario y controladores de la aplicación web.

## Requisitos

- Visual Studio 2026 con la carga de trabajo de desarrollo web de ASP.NET y el SDK de .NET 10.
- Acceso autorizado a la base de datos SQL Server del equipo.
- Credenciales de Cloudinary del almacenamiento utilizado por el proyecto.
- Conexión a internet para restaurar paquetes, acceder a la base de datos, utilizar Cloudinary y cargar los recursos de Tabler.

## Abrir y ejecutar

1. Clonar el repositorio.
2. Abrir el archivo `SistemadeRenta.slnx`, ubicado en
   `AlquilerVehiculos/AlquilerVehiculos` dentro del repositorio.
3. Esperar a que termine la restauración de paquetes.
4. Configurar los secretos de usuario como se explica a continuación.
5. Establecer `SistemadeRenta.Presentation` como proyecto de inicio.
6. Compilar la solución.
7. Seleccionar el perfil `https` y ejecutar con `Ctrl + F5`.
8. Si se solicita, confiar en el certificado local de desarrollo.
9. Iniciar sesión con la cuenta y el rol correspondientes.

## Configuración de desarrollo

Cada integrante debe configurar la conexión y las credenciales en su computadora:

1. Hacer clic derecho en `SistemadeRenta.Presentation`.
2. Seleccionar **Administrar secretos de usuario**.
3. Guardar esta estructura en `secrets.json`, sustituyendo los valores de ejemplo:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "PONER LINEA DE CONEXION AQUI"
  },
  "Cloudinary": {
    "CloudName": "CLOUD_NAME",
    "ApiKey": "API_KEY",
    "ApiSecret": "API_SECRET"
  }
}
```

La cadena de conexión y las credenciales reales deben mantenerse fuera del repositorio. En `appsettings.json`, `DefaultConnection` permanece vacío.

Para administrar las imágenes existentes, se debe utilizar el almacenamiento de Cloudinary acordado por el equipo.

Si cambian las credenciales, se deben actualizar los secretos locales y reiniciar la aplicación.

## Base de datos

El módulo de vehículos utiliza:

- **dbo.Vehiculo:** datos del vehículo, precio por día y estado.
- **dbo.VehiculoImagen:** dirección de cada imagen, identificador público de Cloudinary, vehículo asociado y orden de presentación.

La tabla `VehiculoImagen` contiene las columnas:

- `IdVehiculoImagen`.
- `IdVehiculo`.
- `ImagenUrl`.
- `IdPublico`.
- `Orden`.

Existe un índice único por `IdVehiculo` y `Orden`.

Las tablas `Reserva` y `AlquilerContrato` se consultan para impedir que se eliminen vehículos con registros asociados.

Los cambios de estructura de este módulo se aplicaron mediante SQL en la base compartida. No es necesario ejecutar `Update-Database` para utilizar esa base ya configurada.

## Funciones implementadas

### Autenticación y permisos

- Inicio de sesión con autenticación por cookies.
- Control de acceso según los roles `Administrador` y `Usuario`.
- Protección de las acciones administrativas en el controlador.

### Consulta de vehículos

- Catálogo organizado por categorías.
- Búsqueda por marca o modelo.
- Foto principal, precio por día y estado en el catálogo.
- Página de detalles con los datos del vehículo y su galería de imágenes.

### Administración de vehículos

El administrador puede:

- Registrar vehículos.
- Editar marca, modelo, año, placa, categoría, transmisión y precio por día.
- Cambiar manualmente el estado entre `Disponible` y `No disponible`.
- Agregar y quitar fotos.
- Eliminar vehículos mediante una pantalla de confirmación.

El registro establece el estado inicial como `Disponible`.

La eliminación se bloquea cuando el vehículo tiene reservas o contratos asociados. Las imágenes se retiran de Cloudinary después de confirmar la eliminación correspondiente en la base de datos. Si falla esa limpieza, se registra el problema para su revisión.

## Reglas de las imágenes

- Cada vehículo debe conservar entre 5 y 10 fotos.
- Se aceptan archivos JPG y PNG.
- Cada archivo enviado al servidor debe pesar como máximo 5 MB.
- Las fotos pueden seleccionarse una por una o varias a la vez.
- Se muestran vistas previas antes de guardar.
- Las imágenes mayores de 5 MB se intentan reducir en el navegador mediante una copia JPG.
- La copia reducida puede modificar la calidad, los colores y el HDR original. El archivo original de la computadora no se modifica.
- Las fotos se envían a Cloudinary al guardar el formulario.
- Las fotos nuevas se agregan al final.
- La primera foto según el orden guardado se utiliza como principal.
- Si se elimina la principal, la siguiente foto conservada ocupa su lugar.
- Al editar, el total final se calcula como: fotos actuales menos fotos eliminadas más fotos nuevas.
- No es obligatorio subir fotos nuevas si se conserva el mínimo de cinco.
- Si se recarga o abandona el formulario antes de guardar, las fotos nuevas deben seleccionarse nuevamente.

## Estado actual

El módulo de Gestión de Vehículos está implementado.

Se han realizado pruebas manuales de registro, consulta, edición de datos y estado, administración de fotos, eliminación de un vehículo de prueba y acceso según el rol.



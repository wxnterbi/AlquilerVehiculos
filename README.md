# SistemadeRenta

Proyecto universitario de un sistema web para gestionar la renta de vehículos.

## Tecnologías

- C# y ASP.NET Core MVC.
- .NET 10.
- Entity Framework Core.
- SQL Server.

## Organización

La solución contiene cinco proyectos:

- **Domain:** definición de los datos del sistema.
- **Application:** operaciones y reglas del programa.
- **Persistence:** acceso a la base de datos.
- **Infrastructure:** herramientas y servicios de apoyo.
- **Presentation:** pantallas y controladores de la aplicación web.

## Requisitos

- Visual Studio 2026 con desarrollo web de ASP.NET y SDK de .NET 10.
- Acceso autorizado a la base de datos SQL Server del equipo.
- Internet para descargar los paquetes del proyecto.

## Abrir y ejecutar

1. Clonar el repositorio.
2. Abrir el archivo `SistemadeRenta.slnx`, ubicado en
   `AlquilerVehiculos/AlquilerVehiculos` dentro del repositorio.
3. Esperar a que termine la restauración de paquetes.
4. Configurar la conexión como se explica a continuación.
5. Establecer `SistemadeRenta.Presentation` como proyecto de inicio.
6. Seleccionar el perfil `https` y ejecutar con `Ctrl + F5`.
7. Si se solicita, confiar en el certificado local de desarrollo.

## Conexión de desarrollo

Cada integrante debe configurar la conexión en su computadora:

1. Hacer clic derecho en `SistemadeRenta.Presentation`.
2. Seleccionar **Administrar secretos de usuario**.
3. Guardar esta estructura en `secrets.json`, sustituyendo el texto
   de ejemplo por la conexión autorizada del equipo:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Poner la linea de conexión aqui"
  }
}
```

La conexión debe mantenerse fuera del repositorio. En `appsettings.json`,
`DefaultConnection` permanece vacío.

Si cambia la contraseña del servidor, se debe actualizar la conexión
local y reiniciar la aplicación.

## Estado actual

- Solución, proyectos y carpetas renombrados a SistemadeRenta.
- Compilación y apertura de la página verificadas en Visual Studio 2026.
- Advertencias de seguridad detectadas durante la compilación corregidas.
- Pendiente comprobar la conexión real con SQL Server y completar
  los módulos de usuarios y vehículos.

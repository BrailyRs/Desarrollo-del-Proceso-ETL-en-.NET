# ETLWorkerService

## Descripción General

`ETLWorkerService` es un servicio de .NET Core diseñado para realizar procesos de Extracción, Transformación y Carga (ETL) de datos relacionados con opiniones de clientes. Su objetivo principal es consolidar datos de diversas fuentes (archivos CSV, APIs y bases de datos relacionales) en un Data Warehouse (`OpinionDW`) para facilitar el análisis y la generación de informes.

## Arquitectura

El proyecto sigue principios de una arquitectura limpia (Clean Architecture), promoviendo la separación de preocupaciones, la mantenibilidad, la escalabilidad y la facilidad de prueba.

### Justificación de la Arquitectura

*   **Separación de Preocupaciones:** Cada capa tiene una responsabilidad clara, lo que reduce el acoplamiento y facilita el desarrollo y mantenimiento.
*   **Mantenibilidad:** Los cambios en una capa tienen un impacto mínimo en otras, lo que simplifica la introducción de nuevas características o la modificación de las existentes.
*   **Testabilidad:** La lógica de negocio (Core y Application) es independiente de la infraestructura, lo que permite probarla de forma aislada.
*   **Flexibilidad:** Permite cambiar las implementaciones de la infraestructura (por ejemplo, cambiar la base de datos o la fuente de datos) sin afectar la lógica de negocio central.

### Componentes Principales

*   **Presentation (ETLWorkerService.Presentation):**
    *   Contiene el `ETLWorker`, que es el `BackgroundService` principal.
    *   Gestiona el ciclo de vida del servicio y la interacción con el usuario a través de la consola para la selección del origen de datos.
    *   Orquesta la ejecución del proceso ETL llamando a `IETLService`.
*   **Application (ETLWorkerService.Application):**
    *   Contiene la lógica de negocio específica del proceso ETL.
    *   `ETLService`: Implementa la interfaz `IETLService` y coordina los pasos de extracción, transformación y carga.
    *   Define las interfaces que la capa de infraestructura debe implementar (`IDataRepository`).
*   **Core (ETLWorkerService.Core):**
    *   Define las entidades de dominio (`Client`, `Product`, `Survey`, etc.) que representan los datos de origen y las dimensiones/hechos del Data Warehouse (`DimFecha`, `FactOpiniones`, etc.).
    *   Contiene interfaces (`IDataRepository`, `IETLService`) que definen los contratos para las operaciones de datos y el proceso ETL.
    *   Es la capa más interna y no tiene dependencias de otras capas del proyecto.
*   **Infrastructure (ETLWorkerService.Infrastructure):**
    *   Contiene las implementaciones concretas de las interfaces definidas en la capa Core.
    *   **Data:** `OpinionRContext` (contexto de la base de datos relacional de origen) y `OpinionDwContext` (contexto del Data Warehouse).
    *   **Repositories:** Implementaciones de `IDataRepository` para diferentes orígenes de datos (`CsvDataRepository`, `ApiDataRepository`, `DbDataRepository`).
    *   Maneja la persistencia de datos y la comunicación con servicios externos.

### Flujo de Ejecución

1.  El `ETLWorker` se inicia como un `BackgroundService`.
2.  Dentro de su bucle principal, presenta un menú en la consola para que el usuario seleccione el origen de datos (CSV, API, Base de Datos).
3.  Basado en la selección del usuario, el `ETLWorker` utiliza una fábrica (`Func<IServiceProvider, string, IETLService>`) para obtener una instancia de `ETLService` configurada con el `IDataRepository` adecuado.
4.  El `ETLService` ejecuta su método `ExecuteAsync`, que realiza los siguientes pasos:
    *   **Extracción:** Utiliza el `IDataRepository` configurado para obtener datos de clientes, productos, comentarios sociales, encuestas y reseñas web.
    *   **Carga de Dimensiones:** Carga y actualiza las tablas de dimensiones (`DimCliente`, `DimProducto`, `DimFuente`, `DimClasificacion`, `DimFecha`) en el Data Warehouse.
    *   **Carga de Hechos:** Carga los datos en la tabla de hechos `FactOpiniones`, relacionándolos con las dimensiones cargadas.
5.  Una vez completado el proceso ETL, el `ETLWorker` vuelve a mostrar el menú, permitiendo al usuario ejecutar otro proceso o salir.

## Documentación Técnica

### Tecnologías Utilizadas

*   **.NET Core 6.0 (o superior):** Framework principal para el desarrollo del servicio.
*   **Entity Framework Core:** ORM para la interacción con las bases de datos SQL Server (`OpinionDB` y `OpinionDW`).
*   **CsvHelper:** Librería para la lectura y mapeo de archivos CSV.
*   **Microsoft.Data.SqlClient:** Proveedor de datos para SQL Server.
*   **Microsoft.Extensions.Hosting:** Para la creación de servicios de trabajo (Worker Services).
*   **Microsoft.Extensions.DependencyInjection:** Para la inyección de dependencias.
*   **SQL Server:** Sistema de gestión de bases de datos.

### Configuración del Origen de Datos

El servicio permite seleccionar el origen de datos en tiempo de ejecución a través de un menú de consola. Internamente, esto configura la implementación de `IDataRepository` que `ETLService` utilizará para la extracción.

### Conexiones a Bases de Datos

Las cadenas de conexión se definen en `appsettings.json`:

*   **`OpinionRContext`:** Conexión a la base de datos relacional de origen (`OpinionDB`).
    *   `"Server=DESKTOP-ENGONQR;Database=OpinionDB;Trusted_Connection=True;TrustServerCertificate=True;"`
*   **`OpinionDwContext`:** Conexión al Data Warehouse (`OpinionDW`).
    *   `"Server=DESKTOP-ENGONQR;Database=OpinionDW;Trusted_Connection=True;TrustServerCertificate=True;"`

### Entidades y Mapeos

Las entidades en `ETLWorkerService.Core.Entities` representan los objetos de negocio. Los mapeos a tablas de base de datos se configuran en `OpinionRContext.OnModelCreating` y `OpinionDwContext.OnModelCreating`.

*   **`Client`:** Mapea a la tabla `Clientes`.
*   **`Product`:** Mapea a la tabla `Productos`. Incluye `IdCategoria` y una propiedad de navegación `Category` para obtener el nombre de la categoría de la tabla `Categorias`.
*   **`Survey`:** Mapea a la tabla `Encuestas`. Incluye `IdClasificacion` y una propiedad de navegación `Classification` para obtener el nombre de la clasificación de la tabla `Clasificaciones`.
*   **`DimFecha`:** Dimensión de tiempo en el Data Warehouse.
*   **`FactOpiniones`:** Tabla de hechos en el Data Warehouse.

### Proceso de Extracción (`IDataRepository`)

La interfaz `IDataRepository` define los contratos para la extracción de diferentes tipos de datos. Sus implementaciones son:

*   **`CsvDataRepository`:** Extrae datos de archivos CSV ubicados en `C:/Users/PC/Desktop/Tareas ITLA/Electiva 1 - Big Data/Unidad 5/csv`. Utiliza `CsvHelper` para el parseo.
*   **`ApiDataRepository`:** Extrae datos de una API REST. La URL base se configura en `ApiSettings:SocialMediaApiBaseUrl` en `appsettings.json`.
*   **`DbDataRepository`:** Extrae datos directamente de la base de datos relacional `OpinionDB` utilizando Entity Framework Core. Incluye lógica para unir tablas y obtener nombres de dimensiones (categorías, clasificaciones).

### Proceso de Carga (`ETLService`)

El `ETLService` coordina la carga de datos en el Data Warehouse:

*   **`LoadDimCliente(clients)`:** Carga clientes en `DimCliente`.
*   **`LoadDimProducto(products)`:** Carga productos en `DimProducto`.
*   **`LoadDimFuente(socialComments, surveys, webReviews)`:** Carga fuentes en `DimFuente`. Este método es crucial para evitar errores de clave foránea, ya que recopila todas las fuentes únicas de los datos extraídos y añade una fuente "Unknown" por defecto.
*   **`LoadDimClasificacion()`:** Carga clasificaciones en `DimClasificacion`.
*   **`LoadDimFecha(socialComments, surveys, webReviews)`:** Carga fechas en `DimFecha`.
*   **`LoadFactOpiniones(socialComments, surveys, webReviews)`:** Carga los datos de hechos en `FactOpiniones`, resolviendo las claves de las dimensiones. Maneja fuentes no encontradas asignando la clave de la fuente "Unknown".

### Manejo de Errores

El servicio incluye bloques `try-catch` en el `ETLWorker` para capturar y registrar excepciones durante la ejecución del proceso ETL, proporcionando información útil para la depuración. Los errores de clave foránea se mitigan asegurando que las dimensiones se carguen completamente y proporcionando valores de respaldo (como "Unknown") para las claves de dimensión no encontradas.

### SocialMediaApi

La `SocialMediaApi` es un servicio externo desarrollado en Python con FastAPI que actúa como una de las fuentes de datos para el `ETLWorkerService`, específicamente para los comentarios sociales.

*   **Propósito:** Proporcionar datos de comentarios sociales a través de una API RESTful.
*   **Tecnología:** Python 3.x, FastAPI.
*   **Ubicación:** `C:\Users\PC\Desktop\Tareas ITLA\Electiva 1 - Big Data\Unidad 4\SocialMediaApi`
*   **Cómo Ejecutar:**
    1.  Abre una terminal y navega al directorio de la API:
        ```bash
        cd "C:\Users\PC\Desktop\Tareas ITLA\Electiva 1 - Big Data\Unidad 4\SocialMediaApi"
        ```
    2.  Asegúrate de tener un entorno virtual activado y las dependencias instaladas (puedes usar `pip install -r requirements.txt` si existe un archivo `requirements.txt`).
    3.  Ejecuta la API (asumiendo que `main.py` es el punto de entrada y se usa `uvicorn`):
        ```bash
        uvicorn main:app --reload --port 8000
        ```
    4.  La API debería estar accesible en `http://localhost:8000`. Mantén esta terminal abierta y la API en ejecución mientras utilizas la opción de extracción por API en `ETLWorkerService`.

## Cómo Ejecutar el Servicio

### Prerrequisitos

*   **.NET SDK 6.0 (o superior):** Necesario para compilar y ejecutar el proyecto.
*   **SQL Server:** Las bases de datos `OpinionDB` y `OpinionDW` deben estar configuradas y accesibles según las cadenas de conexión en `appsettings.json`.
*   **SocialMediaApi:** Si planeas usar la opción de extracción por API, el servicio `SocialMediaApi` debe estar en ejecución. Consulta la sección "SocialMediaApi" para obtener instrucciones sobre cómo iniciarlo.

### Configuración

Asegúrate de que el archivo `appsettings.json` en la raíz del proyecto `ETLWorkerService` contenga las cadenas de conexión correctas para `OpinionRContext` y `OpinionDwContext`, así como la URL base para `ApiSettings`.

### Pasos para Ejecutar

1.  **Abrir una terminal:** Navega hasta el directorio del proyecto `ETLWorkerService`:
    ```bash
    cd "C:\Users\PC\Desktop\Tareas ITLA\Electiva 1 - Big Data\Unidad 5\ETLWorkerService"
    ```

2.  **Compilar el proyecto:**
    ```bash
    dotnet build
    ```

3.  **Ejecutar el servicio:**
    ```bash
    dotnet run
    ```

4.  **Seleccionar el origen de datos:** Cuando el servicio se inicie, verás un menú interactivo en la consola. Ingresa `1` para CSV, `2` para API, `3` para Base de Datos, o `4` para salir, y presiona Enter.

El servicio ejecutará el proceso ETL utilizando el origen de datos seleccionado. Una vez que el proceso termine, se te pedirá que presiones una tecla para volver al menú y elegir otra opción.

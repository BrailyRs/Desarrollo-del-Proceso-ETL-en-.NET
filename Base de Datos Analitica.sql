-- Crear la base de datos para el Data Warehouse
CREATE DATABASE OpinionDW
ON PRIMARY
(
    NAME = N'OpinionDW_Primary',
    FILENAME = 'C:\Users\PC\Desktop\Tareas ITLA\Electiva 1 - Big Data\Unidad3\SQLData\OpinionDW_Primary.mdf',
    SIZE = 10MB,
    MAXSIZE = UNLIMITED,
    FILEGROWTH = 5MB
),
FILEGROUP FG_DW_2024
(
    NAME = 'OpinionDW_2024',
    FILENAME = 'C:\Users\PC\Desktop\Tareas ITLA\Electiva 1 - Big Data\Unidad3\SQLData\Opinion2024\OpinionDW_2024.ndf',
    SIZE = 10MB,
    MAXSIZE = UNLIMITED,
    FILEGROWTH = 5MB
),
FILEGROUP FG_DW_2025
(
    NAME = 'OpinionDW_2025',
    FILENAME = 'C:\Users\PC\Desktop\Tareas ITLA\Electiva 1 - Big Data\Unidad3\SQLData\Opinion2025\OpinionDW_2025.ndf',
    SIZE = 10MB,
    MAXSIZE = UNLIMITED,
    FILEGROWTH = 5MB
),
FILEGROUP FG_DW_2026
(
    NAME = 'OpinionDW_2026',
    FILENAME = 'C:\Users\PC\Desktop\Tareas ITLA\Electiva 1 - Big Data\Unidad3\SQLData\Opinion2026\OpinionDW_2026.ndf',
    SIZE = 10MB,
    MAXSIZE = UNLIMITED,
    FILEGROWTH = 5MB
)
LOG ON
(
    NAME = N'OpinionDW_Log',
    FILENAME = 'C:\Users\PC\Desktop\Tareas ITLA\Electiva 1 - Big Data\Unidad3\SQLData\Logs\OpinionDW.ldf',
    SIZE = 10MB,
    MAXSIZE = 2GB,
    FILEGROWTH = 5MB
);
GO

USE OpinionDW;
GO

-- Dimension de Tiempo
CREATE TABLE DimFecha (
    FechaKey INT PRIMARY KEY,
    Fecha DATE NOT NULL,
    Dia INT NOT NULL,
    Mes INT NOT NULL,
    Anio INT NOT NULL,
    Trimestre INT NOT NULL,
    DiaDeLaSemana INT NOT NULL
);
GO

-- Dimension de Clientes
CREATE TABLE DimCliente (
    ClienteKey INT PRIMARY KEY IDENTITY(1,1),
    IdCliente INT NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL
);
GO

-- Dimension de Productos
CREATE TABLE DimProducto (
    ProductoKey INT PRIMARY KEY IDENTITY(1,1),
    IdProducto INT NOT NULL,
    NombreProducto NVARCHAR(100) NOT NULL,
    NombreCategoria NVARCHAR(100) NOT NULL
);
GO

-- Dimension de Fuentes
CREATE TABLE DimFuente (
    FuenteKey INT PRIMARY KEY IDENTITY(1,1),
    NombreFuente NVARCHAR(100) NOT NULL
);
GO

-- Dimension de Clasificaciones
CREATE TABLE DimClasificacion (
    ClasificacionKey INT PRIMARY KEY IDENTITY(1,1),
    NombreClasificacion NVARCHAR(50) NOT NULL
);
GO

-- Función y Esquema de Partición para la tabla de hechos
CREATE PARTITION FUNCTION pf_FechaRangoDW (INT)
AS RANGE LEFT FOR VALUES (20241231, 20251231);
GO

CREATE PARTITION SCHEME ps_FechaRangoDW
AS PARTITION pf_FechaRangoDW
TO (FG_DW_2024, FG_DW_2025, FG_DW_2026);
GO

-- Tabla de Hechos de Opiniones
CREATE TABLE FactOpiniones (
    OpinionKey BIGINT IDENTITY(1,1),
    FechaKey INT NOT NULL,
    ClienteKey INT NOT NULL,
    ProductoKey INT NOT NULL,
    FuenteKey INT NOT NULL,
    ClasificacionKey INT,
    Rating INT,
    PuntajeSatisfaccion INT,
    SentimentScore DECIMAL(5, 2),
    CONSTRAINT PK_FactOpiniones PRIMARY KEY (OpinionKey, FechaKey),
    CONSTRAINT FK_FactOpiniones_DimFecha FOREIGN KEY (FechaKey) REFERENCES DimFecha(FechaKey),
    CONSTRAINT FK_FactOpiniones_DimCliente FOREIGN KEY (ClienteKey) REFERENCES DimCliente(ClienteKey),
    CONSTRAINT FK_FactOpiniones_DimProducto FOREIGN KEY (ProductoKey) REFERENCES DimProducto(ProductoKey),
    CONSTRAINT FK_FactOpiniones_DimFuente FOREIGN KEY (FuenteKey) REFERENCES DimFuente(FuenteKey),
    CONSTRAINT FK_FactOpiniones_DimClasificacion FOREIGN KEY (ClasificacionKey) REFERENCES DimClasificacion(ClasificacionKey)
)
ON ps_FechaRangoDW(FechaKey);
GO

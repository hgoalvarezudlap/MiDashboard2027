-- =========================================
-- Registro de Tiempo (prefijo RT_)
-- =========================================

-- Requerido por el índice filtrado UX_RT_Categoria_Predeterminada
-- (sqlcmd lo deja en OFF por defecto; SSMS y el portal lo traen en ON).
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE TABLE dbo.RT_Categoria (
    CategoriaId       INT IDENTITY(1,1) NOT NULL,
    Nombre            NVARCHAR(100)     NOT NULL,
    Color             CHAR(7)           NOT NULL,
    EsPredeterminada  BIT               NOT NULL CONSTRAINT DF_RT_Categoria_EsPred DEFAULT (0),
    FechaCreacion     DATETIME2(0)      NOT NULL CONSTRAINT DF_RT_Categoria_FechaCreacion DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_RT_Categoria PRIMARY KEY CLUSTERED (CategoriaId),
    CONSTRAINT CK_RT_Categoria_Color CHECK (Color LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]')
);

CREATE UNIQUE INDEX UX_RT_Categoria_Nombre ON dbo.RT_Categoria (Nombre);
CREATE UNIQUE INDEX UX_RT_Categoria_Color  ON dbo.RT_Categoria (Color);
-- Solo puede existir una categoría predeterminada
CREATE UNIQUE INDEX UX_RT_Categoria_Predeterminada
    ON dbo.RT_Categoria (EsPredeterminada) WHERE EsPredeterminada = 1;

CREATE TABLE dbo.RT_Registro (
    RegistroId         INT IDENTITY(1,1) NOT NULL,
    Tarea              NVARCHAR(200)     NOT NULL,
    Minutos            SMALLINT          NOT NULL,
    Fecha              DATE              NOT NULL,
    CategoriaId        INT               NOT NULL,
    Comentarios        NVARCHAR(1000)    NULL,
    FechaCreacion      DATETIME2(0)      NOT NULL CONSTRAINT DF_RT_Registro_FechaCreacion DEFAULT (SYSUTCDATETIME()),
    FechaModificacion  DATETIME2(0)      NULL,
    CONSTRAINT PK_RT_Registro PRIMARY KEY CLUSTERED (RegistroId),
    CONSTRAINT FK_RT_Registro_Categoria FOREIGN KEY (CategoriaId) REFERENCES dbo.RT_Categoria (CategoriaId),
    CONSTRAINT CK_RT_Registro_Minutos CHECK (Minutos BETWEEN 15 AND 480 AND Minutos % 15 = 0),
    CONSTRAINT CK_RT_Registro_Tarea CHECK (LEN(LTRIM(RTRIM(Tarea))) > 0)
);

-- Dashboard: consultas por rango de fechas
CREATE INDEX IX_RT_Registro_Fecha
    ON dbo.RT_Registro (Fecha) INCLUDE (Minutos, CategoriaId);
-- Filtro por categoría
CREATE INDEX IX_RT_Registro_Categoria
    ON dbo.RT_Registro (CategoriaId, Fecha) INCLUDE (Minutos);
-- Autocompletar nombre de tarea
CREATE INDEX IX_RT_Registro_Tarea
    ON dbo.RT_Registro (Tarea);
GO

-- Categoría predeterminada (gris)
INSERT INTO dbo.RT_Categoria (Nombre, Color, EsPredeterminada)
VALUES (N'Sin categoría', '#9E9E9E', 1);
GO

-- Eliminar categoría: reasigna sus registros a la predeterminada
CREATE PROCEDURE dbo.RT_sp_EliminarCategoria
    @CategoriaId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.RT_Categoria WHERE CategoriaId = @CategoriaId)
        THROW 50002, 'La categoría no existe.', 1;

    IF EXISTS (SELECT 1 FROM dbo.RT_Categoria WHERE CategoriaId = @CategoriaId AND EsPredeterminada = 1)
        THROW 50001, 'La categoría predeterminada no se puede eliminar.', 1;

    DECLARE @DefaultId INT =
        (SELECT CategoriaId FROM dbo.RT_Categoria WHERE EsPredeterminada = 1);

    BEGIN TRAN;
        UPDATE dbo.RT_Registro
           SET CategoriaId = @DefaultId, FechaModificacion = SYSUTCDATETIME()
         WHERE CategoriaId = @CategoriaId;

        DELETE FROM dbo.RT_Categoria WHERE CategoriaId = @CategoriaId;
    COMMIT;
END;
GO

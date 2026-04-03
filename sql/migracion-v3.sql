-- =============================================================================
-- SOFTMED V3 - Script de migración de base de datos
-- Ejecutar DESPUÉS de tener las tablas V2 con nombres simplificados
-- =============================================================================

-- ─── TBL_ESTADO: Asegurar que existen los 5 estados ─────────────────────────
INSERT IGNORE INTO TBL_ESTADO (id_estado, nombre, orden, activo) VALUES
    (1, 'PENDIENTE',    1, 1),
    (2, 'EN_PROCESO',   2, 1),
    (3, 'FIRMADO',      3, 1),
    (4, 'ENTREGADO',    4, 1),
    (5, 'RECHAZADO',    5, 1);

-- ─── TBL_EXAMEN: Agregar columnas faltantes ─────────────────────────────────
ALTER TABLE TBL_EXAMEN
    ADD COLUMN IF NOT EXISTS fecha_recepcion TIMESTAMP NULL DEFAULT NULL,
    ADD COLUMN IF NOT EXISTS fecha_firma     TIMESTAMP NULL DEFAULT NULL,
    ADD COLUMN IF NOT EXISTS fecha_entrega   TIMESTAMP NULL DEFAULT NULL,
    ADD COLUMN IF NOT EXISTS critico         TINYINT(1) DEFAULT 0,
    ADD COLUMN IF NOT EXISTS urgente         TINYINT(1) DEFAULT 0,
    ADD COLUMN IF NOT EXISTS id_signatario   BIGINT NULL DEFAULT NULL;

-- ─── TBL_EXAMEN_EXTENDIDO: Agregar columnas V3 ──────────────────────────────
ALTER TABLE TBL_EXAMEN_EXTENDIDO
    ADD COLUMN IF NOT EXISTS observaciones          TEXT NULL,
    ADD COLUMN IF NOT EXISTS diagnostico_presuntivo  TEXT NULL,
    ADD COLUMN IF NOT EXISTS medico_solicitante      VARCHAR(500) NULL,
    ADD COLUMN IF NOT EXISTS datos_adicionales        TEXT NULL,
    ADD COLUMN IF NOT EXISTS macroscopia              TEXT NULL,
    ADD COLUMN IF NOT EXISTS microscopia              TEXT NULL,
    ADD COLUMN IF NOT EXISTS diagnostico              TEXT NULL,
    ADD COLUMN IF NOT EXISTS conclusion               TEXT NULL,
    ADD COLUMN IF NOT EXISTS histologia               TEXT NULL;

-- ─── TBL_LOG_EXAMEN: Agregar columnas V3 ────────────────────────────────────
ALTER TABLE TBL_LOG_EXAMEN
    ADD COLUMN IF NOT EXISTS estado_anterior INT NULL,
    ADD COLUMN IF NOT EXISTS estado_nuevo    INT NOT NULL DEFAULT 1,
    ADD COLUMN IF NOT EXISTS ip              VARCHAR(45) NULL;

-- ─── TBL_LOG_EDICION_EXAMEN: Crear tabla si no existe ───────────────────────
CREATE TABLE IF NOT EXISTS TBL_LOG_EDICION_EXAMEN (
    id_log_edicion  BIGINT PRIMARY KEY AUTO_INCREMENT,
    id_examen       BIGINT NOT NULL,
    id_usuario      BIGINT NOT NULL,
    motivo          VARCHAR(500) NOT NULL,
    cambios         JSON NOT NULL,
    ip              VARCHAR(45) NULL,
    pdf_regenerado  TINYINT(1) DEFAULT 0,
    fecha_creacion  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (id_examen) REFERENCES TBL_EXAMEN(id_examen)
);

-- ─── TBL_COBROS_EXAMEN: Crear tabla si no existe ────────────────────────────
CREATE TABLE IF NOT EXISTS TBL_COBROS_EXAMEN (
    id_cobro        INT PRIMARY KEY AUTO_INCREMENT,
    id_examen       BIGINT NOT NULL,
    forma_pago      VARCHAR(50),
    codigo_cobro    VARCHAR(50),
    valor           DECIMAL(12,2) DEFAULT 0,
    observacion     TEXT,
    fecha_creacion  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (id_examen) REFERENCES TBL_EXAMEN(id_examen)
);

-- ─── TBL_FEATURE_FLAG: Insertar flags básicos ──────────────────────────────
INSERT IGNORE INTO TBL_FEATURE_FLAG (modulo, tenant_id, activo) VALUES
    ('examenes',        0, 1),
    ('productos',       0, 1),
    ('edicion_examen',  0, 0),
    ('cobros_fonasa',   0, 0),
    ('transbank_pagos', 0, 0),
    ('adjuntos_s3',     0, 0);

-- ─── TBL_USUARIO: Asegurar columna requiere_cambio ──────────────────────────
ALTER TABLE TBL_USUARIO
    ADD COLUMN IF NOT EXISTS requiere_cambio TINYINT(1) DEFAULT 1;

-- ─── TBL_REFRESH_TOKEN: Crear tabla si no existe ────────────────────────────
CREATE TABLE IF NOT EXISTS TBL_REFRESH_TOKEN (
    id_refresh      BIGINT PRIMARY KEY AUTO_INCREMENT,
    id_usuario      BIGINT NOT NULL,
    token           VARCHAR(500) NOT NULL,
    expiracion      TIMESTAMP NOT NULL,
    fecha_creacion  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (id_usuario) REFERENCES TBL_USUARIO(id_usuario)
);

-- ─── ÍNDICES OPTIMIZACIÓN ───────────────────────────────────────────────────
CREATE INDEX IF NOT EXISTS idx_examen_estado       ON TBL_EXAMEN(id_estado);
CREATE INDEX IF NOT EXISTS idx_examen_institucion  ON TBL_EXAMEN(id_institucion);
CREATE INDEX IF NOT EXISTS idx_examen_paciente     ON TBL_EXAMEN(id_paciente);
CREATE INDEX IF NOT EXISTS idx_examen_fecha_crea   ON TBL_EXAMEN(fecha_creacion);
CREATE INDEX IF NOT EXISTS idx_examen_barcode      ON TBL_EXAMEN(barcode);
CREATE INDEX IF NOT EXISTS idx_log_examen_examen   ON TBL_LOG_EXAMEN(id_examen);
CREATE INDEX IF NOT EXISTS idx_cobro_examen        ON TBL_COBROS_EXAMEN(id_examen);
CREATE INDEX IF NOT EXISTS idx_persona_ident       ON TBL_PERSONA(identificador);
CREATE INDEX IF NOT EXISTS idx_usuario_email       ON TBL_USUARIO(email);

-- ─── FIN ────────────────────────────────────────────────────────────────────
SELECT 'Migración V3 completada exitosamente' AS resultado;

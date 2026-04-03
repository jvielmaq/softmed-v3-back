-- =============================================================================
-- SOFTMED V3 - Script de migración de base de datos
-- Ejecutar DESPUÉS de tener las tablas V2 con nombres simplificados
-- =============================================================================

-- ─── TBL_ESTADO: Todos los estados del flujo de exámenes ────────────────────
INSERT IGNORE INTO TBL_ESTADO (id_estado, nombre, orden, activo) VALUES
    ( 1, 'CREADO',           1, 1),
    ( 2, 'PARA_ETIQUETAR',   2, 1),
    ( 3, 'PARA_ACOPIO',      3, 1),
    ( 4, 'EN_ACOPIO',        4, 1),
    ( 5, 'VIA_ACOPIO',       5, 1),
    ( 6, 'EN_TRANSITO',      6, 1),
    ( 7, 'EN_LABORATORIO',   7, 1),
    ( 8, 'EN_PROCESO',       8, 1),
    ( 9, 'EN_MACROSCOPIA',   9, 1),
    (10, 'EN_LAB_TEC',      10, 1),
    (11, 'EN_MICROSCOPIA',  11, 1),
    (12, 'PROCESADA',       12, 1),
    (13, 'EN_TRANSCRIPCION',13, 1),
    (14, 'EN_DIAGNOSTICO',  14, 1),
    (15, 'POR_VALIDAR',     15, 1),
    (16, 'POR_FIRMAR',      16, 1),
    (17, 'FIRMADO',         17, 1),
    (18, 'ENTREGADO',       18, 1),
    (19, 'RECHAZADO',       19, 1),
    (20, 'ANULADO',         20, 1),
    (21, 'RE_FIRMADO',      21, 1),
    (22, 'RE_ENTREGADO',    22, 1);

-- Renombrar estados existentes si venían del esquema viejo con nombres distintos
UPDATE TBL_ESTADO SET nombre = 'CREADO' WHERE id_estado = 1 AND nombre = 'PENDIENTE';

-- ─── TBL_ETAPA_ESTADO: Transiciones válidas entre estados ───────────────────
CREATE TABLE IF NOT EXISTS TBL_ETAPA_ESTADO (
    id_etapa_estado   INT PRIMARY KEY AUTO_INCREMENT,
    id_estado_origen  INT NOT NULL,
    id_estado_destino INT NOT NULL,
    rol               VARCHAR(50) NOT NULL,
    descripcion       VARCHAR(200) NULL,
    activo            TINYINT(1) DEFAULT 1,
    FOREIGN KEY (id_estado_origen)  REFERENCES TBL_ESTADO(id_estado),
    FOREIGN KEY (id_estado_destino) REFERENCES TBL_ESTADO(id_estado),
    UNIQUE KEY uq_transicion_rol (id_estado_origen, id_estado_destino, rol)
);

-- ─── Insertar TODAS las transiciones válidas ────────────────────────────────
-- Flujo secuencial principal: CREADO(1) → ... → ENTREGADO(18)
-- Roles admin (SM_DEVELOPER, SM_ADM_SISTEMA): todas las transiciones
-- SM_SOLICITANTE: solo CREADO → PARA_ETIQUETAR
-- SM_JEFE_LABORATORIO: desde EN_LABORATORIO(7) hasta ENTREGADO(18) + RECHAZADO
-- SM_INFORMANTE: no puede cambiar estado (no se insertan registros)

-- Limpiar transiciones anteriores para re-insertar
DELETE FROM TBL_ETAPA_ESTADO;

INSERT INTO TBL_ETAPA_ESTADO (id_estado_origen, id_estado_destino, rol, descripcion) VALUES
    -- ═══ Flujo secuencial: SM_DEVELOPER ═══
    ( 1,  2, 'SM_DEVELOPER', 'Creado → Para etiquetar'),
    ( 2,  3, 'SM_DEVELOPER', 'Para etiquetar → Para acopio'),
    ( 3,  4, 'SM_DEVELOPER', 'Para acopio → En acopio'),
    ( 4,  5, 'SM_DEVELOPER', 'En acopio → Vía acopio'),
    ( 5,  6, 'SM_DEVELOPER', 'Vía acopio → En tránsito'),
    ( 6,  7, 'SM_DEVELOPER', 'En tránsito → En laboratorio'),
    ( 7,  8, 'SM_DEVELOPER', 'En laboratorio → En proceso'),
    ( 8,  9, 'SM_DEVELOPER', 'En proceso → En macroscopía'),
    ( 9, 10, 'SM_DEVELOPER', 'En macroscopía → En lab técnico'),
    (10, 11, 'SM_DEVELOPER', 'En lab técnico → En microscopía'),
    (11, 12, 'SM_DEVELOPER', 'En microscopía → Procesada'),
    (12, 13, 'SM_DEVELOPER', 'Procesada → En transcripción'),
    (13, 14, 'SM_DEVELOPER', 'En transcripción → En diagnóstico'),
    (14, 15, 'SM_DEVELOPER', 'En diagnóstico → Por validar'),
    (15, 16, 'SM_DEVELOPER', 'Por validar → Por firmar'),
    (16, 17, 'SM_DEVELOPER', 'Por firmar → Firmado'),
    (17, 18, 'SM_DEVELOPER', 'Firmado → Entregado'),
    -- RECHAZADO desde EN_LABORATORIO(7) hasta POR_FIRMAR(16)
    ( 7, 19, 'SM_DEVELOPER', 'Rechazar desde En laboratorio'),
    ( 8, 19, 'SM_DEVELOPER', 'Rechazar desde En proceso'),
    ( 9, 19, 'SM_DEVELOPER', 'Rechazar desde En macroscopía'),
    (10, 19, 'SM_DEVELOPER', 'Rechazar desde En lab técnico'),
    (11, 19, 'SM_DEVELOPER', 'Rechazar desde En microscopía'),
    (12, 19, 'SM_DEVELOPER', 'Rechazar desde Procesada'),
    (13, 19, 'SM_DEVELOPER', 'Rechazar desde En transcripción'),
    (14, 19, 'SM_DEVELOPER', 'Rechazar desde En diagnóstico'),
    (15, 19, 'SM_DEVELOPER', 'Rechazar desde Por validar'),
    (16, 19, 'SM_DEVELOPER', 'Rechazar desde Por firmar'),
    -- ANULADO desde cualquier estado pre-firma (1-16)
    ( 1, 20, 'SM_DEVELOPER', 'Anular desde Creado'),
    ( 2, 20, 'SM_DEVELOPER', 'Anular desde Para etiquetar'),
    ( 3, 20, 'SM_DEVELOPER', 'Anular desde Para acopio'),
    ( 4, 20, 'SM_DEVELOPER', 'Anular desde En acopio'),
    ( 5, 20, 'SM_DEVELOPER', 'Anular desde Vía acopio'),
    ( 6, 20, 'SM_DEVELOPER', 'Anular desde En tránsito'),
    ( 7, 20, 'SM_DEVELOPER', 'Anular desde En laboratorio'),
    ( 8, 20, 'SM_DEVELOPER', 'Anular desde En proceso'),
    ( 9, 20, 'SM_DEVELOPER', 'Anular desde En macroscopía'),
    (10, 20, 'SM_DEVELOPER', 'Anular desde En lab técnico'),
    (11, 20, 'SM_DEVELOPER', 'Anular desde En microscopía'),
    (12, 20, 'SM_DEVELOPER', 'Anular desde Procesada'),
    (13, 20, 'SM_DEVELOPER', 'Anular desde En transcripción'),
    (14, 20, 'SM_DEVELOPER', 'Anular desde En diagnóstico'),
    (15, 20, 'SM_DEVELOPER', 'Anular desde Por validar'),
    (16, 20, 'SM_DEVELOPER', 'Anular desde Por firmar'),
    -- RE_FIRMADO desde ENTREGADO
    (18, 21, 'SM_DEVELOPER', 'Entregado → Re-firmado (edición post-emisión)'),
    -- RE_ENTREGADO desde RE_FIRMADO
    (21, 22, 'SM_DEVELOPER', 'Re-firmado → Re-entregado'),

    -- ═══ Flujo secuencial: SM_ADM_SISTEMA (idéntico a DEVELOPER) ═══
    ( 1,  2, 'SM_ADM_SISTEMA', 'Creado → Para etiquetar'),
    ( 2,  3, 'SM_ADM_SISTEMA', 'Para etiquetar → Para acopio'),
    ( 3,  4, 'SM_ADM_SISTEMA', 'Para acopio → En acopio'),
    ( 4,  5, 'SM_ADM_SISTEMA', 'En acopio → Vía acopio'),
    ( 5,  6, 'SM_ADM_SISTEMA', 'Vía acopio → En tránsito'),
    ( 6,  7, 'SM_ADM_SISTEMA', 'En tránsito → En laboratorio'),
    ( 7,  8, 'SM_ADM_SISTEMA', 'En laboratorio → En proceso'),
    ( 8,  9, 'SM_ADM_SISTEMA', 'En proceso → En macroscopía'),
    ( 9, 10, 'SM_ADM_SISTEMA', 'En macroscopía → En lab técnico'),
    (10, 11, 'SM_ADM_SISTEMA', 'En lab técnico → En microscopía'),
    (11, 12, 'SM_ADM_SISTEMA', 'En microscopía → Procesada'),
    (12, 13, 'SM_ADM_SISTEMA', 'Procesada → En transcripción'),
    (13, 14, 'SM_ADM_SISTEMA', 'En transcripción → En diagnóstico'),
    (14, 15, 'SM_ADM_SISTEMA', 'En diagnóstico → Por validar'),
    (15, 16, 'SM_ADM_SISTEMA', 'Por validar → Por firmar'),
    (16, 17, 'SM_ADM_SISTEMA', 'Por firmar → Firmado'),
    (17, 18, 'SM_ADM_SISTEMA', 'Firmado → Entregado'),
    ( 7, 19, 'SM_ADM_SISTEMA', 'Rechazar desde En laboratorio'),
    ( 8, 19, 'SM_ADM_SISTEMA', 'Rechazar desde En proceso'),
    ( 9, 19, 'SM_ADM_SISTEMA', 'Rechazar desde En macroscopía'),
    (10, 19, 'SM_ADM_SISTEMA', 'Rechazar desde En lab técnico'),
    (11, 19, 'SM_ADM_SISTEMA', 'Rechazar desde En microscopía'),
    (12, 19, 'SM_ADM_SISTEMA', 'Rechazar desde Procesada'),
    (13, 19, 'SM_ADM_SISTEMA', 'Rechazar desde En transcripción'),
    (14, 19, 'SM_ADM_SISTEMA', 'Rechazar desde En diagnóstico'),
    (15, 19, 'SM_ADM_SISTEMA', 'Rechazar desde Por validar'),
    (16, 19, 'SM_ADM_SISTEMA', 'Rechazar desde Por firmar'),
    ( 1, 20, 'SM_ADM_SISTEMA', 'Anular desde Creado'),
    ( 2, 20, 'SM_ADM_SISTEMA', 'Anular desde Para etiquetar'),
    ( 3, 20, 'SM_ADM_SISTEMA', 'Anular desde Para acopio'),
    ( 4, 20, 'SM_ADM_SISTEMA', 'Anular desde En acopio'),
    ( 5, 20, 'SM_ADM_SISTEMA', 'Anular desde Vía acopio'),
    ( 6, 20, 'SM_ADM_SISTEMA', 'Anular desde En tránsito'),
    ( 7, 20, 'SM_ADM_SISTEMA', 'Anular desde En laboratorio'),
    ( 8, 20, 'SM_ADM_SISTEMA', 'Anular desde En proceso'),
    ( 9, 20, 'SM_ADM_SISTEMA', 'Anular desde En macroscopía'),
    (10, 20, 'SM_ADM_SISTEMA', 'Anular desde En lab técnico'),
    (11, 20, 'SM_ADM_SISTEMA', 'Anular desde En microscopía'),
    (12, 20, 'SM_ADM_SISTEMA', 'Anular desde Procesada'),
    (13, 20, 'SM_ADM_SISTEMA', 'Anular desde En transcripción'),
    (14, 20, 'SM_ADM_SISTEMA', 'Anular desde En diagnóstico'),
    (15, 20, 'SM_ADM_SISTEMA', 'Anular desde Por validar'),
    (16, 20, 'SM_ADM_SISTEMA', 'Anular desde Por firmar'),
    (18, 21, 'SM_ADM_SISTEMA', 'Entregado → Re-firmado'),
    (21, 22, 'SM_ADM_SISTEMA', 'Re-firmado → Re-entregado'),

    -- ═══ SM_JEFE_LABORATORIO: desde EN_LABORATORIO(7) hasta ENTREGADO(18) + RECHAZADO ═══
    ( 7,  8, 'SM_JEFE_LABORATORIO', 'En laboratorio → En proceso'),
    ( 8,  9, 'SM_JEFE_LABORATORIO', 'En proceso → En macroscopía'),
    ( 9, 10, 'SM_JEFE_LABORATORIO', 'En macroscopía → En lab técnico'),
    (10, 11, 'SM_JEFE_LABORATORIO', 'En lab técnico → En microscopía'),
    (11, 12, 'SM_JEFE_LABORATORIO', 'En microscopía → Procesada'),
    (12, 13, 'SM_JEFE_LABORATORIO', 'Procesada → En transcripción'),
    (13, 14, 'SM_JEFE_LABORATORIO', 'En transcripción → En diagnóstico'),
    (14, 15, 'SM_JEFE_LABORATORIO', 'En diagnóstico → Por validar'),
    (15, 16, 'SM_JEFE_LABORATORIO', 'Por validar → Por firmar'),
    (16, 17, 'SM_JEFE_LABORATORIO', 'Por firmar → Firmado'),
    (17, 18, 'SM_JEFE_LABORATORIO', 'Firmado → Entregado'),
    ( 7, 19, 'SM_JEFE_LABORATORIO', 'Rechazar desde En laboratorio'),
    ( 8, 19, 'SM_JEFE_LABORATORIO', 'Rechazar desde En proceso'),
    ( 9, 19, 'SM_JEFE_LABORATORIO', 'Rechazar desde En macroscopía'),
    (10, 19, 'SM_JEFE_LABORATORIO', 'Rechazar desde En lab técnico'),
    (11, 19, 'SM_JEFE_LABORATORIO', 'Rechazar desde En microscopía'),
    (12, 19, 'SM_JEFE_LABORATORIO', 'Rechazar desde Procesada'),
    (13, 19, 'SM_JEFE_LABORATORIO', 'Rechazar desde En transcripción'),
    (14, 19, 'SM_JEFE_LABORATORIO', 'Rechazar desde En diagnóstico'),
    (15, 19, 'SM_JEFE_LABORATORIO', 'Rechazar desde Por validar'),
    (16, 19, 'SM_JEFE_LABORATORIO', 'Rechazar desde Por firmar'),
    (18, 21, 'SM_JEFE_LABORATORIO', 'Entregado → Re-firmado'),
    (21, 22, 'SM_JEFE_LABORATORIO', 'Re-firmado → Re-entregado'),

    -- ═══ SM_SOLICITANTE: solo CREADO → PARA_ETIQUETAR ═══
    ( 1,  2, 'SM_SOLICITANTE', 'Creado → Para etiquetar');

CREATE INDEX IF NOT EXISTS idx_etapa_estado_origen ON TBL_ETAPA_ESTADO(id_estado_origen);
CREATE INDEX IF NOT EXISTS idx_etapa_estado_rol    ON TBL_ETAPA_ESTADO(rol);

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

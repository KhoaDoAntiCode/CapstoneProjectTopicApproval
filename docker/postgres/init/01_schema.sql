-- =============================================================================
-- Capstone Project Registration Tool — Initial Schema
-- PostgreSQL 16
-- =============================================================================

-- ── Extensions ────────────────────────────────────────────────────────────────
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ── Users ─────────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS users (
    id          UUID         NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    email       VARCHAR(255) NOT NULL,
    full_name   VARCHAR(255) NOT NULL,
    avatar_url  VARCHAR(500),
    role        VARCHAR(20)  NOT NULL CHECK (role IN ('Lecturer', 'Student', 'Admin')),
    created_at  TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_users_email ON users (email);

-- ── Semesters ─────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS semesters (
    id          VARCHAR(20)  NOT NULL PRIMARY KEY,   -- e.g. 'SU26'
    name        VARCHAR(100) NOT NULL,
    start_date  DATE         NOT NULL,
    end_date    DATE         NOT NULL,
    is_active   BOOLEAN      NOT NULL DEFAULT FALSE
);

-- ── Capstone Projects ─────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS capstone_projects (
    id                          UUID         NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    project_code                VARCHAR(20)  NOT NULL,
    semester_id                 VARCHAR(20)  NOT NULL REFERENCES semesters (id) ON DELETE RESTRICT,
    created_by_id               UUID         NOT NULL REFERENCES users (id) ON DELETE RESTRICT,
    english_name                VARCHAR(500) NOT NULL,
    vietnamese_name             VARCHAR(500) NOT NULL,
    abbreviation                VARCHAR(20),
    is_research_project         BOOLEAN      NOT NULL DEFAULT FALSE,
    is_enterprise_project       BOOLEAN      NOT NULL DEFAULT FALSE,
    context                     TEXT,
    proposed_solutions          TEXT,
    functional_requirements     TEXT,
    non_functional_requirements TEXT,
    theory_and_practice         TEXT,
    products                    TEXT,
    proposed_tasks              TEXT,
    created_at                  TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at                  TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_capstone_projects_project_code ON capstone_projects (project_code);
CREATE        INDEX IF NOT EXISTS idx_capstone_projects_semester_id  ON capstone_projects (semester_id);

-- ── Project Supervisors ───────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS project_supervisors (
    id            UUID         NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    project_id    UUID         NOT NULL REFERENCES capstone_projects (id) ON DELETE CASCADE,
    full_name     VARCHAR(255) NOT NULL,
    phone         VARCHAR(20),
    email         VARCHAR(255),
    title         VARCHAR(100),
    is_primary    BOOLEAN      NOT NULL DEFAULT FALSE,
    display_order INT          NOT NULL DEFAULT 0,
    created_at    TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

-- ── Project Students ──────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS project_students (
    id            UUID         NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    project_id    UUID         NOT NULL REFERENCES capstone_projects (id) ON DELETE CASCADE,
    full_name     VARCHAR(255) NOT NULL,
    student_code  VARCHAR(20),
    phone         VARCHAR(20),
    email         VARCHAR(255),
    role_in_group VARCHAR(10)  CHECK (role_in_group IN ('Leader', 'Member')),
    display_order INT          NOT NULL DEFAULT 0,
    created_at    TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

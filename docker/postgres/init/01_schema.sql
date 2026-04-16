
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

CREATE TABLE IF NOT EXISTS users (
    id                 UUID         NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    email              VARCHAR(255) NOT NULL,
    full_name          VARCHAR(255) NOT NULL,
    avatar_url         VARCHAR(500),
    role               VARCHAR(20)  NOT NULL CHECK (role IN ('Lecturer', 'Student', 'Admin')),
    password_hash      VARCHAR(255) NOT NULL,
    is_email_verified  BOOLEAN      NOT NULL DEFAULT FALSE,
    created_at         TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at         TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_users_email ON users (email);

CREATE TABLE IF NOT EXISTS capstone_projects (
    id                          UUID         NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    project_code                VARCHAR(20)  NOT NULL,
    semester_id                 VARCHAR(20)  NOT NULL,
    created_by_id               UUID         NOT NULL REFERENCES users (id) ON DELETE RESTRICT,
    english_name                VARCHAR(500) NOT NULL,
    vietnamese_name             VARCHAR(500) NOT NULL,
    abbreviation                VARCHAR(255),
    is_research_project         BOOLEAN      NOT NULL DEFAULT FALSE,
    is_enterprise_project       BOOLEAN      NOT NULL DEFAULT FALSE,
    context                     TEXT,
    proposed_solutions          TEXT,
    functional_requirements     TEXT,
    non_functional_requirements TEXT,
    theory_and_practice         TEXT,
    products                    TEXT,
    proposed_tasks              TEXT,
    class                       VARCHAR(20),
    duration_from               DATE,
    duration_to                 DATE,
    profession                  VARCHAR(100),
    specialty                   VARCHAR(10)  CHECK (specialty IN (
                                               'SE','IA','AI','IS','IoT','GD','JS','AS','IC',
                                               'DM','IB','HM','TM','LG','FT',
                                               'MC','PR','EN','JP','KR','CN',
                                               'EL'
                                             )),
    register_kind               VARCHAR(20)  CHECK (register_kind IN ('Lecturer', 'Students')),
    status                      VARCHAR(20)  NOT NULL DEFAULT 'Pending'
                                             CHECK (status IN ('Pending', 'Accepted', 'Denied')),
    created_at                  TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at                  TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_capstone_projects_project_code ON capstone_projects (project_code);
CREATE        INDEX IF NOT EXISTS idx_capstone_projects_semester_id  ON capstone_projects (semester_id);

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

CREATE TABLE IF NOT EXISTS project_reviews (
    id               UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    project_id       UUID        NOT NULL REFERENCES capstone_projects (id) ON DELETE CASCADE,
    reviewed_by_id   UUID        NOT NULL REFERENCES users (id) ON DELETE RESTRICT,
    decision         VARCHAR(20) NOT NULL CHECK (decision IN ('Accepted', 'Denied')),
    comment          TEXT,
    reviewed_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_project_reviews_project_id     ON project_reviews (project_id);
CREATE INDEX IF NOT EXISTS idx_project_reviews_reviewed_by_id ON project_reviews (reviewed_by_id);

/*
    Demo SQL Server schema for the capstone topic registration project.
    Notes:
    - Password is stored as plain text on purpose for this demo only.
    - The schema is centered around one capstone submission that can
      include a supervisor, a student group, and multiple generated DOCX files.
*/

IF DB_ID(N'CapstoneRegistrationDemo') IS NULL
BEGIN
    CREATE DATABASE [CapstoneRegistrationDemo];
END;
GO

USE [CapstoneRegistrationDemo];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.submission_documents', N'U') IS NOT NULL DROP TABLE dbo.submission_documents;
IF OBJECT_ID(N'dbo.capstone_submissions', N'U') IS NOT NULL DROP TABLE dbo.capstone_submissions;
IF OBJECT_ID(N'dbo.student_group_members', N'U') IS NOT NULL DROP TABLE dbo.student_group_members;
IF OBJECT_ID(N'dbo.student_groups', N'U') IS NOT NULL DROP TABLE dbo.student_groups;
IF OBJECT_ID(N'dbo.students', N'U') IS NOT NULL DROP TABLE dbo.students;
IF OBJECT_ID(N'dbo.instructors', N'U') IS NOT NULL DROP TABLE dbo.instructors;
IF OBJECT_ID(N'dbo.admin_users', N'U') IS NOT NULL DROP TABLE dbo.admin_users;
GO

CREATE TABLE dbo.admin_users
(
    id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT PK_admin_users PRIMARY KEY
        CONSTRAINT DF_admin_users_id DEFAULT NEWID(),
    username NVARCHAR(100) NOT NULL,
    [password] NVARCHAR(255) NOT NULL,
    full_name NVARCHAR(255) NOT NULL,
    email NVARCHAR(255) NULL,
    created_at DATETIME2(0) NOT NULL
        CONSTRAINT DF_admin_users_created_at DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2(0) NOT NULL
        CONSTRAINT DF_admin_users_updated_at DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_admin_users_username UNIQUE (username)
);
GO

CREATE TABLE dbo.instructors
(
    id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT PK_instructors PRIMARY KEY
        CONSTRAINT DF_instructors_id DEFAULT NEWID(),
    full_name NVARCHAR(255) NOT NULL,
    title NVARCHAR(100) NULL,
    phone NVARCHAR(20) NULL,
    email NVARCHAR(255) NULL,
    department NVARCHAR(150) NULL,
    active BIT NOT NULL
        CONSTRAINT DF_instructors_active DEFAULT 1,
    created_at DATETIME2(0) NOT NULL
        CONSTRAINT DF_instructors_created_at DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2(0) NOT NULL
        CONSTRAINT DF_instructors_updated_at DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE dbo.students
(
    id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT PK_students PRIMARY KEY
        CONSTRAINT DF_students_id DEFAULT NEWID(),
    student_code NVARCHAR(20) NULL,
    full_name NVARCHAR(255) NOT NULL,
    phone NVARCHAR(20) NULL,
    email NVARCHAR(255) NULL,
    major NVARCHAR(100) NULL,
    specialty NVARCHAR(50) NULL,
    active BIT NOT NULL
        CONSTRAINT DF_students_active DEFAULT 1,
    created_at DATETIME2(0) NOT NULL
        CONSTRAINT DF_students_created_at DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2(0) NOT NULL
        CONSTRAINT DF_students_updated_at DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE dbo.student_groups
(
    id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT PK_student_groups PRIMARY KEY
        CONSTRAINT DF_student_groups_id DEFAULT NEWID(),
    group_code NVARCHAR(30) NULL,
    class_name NVARCHAR(50) NULL,
    profession NVARCHAR(100) NULL,
    specialty NVARCHAR(50) NULL,
    duration_from DATE NULL,
    duration_to DATE NULL,
    created_by_admin_id UNIQUEIDENTIFIER NOT NULL,
    created_at DATETIME2(0) NOT NULL
        CONSTRAINT DF_student_groups_created_at DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2(0) NOT NULL
        CONSTRAINT DF_student_groups_updated_at DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_student_groups_admin_users
        FOREIGN KEY (created_by_admin_id) REFERENCES dbo.admin_users(id)
);
GO

CREATE TABLE dbo.student_group_members
(
    id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT PK_student_group_members PRIMARY KEY
        CONSTRAINT DF_student_group_members_id DEFAULT NEWID(),
    group_id UNIQUEIDENTIFIER NOT NULL,
    student_id UNIQUEIDENTIFIER NOT NULL,
    role_in_group NVARCHAR(20) NOT NULL,
    display_order INT NOT NULL
        CONSTRAINT DF_student_group_members_display_order DEFAULT 1,
    created_at DATETIME2(0) NOT NULL
        CONSTRAINT DF_student_group_members_created_at DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_student_group_members_student_groups
        FOREIGN KEY (group_id) REFERENCES dbo.student_groups(id) ON DELETE CASCADE,
    CONSTRAINT FK_student_group_members_students
        FOREIGN KEY (student_id) REFERENCES dbo.students(id),
    CONSTRAINT CK_student_group_members_role
        CHECK (role_in_group IN (N'Leader', N'Member'))
);
GO

CREATE TABLE dbo.capstone_submissions
(
    id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT PK_capstone_submissions PRIMARY KEY
        CONSTRAINT DF_capstone_submissions_id DEFAULT NEWID(),
    submission_code NVARCHAR(30) NOT NULL,
    group_id UNIQUEIDENTIFIER NOT NULL,
    supervisor_id UNIQUEIDENTIFIER NULL,
    created_by_admin_id UNIQUEIDENTIFIER NOT NULL,
    register_kind NVARCHAR(20) NULL,
    topic_name_en NVARCHAR(500) NOT NULL,
    topic_name_vi NVARCHAR(500) NOT NULL,
    abbreviation NVARCHAR(255) NULL,
    [context] NVARCHAR(MAX) NULL,
    functional_requirements NVARCHAR(MAX) NULL,
    non_functional_requirements NVARCHAR(MAX) NULL,
    main_proposal_content NVARCHAR(MAX) NULL,
    server_side_technologies NVARCHAR(MAX) NULL,
    client_side_technologies NVARCHAR(MAX) NULL,
    expected_deliverables NVARCHAR(MAX) NULL,
    proposed_tasks NVARCHAR(MAX) NULL,
    other_comments NVARCHAR(MAX) NULL,
    sign_place NVARCHAR(100) NULL,
    sign_date DATE NULL,
    status NVARCHAR(20) NOT NULL
        CONSTRAINT DF_capstone_submissions_status DEFAULT N'draft',
    created_at DATETIME2(0) NOT NULL
        CONSTRAINT DF_capstone_submissions_created_at DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2(0) NOT NULL
        CONSTRAINT DF_capstone_submissions_updated_at DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_capstone_submissions_submission_code UNIQUE (submission_code),
    CONSTRAINT FK_capstone_submissions_student_groups
        FOREIGN KEY (group_id) REFERENCES dbo.student_groups(id),
    CONSTRAINT FK_capstone_submissions_instructors
        FOREIGN KEY (supervisor_id) REFERENCES dbo.instructors(id),
    CONSTRAINT FK_capstone_submissions_admin_users
        FOREIGN KEY (created_by_admin_id) REFERENCES dbo.admin_users(id),
    CONSTRAINT CK_capstone_submissions_status
        CHECK (status IN (N'draft', N'submitted', N'updated')),
    CONSTRAINT CK_capstone_submissions_register_kind
        CHECK (register_kind IS NULL OR register_kind IN (N'Lecturer', N'Students', N'Both'))
);
GO

CREATE TABLE dbo.submission_documents
(
    id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT PK_submission_documents PRIMARY KEY
        CONSTRAINT DF_submission_documents_id DEFAULT NEWID(),
    submission_id UNIQUEIDENTIFIER NOT NULL,
    version_no INT NOT NULL,
    file_name NVARCHAR(255) NOT NULL,
    file_path NVARCHAR(500) NOT NULL,
    generated_by_admin_id UNIQUEIDENTIFIER NOT NULL,
    generated_at DATETIME2(0) NOT NULL
        CONSTRAINT DF_submission_documents_generated_at DEFAULT SYSUTCDATETIME(),
    snapshot_json NVARCHAR(MAX) NULL,
    notes NVARCHAR(500) NULL,
    CONSTRAINT FK_submission_documents_capstone_submissions
        FOREIGN KEY (submission_id) REFERENCES dbo.capstone_submissions(id) ON DELETE CASCADE,
    CONSTRAINT FK_submission_documents_admin_users
        FOREIGN KEY (generated_by_admin_id) REFERENCES dbo.admin_users(id),
    CONSTRAINT UQ_submission_documents_version UNIQUE (submission_id, version_no)
);
GO

CREATE INDEX IX_students_student_code ON dbo.students(student_code);
CREATE INDEX IX_student_group_members_group_id ON dbo.student_group_members(group_id);
CREATE INDEX IX_capstone_submissions_group_id ON dbo.capstone_submissions(group_id);
CREATE INDEX IX_capstone_submissions_supervisor_id ON dbo.capstone_submissions(supervisor_id);
CREATE INDEX IX_capstone_submissions_status ON dbo.capstone_submissions(status);
CREATE INDEX IX_submission_documents_submission_id ON dbo.submission_documents(submission_id);
GO

INSERT INTO dbo.admin_users (username, [password], full_name, email)
VALUES (N'admin', N'admin123', N'System Admin', N'admin@local.demo');
GO

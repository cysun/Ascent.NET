﻿CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

CREATE TABLE "Files" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Name" character varying(1000) NOT NULL,
    "Version" integer NOT NULL,
    "ContentType" character varying(255) NULL,
    "Size" bigint NOT NULL,
    "Created" timestamp with time zone NOT NULL,
    "Updated" timestamp with time zone NOT NULL,
    "IsFolder" boolean NOT NULL,
    "ParentId" integer NULL,
    "AccessCount" integer NOT NULL,
    "IsSystem" boolean NOT NULL,
    "IsPublic" boolean NOT NULL,
    CONSTRAINT "PK_Files" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Files_Files_ParentId" FOREIGN KEY ("ParentId") REFERENCES "Files" ("Id")
);

CREATE TABLE "Grades" (
    "Symbol" character varying(4) NOT NULL,
    "Value" numeric NULL,
    "Description" character varying(255) NULL,
    CONSTRAINT "PK_Grades" PRIMARY KEY ("Symbol")
);

CREATE TABLE "Pages" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Subject" character varying(80) NOT NULL,
    "Content" text NULL,
    "TimeCreated" timestamp with time zone NOT NULL,
    "TimeUpdated" timestamp with time zone NOT NULL,
    "TimeViewed" timestamp with time zone NOT NULL,
    "IsPublic" boolean NOT NULL,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Pages" PRIMARY KEY ("Id")
);

CREATE TABLE "Persons" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "CampusId" character varying(100) NOT NULL,
    "FirstName" character varying(255) NOT NULL,
    "MiddleName" character varying(255) NULL,
    "LastName" character varying(255) NOT NULL,
    "ScreenName" character varying(100) NULL,
    "SchoolEmail" character varying(255) NULL,
    "PersonalEmail" character varying(255) NULL,
    "BgTerm_Code" integer NULL,
    "MgTerm_Code" integer NULL,
    "IsDeleted" boolean NOT NULL DEFAULT FALSE,
    CONSTRAINT "PK_Persons" PRIMARY KEY ("Id")
);

CREATE TABLE "Courses" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Subject" character varying(6) NOT NULL,
    "Number" character varying(6) NOT NULL,
    "Title" character varying(255) NOT NULL,
    "MinUnits" integer NOT NULL,
    "MaxUnits" integer NOT NULL,
    "CatalogDescription" text NULL,
    "AbetDescriptionId" integer NULL,
    "IsObsolete" boolean NOT NULL,
    CONSTRAINT "PK_Courses" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_Courses_Subject_Number" UNIQUE ("Subject", "Number"),
    CONSTRAINT "FK_Courses_Files_AbetDescriptionId" FOREIGN KEY ("AbetDescriptionId") REFERENCES "Files" ("Id")
);

CREATE TABLE "FileHistories" (
    "FileId" integer NOT NULL,
    "Version" integer NOT NULL,
    "Name" character varying(1000) NOT NULL,
    "ContentType" character varying(255) NULL,
    "Size" bigint NOT NULL,
    "Created" timestamp with time zone NOT NULL,
    "Updated" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_FileHistories" PRIMARY KEY ("FileId", "Version"),
    CONSTRAINT "FK_FileHistories_Files_FileId" FOREIGN KEY ("FileId") REFERENCES "Files" ("Id") ON DELETE CASCADE
);

CREATE TABLE "PageHistories" (
    "PageId" integer NOT NULL,
    "TimeCreated" timestamp with time zone NOT NULL,
    "Subject" character varying(80) NOT NULL,
    "Content" text NULL,
    CONSTRAINT "PK_PageHistories" PRIMARY KEY ("PageId", "TimeCreated"),
    CONSTRAINT "FK_PageHistories_Pages_PageId" FOREIGN KEY ("PageId") REFERENCES "Pages" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Sections" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Term_Code" integer NULL,
    "CourseId" integer NOT NULL,
    "InstructorId" integer NULL,
    CONSTRAINT "PK_Sections" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Sections_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES "Courses" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Sections_Persons_InstructorId" FOREIGN KEY ("InstructorId") REFERENCES "Persons" ("Id")
);

CREATE TABLE "Enrollments" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "SectionId" integer NOT NULL,
    "StudentId" integer NOT NULL,
    "GradeSymbol" character varying(4) NULL,
    CONSTRAINT "PK_Enrollments" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_Enrollments_SectionId_StudentId" UNIQUE ("SectionId", "StudentId"),
    CONSTRAINT "FK_Enrollments_Grades_GradeSymbol" FOREIGN KEY ("GradeSymbol") REFERENCES "Grades" ("Symbol"),
    CONSTRAINT "FK_Enrollments_Persons_StudentId" FOREIGN KEY ("StudentId") REFERENCES "Persons" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Enrollments_Sections_SectionId" FOREIGN KEY ("SectionId") REFERENCES "Sections" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Courses_AbetDescriptionId" ON "Courses" ("AbetDescriptionId");

CREATE INDEX "IX_Enrollments_GradeSymbol" ON "Enrollments" ("GradeSymbol");

CREATE INDEX "IX_Enrollments_StudentId" ON "Enrollments" ("StudentId");

CREATE INDEX "IX_Files_ParentId" ON "Files" ("ParentId");

CREATE UNIQUE INDEX "IX_Persons_CampusId" ON "Persons" ("CampusId");

CREATE INDEX "IX_Sections_CourseId" ON "Sections" ("CourseId");

CREATE INDEX "IX_Sections_InstructorId" ON "Sections" ("InstructorId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20220629190425_InitialSchema', '6.0.6');

COMMIT;


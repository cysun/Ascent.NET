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
    "TimeCreated" timestamp with time zone NOT NULL,
    "ParentId" integer NULL,
    "IsFolder" boolean NOT NULL,
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

CREATE TABLE "MftDistributionTypes" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Alias" character varying(15) NOT NULL,
    "Name" character varying(255) NULL,
    "Min" integer NOT NULL,
    "Max" integer NOT NULL,
    "ValueLabel" character varying(32) NULL,
    CONSTRAINT "PK_MftDistributionTypes" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_MftDistributionTypes_Alias" UNIQUE ("Alias")
);

CREATE TABLE "MftIndicators" (
    "Year" integer NOT NULL,
    "NumOfStudents" integer NOT NULL,
    "Scores" integer[] NULL,
    "Percentiles" integer[] NULL DEFAULT ('{null, null, null}'),
    CONSTRAINT "PK_MftIndicators" PRIMARY KEY ("Year")
);

CREATE TABLE "MftScores" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Year" integer NOT NULL,
    "StudentId" character varying(100) NOT NULL,
    "FirstName" character varying(255) NOT NULL,
    "LastName" character varying(255) NOT NULL,
    "Score" integer NOT NULL,
    "Percentile" integer NULL,
    CONSTRAINT "PK_MftScores" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_MftScores_Year_StudentId" UNIQUE ("Year", "StudentId")
);

CREATE TABLE "MftScoreStats" (
    "Year" integer NOT NULL,
    "Count" integer NOT NULL,
    "Mean" integer NOT NULL,
    "Median" integer NOT NULL,
    "MeanPercentile" integer NULL,
    "MedianPercentile" integer NULL,
    "InstitutionPercentile" integer NULL,
    CONSTRAINT "PK_MftScoreStats" PRIMARY KEY ("Year")
);

CREATE TABLE "Pages" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Version" integer NOT NULL,
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

CREATE TABLE "Surveys" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Name" character varying(100) NOT NULL,
    "Description" text NULL,
    "TimeCreated" timestamp with time zone NOT NULL,
    "TimePublished" timestamp with time zone NULL,
    "TimeClosed" timestamp with time zone NULL,
    "QuestionCount" integer NOT NULL,
    "ResponseCount" integer NOT NULL,
    "AllowMultipleSubmissions" boolean NOT NULL,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Surveys" PRIMARY KEY ("Id")
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

CREATE TABLE "FileRevisions" (
    "FileId" integer NOT NULL,
    "Version" integer NOT NULL,
    "Name" character varying(1000) NOT NULL,
    "ContentType" character varying(255) NULL,
    "Size" bigint NOT NULL,
    "TimeCreated" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_FileRevisions" PRIMARY KEY ("FileId", "Version"),
    CONSTRAINT "FK_FileRevisions_Files_FileId" FOREIGN KEY ("FileId") REFERENCES "Files" ("Id") ON DELETE CASCADE
);

CREATE TABLE "MftDistributions" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Year" integer NOT NULL,
    "TypeAlias" character varying(15) NOT NULL,
    "FromDate" date NOT NULL,
    "ToDate" date NOT NULL,
    "NumOfSamples" integer NOT NULL,
    "Mean" double precision NOT NULL,
    "Median" double precision NOT NULL,
    "StdDev" double precision NOT NULL,
    "Ranks" text NULL,
    CONSTRAINT "PK_MftDistributions" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_MftDistributions_Year_TypeAlias" UNIQUE ("Year", "TypeAlias"),
    CONSTRAINT "FK_MftDistributions_MftDistributionTypes_TypeAlias" FOREIGN KEY ("TypeAlias") REFERENCES "MftDistributionTypes" ("Alias") ON DELETE CASCADE
);

CREATE TABLE "PageRevisions" (
    "PageId" integer NOT NULL,
    "Version" integer NOT NULL,
    "Subject" character varying(80) NOT NULL,
    "Content" text NULL,
    "TimeCreated" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_PageRevisions" PRIMARY KEY ("PageId", "Version"),
    CONSTRAINT "FK_PageRevisions_Pages_PageId" FOREIGN KEY ("PageId") REFERENCES "Pages" ("Id") ON DELETE CASCADE
);

CREATE TABLE "SurveyQuestions" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Type" character varying(10) NOT NULL,
    "Description" text NULL,
    "SurveyId" integer NOT NULL,
    "Index" integer NOT NULL,
    "TextLength" integer NOT NULL,
    "MinRating" integer NOT NULL,
    "MaxRating" integer NOT NULL,
    "IncludeNotApplicable" boolean NOT NULL,
    "Choices" text NULL,
    "MinSelection" integer NOT NULL,
    "MaxSelection" integer NOT NULL,
    CONSTRAINT "PK_SurveyQuestions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_SurveyQuestions_Surveys_SurveyId" FOREIGN KEY ("SurveyId") REFERENCES "Surveys" ("Id") ON DELETE CASCADE
);

CREATE TABLE "SurveyResponses" (
    "Id" uuid NOT NULL,
    "SurveyId" integer NOT NULL,
    "TimeSubmitted" timestamp with time zone NULL,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_SurveyResponses" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_SurveyResponses_Surveys_SurveyId" FOREIGN KEY ("SurveyId") REFERENCES "Surveys" ("Id") ON DELETE CASCADE
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

CREATE TABLE "SurveyAnswers" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "QuestionId" integer NOT NULL,
    "ResponseId" uuid NOT NULL,
    "Text" text NULL,
    "Rating" integer NULL,
    "NotApplicable" boolean NOT NULL,
    "Selections" text NULL,
    CONSTRAINT "PK_SurveyAnswers" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_SurveyAnswers_ResponseId_QuestionId" UNIQUE ("ResponseId", "QuestionId"),
    CONSTRAINT "FK_SurveyAnswers_SurveyQuestions_QuestionId" FOREIGN KEY ("QuestionId") REFERENCES "SurveyQuestions" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_SurveyAnswers_SurveyResponses_ResponseId" FOREIGN KEY ("ResponseId") REFERENCES "SurveyResponses" ("Id") ON DELETE CASCADE
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

CREATE INDEX "IX_MftDistributions_TypeAlias" ON "MftDistributions" ("TypeAlias");

CREATE UNIQUE INDEX "IX_Persons_CampusId" ON "Persons" ("CampusId");

CREATE INDEX "IX_Sections_CourseId" ON "Sections" ("CourseId");

CREATE INDEX "IX_Sections_InstructorId" ON "Sections" ("InstructorId");

CREATE INDEX "IX_SurveyAnswers_QuestionId" ON "SurveyAnswers" ("QuestionId");

CREATE INDEX "IX_SurveyQuestions_SurveyId_Index" ON "SurveyQuestions" ("SurveyId", "Index");

CREATE INDEX "IX_SurveyResponses_SurveyId" ON "SurveyResponses" ("SurveyId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20220802215654_InitialSchema', '6.0.7');

COMMIT;


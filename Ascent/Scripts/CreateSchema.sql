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
    "ContentType" character varying(255),
    "Size" bigint NOT NULL,
    "TimeCreated" timestamp with time zone NOT NULL,
    "ParentId" integer,
    "IsFolder" boolean NOT NULL,
    "IsPublic" boolean NOT NULL,
    "IsRegular" boolean NOT NULL,
    CONSTRAINT "PK_Files" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Files_Files_ParentId" FOREIGN KEY ("ParentId") REFERENCES "Files" ("Id")
);

CREATE TABLE "Grades" (
    "Symbol" character varying(4) NOT NULL,
    "Value" numeric,
    "Description" character varying(255),
    CONSTRAINT "PK_Grades" PRIMARY KEY ("Symbol")
);

CREATE TABLE "Groups" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Name" character varying(32) NOT NULL,
    "Description" text,
    "EmailPreference" character varying(10) NOT NULL,
    "MemberCount" integer NOT NULL,
    "IsVirtual" boolean NOT NULL,
    CONSTRAINT "PK_Groups" PRIMARY KEY ("Id")
);

CREATE TABLE "Messages" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Author_Id" character varying(255) NOT NULL,
    "Author_FirstName" character varying(255) NOT NULL,
    "Author_LastName" character varying(255) NOT NULL,
    "Author_Email" character varying(255) NOT NULL,
    "Recipient" character varying(255) NOT NULL,
    "Subject" character varying(255) NOT NULL,
    "Content" text NOT NULL,
    "UseBcc" boolean NOT NULL,
    "TimeSent" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Messages" PRIMARY KEY ("Id")
);

CREATE TABLE "MftDistributionTypes" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Alias" character varying(15) NOT NULL,
    "Name" character varying(255),
    "Min" integer NOT NULL,
    "Max" integer NOT NULL,
    "ValueLabel" character varying(32),
    CONSTRAINT "PK_MftDistributionTypes" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_MftDistributionTypes_Alias" UNIQUE ("Alias")
);

CREATE TABLE "MftIndicators" (
    "Year" integer NOT NULL,
    "NumOfStudents" integer NOT NULL,
    "Scores" integer[],
    "Percentiles" integer[] DEFAULT ('{null, null, null}'),
    CONSTRAINT "PK_MftIndicators" PRIMARY KEY ("Year")
);

CREATE TABLE "MftScores" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Year" integer NOT NULL,
    "StudentId" character varying(100) NOT NULL,
    "FirstName" character varying(255) NOT NULL,
    "LastName" character varying(255) NOT NULL,
    "Score" integer NOT NULL,
    "Percentile" integer,
    CONSTRAINT "PK_MftScores" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_MftScores_Year_StudentId" UNIQUE ("Year", "StudentId")
);

CREATE TABLE "MftScoreStats" (
    "Year" integer NOT NULL,
    "Count" integer NOT NULL,
    "Mean" integer NOT NULL,
    "Median" integer NOT NULL,
    "MeanPercentile" integer,
    "MedianPercentile" integer,
    "InstitutionPercentile" integer,
    CONSTRAINT "PK_MftScoreStats" PRIMARY KEY ("Year")
);

CREATE TABLE "Pages" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Version" integer NOT NULL,
    "Subject" character varying(255) NOT NULL,
    "Content" text,
    "TimeCreated" timestamp with time zone NOT NULL,
    "TimeUpdated" timestamp with time zone NOT NULL,
    "TimeViewed" timestamp with time zone NOT NULL,
    "IsPinned" boolean NOT NULL,
    "IsPublic" boolean NOT NULL,
    "IsRegular" boolean NOT NULL,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Pages" PRIMARY KEY ("Id")
);

CREATE TABLE "Persons" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "CampusId" character varying(100) NOT NULL,
    "FirstName" character varying(255) NOT NULL,
    "MiddleName" character varying(255),
    "LastName" character varying(255) NOT NULL,
    "ScreenName" character varying(100),
    "SchoolEmail" character varying(255),
    "PersonalEmail" character varying(255),
    "BgTerm_Code" integer,
    "MgTerm_Code" integer,
    "CanvasId" integer,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Persons" PRIMARY KEY ("Id")
);

CREATE TABLE "Projects" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "AcademicYear" character varying(12),
    "Title" character varying(255) NOT NULL,
    "Description" text,
    "Sponsor" character varying(255),
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Projects" PRIMARY KEY ("Id")
);

CREATE TABLE "RubricDataImportLog" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "RubricId" integer NOT NULL,
    "TermCode" integer NOT NULL,
    "CourseId" integer NOT NULL,
    "SourceType" integer NOT NULL,
    "SourceId" character varying(100) NOT NULL,
    "Timestamp" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_RubricDataImportLog" PRIMARY KEY ("Id")
);

CREATE TABLE "Rubrics" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Name" character varying(80) NOT NULL,
    "Description" text,
    "CriteriaCount" integer NOT NULL,
    "TimePublished" timestamp with time zone,
    "IsObsolete" boolean NOT NULL,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Rubrics" PRIMARY KEY ("Id")
);

CREATE TABLE "Surveys" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Name" character varying(100) NOT NULL,
    "Description" text,
    "TimeCreated" timestamp with time zone NOT NULL,
    "TimePublished" timestamp with time zone,
    "TimeClosed" timestamp with time zone,
    "QuestionCount" integer NOT NULL,
    "ResponseCount" integer NOT NULL,
    "AllowMultipleSubmissions" boolean NOT NULL,
    "IsPinned" boolean NOT NULL,
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
    "CatalogDescription" text,
    "AbetSyllabusId" integer,
    "IsRequired" boolean NOT NULL,
    "IsElective" boolean NOT NULL,
    "IsService" boolean NOT NULL,
    "IsObsolete" boolean NOT NULL,
    CONSTRAINT "PK_Courses" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_Courses_Subject_Number" UNIQUE ("Subject", "Number"),
    CONSTRAINT "FK_Courses_Files_AbetSyllabusId" FOREIGN KEY ("AbetSyllabusId") REFERENCES "Files" ("Id")
);

CREATE TABLE "FileRevisions" (
    "FileId" integer NOT NULL,
    "Version" integer NOT NULL,
    "Name" character varying(1000) NOT NULL,
    "ContentType" character varying(255),
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
    "Ranks" text,
    CONSTRAINT "PK_MftDistributions" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_MftDistributions_Year_TypeAlias" UNIQUE ("Year", "TypeAlias"),
    CONSTRAINT "FK_MftDistributions_MftDistributionTypes_TypeAlias" FOREIGN KEY ("TypeAlias") REFERENCES "MftDistributionTypes" ("Alias") ON DELETE CASCADE
);

CREATE TABLE "PageRevisions" (
    "PageId" integer NOT NULL,
    "Version" integer NOT NULL,
    "Subject" character varying(80) NOT NULL,
    "Content" text,
    "TimeCreated" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_PageRevisions" PRIMARY KEY ("PageId", "Version"),
    CONSTRAINT "FK_PageRevisions_Pages_PageId" FOREIGN KEY ("PageId") REFERENCES "Pages" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Programs" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Name" character varying(255) NOT NULL,
    "HasObjectives" boolean NOT NULL,
    "Objectives" text,
    "ObjectivesDescriptionId" integer,
    "ModuleCount" integer NOT NULL,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Programs" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Programs_Pages_ObjectivesDescriptionId" FOREIGN KEY ("ObjectivesDescriptionId") REFERENCES "Pages" ("Id")
);

CREATE TABLE "GroupMembers" (
    "GroupId" integer NOT NULL,
    "PersonId" integer NOT NULL,
    CONSTRAINT "PK_GroupMembers" PRIMARY KEY ("GroupId", "PersonId"),
    CONSTRAINT "FK_GroupMembers_Groups_GroupId" FOREIGN KEY ("GroupId") REFERENCES "Groups" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_GroupMembers_Persons_PersonId" FOREIGN KEY ("PersonId") REFERENCES "Persons" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ProjectMembers" (
    "ProjectId" integer NOT NULL,
    "PersonId" integer NOT NULL,
    "Type" integer NOT NULL,
    CONSTRAINT "PK_ProjectMembers" PRIMARY KEY ("ProjectId", "PersonId", "Type"),
    CONSTRAINT "FK_ProjectMembers_Persons_PersonId" FOREIGN KEY ("PersonId") REFERENCES "Persons" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ProjectMembers_Projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ProjectResources" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "ProjectId" integer NOT NULL,
    "Name" character varying(255) NOT NULL,
    "Type" character varying(10) NOT NULL,
    "FileId" integer,
    "Text" text,
    "Url" character varying(2000),
    "IsPrivate" boolean NOT NULL,
    CONSTRAINT "PK_ProjectResources" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ProjectResources_Files_FileId" FOREIGN KEY ("FileId") REFERENCES "Files" ("Id"),
    CONSTRAINT "FK_ProjectResources_Projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("Id") ON DELETE CASCADE
);

CREATE TABLE "RubricCriteria" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Name" character varying(80) NOT NULL,
    "Description" text,
    "RubricId" integer NOT NULL,
    "Index" integer NOT NULL,
    CONSTRAINT "PK_RubricCriteria" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RubricCriteria_Rubrics_RubricId" FOREIGN KEY ("RubricId") REFERENCES "Rubrics" ("Id") ON DELETE CASCADE
);

CREATE TABLE "SurveyQuestions" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Type" character varying(10) NOT NULL,
    "Description" text,
    "SurveyId" integer NOT NULL,
    "Index" integer NOT NULL,
    "TextLength" integer NOT NULL,
    "MinRating" integer NOT NULL,
    "MaxRating" integer NOT NULL,
    "IncludeNotApplicable" boolean NOT NULL,
    "Choices" text,
    "MinSelection" integer NOT NULL,
    "MaxSelection" integer NOT NULL,
    CONSTRAINT "PK_SurveyQuestions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_SurveyQuestions_Surveys_SurveyId" FOREIGN KEY ("SurveyId") REFERENCES "Surveys" ("Id") ON DELETE CASCADE
);

CREATE TABLE "SurveyResponses" (
    "Id" uuid NOT NULL,
    "SurveyId" integer NOT NULL,
    "TimeSubmitted" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_SurveyResponses" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_SurveyResponses_Surveys_SurveyId" FOREIGN KEY ("SurveyId") REFERENCES "Surveys" ("Id") ON DELETE CASCADE
);

CREATE TABLE "CourseCoordinators" (
    "CourseId" integer NOT NULL,
    "PersonId" integer NOT NULL,
    CONSTRAINT "PK_CourseCoordinators" PRIMARY KEY ("CourseId", "PersonId"),
    CONSTRAINT "FK_CourseCoordinators_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES "Courses" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_CourseCoordinators_Persons_PersonId" FOREIGN KEY ("PersonId") REFERENCES "Persons" ("Id") ON DELETE CASCADE
);

CREATE TABLE "CourseJournals" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "CourseId" integer NOT NULL,
    "Term_Code" integer,
    "InstructorId" integer NOT NULL,
    "CourseUrl" character varying(255) NOT NULL,
    "SampleStudentWorkUrl" character varying(255),
    CONSTRAINT "PK_CourseJournals" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_CourseJournals_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES "Courses" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_CourseJournals_Persons_InstructorId" FOREIGN KEY ("InstructorId") REFERENCES "Persons" ("Id") ON DELETE CASCADE
);

CREATE TABLE "CourseTemplates" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "CourseId" integer NOT NULL,
    CONSTRAINT "PK_CourseTemplates" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_CourseTemplates_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES "Courses" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Sections" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Term_Code" integer,
    "CourseId" integer NOT NULL,
    "InstructorId" integer NOT NULL,
    CONSTRAINT "PK_Sections" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Sections_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES "Courses" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Sections_Persons_InstructorId" FOREIGN KEY ("InstructorId") REFERENCES "Persons" ("Id") ON DELETE CASCADE
);

CREATE TABLE "OutcomeSurveys" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "ConstituencyType" integer NOT NULL,
    "SurveyId" integer NOT NULL,
    "ProgramId" integer NOT NULL,
    "QuestionIds" integer[],
    "DataImportTime" timestamp with time zone,
    CONSTRAINT "PK_OutcomeSurveys" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_OutcomeSurveys_Programs_ProgramId" FOREIGN KEY ("ProgramId") REFERENCES "Programs" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_OutcomeSurveys_Surveys_SurveyId" FOREIGN KEY ("SurveyId") REFERENCES "Surveys" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ProgramModules" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "ProgramId" integer NOT NULL,
    "Index" integer NOT NULL,
    "Name" character varying(64) NOT NULL,
    "ResourceCount" integer NOT NULL,
    CONSTRAINT "PK_ProgramModules" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ProgramModules_Programs_ProgramId" FOREIGN KEY ("ProgramId") REFERENCES "Programs" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ProgramOutcomes" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "ProgramId" integer NOT NULL,
    "Index" integer NOT NULL,
    "Text" text NOT NULL,
    "DescriptionId" integer NOT NULL,
    CONSTRAINT "PK_ProgramOutcomes" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ProgramOutcomes_Pages_DescriptionId" FOREIGN KEY ("DescriptionId") REFERENCES "Pages" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ProgramOutcomes_Programs_ProgramId" FOREIGN KEY ("ProgramId") REFERENCES "Programs" ("Id") ON DELETE CASCADE
);

CREATE TABLE "RubricRatings" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "CriterionId" integer NOT NULL,
    "Index" integer NOT NULL,
    "Value" double precision NOT NULL,
    "Name" character varying(80),
    "Description" text,
    CONSTRAINT "PK_RubricRatings" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RubricRatings_RubricCriteria_CriterionId" FOREIGN KEY ("CriterionId") REFERENCES "RubricCriteria" ("Id") ON DELETE CASCADE
);

CREATE TABLE "SurveyAnswers" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "QuestionId" integer NOT NULL,
    "ResponseId" uuid NOT NULL,
    "Text" text,
    "Rating" integer,
    "NotApplicable" boolean NOT NULL,
    "SingleSelection" text,
    "Selections" text,
    CONSTRAINT "PK_SurveyAnswers" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_SurveyAnswers_ResponseId_QuestionId" UNIQUE ("ResponseId", "QuestionId"),
    CONSTRAINT "FK_SurveyAnswers_SurveyQuestions_QuestionId" FOREIGN KEY ("QuestionId") REFERENCES "SurveyQuestions" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_SurveyAnswers_SurveyResponses_ResponseId" FOREIGN KEY ("ResponseId") REFERENCES "SurveyResponses" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AssignmentTemplates" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Name" character varying(100) NOT NULL,
    "Description" text,
    "CourseTemplateId" integer NOT NULL,
    "RubricId" integer,
    "IsPeerReviewed" boolean NOT NULL,
    CONSTRAINT "PK_AssignmentTemplates" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AssignmentTemplates_CourseTemplates_CourseTemplateId" FOREIGN KEY ("CourseTemplateId") REFERENCES "CourseTemplates" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AssignmentTemplates_Rubrics_RubricId" FOREIGN KEY ("RubricId") REFERENCES "Rubrics" ("Id")
);

CREATE TABLE "Enrollments" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "SectionId" integer NOT NULL,
    "StudentId" integer NOT NULL,
    "GradeSymbol" character varying(4),
    CONSTRAINT "PK_Enrollments" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_Enrollments_SectionId_StudentId" UNIQUE ("SectionId", "StudentId"),
    CONSTRAINT "FK_Enrollments_Grades_GradeSymbol" FOREIGN KEY ("GradeSymbol") REFERENCES "Grades" ("Symbol"),
    CONSTRAINT "FK_Enrollments_Persons_StudentId" FOREIGN KEY ("StudentId") REFERENCES "Persons" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Enrollments_Sections_SectionId" FOREIGN KEY ("SectionId") REFERENCES "Sections" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ProgramResources" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "ModuleId" integer NOT NULL,
    "Index" integer NOT NULL,
    "Type" character varying(10) NOT NULL,
    "FileId" integer,
    "PageId" integer,
    CONSTRAINT "PK_ProgramResources" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ProgramResources_Files_FileId" FOREIGN KEY ("FileId") REFERENCES "Files" ("Id"),
    CONSTRAINT "FK_ProgramResources_Pages_PageId" FOREIGN KEY ("PageId") REFERENCES "Pages" ("Id"),
    CONSTRAINT "FK_ProgramResources_ProgramModules_ModuleId" FOREIGN KEY ("ModuleId") REFERENCES "ProgramModules" ("Id") ON DELETE CASCADE
);

CREATE TABLE "RubricData" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Year" integer NOT NULL,
    "Term_Code" integer,
    "CourseId" integer NOT NULL,
    "AssessmentType" integer NOT NULL,
    "EvaluatorId" integer NOT NULL,
    "EvaluateeId" integer NOT NULL,
    "RubricId" integer NOT NULL,
    "CriterionId" integer NOT NULL,
    "RatingId" integer NOT NULL,
    "Comments" text,
    "SourceType" integer,
    "SourceId" character varying(100),
    CONSTRAINT "PK_RubricData" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RubricData_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES "Courses" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_RubricData_Persons_EvaluateeId" FOREIGN KEY ("EvaluateeId") REFERENCES "Persons" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_RubricData_Persons_EvaluatorId" FOREIGN KEY ("EvaluatorId") REFERENCES "Persons" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_RubricData_RubricCriteria_CriterionId" FOREIGN KEY ("CriterionId") REFERENCES "RubricCriteria" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_RubricData_RubricRatings_RatingId" FOREIGN KEY ("RatingId") REFERENCES "RubricRatings" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_RubricData_Rubrics_RubricId" FOREIGN KEY ("RubricId") REFERENCES "Rubrics" ("Id") ON DELETE CASCADE
);

CREATE TABLE "SurveyData" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "ConstituencyType" integer NOT NULL,
    "Year" integer NOT NULL,
    "ProgramId" integer NOT NULL,
    "OutcomeId" integer NOT NULL,
    "SurveyId" integer NOT NULL,
    "AnswerId" integer NOT NULL,
    "Value" integer NOT NULL,
    CONSTRAINT "PK_SurveyData" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_SurveyData_ProgramOutcomes_OutcomeId" FOREIGN KEY ("OutcomeId") REFERENCES "ProgramOutcomes" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_SurveyData_Programs_ProgramId" FOREIGN KEY ("ProgramId") REFERENCES "Programs" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_SurveyData_SurveyAnswers_AnswerId" FOREIGN KEY ("AnswerId") REFERENCES "SurveyAnswers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_SurveyData_Surveys_SurveyId" FOREIGN KEY ("SurveyId") REFERENCES "Surveys" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AssignmentTemplates_CourseTemplateId" ON "AssignmentTemplates" ("CourseTemplateId");

CREATE INDEX "IX_AssignmentTemplates_RubricId" ON "AssignmentTemplates" ("RubricId");

CREATE INDEX "IX_CourseCoordinators_PersonId" ON "CourseCoordinators" ("PersonId");

CREATE UNIQUE INDEX "IX_CourseJournals_CourseId" ON "CourseJournals" ("CourseId");

CREATE INDEX "IX_CourseJournals_InstructorId" ON "CourseJournals" ("InstructorId");

CREATE INDEX "IX_Courses_AbetSyllabusId" ON "Courses" ("AbetSyllabusId");

CREATE INDEX "IX_CourseTemplates_CourseId" ON "CourseTemplates" ("CourseId");

CREATE INDEX "IX_Enrollments_GradeSymbol" ON "Enrollments" ("GradeSymbol");

CREATE INDEX "IX_Enrollments_StudentId" ON "Enrollments" ("StudentId");

CREATE INDEX "IX_Files_ParentId" ON "Files" ("ParentId");

CREATE INDEX "IX_GroupMembers_PersonId" ON "GroupMembers" ("PersonId");

CREATE UNIQUE INDEX "IX_Groups_Name" ON "Groups" ("Name");

CREATE INDEX "IX_MftDistributions_TypeAlias" ON "MftDistributions" ("TypeAlias");

CREATE INDEX "IX_OutcomeSurveys_ProgramId" ON "OutcomeSurveys" ("ProgramId");

CREATE INDEX "IX_OutcomeSurveys_SurveyId" ON "OutcomeSurveys" ("SurveyId");

CREATE UNIQUE INDEX "IX_Persons_CampusId" ON "Persons" ("CampusId");

CREATE UNIQUE INDEX "IX_Persons_CanvasId" ON "Persons" ("CanvasId");

CREATE INDEX "IX_ProgramModules_ProgramId_Index" ON "ProgramModules" ("ProgramId", "Index");

CREATE INDEX "IX_ProgramOutcomes_DescriptionId" ON "ProgramOutcomes" ("DescriptionId");

CREATE INDEX "IX_ProgramOutcomes_ProgramId_Index" ON "ProgramOutcomes" ("ProgramId", "Index");

CREATE INDEX "IX_ProgramResources_FileId" ON "ProgramResources" ("FileId");

CREATE INDEX "IX_ProgramResources_ModuleId_Index" ON "ProgramResources" ("ModuleId", "Index");

CREATE INDEX "IX_ProgramResources_PageId" ON "ProgramResources" ("PageId");

CREATE INDEX "IX_Programs_ObjectivesDescriptionId" ON "Programs" ("ObjectivesDescriptionId");

CREATE INDEX "IX_ProjectMembers_PersonId" ON "ProjectMembers" ("PersonId");

CREATE INDEX "IX_ProjectResources_FileId" ON "ProjectResources" ("FileId");

CREATE INDEX "IX_ProjectResources_ProjectId" ON "ProjectResources" ("ProjectId");

CREATE INDEX "IX_RubricCriteria_RubricId" ON "RubricCriteria" ("RubricId");

CREATE INDEX "IX_RubricData_CourseId" ON "RubricData" ("CourseId");

CREATE INDEX "IX_RubricData_CriterionId" ON "RubricData" ("CriterionId");

CREATE INDEX "IX_RubricData_EvaluateeId" ON "RubricData" ("EvaluateeId");

CREATE INDEX "IX_RubricData_EvaluatorId" ON "RubricData" ("EvaluatorId");

CREATE INDEX "IX_RubricData_RatingId" ON "RubricData" ("RatingId");

CREATE INDEX "IX_RubricData_RubricId" ON "RubricData" ("RubricId");

CREATE INDEX "IX_RubricDataImportLog_SourceId_SourceType" ON "RubricDataImportLog" ("SourceId", "SourceType");

CREATE INDEX "IX_RubricRatings_CriterionId" ON "RubricRatings" ("CriterionId");

CREATE INDEX "IX_Sections_CourseId" ON "Sections" ("CourseId");

CREATE INDEX "IX_Sections_InstructorId" ON "Sections" ("InstructorId");

CREATE INDEX "IX_SurveyAnswers_QuestionId" ON "SurveyAnswers" ("QuestionId");

CREATE INDEX "IX_SurveyData_AnswerId" ON "SurveyData" ("AnswerId");

CREATE INDEX "IX_SurveyData_OutcomeId" ON "SurveyData" ("OutcomeId");

CREATE INDEX "IX_SurveyData_ProgramId" ON "SurveyData" ("ProgramId");

CREATE INDEX "IX_SurveyData_SurveyId" ON "SurveyData" ("SurveyId");

CREATE INDEX "IX_SurveyQuestions_SurveyId_Index" ON "SurveyQuestions" ("SurveyId", "Index");

CREATE INDEX "IX_SurveyResponses_SurveyId" ON "SurveyResponses" ("SurveyId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20240821203922_InitialSchema', '8.0.8');

COMMIT;


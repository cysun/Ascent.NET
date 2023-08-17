ALTER TABLE "CourseJournals" DROP COLUMN "SyllabusUrl";
ALTER TABLE "CourseJournals" DROP COLUMN "SampleStudentAUrl";
ALTER TABLE "CourseJournals" DROP COLUMN "SampleStudentBUrl";
ALTER TABLE "CourseJournals" DROP COLUMN "SampleStudentCUrl";
ALTER TABLE "CourseJournals" ADD COLUMN "SampleStudentWorkUrl" character varying(255) NULL;

DELETE FROM "__EFMigrationsHistory";
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES ('20230817161722_InitialSchema', '7.0.10');


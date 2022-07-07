DROP TABLE "MftDistributions";
DROP TABLE "MftDistributionTypes";
DROP TABLE "MftIndicators";
DROP TABLE "MftScoreStats";
DROP TABLE "MftScores";

DROP TRIGGER "FilesTsTrigger" ON "Files";
DROP FUNCTION "FilesTsTriggerFunction"();
DROP FUNCTION "SearchFiles"(varchar, integer);

DROP TRIGGER "PagesTsTrigger" ON "Pages";
DROP FUNCTION "PagesTsTriggerFunction"();
DROP FUNCTION "SearchPages"(varchar, integer);

DROP FUNCTION "SearchPersons"(varchar, integer);

DROP TABLE "Enrollments";
DROP TABLE "Sections";
DROP TABLE "Grades";
DROP TABLE "Courses";
DROP TABLE "Persons";
DROP TABLE "PageRevisions";
DROP TABLE "FileRevisions";
DROP TABLE "Pages";
DROP TABLE "Files";
DROP TABLE "__EFMigrationsHistory";

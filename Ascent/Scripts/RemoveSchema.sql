DROP TABLE "ProgramItems";
DROP TABLE "ProgramModules";
DROP TABLE "ProgramOutcomes";
DROP TABLE "Programs";

DROP TABLE "SurveyAnswers";
DROP TABLE "SurveyResponses";
DROP TABLE "SurveyQuestions";
DROP TABLE "Surveys";

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
DROP TABLE "GroupMembers";
DROP TABLE "Groups";
DROP TABLE "Persons";
DROP TABLE "PageRevisions";
DROP TABLE "FileRevisions";
DROP TABLE "Pages";
DROP TABLE "Files";
DROP TABLE "__EFMigrationsHistory";

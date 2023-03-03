DROP TABLE "SurveyData";
DROP TABLE "OutcomeSurveys";

DROP VIEW "RubricDataByPerson";
DROP TABLE "RubricData";

DROP TRIGGER "ProjectsTsTrigger" ON "Projects";
DROP FUNCTION "ProjectsTsTriggerFunction"();
DROP FUNCTION "SearchProjects"(varchar, integer);

DROP TABLE "ProjectResources";
DROP TABLE "ProjectMembers";
DROP TABLE "Projects";

DROP TABLE "RubricRatings";
DROP TABLE "RubricCriteria";
DROP TABLE "Rubrics";

DROP TABLE "ProgramResources";
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

DROP TRIGGER "SectionsTsTrigger" ON "Sections";
DROP FUNCTION "SectionsTsTriggerFunction"();
DROP FUNCTION "SearchSections"(varchar, integer);

ALTER TABLE "Courses" DROP CONSTRAINT "FK_Courses_CourseJournals_CourseJournalId";

DROP TABLE "Enrollments";
DROP TABLE "Sections";
DROP TABLE "Grades";
DROP TABLE "StudentSamples";
DROP TABLE "CourseJournals";
DROP TABLE "Courses";
DROP TABLE "Messages";
DROP TABLE "GroupMembers";
DROP TABLE "Groups";
DROP TABLE "Persons";
DROP TABLE "PageRevisions";
DROP TABLE "FileRevisions";
DROP TABLE "Pages";
DROP TABLE "Files";
DROP TABLE "__EFMigrationsHistory";

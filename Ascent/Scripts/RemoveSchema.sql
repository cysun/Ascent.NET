DROP TRIGGER "PagesTsTrigger" ON "Pages";
DROP FUNCTION "PagesTsTriggerFunction"();

DROP FUNCTION "SearchPages"(varchar, integer);
DROP FUNCTION "SearchPersons"(varchar, integer);

DROP TABLE "Enrollments";
DROP TABLE "Sections";
DROP TABLE "Grades";
DROP TABLE "Courses";
DROP TABLE "Persons";
DROP TABLE "PageHistories";
DROP TABLE "FileHistories";
DROP TABLE "Pages";
DROP TABLE "Files";
DROP TABLE "__EFMigrationsHistory";

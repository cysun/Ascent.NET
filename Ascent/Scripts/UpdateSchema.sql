DROP TABLE "SampleStudents";
ALTER TABLE "CourseJournals" ADD COLUMN "SampleStudentAUrl" character varying(255) NOT NULL default '';
ALTER TABLE "CourseJournals" ADD COLUMN "SampleStudentBUrl" character varying(255) NOT NULL default '';
ALTER TABLE "CourseJournals" ADD COLUMN "SampleStudentCUrl" character varying(255) NOT NULL default '';

CREATE TABLE "CourseCoordinators" (
    "CourseId" integer NOT NULL,
    "PersonId" integer NOT NULL,
    CONSTRAINT "PK_CourseCoordinators" PRIMARY KEY ("CourseId", "PersonId"),
    CONSTRAINT "FK_CourseCoordinators_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES "Courses" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_CourseCoordinators_Persons_PersonId" FOREIGN KEY ("PersonId") REFERENCES "Persons" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_CourseCoordinators_PersonId" ON "CourseCoordinators" ("PersonId");

DROP INDEX "IX_CourseJournals_CourseId";
CREATE UNIQUE INDEX "IX_CourseJournals_CourseId" ON "CourseJournals" ("CourseId");

DROP INDEX "IX_Courses_CourseJournalId";
ALTER TABLE "Courses" DROP COLUMN "CourseJournalId";

ALTER TABLE "Courses" ADD COLUMN "AbetSyllabusId" integer NULL;
ALTER TABLE "Courses" ADD CONSTRAINT "FK_Courses_Files_AbetSyllabusId" FOREIGN KEY ("AbetSyllabusId") REFERENCES "Files" ("Id");
CREATE INDEX "IX_Courses_AbetSyllabusId" ON "Courses" ("AbetSyllabusId");

DELETE FROM "__EFMigrationsHistory";
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES ('20230424203034_InitialSchema', '7.0.5');

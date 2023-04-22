CREATE TABLE "CourseCoordinators" (
    "CourseId" integer NOT NULL,
    "PersonId" integer NOT NULL,
    CONSTRAINT "PK_CourseCoordinators" PRIMARY KEY ("CourseId", "PersonId"),
    CONSTRAINT "FK_CourseCoordinators_Persons_PersonId" FOREIGN KEY ("PersonId") REFERENCES "Persons" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_CourseCoordinators_PersonId" ON "CourseCoordinators" ("PersonId");

ALTER TABLE "CourseCoordinators" ADD CONSTRAINT "FK_CourseCoordinators_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES "Courses" ("Id") ON DELETE CASCADE;

DELETE FROM "__EFMigrationsHistory";
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES ('20230421233539_InitialSchema', '7.0.5');

ALTER TABLE "ProgramModules" RENAME "ItemCount" TO "ResourceCount";
ALTER TABLE "ProgramItems" RENAME TO "ProgramResources";

ALTER TABLE "Projects" DROP COLUMN "IsPrivate";
ALTER TABLE "Projects" ADD COLUMN "IsDeleted" boolean NOT NULL DEFAULT false;

ALTER TABLE "ProjectItems" RENAME TO "ProjectResources";

CREATE TABLE "ProjectMembers" (
    "ProjectId" integer NOT NULL,
    "PersonId" integer NOT NULL,
    "Type" integer NOT NULL,
    CONSTRAINT "PK_ProjectMembers" PRIMARY KEY ("ProjectId", "PersonId", "Type"),
    CONSTRAINT "FK_ProjectMembers_Persons_PersonId" FOREIGN KEY ("PersonId") REFERENCES "Persons" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ProjectMembers_Projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("Id") ON DELETE CASCADE
);

INSERT INTO "ProjectMembers" ("ProjectId", "PersonId", "Type") SELECT "ProjectId", "PersonId", 1 FROM "ProjectStudents";
INSERT INTO "ProjectMembers" ("ProjectId", "PersonId", "Type") SELECT "ProjectId", "PersonId", 2 FROM "ProjectAdvisors";
INSERT INTO "ProjectMembers" ("ProjectId", "PersonId", "Type") SELECT "ProjectId", "PersonId", 3 FROM "ProjectLiaisons";

DROP TABLE "ProjectStudents";
DROP TABLE "ProjectAdvisors";
DROP TABLE "ProjectLiaisons";

DELETE FROM "__EFMigrationsHistory";
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES ('20230127223800_InitialSchema', '7.0.2');

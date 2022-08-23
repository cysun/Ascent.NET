ALTER TABLE "Groups" ADD COLUMN "MemberCount" integer NOT NULL DEFAULT 0;

UPDATE "Groups" g SET "MemberCount" = (SELECT COUNT(*) FROM "GroupMembers" WHERE "GroupId" = g."Id");

DELETE FROM "__EFMigrationsHistory";
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES ('20220823223655_InitialSchema', '6.0.8');

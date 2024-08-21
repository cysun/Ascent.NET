ALTER TABLE "Messages" DROP COLUMN "IsFailed";

DELETE FROM "__EFMigrationsHistory";
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES ('20240821203922_InitialSchema', '8.0.8');

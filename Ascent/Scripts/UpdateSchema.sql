ALTER TABLE "Surveys" ADD COLUMN "IsPinned" boolean NOT NULL default FALSE;

DELETE FROM "__EFMigrationsHistory";
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES ('20240229035244_InitialSchema', '8.0.2');

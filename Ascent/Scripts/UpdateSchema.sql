ALTER TABLE "Pages" ALTER COLUMN "Subject" TYPE character varying(255);

ALTER TABLE "Files" ADD COLUMN "IsRegular" boolean NOT NULL DEFAULT FALSE;
UPDATE "Files" SET "IsRegular" = true;

CREATE OR REPLACE FUNCTION "SearchFiles"(varchar, integer DEFAULT NULL)
RETURNS SETOF "Files" AS $$
BEGIN
    RETURN QUERY SELECT * FROM "Files" WHERE "IsRegular" AND plainto_tsquery($1) @@ tsv LIMIT $2;
    RETURN;
 END
$$ LANGUAGE plpgsql;

DELETE FROM "__EFMigrationsHistory";
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES ('20220810173856_InitialSchema', '6.0.8');

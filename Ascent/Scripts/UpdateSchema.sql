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

CREATE INDEX ON "Persons" (lower("LastName" || ', ' || "FirstName") varchar_pattern_ops);

CREATE OR REPLACE FUNCTION "SearchPersons"(varchar, integer DEFAULT NULL)
RETURNS SETOF "Persons" AS $$
    SELECT * FROM "Persons" WHERE "CampusId" LIKE $1
        OR lower("FirstName") LIKE $1
        OR lower("LastName") LIKE $1
        OR lower("FirstName" || ' ' || "LastName") LIKE $1
        OR lower("LastName" || ', ' || "FirstName") LIKE $1
        OR lower("SchoolEmail") LIKE $1
        OR lower("PersonalEmail") LIKE $1
        ORDER BY "FirstName", "LastName" asc
        LIMIT $2;
$$ LANGUAGE sql;

DELETE FROM "__EFMigrationsHistory";
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES ('20220810173856_InitialSchema', '6.0.8');

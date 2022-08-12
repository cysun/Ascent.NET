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

ALTER TABLE "Programs" ADD COLUMN "ModuleCount" integer NOT NULL;

CREATE OR REPLACE FUNCTION "SearchPages"(varchar, integer DEFAULT NULL)
RETURNS SETOF "Pages" AS $$
DECLARE
    l_query text;
BEGIN
    l_query := plainto_tsquery($1)::text;
    IF l_query <> '' THEN
        l_query := l_query || ':*';
    END IF;
    RETURN QUERY SELECT * FROM "Pages" WHERE "IsRegular" AND l_query::tsquery @@ tsv LIMIT $2;
    RETURN;
 END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION "SearchFiles"(varchar, integer DEFAULT NULL)
RETURNS SETOF "Files" AS $$
DECLARE
    l_query text;
BEGIN
    l_query := plainto_tsquery($1)::text;
    IF l_query <> '' THEN
        l_query := l_query || ':*';
    END IF;
    RETURN QUERY SELECT * FROM "Files" WHERE "IsRegular" AND l_query::tsquery @@ tsv LIMIT $2;
    RETURN;
 END
$$ LANGUAGE plpgsql;

DELETE FROM "__EFMigrationsHistory";
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES ('20220811174125_InitialSchema', '6.0.8');

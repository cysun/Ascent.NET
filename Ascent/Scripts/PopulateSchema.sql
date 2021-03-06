-- Create indexes for prefix search of persons
CREATE INDEX ON "Persons" ("CampusId" varchar_pattern_ops);
CREATE INDEX ON "Persons" (lower("FirstName") varchar_pattern_ops);
CREATE INDEX ON "Persons" (lower("LastName") varchar_pattern_ops);
CREATE INDEX ON "Persons" (lower("FirstName" || ' ' || "LastName") varchar_pattern_ops);
CREATE INDEX ON "Persons" (lower("SchoolEmail") varchar_pattern_ops);
CREATE INDEX ON "Persons" (lower("PersonalEmail") varchar_pattern_ops);

-- The first argument is the pattern, e.g. 'John%', and the second argument is the max number of results.
CREATE OR REPLACE FUNCTION "SearchPersons"(varchar, integer DEFAULT NULL)
RETURNS SETOF "Persons" AS $$
    SELECT * FROM "Persons" WHERE "CampusId" LIKE $1
        OR lower("FirstName") LIKE $1
        OR lower("LastName") LIKE $1
        OR lower("FirstName" || ' ' || "LastName") LIKE $1
        OR lower("SchoolEmail") LIKE $1
        OR lower("PersonalEmail") LIKE $1
        ORDER BY "FirstName", "LastName" asc
        LIMIT $2;
$$ LANGUAGE sql;

CREATE INDEX ON "Courses" ("Number" varchar_pattern_ops);

-- FTS on Pages

ALTER TABLE "Pages" ADD COLUMN tsv tsvector;

CREATE INDEX "PagesTsIndex" ON "Pages" USING GIN(tsv);

CREATE OR REPLACE FUNCTION "PagesTsTriggerFunction"() RETURNS TRIGGER AS $$
BEGIN
    NEW.tsv := setweight(to_tsvector(coalesce(NEW."Subject",'')), 'A') ||
               setweight(to_tsvector(coalesce(NEW."Content",'')), 'D');
    RETURN NEW;
END
$$ LANGUAGE plpgsql;

CREATE TRIGGER "PagesTsTrigger"
    BEFORE INSERT OR UPDATE ON "Pages"
    FOR EACH ROW EXECUTE PROCEDURE "PagesTsTriggerFunction"();

CREATE OR REPLACE FUNCTION "SearchPages"(varchar, integer DEFAULT NULL)
RETURNS SETOF "Pages" AS $$
BEGIN
    RETURN QUERY SELECT * FROM "Pages" WHERE plainto_tsquery($1) @@ tsv LIMIT $2;
    RETURN;
 END
$$ LANGUAGE plpgsql;

-- Start File Id from 1000000 --

ALTER SEQUENCE "Files_Id_seq" RESTART WITH 1000000;

-- FTS on File Names --

ALTER TABLE "Files" ADD COLUMN tsv tsvector;

CREATE INDEX "FilesTsIndex" ON "Files" USING GIN(tsv);

CREATE OR REPLACE FUNCTION "FilesTsTriggerFunction"() RETURNS TRIGGER AS $$
BEGIN
    NEW.tsv := to_tsvector(NEW."Name");
    RETURN NEW;
END
$$ LANGUAGE plpgsql;

CREATE TRIGGER "FilesTsTrigger"
    BEFORE INSERT OR UPDATE ON "Files"
    FOR EACH ROW EXECUTE PROCEDURE "FilesTsTriggerFunction"();

-- Search Files by FTS --

CREATE OR REPLACE FUNCTION "SearchFiles"(varchar, integer DEFAULT NULL)
RETURNS SETOF "Files" AS $$
BEGIN
    RETURN QUERY SELECT * FROM "Files" WHERE plainto_tsquery($1) @@ tsv LIMIT $2;
    RETURN;
 END
$$ LANGUAGE plpgsql;

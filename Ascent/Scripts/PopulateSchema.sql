-- Create indexes for prefix search of persons
CREATE INDEX ON "Persons" ("CampusId" varchar_pattern_ops);
CREATE INDEX ON "Persons" (lower("FirstName") varchar_pattern_ops);
CREATE INDEX ON "Persons" (lower("LastName") varchar_pattern_ops);
CREATE INDEX ON "Persons" (lower("FirstName" || ' ' || "LastName") varchar_pattern_ops);
CREATE INDEX ON "Persons" (lower("LastName" || ', ' || "FirstName") varchar_pattern_ops);
CREATE INDEX ON "Persons" (lower("SchoolEmail") varchar_pattern_ops);
CREATE INDEX ON "Persons" (lower("PersonalEmail") varchar_pattern_ops);

-- The first argument is the pattern, e.g. 'John%', and the second argument is the max number of results.
CREATE OR REPLACE FUNCTION "SearchPersons"(varchar, integer DEFAULT NULL)
RETURNS SETOF "Persons" AS $$
    SELECT * FROM "Persons" WHERE NOT "IsDeleted"
        AND ("CampusId" LIKE $1
        OR lower("FirstName") LIKE $1
        OR lower("LastName") LIKE $1
        OR lower("FirstName" || ' ' || "LastName") LIKE $1
        OR lower("LastName" || ', ' || "FirstName") LIKE $1
        OR lower("SchoolEmail") LIKE $1
        OR lower("PersonalEmail") LIKE $1)
        ORDER BY "FirstName", "LastName" asc
        LIMIT $2;
$$ LANGUAGE sql;

INSERT INTO "Groups" ("Name", "Description", "EmailPreference", "IsVirtual", "MemberCount") VALUES ('BS Alumni',
  'This is a virtual group that includes everyone who has a valid BG Term and a personal (i.e. non-CSULA) email.',
  'Personal', true, 0);

INSERT INTO "Groups" ("Name", "Description", "EmailPreference", "IsVirtual", "MemberCount") VALUES ('MS Alumni',
  'This is a virtual group that includes everyone who has a valid GG Term and a personal (i.e. non-CSULA) email.',
  'Personal', true, 0);

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
DECLARE
    l_query text;
BEGIN
    l_query := plainto_tsquery($1)::text;
    IF l_query <> '' THEN
        l_query := l_query || ':*';
    END IF;
    RETURN QUERY SELECT * FROM "Pages" WHERE NOT "IsDeleted" AND "IsRegular" AND l_query::tsquery @@ tsv LIMIT $2;
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

-- FTS on Sections

ALTER TABLE "Sections" ADD COLUMN tsv tsvector;

CREATE INDEX "SectionsTsIndex" ON "Sections" USING GIN(tsv);

CREATE OR REPLACE FUNCTION "SectionsTsTriggerFunction"() RETURNS TRIGGER AS $$
DECLARE
    l_instructor    "Persons"%rowtype;
    l_course        "Courses"%rowtype;
BEGIN
    SELECT * INTO l_course FROM "Courses" WHERE "Id" = NEW."CourseId";
    SELECT * INTO l_instructor FROM "Persons" WHERE "Id" = NEW."InstructorId";
    NEW.tsv := setweight(to_tsvector(l_course."Subject"), 'A') ||
               setweight(to_tsvector(l_course."Number"), 'A') ||
               setweight(to_tsvector(l_course."Title"), 'B') ||
               setweight(to_tsvector(l_instructor."FirstName"), 'A') ||
               setweight(to_tsvector(l_instructor."LastName"), 'A');
    RETURN NEW;
END
$$ LANGUAGE plpgsql;

CREATE TRIGGER "SectionsTsTrigger"
    BEFORE INSERT OR UPDATE ON "Sections"
    FOR EACH ROW EXECUTE PROCEDURE "SectionsTsTriggerFunction"();

CREATE OR REPLACE FUNCTION "SearchSections"(varchar, integer DEFAULT NULL)
RETURNS SETOF "Sections" AS $$
BEGIN
    RETURN QUERY SELECT * FROM "Sections" WHERE plainto_tsquery($1) @@ tsv LIMIT $2;
    RETURN;
 END
$$ LANGUAGE plpgsql;

-- FTS on Projects

ALTER TABLE "Projects" ADD COLUMN tsv tsvector;

CREATE INDEX "ProjectsTsIndex" ON "Projects" USING GIN(tsv);

CREATE OR REPLACE FUNCTION "ProjectsTsTriggerFunction"() RETURNS TRIGGER AS $$
BEGIN
    NEW.tsv := setweight(to_tsvector(NEW."Title"), 'A') ||
               setweight(to_tsvector(coalesce(NEW."Sponsor",'')), 'A') ||
               setweight(to_tsvector(coalesce(NEW."Description",'')), 'D');
    RETURN NEW;
END
$$ LANGUAGE plpgsql;

CREATE TRIGGER "ProjectsTsTrigger"
    BEFORE INSERT OR UPDATE ON "Projects"
    FOR EACH ROW EXECUTE PROCEDURE "ProjectsTsTriggerFunction"();

CREATE OR REPLACE FUNCTION "SearchProjects"(varchar, integer DEFAULT NULL)
RETURNS SETOF "Projects" AS $$
BEGIN
    RETURN QUERY SELECT * FROM "Projects" WHERE plainto_tsquery($1) @@ tsv LIMIT $2;
    RETURN;
 END
$$ LANGUAGE plpgsql;

CREATE VIEW "RubricDataByPerson" AS
    SELECT d."Year", d."Term_Code" as "TermCode", d."CourseId", d."AssessmentType", d."EvaluateeId", d."RubricId", d."CriterionId",
        avg(r."Value") as "AvgRatingValue"
    FROM "RubricData" d INNER JOIN "RubricRatings" r on d."RatingId" = r."Id"
    GROUP BY d."Year", d."Term_Code", d."CourseId", d."AssessmentType", d."EvaluateeId", d."RubricId", d."CriterionId";

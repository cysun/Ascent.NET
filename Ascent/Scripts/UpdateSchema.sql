ALTER TABLE "Sections" DROP CONSTRAINT "FK_Sections_Persons_InstructorId";
ALTER TABLE "Sections" ALTER COLUMN "InstructorId" SET NOT NULL;
ALTER TABLE "Sections" ADD CONSTRAINT "FK_Sections_Persons_InstructorId"
    FOREIGN KEY ("InstructorId") REFERENCES "Persons" ("Id") ON DELETE CASCADE;

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

UPDATE "Sections" SET tsv = null;

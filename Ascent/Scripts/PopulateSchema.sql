-- Create indexes for prefix search of persons
CREATE INDEX ON "Persons" ("CampusId" varchar_pattern_ops);
CREATE INDEX ON "Persons" (lower("FirstName") varchar_pattern_ops);
CREATE INDEX ON "Persons" (lower("LastName") varchar_pattern_ops);
CREATE INDEX ON "Persons" (lower("FirstName" || ' ' || "LastName") varchar_pattern_ops);
CREATE INDEX ON "Persons" (lower("ScreenName") varchar_pattern_ops);
CREATE INDEX ON "Persons" (lower("SchoolEmail") varchar_pattern_ops);
CREATE INDEX ON "Persons" (lower("PersonalEmail") varchar_pattern_ops);

-- The first argument is the pattern, e.g. 'John%', and the second argument is the max number of results.
CREATE OR REPLACE FUNCTION search_persons_by_pattern(varchar, integer default null)
RETURNS SETOF "Persons" AS $$
    SELECT * FROM "Persons" WHERE "CampusId" LIKE $1
        OR lower("FirstName") LIKE $1
        OR lower("LastName") LIKE $1
        OR lower("FirstName" || ' ' || "LastName") LIKE $1
        OR lower("ScreenName") LIKE $1
        OR lower("SchoolEmail") LIKE $1
        OR lower("PersonalEmail") LIKE $1
        ORDER BY "FirstName" asc
        LIMIT $2;
$$ LANGUAGE sql;

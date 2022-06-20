-- Import CSNS2 data from csns2 schema

-- skip localhost and enabled = false
INSERT INTO "Persons" ("Id", "CampusId", "FirstName", "MiddleName", "LastName", "ScreenName", "SchoolEmail", "PersonalEmail")
SELECT id, cin, first_name, middle_name, last_name, username,
    CASE 
        WHEN primary_email IS NOT NULL AND lower(primary_email) LIKE '%calstatela%' THEN primary_email
        WHEN secondary_email IS NOT NULL AND lower(secondary_email) LIKE '%calstatela%' THEN secondary_email
    END,
    CASE 
        WHEN primary_email IS NOT NULL AND lower(primary_email) NOT LIKE '%calstatela%'
            AND lower(primary_email) NOT LIKE '%localhost%' THEN primary_email
        WHEN secondary_email IS NOT NULL AND lower(secondary_email) NOT LIKE '%calstatela%'
            AND lower(primary_email) NOT LIKE '%localhost' THEN secondary_email
    END
FROM csns2.users where enabled = 't' and char_length(cin) >= 4;

UPDATE "Persons" p SET
    "BgTerm_Code" = (SELECT term FROM csns2.academic_standings a WHERE
        a.student_id = p."Id" AND a.standing_id = 23009 AND a.department_id = 200),
    "MgTerm_Code" = (SELECT term FROM csns2.academic_standings a WHERE
        a.student_id = p."Id" AND a.standing_id = 23019 AND a.department_id = 200);


INSERT INTO "Courses" ("Id", "Subject", "Number", "Title", "MinUnits", "MaxUnits", "CatalogDescription", "IsObsolete")
SELECT id, substring(code from 1 for 2), substring(code from 3), name, units, units, catalog_description,
    obsolete OR unit_factor < 1
FROM csns2.courses WHERE department_id = 200;

DELETE FROM "Courses" WHERE "Number" IN ('2000P', '2000S', '2000T', '600F', '600S', '9000');

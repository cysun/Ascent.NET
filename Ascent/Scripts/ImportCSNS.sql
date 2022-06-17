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

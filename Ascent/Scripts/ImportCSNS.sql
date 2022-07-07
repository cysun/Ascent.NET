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

INSERT INTO "Grades" ("Symbol", "Value", "Description")
SELECT symbol, value, description FROM csns2.grades;

INSERT INTO "Sections" ("Id", "Term_Code", "CourseId", "InstructorId")
SELECT s.id, s.term, s.course_id,
    CASE
        WHEN i.instructor_id = 6631409 THEN 7149406
        ELSE i.instructor_id
    END
FROM csns2.sections s INNER JOIN csns2.section_instructors i ON s.id = i.section_id
WHERE s.deleted = 'f' AND s.course_id IS NOT NULL AND s.course_id IN (SELECT "Id" FROM "Courses") AND i.instructor_order = 0;

INSERT INTO "Enrollments" ("Id", "SectionId", "StudentId", "GradeSymbol")
SELECT e.id, e.section_id, e.student_id, g.symbol
FROM csns2.enrollments e INNER JOIN csns2.grades g ON e.grade_id = g.id
WHERE e.section_id IN (SELECT "Id" FROM "Sections") AND e.student_id IN (SELECT "Id" FROM "Persons");

DELETE FROM "Sections" s WHERE NOT EXISTS (SELECT * FROM "Enrollments" WHERE "SectionId" = s."Id");

INSERT INTO "MftScores" ("Id", "Date", "StudentId", "FirstName", "LastName", "Score")
SELECT m.id, m.date, u.cin, u.first_name, u.last_name, m.value
FROM csns2.mft_scores m INNER JOIN csns2.users u ON m.user_id = u.id;

INSERT INTO "MftIndicators" ("Id", "Date", "NumOfStudents", "Scores")
SELECt id, date(date), num_of_students, array[ai1, ai2, ai3] FROM csns2.mft_indicators;

INSERT INTO "MftDistributionTypes" ("Id", "Alias", "Name", "Min", "Max", "ValueLabel")
SELECT id, alias, name, min, max, value_label FROM csns2.mft_distribution_types;

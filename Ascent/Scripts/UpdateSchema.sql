UPDATE "Files" f SET "IsPublic" = true WHERE EXISTS (SELECT * FROM "ProjectItems"
    WHERE "FileId" = f."Id" AND "IsPrivate" = false);

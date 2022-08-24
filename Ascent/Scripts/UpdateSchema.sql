CREATE TABLE "Messages" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Author_Id" character varying(255) NOT NULL,
    "Author_FirstName" character varying(255) NOT NULL,
    "Author_LastName" character varying(255) NOT NULL,
    "Author_Email" character varying(255) NOT NULL,
    "Recipient" character varying(255) NOT NULL,
    "Subject" character varying(255) NOT NULL,
    "Content" text NOT NULL,
    "UseBcc" boolean NOT NULL,
    "TimeSent" timestamp with time zone NOT NULL,
    "IsFailed" boolean NOT NULL,
    CONSTRAINT "PK_Messages" PRIMARY KEY ("Id")
);

ALTER TABLE "Files" ALTER COLUMN "IsRegular" DROP DEFAULT;
ALTER TABLE "Groups" ALTER COLUMN "MemberCount" DROP DEFAULT;
ALTER TABLE "Pages" ALTER COLUMN "IsPinned" DROP DEFAULT;
ALTER TABLE "Pages" ALTER COLUMN "IsRegular" DROP DEFAULT;
ALTER TABLE "Persons" ALTER COLUMN "IsDeleted" DROP DEFAULT;

DELETE FROM "__EFMigrationsHistory";
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES ('20220824165043_InitialSchema', '6.0.8');

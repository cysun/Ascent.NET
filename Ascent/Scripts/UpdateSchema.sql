ALTER TABLE "Courses" ADD COLUMN "IsRequired" boolean NOT NULL DEFAULT false;
ALTER TABLE "Courses" ADD COLUMN "IsElective" boolean NOT NULL DEFAULT false;
ALTER TABLE "Courses" ADD COLUMN "IsService" boolean NOT NULL DEFAULT false;

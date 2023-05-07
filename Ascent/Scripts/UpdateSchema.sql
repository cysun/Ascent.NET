ALTER TABLE "Persons" ADD COLUMN "CanvasId" integer NULL;
CREATE UNIQUE INDEX "IX_Persons_CanvasId" ON "Persons" ("CanvasId");

ALTER TABLE "RubricData" ADD COLUMN "Comments" text NULL;

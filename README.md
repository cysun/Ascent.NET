# Ascent (ASsessment CENTer)

Ascent is a web-based system designed to facilitate program assessment, especially program assessment
for the purpose of accreditation by organizations like [ABET](https://www.abet.org/).

## Background

I got involved in program assessment and accreditation when I joined the Computer Science Department
at Cal State LA in 2004. Since then, our undergraduate program went through three ABET reviews and
received the full 6-year accreditation every time.

From the very beginning, I tried to leverage my computer science knowledge to build something that
makes the assessment process more efficient and sustainable. In 2006, we created a static website
to host various assessment documents. In 2012, we digitized course journals and saved many trees.
By 2018, we had [CSNS](https://github.com/cysun/csns2), which was our own learning management
system (LMS) with built-in program assessment functions.

When the pandemic hit, the university required that all classes use Canvas as the learning management
system. Because CS faculty stopped using CSNS for their classes, the assessment functions on CSNS
became much less effective. For example, a course journal can no longer be created easily because
the course is no longer hosted on CSNS. This prompted me to build Ascent, which focuses on program
assessment functions, but can also interact with an LMS like Canvas as long as the LMS provides a
web API.

## Overview

Ascent manages a number of resources that are directly or indirectly related to program assessment:

- Basic information of students and faculty, including first name, last name, CIN, email, and
  term(s) of graduation.
- *Groups*, e.g. undergraduate alumni or Industry Advisory Board (IAB). This allows us to contact
  our constituents easily via email.
- *Courses* and *Sections* - allow us to build course journals, keep track of when and where certain
  assessment takes place, and so on.
- *Senior Design Projects* - our senior design program was considered a strength of our program by
  ABET reviewers in 2018.
- *Pages* - like Pages in Canvas where a user can create a web page directly in browser using a Rich
  Text Editor (a.k.a. WYSIWYG Editor). Pages in Ascent support auto save and revision history.
- *Files* - support folders and file versioning.

The features that are intended specifically for program assessment include the following:

- *Rubrics*, including rubrics management, importing assessment data from file or Canvas, and
  automatic table and chart generation.
- *Surveys*, including creating and taking surveys, and automatic response summary by academic
  year. Student outcome surveys also support automatic table and chart generation by outcome.
- *Course Journals*
- *Major Field Test (MFT)* - import MFT results and national distribution data, automatically
  compute individual and institution percentiles, and generate comparison tables and charts over
  selected years.

Ascent can interact with Canvas via Canvas Web API. The functions currently implemented are:

- Import rubric assessments from Canvas.
- Create rubrics and assignments in Canvas using Ascent course template.

## Authentication and Authorization

Ascent uses OpenID Connect (OIDC) for authentication. We currently use [Alice Identity
Service (AIS)](https://identity.cysun.org/), but any OIDC identity provider would do as
long as it provides the following claims in addition to the claims in the standard OIDC
scopes `openid`, `profile`, and `email`:

- `ascent_read` - users with this claim can *read* any data on Ascent but cannot add,
  edit, or delete any data.
- `ascent_write` - users with this claim can add, edit, and delete data on Ascent.
- `ascent_project` - this claim allows a user to manage a specific senior design project
  hosted on Ascent, so basically it's like a combination of both `ascent_read` and
  `ascent_write` but limited to only one project.

## Installation

Ascent is implemented in C# with ASP.NET Core and requires a PostgreSQL database for operation.
To run your own Ascent, you'll need to have an OIDC identity provider as described in the
previous section, then follow these steps:

1. Create an empty database.
2. Populate the database using the following two SQL scripts:
  - `Ascent/Scripts/CreateSchema.sql`
  - `Ascent/Scripts/PopulateSchema.sql`
3. Copy `Ascent/appsettings.json.sample` to `Ascent/appsettings.json`, then edit
  `Ascent/appsettings.json` to match your environment.
4. Run Ascent with `dotnet run` or configure it to run as a service.

Note that to use the Canvas functions, you'll need to ask your Canvas administrator to add a
[developer key](https://canvas.instructure.com/doc/api/file.developer_keys.html) for Ascent.

## Web Site, Screenshots and Videos

Ascent is located at [https://ascent.cysun.org/](https://ascent.cysun.org/). You can see the
features and information that are available to the public.

Some screenshots of additional Ascent features are available
[here](https://ascent.cysun.org/folder/view/1001496).

And here is a [Youtube video](https://www.youtube.com/watch?v=1G7S_4ScdIo) demonstrating the
Canvas functions, i.e. importing assessments from Canvas and "exporting" rubrics and assignments
to Canvas.

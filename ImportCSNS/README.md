# Import CSNS Data to Ascent

This project is used to import CSNS data into Ascent when the importing cannot be done easily with SQL.
The following settings in appsettings.json are required:

- `ConnectionStrings/CsnsConnection` - The connection string for the CSNS database
- `FileDirs` - CSNS file directories

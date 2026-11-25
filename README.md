# Restaurant Menu Application

## Local Development and IIS Deployment Guide

This guide covers running the Restaurant Menu ASP.NET Core MVC
application locally and publishing it to Microsoft IIS.

> **Project assumptions:** The project is named `RestaurantMenuApp`,
> targets .NET 10, uses Entity Framework Core with SQLite, and stores
> its database in `restaurant.db` unless you change the connection
> string. Adjust paths and DLL names if your project uses different
> names.

------------------------------------------------------------------------

## 1. Prerequisites

Install the following tools:

-   **.NET 10 SDK** on the development machine.
-   **Visual Studio 2026** (or a compatible editor such as Visual Studio
    Code) if you want an IDE.
-   **Git** (optional, for source control).
-   For IIS hosting: a supported Windows version with **IIS** enabled
    and the **.NET 10 ASP.NET Core Hosting Bundle** installed.

Verify the SDK:

``` powershell
dotnet --info
dotnet --list-sdks
```

Verify that the project file exists in the current directory:

``` powershell
Get-ChildItem .\RestaurantMenuApp.csproj
```

Run all `dotnet` commands below from the directory containing
`RestaurantMenuApp.csproj`, unless a command says otherwise.

------------------------------------------------------------------------

# Part A --- Run the application locally

## 2. Restore NuGet packages

``` powershell
dotnet restore
```

## 3. Install or verify the EF Core command-line tool

Check whether `dotnet-ef` is available:

``` powershell
dotnet ef --version
```

If the command is not found, install it:

``` powershell
dotnet tool install --global dotnet-ef --version 10.*
```

If it is already installed and you need to update it:

``` powershell
dotnet tool update --global dotnet-ef --version 10.*
```

Close and reopen the terminal if the global tool is not immediately
recognized.

## 4. Configure the administrator account

The application can seed an administrator when these environment
variables are present:

-   `AdminSeed__Email`
-   `AdminSeed__Password`

For the current PowerShell session, set them before starting the
application:

``` powershell
$env:AdminSeed__Email = "admin@example.com"
$env:AdminSeed__Password = "Use-A-Unique-Strong-Password!"
```

Choose your own email and a unique strong password. Do not commit real
credentials to source control or place them in a publicly accessible
configuration file.

If you prefer not to seed an administrator at startup, leave these
variables unset and use the application's configured account-creation
process, if one is provided.

## 5. Create and apply the database migration

First inspect the `Data` and `Migrations` folders. If the project does
not already contain an initial EF Core migration, create one:

``` powershell
dotnet ef migrations add InitialCreate
```

Apply migrations to the SQLite database:

``` powershell
dotnet ef database update
```

The configured connection string in `appsettings.json` is expected to
look similar to:

``` json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=restaurant.db"
  }
}
```

A relative SQLite path is resolved relative to the application's
working/content-root configuration. For local development, keep the
database in the project folder or use an explicit absolute path if you
need a different location.

**Do not run `migrations add InitialCreate` again if that migration
already exists.** Use `dotnet ef database update` to apply existing
migrations.

## 6. Run the application

For the normal development workflow:

``` powershell
dotnet run
```

For automatic rebuild/reload during development:

``` powershell
dotnet watch run
```

The terminal prints the listening URLs, commonly similar to:

``` text
https://localhost:7xxx
http://localhost:5xxx
```

Use the exact URL printed by your application. Do not assume that port
`5000` is configured.

Useful pages (adjust the host and port to match the terminal output):

-   Public home page: `/`
-   Menu: `/Menu`
-   Administrator login: `/Account/Login`
-   Admin dashboard: `/Admin`

## 7. Local troubleshooting

### `dotnet` is not recognized

Install the .NET 10 SDK, reopen PowerShell, and run:

``` powershell
dotnet --info
```

### `dotnet ef` is not recognized

Install the tool using the commands in section 3, then reopen the
terminal.

### Database or migration errors

Confirm that the connection string is correct and that the process can
write to the database directory. Check whether a migration already
exists before creating another.

### Admin creation fails because of password policy

Use a password that meets the application's ASP.NET Core Identity
policy. The policy reported by this project requires at least 10
characters and at least one non-alphanumeric character. A longer, unique
password is recommended.

### Port already in use

Use the URL and port printed by `dotnet run`, or configure a different
local URL using ASP.NET Core's supported launch settings/environment
configuration.

------------------------------------------------------------------------

# Part B --- Publish and host on IIS

## 8. Prepare the Windows IIS server

On the IIS server:

1.  Enable **Internet Information Services (IIS)** in Windows Features
    or Server Manager.
2.  Install the **.NET 10 ASP.NET Core Hosting Bundle** from the
    official .NET download page:
    https://dotnet.microsoft.com/download/dotnet/10.0
3.  Run the Hosting Bundle installer as Administrator.
4.  If IIS was installed after the Hosting Bundle, run the Hosting
    Bundle installer again and choose Repair.
5.  Restart IIS after installation.

Restart from an elevated PowerShell or Command Prompt:

``` powershell
iisreset
```

The Hosting Bundle installs the ASP.NET Core Module used by IIS to host
ASP.NET Core applications.

## 9. Verify the IIS ASP.NET Core Module

Run PowerShell as Administrator:

``` powershell
Import-Module WebAdministration
Get-WebGlobalModule | Where-Object Name -eq "AspNetCoreModuleV2"
```

You should see an entry named `AspNetCoreModuleV2`.

You can also check whether the module DLL exists:

``` powershell
Test-Path "C:\Program Files\IIS\Asp.Net Core Module\V2\aspnetcorev2.dll"
```

Expected result:

``` text
True
```

If the module is not listed, repair/reinstall the .NET 10 Hosting
Bundle, then restart IIS and check again.

## 10. Publish the application

On the development machine, from the project directory:

``` powershell
dotnet restore
dotnet publish .\RestaurantMenuApp.csproj -c Release -o .\publish
```

The publish directory should contain files similar to:

``` text
publish\
  web.config
  RestaurantMenuApp.dll
  appsettings.json
  appsettings.Production.json   (if included)
  wwwroot\
  ...
```

The .NET Web SDK normally generates `web.config` during publishing.
Prefer the generated file. If you maintain a custom `web.config`, make
sure the DLL name in its `arguments` attribute matches the actual
published DLL.

Copy the **contents** of the publish folder to the IIS website
directory, for example:

``` text
D:\RestaurantMenuApp\publish
```

Do not point IIS at the source-code folder. Point it at the published
output folder.

## 11. Configure the production SQLite database path

For IIS, use a database directory outside the published application
folder so that deployments do not overwrite or remove the database.

Create a data directory:

``` powershell
New-Item -ItemType Directory -Path "C:\RestaurantData" -Force
```

Set the production connection string in `appsettings.Production.json`
(create this file in the published application directory if needed):

``` json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=C:/RestaurantData/restaurant.db"
  }
}
```

**Important:** The connection-string key must match the key used by the
application's `Program.cs`. If your application uses a different key
than `DefaultConnection`, use that exact key.

Back up `restaurant.db` regularly. SQLite is a file-based database; the
IIS worker process needs write access to the database directory, not
just read access to the database file.

## 12. Configure the IIS application pool

In IIS Manager:

1.  Open **Application Pools**.
2.  Select **Add Application Pool**.
3.  Name it `RestaurantMenuAppPool`.
4.  Set **.NET CLR version** to **No Managed Code**.
5.  Set **Managed pipeline mode** to **Integrated**.
6.  Click OK.
7.  Select the pool and open **Advanced Settings**.
8.  Confirm **Enable 32-Bit Applications** is `False` for a normal
    64-bit deployment unless you intentionally published for 32-bit.
9.  Keep **Maximum Worker Processes** at `1` when using a local SQLite
    database.

## 13. Create the IIS website

In IIS Manager:

1.  Right-click **Sites** and select **Add Website**.
2.  Site name: `RestaurantMenuApp`.
3.  Physical path: `D:\RestaurantMenuApp\publish` (or the directory
    where you copied the publish output).
4.  Select `RestaurantMenuAppPool`.
5.  Configure an available HTTP port and host name.
6.  Click OK.

For production internet access, configure a DNS record and an HTTPS
binding with a valid TLS certificate. Avoid exposing an administrator
login over plain HTTP.

## 14. Grant folder permissions

The application pool identity needs permission to read and execute the
published application, and modify the SQLite database and upload
directories.

For the example application pool, the Windows identity is:

``` text
IIS AppPool\RestaurantMenuAppPool
```

Grant **Read & execute** to the publish directory. Grant **Modify** only
to writable directories, such as the database directory and the
image-upload directory.

Example PowerShell commands (run elevated):

``` powershell
icacls "D:\RestaurantMenuApp\publish" /grant "IIS AppPool\RestaurantMenuAppPool:(OI)(CI)(RX)" /T
icacls "C:\RestaurantData" /grant "IIS AppPool\RestaurantMenuAppPool:(OI)(CI)(M)" /T
icacls "D:\RestaurantMenuApp\publish\wwwroot\uploads" /grant "IIS AppPool\RestaurantMenuAppPool:(OI)(CI)(M)" /T
```

If the uploads directory does not exist, create it first:

``` powershell
New-Item -ItemType Directory -Path "D:\RestaurantMenuApp\publish\wwwroot\uploads" -Force
```

Do not grant `Everyone` full control. Grant access only to the
application pool identity and administrators who need it.

## 15. Configure administrator credentials for IIS

Set machine-level environment variables from an elevated PowerShell
session:

``` powershell
[Environment]::SetEnvironmentVariable(
    "AdminSeed__Email",
    "admin@example.com",
    "Machine"
)

[Environment]::SetEnvironmentVariable(
    "AdminSeed__Password",
    "Use-A-Different-Unique-Strong-Password!",
    "Machine"
)
```

Use your own credentials. Protect access to the server and do not put
production passwords in source control.

After changing machine-level environment variables, restart the
application pool or IIS so the worker process receives the new values:

``` powershell
iisreset
```

The application must be configured to seed/create the administrator
based on these variables. If an administrator already exists, changing
the seed password environment variable may not change that user's
existing password.

## 16. Apply EF Core migrations in production

Apply migrations as a controlled deployment step, before the site starts
serving traffic.

Run the EF Core migration command against the intended production
database using the application's production configuration. One common
approach is to run the migration command from a deployment environment
where the production connection string is supplied securely:

``` powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
dotnet ef database update
```

This command must run from the source project directory (with the
required SDK and EF tooling) and must use the same production connection
string as the deployed application. If the IIS server only has the .NET
Hosting Bundle and not the SDK, run the migration from a separate
deployment machine or use a reviewed migration bundle.

**Do not assume that running `dotnet ef database update` from an
arbitrary folder updates the IIS database.** Verify the environment and
connection string before applying migrations. Back up the database
first.

If your current `Program.cs` automatically calls `Database.Migrate()` at
application startup, review that behavior before production deployment.
For controlled production deployments, prefer applying reviewed
migrations as a deployment step rather than allowing every application
startup to change the schema.

## 17. Verify the IIS deployment

1.  Confirm `web.config` and `RestaurantMenuApp.dll` are in the IIS
    physical path.
2.  Confirm the application pool is started.
3.  Browse to the configured binding, for example `http://localhost` on
    the IIS server.
4.  Check the public menu and administrator login.
5.  Add a test menu item and upload a test image to verify write
    permissions.
6.  Confirm the database file is created at the intended production
    path.

Use the actual site binding/port configured in IIS.
`http://localhost:5000` is not automatically the IIS address; it is only
correct if you explicitly configured that binding.

## 18. IIS troubleshooting

### HTTP Error 500.19 --- `0x8007000d`

Common causes include malformed `web.config` XML or an unrecognized
configuration section/module.

-   Validate the XML and confirm that `web.config` is in the IIS
    physical root.
-   Confirm `AspNetCoreModuleV2` is installed and registered.
-   Repair/reinstall the correct .NET 10 Hosting Bundle if the module is
    missing.
-   Restart IIS after installing the Hosting Bundle.

### HTTP Error 500.30 --- ASP.NET Core app failed to start

The IIS module may be installed, but the application process is failing
during startup.

-   Check Windows Event Viewer → Windows Logs → Application.
-   Temporarily enable stdout logging in `web.config` as described
    below.
-   Check application configuration, environment variables, database
    path, migrations, and file permissions.

### Temporarily enable stdout logging

In `web.config`, set:

``` xml
stdoutLogEnabled="true"
stdoutLogFile=".\logs\stdout"
```

Create the logs directory and grant the application pool identity Modify
permission:

``` powershell
New-Item -ItemType Directory -Path "D:\RestaurantMenuApp\publish\logs" -Force
icacls "D:\RestaurantMenuApp\publish\logs" /grant "IIS AppPool\RestaurantMenuAppPool:(OI)(CI)(M)" /T
```

Reproduce the error and inspect the generated stdout log. **Turn stdout
logging off again after troubleshooting** because logs can grow and may
contain sensitive diagnostic information.

### SQLite "unable to open database file" or write errors

-   Confirm the connection string points to the intended absolute path.
-   Confirm the directory exists.
-   Grant Modify permission on the database directory to the application
    pool identity.
-   Check that the database is not locked by another process and that
    the disk has free space.

### Image upload fails

-   Confirm `wwwroot\uploads` exists.
-   Grant Modify permission to the application pool identity on that
    directory.
-   Check any application-level upload size and file-type validation.

------------------------------------------------------------------------

## 19. Deployment checklist

### Local development

-   [ ] .NET 10 SDK installed.
-   [ ] NuGet packages restored.
-   [ ] EF Core tool available.
-   [ ] Initial migration exists.
-   [ ] Database migrations applied.
-   [ ] Admin seed credentials configured securely.
-   [ ] `dotnet run` starts successfully.

### IIS

-   [ ] IIS role/service installed.
-   [ ] .NET 10 Hosting Bundle installed.
-   [ ] `AspNetCoreModuleV2` registered.
-   [ ] Application published in Release mode.
-   [ ] IIS physical path points to the publish directory.
-   [ ] Application pool uses **No Managed Code**.
-   [ ] Publish directory has read/execute permissions.
-   [ ] Database directory has Modify permissions.
-   [ ] Upload directory has Modify permissions.
-   [ ] Production connection string points to the intended database.
-   [ ] Production migrations applied and database backed up.
-   [ ] Admin credentials configured securely.
-   [ ] HTTPS binding and certificate configured for public production
    access.
-   [ ] Stdout logging disabled after troubleshooting.

------------------------------------------------------------------------

## 20. Official references

-   .NET 10 downloads: https://dotnet.microsoft.com/download/dotnet/10.0
-   Host ASP.NET Core on IIS:
    https://learn.microsoft.com/aspnet/core/host-and-deploy/iis/
-   EF Core migrations:
    https://learn.microsoft.com/ef/core/managing-schemas/migrations/
-   IIS documentation: https://learn.microsoft.com/iis/

------------------------------------------------------------------------

**Before going live:** Test the exact published build on the IIS server,
back up the SQLite database, use HTTPS, and verify that only the
intended application-pool identity can modify the database and upload
folders.

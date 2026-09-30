# IIS setup

This POC has two ASP.NET Core applications:

- `DynamicRuleEngine.Api` - API endpoint
- `DynamicRuleEngine.Web` - MudBlazor UI

## API

Publish the API to a folder such as `C:\IIS\DynamicRuleEngine\Api`.
Create an IIS site/application that binds to the port configured for the API. The sample uses `5101` for HTTP.

Test:

`http://localhost:5101/api/framework/workflows`

## Web

Publish the Web project to a folder such as `C:\IIS\DynamicRuleEngine\Web`.
Create a separate IIS site/application for it.

The Web project reads `ApiBaseUrl` from `appsettings.json`. If the API binding changes, update the URL there.

## Database

The supplied development configuration points to:

`(localdb)\MSSQLLocalDB`

with database `DynamicRuleEngineDb`. LocalDB normally belongs to a Windows user profile. For IIS production-style hosting, SQL Server Express/full SQL Server is preferable. If LocalDB is used for the POC, run the application pool under a Windows identity that can access the LocalDB instance.

## Application pool

For ASP.NET Core on IIS, use an application pool with:

- .NET CLR Version: `No Managed Code`
- Managed pipeline: `Integrated`

Install the ASP.NET Core Hosting Bundle that matches the application's target runtime before deploying.

## Clean Architecture relationship

`DynamicRuleEngine.Infrastructure` references the source `RulesEngine` class library at `Libraries/RulesEngine`. The Application and Domain projects do not reference RulesEngine.

`ReportService` depends on `IRuleEngineService`, so the report page never creates `RulesEngine.RulesEngine` directly.

## Framework workflows

The sample database contains three workflows:

- `BusinessFramework`
- `OperationalFramework`
- `FinancialFramework`

If Web and API are installed as IIS applications under one site, set the Web `ApiBaseUrl` to the API IIS application path, for example `http://localhost/DynamicRuleEngineApi/`. Use the actual IIS application alias configured on your server.

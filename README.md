# DynamicRuleEngine - Corporate Framework (3 Workflows)

This version contains:

- ASP.NET Core 10 API
- MudBlazor Web UI
- EF Core + SQL Server
- Repository + Service pattern
- Microsoft RulesEngine source library included under `Libraries/RulesEngine`
- No stored procedures
- Business, Operational and Financial Framework workflows
- Parameter metadata and score definitions synchronized from Workflow JSON
- Runtime framework calculations executed from published WorkflowJson through RulesEngine

## Important calculation rule

The Framework UI does **not** calculate weighted averages in C#.

The published WorkflowJson contains:

- `CalculateTotalWeight`
- `CalculateWeightedAverage`

The calculation formula is executed by the RulesEngine library.

For example, the published JSON can contain:

```text
(input1.Score_PpaAvailability * 15 + input1.Score_AssetDiversification * 25 + input1.Score_CounterpartyQuality * 30 + input1.Score_PPAMix * 30) / 100
```

Changing the formula in Workflow JSON changes the runtime calculation without changing the MudBlazor page.

## Database

Run:

`Database/01_Create_DynamicRuleEngine_Framework.sql`

The script creates and seeds:

- WorkflowMaster
- ParameterDefinition
- ScoreDefinition
- RuleDefinition
- WorkflowJson

## JSON synchronization

Workflow JSON -> Save JSON & Sync Framework synchronizes parameter metadata, score definitions and rules into SQL.

Publish From DB regenerates the framework weight formulas from the current database parameter weights and stores them in WorkflowJson.

## IIS

Both API and Web can be published and hosted under IIS. Configure the Web `ApiBaseUrl` to the IIS API application path.

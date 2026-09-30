# Dynamic Rule Engine - Corporate Framework POC

Run `01_Create_DynamicRuleEngine_Framework.sql` in SSMS.

The script creates one database and three workflows:

- Business Framework
- Operational Framework
- Financial Framework

Tables:

- WorkflowMaster
- ParameterDefinition
- ScoreDefinition
- RuleDefinition
- WorkflowJson

There is intentionally **no FrameworkParameters table**.

## JSON is the synchronization point

Each workflow has one `WorkflowJson` row. The JSON contains both:

1. Microsoft RulesEngine workflow rules
2. Framework parameter metadata

The parameter metadata contains:

- parameterCode
- displayName
- dataType
- weight
- whyItMatters
- howToMeasure
- scoreDefinitions

When JSON is edited in the **Workflow JSON** page and `Save JSON & Sync Framework` is clicked:

1. JSON is validated as a RulesEngine workflow.
2. Parameters are synchronized to `ParameterDefinition`.
3. Score definitions are synchronized to `ScoreDefinition`.
4. Rules are synchronized to `RuleDefinition`.
5. The exact JSON is saved in `WorkflowJson`.
6. Workflow version is incremented when the JSON changes.
7. RulesEngine cache is invalidated.

The **Framework Evaluation** page reads the synchronized SQL configuration, so JSON parameter changes appear there without changing the Razor page.

## Publish From DB

Parameter and rule configuration can also be maintained through the normal configuration pages. `Publish From DB` builds a new RulesEngine JSON from the current SQL definitions and stores it in `WorkflowJson`.

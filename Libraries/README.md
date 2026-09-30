# Referenced RulesEngine class library

`Libraries/RulesEngine` contains the `RulesEngine` source from the uploaded RulesEngine repository.

The Dynamic Rule Engine application references this project from `DynamicRuleEngine.Infrastructure` instead of consuming the RulesEngine NuGet package.

This keeps the external rule engine implementation isolated from Domain and Application.

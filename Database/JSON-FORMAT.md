# Workflow JSON format

The JSON remains a normal Microsoft RulesEngine workflow array. The additional `parameters` property is metadata for the application's framework UI. RulesEngine ignores that extra property while reading the workflow.

Example:

```json
[
  {
    "workflowName": "BusinessFramework",
    "workflowDisplayName": "Business Framework",
    "ruleExpressionType": "LambdaExpression",
    "parameters": [
      {
        "parameterCode": "PpaAvailability",
        "displayName": "PPA Availability",
        "dataType": "Decimal",
        "isRequired": false,
        "sortOrder": 1,
        "isActive": true,
        "weight": 15,
        "whyItMatters": "Why the parameter matters.",
        "howToMeasure": "How the parameter is measured.",
        "scoreDefinitions": [
          { "score": 1, "description": "Below target" },
          { "score": 2, "description": "Low" },
          { "score": 3, "description": "Average" },
          { "score": 4, "description": "Good" },
          { "score": 5, "description": "Excellent" }
        ]
      }
    ],
    "rules": [
      {
        "ruleName": "ValidatePpaAvailability",
        "ruleDisplayName": "PPA availability must be between 0 and 100",
        "ruleType": "Validation",
        "errorMessage": "PPA availability must be between 0 and 100.",
        "enabled": true,
        "ruleExpressionType": "LambdaExpression",
        "expression": "input1.PpaAvailability >= 0 && input1.PpaAvailability <= 100",
        "successEvent": null,
        "sortOrder": 1,
        "actions": null
      }
    ]
  }
]
```

## Framework calculations

Framework score calculations are not performed by the MudBlazor page or by a hardcoded C# formula.
When `Publish From DB` is used, the application writes these calculation rules into the published `WorkflowJson`:

- `CalculateTotalWeight`
- `CalculateWeightedAverage`

`CalculateWeightedAverage` uses `Score_<ParameterCode>` values and the weights stored in the JSON parameter definitions. The Framework page sends the selected scores to the API, and the API executes the published JSON with the Microsoft RulesEngine library.

If Workflow JSON is edited directly, the calculation expression in that JSON is the runtime formula. Saving JSON synchronizes parameters, score definitions and rules to SQL, and the Framework page reads the synchronized parameter metadata from SQL.

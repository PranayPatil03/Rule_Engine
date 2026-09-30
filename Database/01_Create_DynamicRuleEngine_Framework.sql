USE master;
GO
IF DB_ID(N'DynamicRuleEngineDb') IS NOT NULL
BEGIN
    ALTER DATABASE DynamicRuleEngineDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE DynamicRuleEngineDb;
END
GO
CREATE DATABASE DynamicRuleEngineDb;
GO
USE DynamicRuleEngineDb;
GO
CREATE TABLE dbo.WorkflowMaster (
 Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WorkflowMaster PRIMARY KEY,
 WorkflowCode NVARCHAR(100) NOT NULL,
 WorkflowName NVARCHAR(200) NOT NULL,
 IsActive BIT NOT NULL CONSTRAINT DF_WorkflowMaster_IsActive DEFAULT (1),
 CONSTRAINT UQ_WorkflowMaster_WorkflowCode UNIQUE (WorkflowCode)
);
GO

CREATE TABLE dbo.ParameterDefinition (
 Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ParameterDefinition PRIMARY KEY,
 WorkflowId INT NOT NULL,
 ParameterCode NVARCHAR(100) NOT NULL,
 DisplayName NVARCHAR(200) NOT NULL,
 DataType NVARCHAR(20) NOT NULL,
 IsRequired BIT NOT NULL CONSTRAINT DF_ParameterDefinition_IsRequired DEFAULT (0),
 SortOrder INT NOT NULL CONSTRAINT DF_ParameterDefinition_SortOrder DEFAULT (0),
 IsActive BIT NOT NULL CONSTRAINT DF_ParameterDefinition_IsActive DEFAULT (1),
 Weight DECIMAL(18,2) NOT NULL CONSTRAINT DF_ParameterDefinition_Weight DEFAULT (0),
 WhyItMatters NVARCHAR(2000) NULL,
 HowToMeasure NVARCHAR(2000) NULL,
 CONSTRAINT FK_ParameterDefinition_Workflow FOREIGN KEY (WorkflowId) REFERENCES dbo.WorkflowMaster(Id),
 CONSTRAINT UQ_ParameterDefinition_Code UNIQUE (WorkflowId, ParameterCode)
);
GO

CREATE TABLE dbo.ScoreDefinition (
 Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ScoreDefinition PRIMARY KEY,
 ParameterDefinitionId INT NOT NULL,
 Score INT NOT NULL,
 Description NVARCHAR(2000) NOT NULL,
 CONSTRAINT FK_ScoreDefinition_ParameterDefinition FOREIGN KEY (ParameterDefinitionId) REFERENCES dbo.ParameterDefinition(Id) ON DELETE CASCADE,
 CONSTRAINT CK_ScoreDefinition_Score CHECK (Score BETWEEN 1 AND 5),
 CONSTRAINT UQ_ScoreDefinition UNIQUE (ParameterDefinitionId, Score)
);
GO

CREATE TABLE dbo.RuleDefinition (
 Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RuleDefinition PRIMARY KEY,
 WorkflowId INT NOT NULL,
 RuleCode NVARCHAR(100) NOT NULL,
 RuleName NVARCHAR(200) NOT NULL,
 RuleType NVARCHAR(20) NOT NULL,
 ConditionExpression NVARCHAR(MAX) NOT NULL,
 CalculationExpression NVARCHAR(MAX) NULL,
 ErrorMessage NVARCHAR(1000) NULL,
 SuccessEvent NVARCHAR(200) NULL,
 Enabled BIT NOT NULL CONSTRAINT DF_RuleDefinition_Enabled DEFAULT (1),
 SortOrder INT NOT NULL CONSTRAINT DF_RuleDefinition_SortOrder DEFAULT (0),
 CONSTRAINT FK_RuleDefinition_Workflow FOREIGN KEY (WorkflowId) REFERENCES dbo.WorkflowMaster(Id),
 CONSTRAINT UQ_RuleDefinition_Code UNIQUE (WorkflowId, RuleCode),
 CONSTRAINT CK_RuleDefinition_Type CHECK (RuleType IN ('Validation','Calculation'))
);
GO

CREATE TABLE dbo.WorkflowJson (
 Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WorkflowJson PRIMARY KEY,
 WorkflowId INT NOT NULL,
 Version INT NOT NULL,
 SourceHash CHAR(64) NOT NULL,
 JsonContent NVARCHAR(MAX) NOT NULL,
 GeneratedAtUtc DATETIME2(3) NOT NULL,
 CONSTRAINT FK_WorkflowJson_Workflow FOREIGN KEY (WorkflowId) REFERENCES dbo.WorkflowMaster(Id),
 CONSTRAINT UQ_WorkflowJson_Workflow UNIQUE (WorkflowId)
);
GO

CREATE INDEX IX_ParameterDefinition_WorkflowId_SortOrder ON dbo.ParameterDefinition(WorkflowId, SortOrder);
CREATE INDEX IX_RuleDefinition_WorkflowId_SortOrder ON dbo.RuleDefinition(WorkflowId, SortOrder);
GO
INSERT INTO dbo.WorkflowMaster (WorkflowCode, WorkflowName) VALUES (N'BusinessFramework', N'Business Framework');
DECLARE @WorkflowId INT = SCOPE_IDENTITY();
DECLARE @ParameterId INT;
INSERT INTO dbo.ParameterDefinition (WorkflowId, ParameterCode, DisplayName, DataType, IsRequired, SortOrder, IsActive, Weight, WhyItMatters, HowToMeasure) VALUES (@WorkflowId, N'PpaAvailability', N'PPA Availability', N'Decimal', 0, 1, 1, 15, N'PPA availability indicates the portion of expected contracted availability that can be relied upon.', N'Use the contracted PPA availability percentage for the assessment period');
SET @ParameterId = SCOPE_IDENTITY();
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 1, N'Below 60%');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 2, N'60% - 74%');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 3, N'75% - 84%');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 4, N'85% - 94%');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 5, N'95% - 100%');
INSERT INTO dbo.ParameterDefinition (WorkflowId, ParameterCode, DisplayName, DataType, IsRequired, SortOrder, IsActive, Weight, WhyItMatters, HowToMeasure) VALUES (@WorkflowId, N'AssetDiversification', N'Asset Diversification', N'Decimal', 0, 2, 1, 25, N'Asset diversification reduces concentration risk by spreading projects across locations.', N'Measure the geographic spread of the operating project portfolio');
SET @ParameterId = SCOPE_IDENTITY();
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 1, N'Single location');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 2, N'2 locations');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 3, N'3 locations');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 4, N'4 locations');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 5, N'5 or more locations');
INSERT INTO dbo.ParameterDefinition (WorkflowId, ParameterCode, DisplayName, DataType, IsRequired, SortOrder, IsActive, Weight, WhyItMatters, HowToMeasure) VALUES (@WorkflowId, N'CounterpartyQuality', N'Counterparty Quality', N'Integer', 0, 3, 1, 30, N'Counterparty quality indicates the strength and reliability of the offtake profile.', N'Assess the counterparty using the approved internal credit scale');
SET @ParameterId = SCOPE_IDENTITY();
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 1, N'Very weak');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 2, N'Weak');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 3, N'Average');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 4, N'Strong');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 5, N'Very strong');
INSERT INTO dbo.ParameterDefinition (WorkflowId, ParameterCode, DisplayName, DataType, IsRequired, SortOrder, IsActive, Weight, WhyItMatters, HowToMeasure) VALUES (@WorkflowId, N'PPAMix', N'PPA Mix', N'Decimal', 0, 4, 1, 30, N'PPA mix indicates the balance between contracted and competitive arrangements.', N'Measure the percentage split of the relevant PPA portfolio');
SET @ParameterId = SCOPE_IDENTITY();
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 1, N'Below 20%');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 2, N'20% - 39%');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 3, N'40% - 59%');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 4, N'60% - 79%');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 5, N'80% - 100%');
INSERT INTO dbo.RuleDefinition (WorkflowId, RuleCode, RuleName, RuleType, ConditionExpression, ErrorMessage, Enabled, SortOrder) VALUES (@WorkflowId, N'ValidatePpaAvailability', N'PPA availability must be between 0 and 100', N'Validation', N'input1.PpaAvailability >= 0 && input1.PpaAvailability <= 100', N'PPA availability must be between 0 and 100.', 1, 1);
INSERT INTO dbo.RuleDefinition (WorkflowId, RuleCode, RuleName, RuleType, ConditionExpression, ErrorMessage, Enabled, SortOrder) VALUES (@WorkflowId, N'ValidateAssetDiversification', N'Asset diversification must be between 0 and 100', N'Validation', N'input1.AssetDiversification >= 0 && input1.AssetDiversification <= 100', N'Asset diversification value must be between 0 and 100.', 1, 2);
INSERT INTO dbo.RuleDefinition (WorkflowId, RuleCode, RuleName, RuleType, ConditionExpression, ErrorMessage, Enabled, SortOrder) VALUES (@WorkflowId, N'ValidateCounterpartyQuality', N'Counterparty quality must be between 1 and 5', N'Validation', N'input1.CounterpartyQuality >= 1 && input1.CounterpartyQuality <= 5', N'Counterparty quality must be between 1 and 5.', 1, 3);
INSERT INTO dbo.RuleDefinition (WorkflowId, RuleCode, RuleName, RuleType, ConditionExpression, ErrorMessage, Enabled, SortOrder) VALUES (@WorkflowId, N'ValidatePPAMix', N'PPA mix must be between 0 and 100', N'Validation', N'input1.PPAMix >= 0 && input1.PPAMix <= 100', N'PPA mix must be between 0 and 100.', 1, 4);
IF NOT EXISTS (SELECT 1 FROM dbo.RuleDefinition WHERE WorkflowId = @WorkflowId AND RuleCode = N'CalculateTotalWeight')
    INSERT INTO dbo.RuleDefinition (WorkflowId, RuleCode, RuleName, RuleType, ConditionExpression, CalculationExpression, SuccessEvent, Enabled, SortOrder)
    VALUES (@WorkflowId, N'CalculateTotalWeight', N'Total Weight', N'Calculation', N'true', N'100', N'TotalWeightCalculated', 1, 900);
IF NOT EXISTS (SELECT 1 FROM dbo.RuleDefinition WHERE WorkflowId = @WorkflowId AND RuleCode = N'CalculateWeightedAverage')
    INSERT INTO dbo.RuleDefinition (WorkflowId, RuleCode, RuleName, RuleType, ConditionExpression, CalculationExpression, SuccessEvent, Enabled, SortOrder)
    VALUES (@WorkflowId, N'CalculateWeightedAverage', N'Weighted Average Score', N'Calculation', N'input1.FrameworkHasScores == true', N'(input1.Score_PpaAvailability * 15 + input1.Score_AssetDiversification * 25 + input1.Score_CounterpartyQuality * 30 + input1.Score_PPAMix * 30) / 100', N'WeightedAverageCalculated', 1, 901);
INSERT INTO dbo.WorkflowJson (WorkflowId, Version, SourceHash, JsonContent, GeneratedAtUtc) VALUES (@WorkflowId, 1, '934D60BD491F33AAD15F129E8A3C29D4A7B1826721F01A31D810E3141EBC68BF', N'[
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
        "whyItMatters": "PPA availability indicates the portion of expected contracted availability that can be relied upon.",
        "howToMeasure": "Use the contracted PPA availability percentage for the assessment period",
        "scoreDefinitions": [
          {
            "score": 1,
            "description": "Below 60%"
          },
          {
            "score": 2,
            "description": "60% - 74%"
          },
          {
            "score": 3,
            "description": "75% - 84%"
          },
          {
            "score": 4,
            "description": "85% - 94%"
          },
          {
            "score": 5,
            "description": "95% - 100%"
          }
        ]
      },
      {
        "parameterCode": "AssetDiversification",
        "displayName": "Asset Diversification",
        "dataType": "Decimal",
        "isRequired": false,
        "sortOrder": 2,
        "isActive": true,
        "weight": 25,
        "whyItMatters": "Asset diversification reduces concentration risk by spreading projects across locations.",
        "howToMeasure": "Measure the geographic spread of the operating project portfolio",
        "scoreDefinitions": [
          {
            "score": 1,
            "description": "Single location"
          },
          {
            "score": 2,
            "description": "2 locations"
          },
          {
            "score": 3,
            "description": "3 locations"
          },
          {
            "score": 4,
            "description": "4 locations"
          },
          {
            "score": 5,
            "description": "5 or more locations"
          }
        ]
      },
      {
        "parameterCode": "CounterpartyQuality",
        "displayName": "Counterparty Quality",
        "dataType": "Integer",
        "isRequired": false,
        "sortOrder": 3,
        "isActive": true,
        "weight": 30,
        "whyItMatters": "Counterparty quality indicates the strength and reliability of the offtake profile.",
        "howToMeasure": "Assess the counterparty using the approved internal credit scale",
        "scoreDefinitions": [
          {
            "score": 1,
            "description": "Very weak"
          },
          {
            "score": 2,
            "description": "Weak"
          },
          {
            "score": 3,
            "description": "Average"
          },
          {
            "score": 4,
            "description": "Strong"
          },
          {
            "score": 5,
            "description": "Very strong"
          }
        ]
      },
      {
        "parameterCode": "PPAMix",
        "displayName": "PPA Mix",
        "dataType": "Decimal",
        "isRequired": false,
        "sortOrder": 4,
        "isActive": true,
        "weight": 30,
        "whyItMatters": "PPA mix indicates the balance between contracted and competitive arrangements.",
        "howToMeasure": "Measure the percentage split of the relevant PPA portfolio",
        "scoreDefinitions": [
          {
            "score": 1,
            "description": "Below 20%"
          },
          {
            "score": 2,
            "description": "20% - 39%"
          },
          {
            "score": 3,
            "description": "40% - 59%"
          },
          {
            "score": 4,
            "description": "60% - 79%"
          },
          {
            "score": 5,
            "description": "80% - 100%"
          }
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
      },
      {
        "ruleName": "ValidateAssetDiversification",
        "ruleDisplayName": "Asset diversification must be between 0 and 100",
        "ruleType": "Validation",
        "errorMessage": "Asset diversification value must be between 0 and 100.",
        "enabled": true,
        "ruleExpressionType": "LambdaExpression",
        "expression": "input1.AssetDiversification >= 0 && input1.AssetDiversification <= 100",
        "successEvent": null,
        "sortOrder": 2,
        "actions": null
      },
      {
        "ruleName": "ValidateCounterpartyQuality",
        "ruleDisplayName": "Counterparty quality must be between 1 and 5",
        "ruleType": "Validation",
        "errorMessage": "Counterparty quality must be between 1 and 5.",
        "enabled": true,
        "ruleExpressionType": "LambdaExpression",
        "expression": "input1.CounterpartyQuality >= 1 && input1.CounterpartyQuality <= 5",
        "successEvent": null,
        "sortOrder": 3,
        "actions": null
      },
      {
        "ruleName": "ValidatePPAMix",
        "ruleDisplayName": "PPA mix must be between 0 and 100",
        "ruleType": "Validation",
        "errorMessage": "PPA mix must be between 0 and 100.",
        "enabled": true,
        "ruleExpressionType": "LambdaExpression",
        "expression": "input1.PPAMix >= 0 && input1.PPAMix <= 100",
        "successEvent": null,
        "sortOrder": 4,
        "actions": null
      },
      {
        "ruleName": "CalculateTotalWeight",
        "ruleDisplayName": "Total Weight",
        "ruleType": "Calculation",
        "errorMessage": null,
        "enabled": true,
        "ruleExpressionType": "LambdaExpression",
        "expression": "true",
        "successEvent": "TotalWeightCalculated",
        "sortOrder": 900,
        "actions": {
          "onSuccess": {
            "name": "OutputExpression",
            "context": {
              "expression": "100"
            }
          }
        }
      },
      {
        "ruleName": "CalculateWeightedAverage",
        "ruleDisplayName": "Weighted Average Score",
        "ruleType": "Calculation",
        "errorMessage": null,
        "enabled": true,
        "ruleExpressionType": "LambdaExpression",
        "expression": "input1.FrameworkHasScores == true",
        "successEvent": "WeightedAverageCalculated",
        "sortOrder": 901,
        "actions": {
          "onSuccess": {
            "name": "OutputExpression",
            "context": {
              "expression": "(input1.Score_PpaAvailability * 15 + input1.Score_AssetDiversification * 25 + input1.Score_CounterpartyQuality * 30 + input1.Score_PPAMix * 30) / 100"
            }
          }
        }
      }
    ]
  }
]', SYSUTCDATETIME());
GO
INSERT INTO dbo.WorkflowMaster (WorkflowCode, WorkflowName) VALUES (N'OperationalFramework', N'Operational Framework');
DECLARE @WorkflowId INT = SCOPE_IDENTITY();
DECLARE @ParameterId INT;
INSERT INTO dbo.ParameterDefinition (WorkflowId, ParameterCode, DisplayName, DataType, IsRequired, SortOrder, IsActive, Weight, WhyItMatters, HowToMeasure) VALUES (@WorkflowId, N'PlantAvailabilityFactor', N'Plant Availability Factor (PAF)', N'Decimal', 0, 1, 1, 15, N'PAF indicates the ability of the plant to remain available for generation.', N'Use the measured plant availability percentage');
SET @ParameterId = SCOPE_IDENTITY();
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 1, N'Below 60%');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 2, N'60% - 74%');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 3, N'75% - 84%');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 4, N'85% - 94%');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 5, N'95% - 100%');
INSERT INTO dbo.ParameterDefinition (WorkflowId, ParameterCode, DisplayName, DataType, IsRequired, SortOrder, IsActive, Weight, WhyItMatters, HowToMeasure) VALUES (@WorkflowId, N'GridAvailability', N'Grid Availability (GA)', N'Decimal', 0, 2, 1, 15, N'Grid availability captures the reliability of evacuation and grid access.', N'Use the measured grid availability percentage');
SET @ParameterId = SCOPE_IDENTITY();
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 1, N'Below 60%');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 2, N'60% - 74%');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 3, N'75% - 84%');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 4, N'85% - 94%');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 5, N'95% - 100%');
INSERT INTO dbo.ParameterDefinition (WorkflowId, ParameterCode, DisplayName, DataType, IsRequired, SortOrder, IsActive, Weight, WhyItMatters, HowToMeasure) VALUES (@WorkflowId, N'PlantLoadFactor', N'Plant Load Factor', N'Decimal', 0, 3, 1, 70, N'PLF reflects actual generation against the plant potential for the period.', N'Calculate generation divided by available generation capacity');
SET @ParameterId = SCOPE_IDENTITY();
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 1, N'Below 10%');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 2, N'10% - 19%');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 3, N'20% - 29%');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 4, N'30% - 39%');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 5, N'40% or above');
INSERT INTO dbo.RuleDefinition (WorkflowId, RuleCode, RuleName, RuleType, ConditionExpression, ErrorMessage, Enabled, SortOrder) VALUES (@WorkflowId, N'ValidatePAF', N'PAF must be between 0 and 100', N'Validation', N'input1.PlantAvailabilityFactor >= 0 && input1.PlantAvailabilityFactor <= 100', N'PAF must be between 0 and 100.', 1, 1);
INSERT INTO dbo.RuleDefinition (WorkflowId, RuleCode, RuleName, RuleType, ConditionExpression, ErrorMessage, Enabled, SortOrder) VALUES (@WorkflowId, N'ValidateGA', N'GA must be between 0 and 100', N'Validation', N'input1.GridAvailability >= 0 && input1.GridAvailability <= 100', N'Grid availability must be between 0 and 100.', 1, 2);
INSERT INTO dbo.RuleDefinition (WorkflowId, RuleCode, RuleName, RuleType, ConditionExpression, ErrorMessage, Enabled, SortOrder) VALUES (@WorkflowId, N'ValidatePLF', N'PLF must be between 0 and 100', N'Validation', N'input1.PlantLoadFactor >= 0 && input1.PlantLoadFactor <= 100', N'Plant load factor must be between 0 and 100.', 1, 3);
IF NOT EXISTS (SELECT 1 FROM dbo.RuleDefinition WHERE WorkflowId = @WorkflowId AND RuleCode = N'CalculateTotalWeight')
    INSERT INTO dbo.RuleDefinition (WorkflowId, RuleCode, RuleName, RuleType, ConditionExpression, CalculationExpression, SuccessEvent, Enabled, SortOrder)
    VALUES (@WorkflowId, N'CalculateTotalWeight', N'Total Weight', N'Calculation', N'true', N'100', N'TotalWeightCalculated', 1, 900);
IF NOT EXISTS (SELECT 1 FROM dbo.RuleDefinition WHERE WorkflowId = @WorkflowId AND RuleCode = N'CalculateWeightedAverage')
    INSERT INTO dbo.RuleDefinition (WorkflowId, RuleCode, RuleName, RuleType, ConditionExpression, CalculationExpression, SuccessEvent, Enabled, SortOrder)
    VALUES (@WorkflowId, N'CalculateWeightedAverage', N'Weighted Average Score', N'Calculation', N'input1.FrameworkHasScores == true', N'(input1.Score_PlantAvailabilityFactor * 15 + input1.Score_GridAvailability * 15 + input1.Score_PlantLoadFactor * 70) / 100', N'WeightedAverageCalculated', 1, 901);
INSERT INTO dbo.WorkflowJson (WorkflowId, Version, SourceHash, JsonContent, GeneratedAtUtc) VALUES (@WorkflowId, 1, 'DA295261AE60628CFC37A9AD30FDF45C4C5816265343FEAE6E319BA4F8ED38CF', N'[
  {
    "workflowName": "OperationalFramework",
    "workflowDisplayName": "Operational Framework",
    "ruleExpressionType": "LambdaExpression",
    "parameters": [
      {
        "parameterCode": "PlantAvailabilityFactor",
        "displayName": "Plant Availability Factor (PAF)",
        "dataType": "Decimal",
        "isRequired": false,
        "sortOrder": 1,
        "isActive": true,
        "weight": 15,
        "whyItMatters": "PAF indicates the ability of the plant to remain available for generation.",
        "howToMeasure": "Use the measured plant availability percentage",
        "scoreDefinitions": [
          {
            "score": 1,
            "description": "Below 60%"
          },
          {
            "score": 2,
            "description": "60% - 74%"
          },
          {
            "score": 3,
            "description": "75% - 84%"
          },
          {
            "score": 4,
            "description": "85% - 94%"
          },
          {
            "score": 5,
            "description": "95% - 100%"
          }
        ]
      },
      {
        "parameterCode": "GridAvailability",
        "displayName": "Grid Availability (GA)",
        "dataType": "Decimal",
        "isRequired": false,
        "sortOrder": 2,
        "isActive": true,
        "weight": 15,
        "whyItMatters": "Grid availability captures the reliability of evacuation and grid access.",
        "howToMeasure": "Use the measured grid availability percentage",
        "scoreDefinitions": [
          {
            "score": 1,
            "description": "Below 60%"
          },
          {
            "score": 2,
            "description": "60% - 74%"
          },
          {
            "score": 3,
            "description": "75% - 84%"
          },
          {
            "score": 4,
            "description": "85% - 94%"
          },
          {
            "score": 5,
            "description": "95% - 100%"
          }
        ]
      },
      {
        "parameterCode": "PlantLoadFactor",
        "displayName": "Plant Load Factor",
        "dataType": "Decimal",
        "isRequired": false,
        "sortOrder": 3,
        "isActive": true,
        "weight": 70,
        "whyItMatters": "PLF reflects actual generation against the plant potential for the period.",
        "howToMeasure": "Calculate generation divided by available generation capacity",
        "scoreDefinitions": [
          {
            "score": 1,
            "description": "Below 10%"
          },
          {
            "score": 2,
            "description": "10% - 19%"
          },
          {
            "score": 3,
            "description": "20% - 29%"
          },
          {
            "score": 4,
            "description": "30% - 39%"
          },
          {
            "score": 5,
            "description": "40% or above"
          }
        ]
      }
    ],
    "rules": [
      {
        "ruleName": "ValidatePAF",
        "ruleDisplayName": "PAF must be between 0 and 100",
        "ruleType": "Validation",
        "errorMessage": "PAF must be between 0 and 100.",
        "enabled": true,
        "ruleExpressionType": "LambdaExpression",
        "expression": "input1.PlantAvailabilityFactor >= 0 && input1.PlantAvailabilityFactor <= 100",
        "successEvent": null,
        "sortOrder": 1,
        "actions": null
      },
      {
        "ruleName": "ValidateGA",
        "ruleDisplayName": "GA must be between 0 and 100",
        "ruleType": "Validation",
        "errorMessage": "Grid availability must be between 0 and 100.",
        "enabled": true,
        "ruleExpressionType": "LambdaExpression",
        "expression": "input1.GridAvailability >= 0 && input1.GridAvailability <= 100",
        "successEvent": null,
        "sortOrder": 2,
        "actions": null
      },
      {
        "ruleName": "ValidatePLF",
        "ruleDisplayName": "PLF must be between 0 and 100",
        "ruleType": "Validation",
        "errorMessage": "Plant load factor must be between 0 and 100.",
        "enabled": true,
        "ruleExpressionType": "LambdaExpression",
        "expression": "input1.PlantLoadFactor >= 0 && input1.PlantLoadFactor <= 100",
        "successEvent": null,
        "sortOrder": 3,
        "actions": null
      },
      {
        "ruleName": "CalculateTotalWeight",
        "ruleDisplayName": "Total Weight",
        "ruleType": "Calculation",
        "errorMessage": null,
        "enabled": true,
        "ruleExpressionType": "LambdaExpression",
        "expression": "true",
        "successEvent": "TotalWeightCalculated",
        "sortOrder": 900,
        "actions": {
          "onSuccess": {
            "name": "OutputExpression",
            "context": {
              "expression": "100"
            }
          }
        }
      },
      {
        "ruleName": "CalculateWeightedAverage",
        "ruleDisplayName": "Weighted Average Score",
        "ruleType": "Calculation",
        "errorMessage": null,
        "enabled": true,
        "ruleExpressionType": "LambdaExpression",
        "expression": "input1.FrameworkHasScores == true",
        "successEvent": "WeightedAverageCalculated",
        "sortOrder": 901,
        "actions": {
          "onSuccess": {
            "name": "OutputExpression",
            "context": {
              "expression": "(input1.Score_PlantAvailabilityFactor * 15 + input1.Score_GridAvailability * 15 + input1.Score_PlantLoadFactor * 70) / 100"
            }
          }
        }
      }
    ]
  }
]', SYSUTCDATETIME());
GO
INSERT INTO dbo.WorkflowMaster (WorkflowCode, WorkflowName) VALUES (N'FinancialFramework', N'Financial Framework');
DECLARE @WorkflowId INT = SCOPE_IDENTITY();
DECLARE @ParameterId INT;
INSERT INTO dbo.ParameterDefinition (WorkflowId, ParameterCode, DisplayName, DataType, IsRequired, SortOrder, IsActive, Weight, WhyItMatters, HowToMeasure) VALUES (@WorkflowId, N'Leverage', N'Leverage', N'Decimal', 0, 1, 1, 60, N'Leverage indicates the level of financial debt relative to the assessed capital structure.', N'Use the approved leverage ratio for the project');
SET @ParameterId = SCOPE_IDENTITY();
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 1, N'Above 80%');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 2, N'60% - 80%');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 3, N'40% - 59%');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 4, N'20% - 39%');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 5, N'Below 20%');
INSERT INTO dbo.ParameterDefinition (WorkflowId, ParameterCode, DisplayName, DataType, IsRequired, SortOrder, IsActive, Weight, WhyItMatters, HowToMeasure) VALUES (@WorkflowId, N'CumulativeDSCR', N'Cumulative DSCR', N'Decimal', 0, 2, 1, 20, N'Cumulative DSCR measures the ability of project cash flow to service debt over the assessment horizon.', N'Use the cumulative debt service coverage ratio');
SET @ParameterId = SCOPE_IDENTITY();
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 1, N'Below 1.00x');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 2, N'1.00x - 1.19x');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 3, N'1.20x - 1.39x');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 4, N'1.40x - 1.59x');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 5, N'1.60x or above');
INSERT INTO dbo.ParameterDefinition (WorkflowId, ParameterCode, DisplayName, DataType, IsRequired, SortOrder, IsActive, Weight, WhyItMatters, HowToMeasure) VALUES (@WorkflowId, N'MinimumDSCR', N'Minimum DSCR', N'Decimal', 0, 3, 1, 20, N'Minimum DSCR highlights the weakest debt service coverage point in the assessment horizon.', N'Use the minimum period DSCR');
SET @ParameterId = SCOPE_IDENTITY();
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 1, N'Below 1.00x');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 2, N'1.00x - 1.19x');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 3, N'1.20x - 1.39x');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 4, N'1.40x - 1.59x');
INSERT INTO dbo.ScoreDefinition (ParameterDefinitionId, Score, Description) VALUES (@ParameterId, 5, N'1.60x or above');
INSERT INTO dbo.RuleDefinition (WorkflowId, RuleCode, RuleName, RuleType, ConditionExpression, ErrorMessage, Enabled, SortOrder) VALUES (@WorkflowId, N'ValidateLeverage', N'Leverage must be between 0 and 100', N'Validation', N'input1.Leverage >= 0 && input1.Leverage <= 100', N'Leverage must be between 0 and 100.', 1, 1);
INSERT INTO dbo.RuleDefinition (WorkflowId, RuleCode, RuleName, RuleType, ConditionExpression, ErrorMessage, Enabled, SortOrder) VALUES (@WorkflowId, N'ValidateCumulativeDSCR', N'Cumulative DSCR must be greater than zero', N'Validation', N'input1.CumulativeDSCR > 0', N'Cumulative DSCR must be greater than zero.', 1, 2);
INSERT INTO dbo.RuleDefinition (WorkflowId, RuleCode, RuleName, RuleType, ConditionExpression, ErrorMessage, Enabled, SortOrder) VALUES (@WorkflowId, N'ValidateMinimumDSCR', N'Minimum DSCR must be greater than zero', N'Validation', N'input1.MinimumDSCR > 0', N'Minimum DSCR must be greater than zero.', 1, 3);
IF NOT EXISTS (SELECT 1 FROM dbo.RuleDefinition WHERE WorkflowId = @WorkflowId AND RuleCode = N'CalculateTotalWeight')
    INSERT INTO dbo.RuleDefinition (WorkflowId, RuleCode, RuleName, RuleType, ConditionExpression, CalculationExpression, SuccessEvent, Enabled, SortOrder)
    VALUES (@WorkflowId, N'CalculateTotalWeight', N'Total Weight', N'Calculation', N'true', N'100', N'TotalWeightCalculated', 1, 900);
IF NOT EXISTS (SELECT 1 FROM dbo.RuleDefinition WHERE WorkflowId = @WorkflowId AND RuleCode = N'CalculateWeightedAverage')
    INSERT INTO dbo.RuleDefinition (WorkflowId, RuleCode, RuleName, RuleType, ConditionExpression, CalculationExpression, SuccessEvent, Enabled, SortOrder)
    VALUES (@WorkflowId, N'CalculateWeightedAverage', N'Weighted Average Score', N'Calculation', N'input1.FrameworkHasScores == true', N'(input1.Score_Leverage * 60 + input1.Score_CumulativeDSCR * 20 + input1.Score_MinimumDSCR * 20) / 100', N'WeightedAverageCalculated', 1, 901);
INSERT INTO dbo.WorkflowJson (WorkflowId, Version, SourceHash, JsonContent, GeneratedAtUtc) VALUES (@WorkflowId, 1, '6D2F26211D1124F9130B7EE2E01F187DB1C1BD0899C6103B48910FF562D0E893', N'[
  {
    "workflowName": "FinancialFramework",
    "workflowDisplayName": "Financial Framework",
    "ruleExpressionType": "LambdaExpression",
    "parameters": [
      {
        "parameterCode": "Leverage",
        "displayName": "Leverage",
        "dataType": "Decimal",
        "isRequired": false,
        "sortOrder": 1,
        "isActive": true,
        "weight": 60,
        "whyItMatters": "Leverage indicates the level of financial debt relative to the assessed capital structure.",
        "howToMeasure": "Use the approved leverage ratio for the project",
        "scoreDefinitions": [
          {
            "score": 1,
            "description": "Above 80%"
          },
          {
            "score": 2,
            "description": "60% - 80%"
          },
          {
            "score": 3,
            "description": "40% - 59%"
          },
          {
            "score": 4,
            "description": "20% - 39%"
          },
          {
            "score": 5,
            "description": "Below 20%"
          }
        ]
      },
      {
        "parameterCode": "CumulativeDSCR",
        "displayName": "Cumulative DSCR",
        "dataType": "Decimal",
        "isRequired": false,
        "sortOrder": 2,
        "isActive": true,
        "weight": 20,
        "whyItMatters": "Cumulative DSCR measures the ability of project cash flow to service debt over the assessment horizon.",
        "howToMeasure": "Use the cumulative debt service coverage ratio",
        "scoreDefinitions": [
          {
            "score": 1,
            "description": "Below 1.00x"
          },
          {
            "score": 2,
            "description": "1.00x - 1.19x"
          },
          {
            "score": 3,
            "description": "1.20x - 1.39x"
          },
          {
            "score": 4,
            "description": "1.40x - 1.59x"
          },
          {
            "score": 5,
            "description": "1.60x or above"
          }
        ]
      },
      {
        "parameterCode": "MinimumDSCR",
        "displayName": "Minimum DSCR",
        "dataType": "Decimal",
        "isRequired": false,
        "sortOrder": 3,
        "isActive": true,
        "weight": 20,
        "whyItMatters": "Minimum DSCR highlights the weakest debt service coverage point in the assessment horizon.",
        "howToMeasure": "Use the minimum period DSCR",
        "scoreDefinitions": [
          {
            "score": 1,
            "description": "Below 1.00x"
          },
          {
            "score": 2,
            "description": "1.00x - 1.19x"
          },
          {
            "score": 3,
            "description": "1.20x - 1.39x"
          },
          {
            "score": 4,
            "description": "1.40x - 1.59x"
          },
          {
            "score": 5,
            "description": "1.60x or above"
          }
        ]
      }
    ],
    "rules": [
      {
        "ruleName": "ValidateLeverage",
        "ruleDisplayName": "Leverage must be between 0 and 100",
        "ruleType": "Validation",
        "errorMessage": "Leverage must be between 0 and 100.",
        "enabled": true,
        "ruleExpressionType": "LambdaExpression",
        "expression": "input1.Leverage >= 0 && input1.Leverage <= 100",
        "successEvent": null,
        "sortOrder": 1,
        "actions": null
      },
      {
        "ruleName": "ValidateCumulativeDSCR",
        "ruleDisplayName": "Cumulative DSCR must be greater than zero",
        "ruleType": "Validation",
        "errorMessage": "Cumulative DSCR must be greater than zero.",
        "enabled": true,
        "ruleExpressionType": "LambdaExpression",
        "expression": "input1.CumulativeDSCR > 0",
        "successEvent": null,
        "sortOrder": 2,
        "actions": null
      },
      {
        "ruleName": "ValidateMinimumDSCR",
        "ruleDisplayName": "Minimum DSCR must be greater than zero",
        "ruleType": "Validation",
        "errorMessage": "Minimum DSCR must be greater than zero.",
        "enabled": true,
        "ruleExpressionType": "LambdaExpression",
        "expression": "input1.MinimumDSCR > 0",
        "successEvent": null,
        "sortOrder": 3,
        "actions": null
      },
      {
        "ruleName": "CalculateTotalWeight",
        "ruleDisplayName": "Total Weight",
        "ruleType": "Calculation",
        "errorMessage": null,
        "enabled": true,
        "ruleExpressionType": "LambdaExpression",
        "expression": "true",
        "successEvent": "TotalWeightCalculated",
        "sortOrder": 900,
        "actions": {
          "onSuccess": {
            "name": "OutputExpression",
            "context": {
              "expression": "100"
            }
          }
        }
      },
      {
        "ruleName": "CalculateWeightedAverage",
        "ruleDisplayName": "Weighted Average Score",
        "ruleType": "Calculation",
        "errorMessage": null,
        "enabled": true,
        "ruleExpressionType": "LambdaExpression",
        "expression": "input1.FrameworkHasScores == true",
        "successEvent": "WeightedAverageCalculated",
        "sortOrder": 901,
        "actions": {
          "onSuccess": {
            "name": "OutputExpression",
            "context": {
              "expression": "(input1.Score_Leverage * 60 + input1.Score_CumulativeDSCR * 20 + input1.Score_MinimumDSCR * 20) / 100"
            }
          }
        }
      }
    ]
  }
]', SYSUTCDATETIME());
GO
SELECT w.WorkflowCode, w.WorkflowName, COUNT(DISTINCT p.Id) AS ParameterCount, COUNT(DISTINCT r.Id) AS RuleCount
FROM dbo.WorkflowMaster w
LEFT JOIN dbo.ParameterDefinition p ON p.WorkflowId = w.Id
LEFT JOIN dbo.RuleDefinition r ON r.WorkflowId = w.Id
GROUP BY w.WorkflowCode, w.WorkflowName
ORDER BY w.Id;
GO

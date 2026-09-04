# Database Schema Diagram

```
+-------------------+       +-----------------------+       +-------------------+
|   Applications    |       |     Dependencies      |       |     ADGroups      |
+-------------------+       +-----------------------+       +-------------------+
| ApplicationId (PK)|<------| SourceApplicationId   |       | GroupId (PK)      |
| Name              |       | TargetApplicationId   |------>| GroupName         |
| Description       |       | DependencyType        |       | Role              |
| BusinessGroup     |       | Direction             |       | Description       |
| BusinessOwner     |       | Description           |       | IsActive          |
| BusinessOwnerEmail|       | CriticalityLevel     |       | CreatedDate       |
| BusinessBackup    |       | Impact               |       | CreatedDate       |
| BusinessBackup    |       | Frequency             |       +-------------------+
| BusinessBackupEmail|      | CreatedDate           |
| TechnicalOwner    |       | ModifiedDate          |
| TechnicalOwnerEmail|      | CreatedBy             |
| CategoryId (FK)   |       | ModifiedBy            |
| Environment       |       +-----------------------+
| CriticalityLevel  |
| Status            |
| ExternalUrl       |
| CreatedDate       |
| ModifiedDate      |
| CreatedBy         |
| ModifiedBy        |
+-------------------+
        |
        | CategoryId -> ApplicationCategories(CategoryId)
        | TargetApplicationId -> Applications(ApplicationId)
        +-----------+  SourceApplicationId -> Applications(ApplicationId)

+----------------------------+
|   ApplicationCategories    |
+----------------------------+
| CategoryId (PK)            |
| Name (unique)              |
| Description (nullable)     |
| IsActive                   |
| SortOrder                  |
| audit cols                 |
+----------------------------+
        |
        | PropertyDefinitionId -> ApplicationPropertyDefinitions

+----------------------------+
| ApplicationPropertyDefinitions |
+----------------------------+
| PropertyDefinitionId (PK)  |
| Key (unique)               |
| Label                      |
| DataType                   |
| ScanPattern (nullable)     |
| Description                |
| IsActive                   |
| SortOrder                  |
| audit cols                 |
+----------------------------+
        |
        | PropertyDefinitionId -> ApplicationPropertyDefinitions
+----------------------------+
| ApplicationPropertyValues  |
+----------------------------+
| PropertyValueId (PK)       |
| ApplicationId              |
| PropertyDefinitionId       |
| Value                      |
| audit cols                 |
+----------------------------+

+-------------------+
|   ActivityLog     |
+-------------------+
| LogId (PK)        |
| Action            |
| EntityType        |
| EntityId          |
| EntityName        |
| Details           |
| PerformedBy       |
| PerformedDate     |
+-------------------+
```

## Dependency Direction Semantics

A row in `Dependencies` means **Source depends on Target**:

- `SourceApplicationId` = the consumer (the application that makes the call / uses the resource)
- `TargetApplicationId` = the provider (the application being depended upon)

**Upstream dependencies** of App X = rows where `SourceApplicationId = X`
(directly in `Dependencies`), and transitively where X's providers depend on others.

**Downstream dependents** of App X = rows where `TargetApplicationId = X`
(who depends on X), and transitively who depends on those.

## Level Calculation (for visualization)

- Level 0 = the root application you are inspecting
- Level 1 = direct upstream dependencies (or downstream dependents)
- Level 2 = dependencies of Level 1, etc.

The `GetDependencyChain` stored procedure returns a `Depth` column that
corresponds to these levels.

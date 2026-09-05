# Database Schema Diagram

```
+-------------------+       +-----------------------+       +-------------------+
|   Applications    |       |     Dependencies      |       |     ADGroups      |
+-------------------+       +-----------------------+       +-------------------+
| ApplicationId (PK)|<------| SourceApplicationId   |       | GroupId (PK)      |
| Name              |       | TargetApplicationId   |------>| GroupName         |
| Version           |       | DependencyType        |       | Role              |
| Description       |       | Direction             |       | Description       |
| BusinessGroup     |       | Description           |       | IsActive          |
| BusinessOwner     |       | CriticalityLevel      |       | CreatedDate       |
| BusinessOwnerEmail|       | Impact                |       | CreatedBy         |
| BusinessBackup    |       | Frequency             |       | ModifiedBy        |
| BusinessBackupEmail|      | CreatedDate           |       +-------------------+
| TechnicalOwnerEmail|      | ModifiedDate          |
| CategoryId (FK)   |       | CreatedBy             |
| FamilyId (FK)     |       | ModifiedBy            |
| TeamId (FK)       |       +-----------------------+
| Environment       |
| CriticalityLevel  |
| Status            |
| ExternalUrl       |
| SourcePath        |
| IsDeleted         |
| audit cols        |
+-------------------+
        |
        | CategoryId -> ApplicationCategories(CategoryId)
        | FamilyId -> ApplicationFamilies(FamilyId)
        | TeamId -> TechnicalOwnershipTeams(TeamId)
        | TargetApplicationId -> Applications(ApplicationId)
        +-----------+  SourceApplicationId -> Applications(ApplicationId)

+----------------------------+
|   ApplicationFamilies      |
+----------------------------+
| FamilyId (PK)              |
| Name (unique)              |
| Description (nullable)     |
| IsActive                   |
| SortOrder                  |
| audit cols                 |
+----------------------------+

+----------------------------+
| TechnicalOwnershipTeams    |
+----------------------------+
| TeamId (PK)                |
| Name (unique)              |
| Description (nullable)     |
| IsActive                   |
| SortOrder                  |
| audit cols                 |
+----------------------------+
        |
        | TeamId -> Applications(TechnicalOwnershipTeamId)

+---------------------------+     +-------------------------+
| ApplicationTagMappings    |     | ApplicationTags         |
+---------------------------+     +-------------------------+
| MappingId (PK)            |     | TagId (PK)              |
| ApplicationId (FK)        |     | Name (unique)           |
| TagId (FK)                |     | Description (nullable)  |
+---------------------------+     | IsActive                |
        |                           | SortOrder               |
        | ApplicationId -> Applications(ApplicationId) | audit cols           |
        | TagId -> ApplicationTags(TagId)              +-------------------------+

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

+----------------------------+
| ApplicationPropertyValues  |
+----------------------------+
| PropertyValueId (PK)       |
| ApplicationId              |
| PropertyDefinitionId       |
| Value                      |
| audit cols                 |
+----------------------------+
        |
        | PropertyDefinitionId -> ApplicationPropertyDefinitions
        | ApplicationId -> Applications(ApplicationId)

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
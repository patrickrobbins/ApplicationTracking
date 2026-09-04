# Application Dependency Tracker - Implementation Plan

## Overview
An enterprise web application for tracking interdependencies between applications, built on .NET Framework 4.7.2 with MS SQL Server backend and modern interactive visualization.

---

## Architecture

### Technology Stack
- **Backend**: ASP.NET MVC 5 (.NET Framework 4.7.2)
- **ORM**: Entity Framework 6 (Database-First or Code-First)
- **Database**: MS SQL Server (2016+)
- **Frontend**: jQuery, Bootstrap 4, Cytoscape.js
- **Authentication**: Windows Authentication (AD)
- **Authorization**: Role-based via AD Groups

### Project Structure
```
DependencyTracker/
├── DependencyTracker.sln
├── DependencyTracker.Web/           # ASP.NET MVC Web Application
│   ├── Controllers/
│   │   ├── HomeController.cs
│   │   ├── ApplicationsController.cs
│   │   ├── DependenciesController.cs
│   │   ├── ApiController.cs         # AJAX endpoints
│   │   └── AdminController.cs
│   ├── Models/
│   │   ├── EF/
│   │   │   ├── DependencyTrackerEntities.cs
│   │   │   ├── Application.cs
│   │   │   ├── Dependency.cs
│   │   │   └── ApplicationEnvironment.cs
│   │   └── ViewModels/
│   │       ├── ApplicationListViewModel.cs
│   │       ├── ApplicationDetailViewModel.cs
│   │       ├── DependencyGraphViewModel.cs
│   │       └── AdminViewModel.cs
│   ├── Views/
│   │   ├── Home/
│   │   ├── Applications/
│   │   ├── Dependencies/
│   │   ├── Admin/
│   │   └── Shared/
│   ├── Scripts/
│   │   ├── app/
│   │   │   ├── dependency-graph.js  # Cytoscape.js visualization
│   │   │   ├── app-search.js
│   │   │   └── admin.js
│   │   └── lib/
│   │       ├── cytoscape.min.js
│   │       └── ...
│   ├── Content/
│   │   └── css/
│   └── App_Start/
│       ├── RouteConfig.cs
│       ├── BundleConfig.cs
│       └── FilterConfig.cs
├── DependencyTracker.Data/          # Data Access Layer
│   ├── Repositories/
│   │   ├── IApplicationRepository.cs
│   │   ├── ApplicationRepository.cs
│   │   ├── IDependencyRepository.cs
│   │   └── DependencyRepository.cs
│   └── Services/
│       ├── IApplicationService.cs
│       ├── ApplicationService.cs
│       ├── IDependencyService.cs
│       └── DependencyService.cs
└── Database/
    ├── Scripts/
    │   ├── 01_CreateTables.sql
    │   ├── 02_SeedData.sql
    │   └── 03_CreateADGroups.sql
    └── diagrams/
        └── DatabaseDiagram.md
```

---

## Database Schema

### Tables

#### Applications
```sql
CREATE TABLE Applications (
    ApplicationId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Version NVARCHAR(50) NULL,  -- deployed version; multiple versions share the same Name
    Description NVARCHAR(2000) NULL,
    BusinessOwner NVARCHAR(200) NULL,
    TechnicalOwner NVARCHAR(200) NULL,
    Environment NVARCHAR(50) NULL,  -- Production, Staging, Development
    CriticalityLevel NVARCHAR(20) NULL,  -- Critical, High, Medium, Low
    DefaultDependencyCriticality NVARCHAR(20) NULL,  -- pre-fill for dependencies this app provides
    DefaultDependencyImpact NVARCHAR(2000) NULL,  -- pre-fill "what breaks" note
    Status NVARCHAR(20) DEFAULT 'Active',  -- Active, Retired, Planned
    ApplicationIDE NVARCHAR(200) NULL,  -- development IDE
    FrameworkVersion NVARCHAR(100) NULL,  -- runtime framework / .NET version
    DocumentationLink NVARCHAR(500) NULL,
    SourceControlLocation NVARCHAR(500) NULL,  -- repository URL / VCS path
    HoursOfOperation NVARCHAR(200) NULL,
    MaintenanceWindow NVARCHAR(200) NULL,
    MaintenanceNotificationEmail NVARCHAR(200) NULL,
    ApplicationSummary NVARCHAR(2000) NULL,
    TriageSteps NVARCHAR(2000) NULL,  -- incident triage / runbook steps
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(128) NULL,
    ModifiedBy NVARCHAR(128) NULL,
    CONSTRAINT UQ_Applications_Name_Version UNIQUE (Name, Version)
);
```

#### ApplicationTechnologies (managed tag list)
```sql
CREATE TABLE ApplicationTechnologies (
    TechnologyId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    IsActive BIT DEFAULT 1,
    SortOrder INT DEFAULT 0,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(128) NULL,
    ModifiedBy NVARCHAR(128) NULL,
    CONSTRAINT UQ_ApplicationTechnologies_Name UNIQUE (Name)
);
```

#### ApplicationTechnologyMappings (many-to-many)
```sql
CREATE TABLE ApplicationTechnologyMappings (
    MappingId INT IDENTITY(1,1) PRIMARY KEY,
    ApplicationId INT NOT NULL,
    TechnologyId INT NOT NULL,
    CONSTRAINT FK_AppTechMap_Application FOREIGN KEY (ApplicationId)
        REFERENCES Applications(ApplicationId) ON DELETE CASCADE,
    CONSTRAINT FK_AppTechMap_Technology FOREIGN KEY (TechnologyId)
        REFERENCES ApplicationTechnologies(TechnologyId) ON DELETE CASCADE,
    CONSTRAINT UQ_AppTechMap_App_Technology UNIQUE (ApplicationId, TechnologyId)
);
```

#### Dependencies
```sql
CREATE TABLE Dependencies (
    DependencyId INT IDENTITY(1,1) PRIMARY KEY,
    SourceApplicationId INT NOT NULL,
    TargetApplicationId INT NOT NULL,
    DependencyType NVARCHAR(50) NOT NULL,  -- API, Database, File, Message, UI
    Description NVARCHAR(500) NULL,
    CriticalityLevel NVARCHAR(20) DEFAULT 'Low',  -- Critical, High, Medium, Low
    Impact NVARCHAR(2000) NULL,  -- what stops working when this dependency breaks
    Frequency NVARCHAR(50) NULL,  -- Real-time, Hourly, Daily, Weekly
    Direction NVARCHAR(20) DEFAULT 'Upstream',  -- Upstream, Downstream, Bidirectional
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(128) NULL,
    CONSTRAINT FK_Dependencies_Source FOREIGN KEY (SourceApplicationId) 
        REFERENCES Applications(ApplicationId),
    CONSTRAINT FK_Dependencies_Target FOREIGN KEY (TargetApplicationId) 
        REFERENCES Applications(ApplicationId),
    CONSTRAINT UQ_Dependencies_Relationship UNIQUE (SourceApplicationId, TargetApplicationId, DependencyType)
);
```

#### ADGroups (for role mapping)
```sql
CREATE TABLE ADGroups (
    GroupId INT IDENTITY(1,1) PRIMARY KEY,
    GroupName NVARCHAR(200) NOT NULL,
    Role NVARCHAR(50) NOT NULL,  -- Admin, Maintenance, Viewer
    IsActive BIT DEFAULT 1,
    CONSTRAINT UQ_ADGroups_Name UNIQUE (GroupName)
);

-- Default entries
INSERT INTO ADGroups (GroupName, Role) VALUES 
('DOMAIN\DependencyTracker_Admin', 'Admin'),
('DOMAIN\DependencyTracker_Maintenance', 'Maintenance');
```

#### ActivityLog (audit trail)
```sql
CREATE TABLE ActivityLog (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    Action NVARCHAR(50) NOT NULL,
    EntityType NVARCHAR(50) NOT NULL,
    EntityId INT NOT NULL,
    Details NVARCHAR(2000) NULL,
    PerformedBy NVARCHAR(128) NOT NULL,
    PerformedDate DATETIME2 DEFAULT GETUTCDATE()
);
```

### Indexes
```sql
CREATE INDEX IX_Dependencies_Source ON Dependencies(SourceApplicationId);
CREATE INDEX IX_Dependencies_Target ON Dependencies(TargetApplicationId);
CREATE INDEX IX_Applications_Name ON Applications(Name);
CREATE INDEX IX_Applications_Status ON Applications(Status);
CREATE INDEX IX_AppTechMap_ApplicationId ON ApplicationTechnologyMappings(ApplicationId);
CREATE INDEX IX_AppTechMap_TechnologyId ON ApplicationTechnologyMappings(TechnologyId);
```

---

## User Roles & Authorization

### Role Definitions
| Role | Access Level |
|------|--------------|
| **Viewer** | Read-only access to all applications and dependencies. Can view graphs and search. (Default for all authenticated users) |
| **Maintenance** | Can create/edit applications and dependencies. Can manage application metadata. |
| **Admin** | Full access including user management, AD group configuration, and system settings. |

### AD Integration Strategy
```csharp
// Windows Authentication + AD Group membership check
[Authorize(Roles = "Viewer,Maintenance,Admin")]
public class ApplicationsController : Controller
{
    // Get user roles from AD groups
    private List<string> GetUserRoles()
    {
        var roles = new List<string>();
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        
        // Check AD group membership
        using (var entry = new DirectoryEntry("LDAP://DC=domain,DC=com"))
        {
            using (var searcher = new DirectorySearcher(entry))
            {
                searcher.Filter = $"(&(objectClass=user)(sAMAccountName={User.Identity.Name.Split('\\')[1]}))";
                var result = searcher.FindOne();
                if (result != null)
                {
                    var memberOf = result.Properties["memberOf"];
                    // Check against configured AD groups
                }
            }
        }
        return roles;
    }
}
```

### Role-Based UI Elements
```html
@if (User.IsInRole("Maintenance") || User.IsInRole("Admin"))
{
    <button class="btn btn-primary" id="btnAddApplication">Add Application</button>
}
@if (User.IsInRole("Admin"))
{
    <a href="@Url.Action("Index", "Admin")" class="btn btn-warning">Admin</a>
}
```

---

## Visualization (Cytoscape.js)

### Core Features
1. **Interactive Dependency Graph**
   - Directed graph showing application relationships
   - Node size based on dependency count
   - Color coding by criticality level
   - Edge labels showing dependency type

2. **Navigation**
   - Click node to see application details
   - Expand/collapse dependency branches
   - Zoom and pan controls
   - Focus on specific application

3. **Level Indicators**
   - Visual depth levels (L0, L1, L2, etc.)
   - Level-based layout (hierarchical)
   - Path highlighting from source to target

4. **Filtering**
   - By environment (Production, Staging, etc.)
   - By criticality level
   - By dependency type
   - By application status

### Cytoscape.js Configuration
```javascript
var cy = cytoscape({
    container: document.getElementById('cy'),
    
    elements: {
        nodes: [
            { data: { id: 'app1', name: 'App 1', level: 0, criticality: 'Critical' } },
            { data: { id: 'app2', name: 'App 2', level: 1, criticality: 'High' } }
        ],
        edges: [
            { data: { source: 'app1', target: 'app2', type: 'API', criticality: 'Critical', impact: 'Consumer sign-in and session flows stop.' } }
        ]
    },
    
    style: [
        {
            selector: 'node',
            style: {
                'background-color': getNodeColor,
                'label': 'data(name)',
                'text-valign': 'center',
                'text-wrap': 'wrap',
                'text-max-width': '100px',
                'width': 'label',
                'height': '40px',
                'shape': 'roundrectangle',
                'border-width': 2,
                'border-color': '#333'
            }
        },
        {
            selector: 'edge',
            style: {
                'width': 2,
                'line-color': '#999',
                'target-arrow-color': '#999',
                'target-arrow-shape': 'triangle',
                'curve-style': 'bezier',
                'label': 'data(type)',
                'text-rotation': 'autorotate',
                'font-size': '10px'
            }
        },
        {
            selector: '.level-0',
            style: { 'background-color': '#e74c3c' }  // Red for level 0
        },
        {
            selector: '.level-1',
            style: { 'background-color': '#e67e22' }  // Orange for level 1
        },
        {
            selector: '.level-2',
            style: { 'background-color': '#f1c40f' }  // Yellow for level 2
        }
    ],
    
    layout: {
        name: 'breadthfirst',
        directed: true,
        roots: '#level-0-nodes',
        spacingFactor: 1.5,
        padding: 50
    },
    
    // Interactive features
    wheelSensitivity: 0.2,
    minZoom: 0.1,
    maxZoom: 3
});
```

### API Endpoints for Graph Data
```csharp
[HttpGet]
public JsonResult GetDependencyGraph(int? applicationId = null, int depth = 3)
{
    var graphData = _dependencyService.GetDependencyGraph(applicationId, depth);
    return Json(graphData, JsonRequestBehavior.AllowGet);
}

[HttpGet]
public JsonResult GetUpstreamDependencies(int applicationId, int depth = 3)
{
    var upstream = _dependencyService.GetUpstreamDependencies(applicationId, depth);
    return Json(upstream, JsonRequestBehavior.AllowGet);
}

[HttpGet]
public JsonResult GetDownstreamDependencies(int applicationId, int depth = 3)
{
    var downstream = _dependencyService.GetDownstreamDependencies(applicationId, depth);
    return Json(downstream, JsonRequestBehavior.AllowGet);
}
```

---

## Key Views

### 1. Dashboard (Home/Index)
- Summary statistics (total apps, dependencies, critical dependencies)
- Recent activity feed
- Quick search
- Quick access to visualization

### 2. Application List (Applications/Index)
- Filterable/searchable grid
- Status indicators
- Quick actions (view, edit, delete)

### 3. Application Detail (Applications/Details)
- Application metadata (owners, category, properties)
- List of upstream dependencies (what this app depends on) — direct and indirect (transitive)
- List of downstream dependencies (what depends on this app) — direct and indirect
- Indirect entries show the chain of applications that cause the dependency (link trail)
- Embedded mini-graph
- Activity history

### 4. Dependency Graph (Home/Graph or Dependencies/Graph)
- Full-screen Cytoscape.js visualization
- Sidebar with filters
- Click-to-navigate functionality
- Level indicators
- Export to PNG/SVG

### 5. Admin Panel (Admin/Index)
- AD Group management
- Role assignments
- System settings
- Audit log viewer

---

## Implementation Phases

### Phase 1: Foundation (Week 1-2)
- [ ] Set up solution structure
- [ ] Create database schema
- [ ] Implement Entity Framework models
- [ ] Set up Windows Authentication
- [ ] Basic CRUD for Applications
- [ ] Basic CRUD for Dependencies

### Phase 2: Core Features (Week 3-4)
- [ ] Role-based authorization
- [ ] AD Group integration
- [ ] Application search and filtering
- [ ] Dependency management UI
- [ ] Activity logging

### Phase 3: Visualization (Week 5-6)
- [ ] Integrate Cytoscape.js
- [ ] Implement dependency graph API
- [ ] Basic graph rendering
- [ ] Node/edge styling
- [ ] Level-based layout

### Phase 4: Advanced Features (Week 7-8)
- [ ] Interactive navigation (expand/collapse)
- [ ] Filtering in graph view
- [ ] Export functionality
- [ ] Application detail mini-graph
- [ ] Performance optimization

### Phase 5: Polish & Testing (Week 9-10)
- [ ] UI/UX refinement
- [ ] Responsive design
- [ ] Security audit
- [ ] Performance testing
- [ ] User acceptance testing
- [ ] Documentation

---

## NuGet Packages Required
```xml
<packages>
  <package id="EntityFramework" version="6.4.4" />
  <package id="Microsoft.AspNet.Mvc" version="5.2.9" />
  <package id="Microsoft.AspNet.Razor" version="3.2.9" />
  <package id="Microsoft.AspNet.Web.Optimization" version="1.1.3" />
  <package id="Microsoft.AspNet.WebApi" version="5.2.9" />
  <package id="Microsoft.AspNet.WebPages" version="3.2.9" />
  <package id="Microsoft.Web.Infrastructure" version="2.0.1" />
  <package id="jQuery" version="3.6.0" />
  <package id="jQuery.Validation" version="1.19.5" />
  <package id="bootstrap" version="4.6.2" />
  <package id="Microsoft.jQuery.Unobtrusive.Validation" version="4.0.0" />
  <package id="WebGrease" version="1.6.0" />
  <package id="System.Diagnostics.DiagnosticSource" version="7.0.0" />
</packages>
```

---

## Security Considerations
1. **Input Validation**: All user inputs sanitized
2. **SQL Injection**: Prevented via Entity Framework parameterized queries
3. **XSS**: Anti-forgery tokens, HTML encoding
4. **CSRF**: Anti-forgery tokens on all POST actions
5. **Authorization**: Server-side role checks on all actions
6. **Audit Trail**: All create/update/delete operations logged

---

## Performance Considerations
1. **Graph Loading**: Paginate large dependency trees (max depth parameter)
2. **Caching**: Cache graph data for frequently viewed applications
3. **Lazy Loading**: Load application details on demand
4. **Database**: Proper indexing on foreign keys and search columns
5. **CDN**: Serve Cytoscape.js from CDN or bundle minified version

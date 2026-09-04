/*
====================================================================
Application Tracking - Post-Deployment seed data
Runs after the schema is deployed. Every block is idempotent so
republishing does not duplicate data.
====================================================================
*/
PRINT N'Starting post-deployment seed.';
GO

:r .\Seed\Seed_ApplicationCategories.sql
:r .\Seed\Seed_ADGroups.sql
:r .\Seed\Seed_Applications.sql
:r .\Seed\Seed_Dependencies.sql
:r .\Seed\Seed_ApplicationDlls.sql
:r .\Seed\Seed_ApplicationTechnologies.sql
:r .\Seed\Seed_ApplicationPropertyDefinitions.sql

PRINT N'Post-deployment seed complete.';
GO

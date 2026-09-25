using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;

namespace DependencyTracker.Data.Models
{
    /// <summary>
    /// Entity Framework DbContext backed by the DependencyTracker database.
    /// </summary>
    public class DependencyTrackerDbContext : DbContext
    {
        public DependencyTrackerDbContext() : base("DependencyTracker")
        {
            // Do not auto-migrate; schema is managed by SQL scripts.
            Database.SetInitializer<DependencyTrackerDbContext>(null);
        }

        public DbSet<Application> Applications { get; set; }

        public DbSet<Dependency> Dependencies { get; set; }

        public DbSet<AdGroup> AdGroups { get; set; }

        public DbSet<ActivityLogEntry> ActivityLog { get; set; }

        public DbSet<ApplicationPropertyDefinition> ApplicationPropertyDefinitions { get; set; }

        public DbSet<ApplicationPropertyValue> ApplicationPropertyValues { get; set; }

        public DbSet<ApplicationCategory> ApplicationCategories { get; set; }

        public DbSet<ApplicationDll> ApplicationDlls { get; set; }

        public DbSet<ApplicationTechnology> ApplicationTechnologies { get; set; }

        public DbSet<ApplicationTechnologyMapping> ApplicationTechnologyMappings { get; set; }

        public DbSet<ApplicationFamily> ApplicationFamilies { get; set; }

        public DbSet<ApplicationType> ApplicationTypes { get; set; }

        public DbSet<TechnicalOwnershipTeam> TechnicalOwnershipTeams { get; set; }

        public DbSet<ApplicationTag> ApplicationTags { get; set; }

        public DbSet<ApplicationTagMapping> ApplicationTagMappings { get; set; }

        public DbSet<PackageType> PackageTypes { get; set; }

        public DbSet<Package> Packages { get; set; }

        public DbSet<ApplicationPackageMapping> ApplicationPackageMappings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // Application self-relationships via Dependencies
            modelBuilder.Entity<Dependency>()
                .HasRequired(d => d.SourceApplication)
                .WithMany(a => a.OutgoingDependencies)
                .HasForeignKey(d => d.SourceApplicationId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Dependency>()
                .HasRequired(d => d.TargetApplication)
                .WithMany(a => a.IncomingDependencies)
                .HasForeignKey(d => d.TargetApplicationId)
                .WillCascadeOnDelete(true);

            // Force non-unique strings to have reasonable max lengths
            modelBuilder.Entity<Application>()
                .Property(a => a.Description).HasMaxLength(2000);
            modelBuilder.Entity<Application>()
                .Property(a => a.ExternalUrl).HasMaxLength(500);
            modelBuilder.Entity<Application>()
                .Property(a => a.SourcePath).HasMaxLength(500);
            modelBuilder.Entity<Dependency>()
                .Property(d => d.Description).HasMaxLength(500);

            // Unique constraint on (application name, version)
            modelBuilder.Entity<Application>()
                .HasIndex(a => new { a.Name, a.Version })
                .IsUnique();

            // Fast filtering of non-deleted applications
            modelBuilder.Entity<Application>()
                .HasIndex(a => a.IsDeleted);

            // Unique constraint on dependency relationships
            modelBuilder.Entity<Dependency>()
                .HasIndex(d => new { d.SourceApplicationId, d.TargetApplicationId, d.DependencyType })
                .IsUnique();

            // Unique constraint on AD groups
            modelBuilder.Entity<AdGroup>()
                .HasIndex(g => g.GroupName)
                .IsUnique();

            // Property definition key is unique and display strings are bounded
            modelBuilder.Entity<ApplicationPropertyDefinition>()
                .HasIndex(d => d.Key)
                .IsUnique();
            modelBuilder.Entity<ApplicationPropertyDefinition>()
                .Property(d => d.ScanPattern).HasMaxLength(500);
            modelBuilder.Entity<ApplicationPropertyDefinition>()
                .Property(d => d.Description).HasMaxLength(500);

            // One property value per (application, definition)
            modelBuilder.Entity<ApplicationPropertyValue>()
                .HasIndex(v => new { v.ApplicationId, v.PropertyDefinitionId })
                .IsUnique();
            modelBuilder.Entity<ApplicationPropertyValue>()
                .Property(v => v.Value).HasMaxLength(2000);

            // Category name is unique and deleting a category leaves apps uncategorized
            modelBuilder.Entity<ApplicationCategory>()
                .HasIndex(c => c.Name)
                .IsUnique();
            modelBuilder.Entity<ApplicationCategory>()
                .Property(c => c.Description).HasMaxLength(500);
            modelBuilder.Entity<Application>()
                .HasOptional(a => a.Category)
                .WithMany()
                .HasForeignKey(a => a.CategoryId)
                .WillCascadeOnDelete(false);

            // Family name is unique and deleting a family leaves apps unfamilied
            modelBuilder.Entity<ApplicationFamily>()
                .HasIndex(f => f.Name)
                .IsUnique();
            modelBuilder.Entity<ApplicationFamily>()
                .Property(f => f.Description).HasMaxLength(500);
            modelBuilder.Entity<Application>()
                .HasOptional(a => a.Family)
                .WithMany()
                .HasForeignKey(a => a.FamilyId)
                .WillCascadeOnDelete(false);

            // Application type name is unique; deleting leaves apps untyped
            modelBuilder.Entity<ApplicationType>()
                .HasIndex(t => t.Name)
                .IsUnique();
            modelBuilder.Entity<ApplicationType>()
                .Property(t => t.Description).HasMaxLength(500);
            modelBuilder.Entity<Application>()
                .HasOptional(a => a.ApplicationType)
                .WithMany()
                .HasForeignKey(a => a.ApplicationTypeId)
                .WillCascadeOnDelete(false);

            // Technical ownership team name is unique; deleting leaves apps unowned
            modelBuilder.Entity<TechnicalOwnershipTeam>()
                .HasIndex(t => t.Name)
                .IsUnique();
            modelBuilder.Entity<TechnicalOwnershipTeam>()
                .Property(t => t.Description).HasMaxLength(500);
            modelBuilder.Entity<Application>()
                .HasOptional(a => a.TechnicalOwnershipTeam)
                .WithMany()
                .HasForeignKey(a => a.TechnicalOwnershipTeamId)
                .WillCascadeOnDelete(false);

            // One DLL row per (application, file name)
            modelBuilder.Entity<Application>()
                .HasMany(a => a.Dlls)
                .WithRequired(d => d.Application)
                .HasForeignKey(d => d.ApplicationId)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<ApplicationDll>()
                .HasIndex(d => new { d.ApplicationId, d.FileName })
                .IsUnique();
            modelBuilder.Entity<ApplicationDll>()
                .Property(d => d.RelativePath).HasMaxLength(1000);

            // Technology tag name is unique; a tag can be applied to many apps
            modelBuilder.Entity<ApplicationTechnology>()
                .HasIndex(t => t.Name)
                .IsUnique();
            modelBuilder.Entity<ApplicationTechnology>()
                .Property(t => t.Description).HasMaxLength(500);

            // One mapping per (application, technology); cascade both ways
            modelBuilder.Entity<ApplicationTechnologyMapping>()
                .HasIndex(m => new { m.ApplicationId, m.TechnologyId })
                .IsUnique();
            modelBuilder.Entity<Application>()
                .HasMany(a => a.TechnologyMappings)
                .WithRequired(m => m.Application)
                .HasForeignKey(m => m.ApplicationId)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<ApplicationTechnology>()
                .HasMany(t => t.Mappings)
                .WithRequired(m => m.Technology)
                .HasForeignKey(m => m.TechnologyId)
                .WillCascadeOnDelete(true);

            // Shared tag name is unique; a tag can be applied to many apps
            modelBuilder.Entity<ApplicationTag>()
                .HasIndex(t => t.Name)
                .IsUnique();
            modelBuilder.Entity<ApplicationTag>()
                .Property(t => t.Description).HasMaxLength(500);

            // One mapping per (application, tag); cascade both ways
            modelBuilder.Entity<ApplicationTagMapping>()
                .HasIndex(m => new { m.ApplicationId, m.TagId })
                .IsUnique();
            modelBuilder.Entity<Application>()
                .HasMany(a => a.TagMappings)
                .WithRequired(m => m.Application)
                .HasForeignKey(m => m.ApplicationId)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<ApplicationTag>()
                .HasMany(t => t.Mappings)
                .WithRequired(m => m.Tag)
                .HasForeignKey(m => m.TagId)
                .WillCascadeOnDelete(true);

            // Package type name is unique; packages reference exactly one type
            modelBuilder.Entity<PackageType>()
                .HasIndex(t => t.Name)
                .IsUnique();
            modelBuilder.Entity<PackageType>()
                .Property(t => t.Description).HasMaxLength(500);

            // Package pool is unique per (type, name) and cascades with its type
            modelBuilder.Entity<Package>()
                .HasIndex(p => new { p.PackageTypeId, p.Name })
                .IsUnique();
            modelBuilder.Entity<PackageType>()
                .HasMany(t => t.Packages)
                .WithRequired(p => p.PackageType)
                .HasForeignKey(p => p.PackageTypeId)
                .WillCascadeOnDelete(true);

            // One mapping per (application, package); cascade both ways
            modelBuilder.Entity<ApplicationPackageMapping>()
                .HasIndex(m => new { m.ApplicationId, m.PackageId })
                .IsUnique();
            modelBuilder.Entity<ApplicationPackageMapping>()
                .Property(m => m.Version).HasMaxLength(100);
            modelBuilder.Entity<Application>()
                .HasMany(a => a.PackageMappings)
                .WithRequired(m => m.Application)
                .HasForeignKey(m => m.ApplicationId)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<Package>()
                .HasMany(p => p.Mappings)
                .WithRequired(m => m.Package)
                .HasForeignKey(m => m.PackageId)
                .WillCascadeOnDelete(true);

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Executes the GetDependencyChain stored procedure and returns edges with depth.
        /// </summary>
        public IEnumerable<DependencyChainResult> GetDependencyChain(int applicationId, int maxDepth, string direction)
        {
            const string sql = @"EXEC [dbo].[GetDependencyChain] @ApplicationId = @ApplicationId, @MaxDepth = @MaxDepth, @Direction = @Direction";

            return Database.SqlQuery<DependencyChainResult>(sql,
                new SqlParameter("@ApplicationId", applicationId),
                new SqlParameter("@MaxDepth", maxDepth),
                new SqlParameter("@Direction", direction));
        }
    }

    /// <summary>
    /// Result row from the GetDependencyChain stored procedure.
    /// </summary>
    public class DependencyChainResult
    {
        public int SourceApplicationId { get; set; }
        public string SourceName { get; set; }
        public string SourceEnvironment { get; set; }
        public string SourceCriticality { get; set; }
        public string SourceStatus { get; set; }
        public string SourceCategory { get; set; }
        public int TargetApplicationId { get; set; }
        public string TargetName { get; set; }
        public string TargetEnvironment { get; set; }
        public string TargetCriticality { get; set; }
        public string TargetStatus { get; set; }
        public string TargetCategory { get; set; }
        public int DependencyId { get; set; }
        public string DependencyType { get; set; }
        public string CriticalityLevel { get; set; }
        public string Impact { get; set; }
        public string Frequency { get; set; }
        public string Direction { get; set; }
        public int Depth { get; set; }
    }
}

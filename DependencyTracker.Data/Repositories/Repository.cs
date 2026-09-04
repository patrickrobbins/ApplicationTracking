using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Repositories
{
    /// <summary>
    /// Generic Entity Framework repository. Each repository instance owns its
    /// own DbContext; repositories are short-lived (created per request).
    /// </summary>
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly DependencyTrackerDbContext Context;
        protected readonly DbSet<TEntity> DbSet;

        public Repository()
        {
            Context = new DependencyTrackerDbContext();
            DbSet = Context.Set<TEntity>();
        }

        public Repository(DependencyTrackerDbContext context)
        {
            Context = context;
            DbSet = Context.Set<TEntity>();
        }

        public TEntity GetById(int id)
        {
            return DbSet.Find(id);
        }

        public IQueryable<TEntity> Query()
        {
            return DbSet;
        }

        public virtual IEnumerable<TEntity> GetAll()
        {
            return DbSet.ToList();
        }

        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return DbSet.Where(predicate).ToList();
        }

        public TEntity Add(TEntity entity)
        {
            return DbSet.Add(entity);
        }

        public void Update(TEntity entity)
        {
            Context.Entry(entity).State = EntityState.Modified;
        }

        public void Remove(TEntity entity)
        {
            DbSet.Remove(entity);
        }

        public int SaveChanges()
        {
            return Context.SaveChanges();
        }

        public void Dispose()
        {
            Context?.Dispose();
        }
    }
}

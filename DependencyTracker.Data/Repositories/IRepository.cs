using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Repositories
{
    public interface IRepository<TEntity> : IDisposable where TEntity : class
    {
        TEntity GetById(int id);
        IQueryable<TEntity> Query();
        IEnumerable<TEntity> GetAll();
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
        TEntity Add(TEntity entity);
        void Update(TEntity entity);
        void Remove(TEntity entity);
        int SaveChanges();
    }
}

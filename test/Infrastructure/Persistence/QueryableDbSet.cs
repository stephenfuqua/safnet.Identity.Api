using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace safnet.Identity.Api.UnitTests.Infrastructure.Persistence
{
    public class QueryableDbSet<TEntity> : DbSet<TEntity>
        where TEntity : class
    {
        public List<TEntity> List = new List<TEntity>();

        public static QueryableDbSet<TEntity> Create(params TEntity[] entities)
        {
            var set = new QueryableDbSet<TEntity>();
            set.List.AddRange(entities);
            return set;
        }

        public Type ElementType => typeof(TEntity);

        public Expression Expression => List.AsQueryable().Expression;

        public ObservableCollection<TEntity> Local => new ObservableCollection<TEntity>(List);

        public IQueryProvider Provider => List.AsQueryable().Provider;
        
        public IEnumerator<TEntity> GetEnumerator()
        {
            return List.GetEnumerator();
        }
        
        public string IncludeTable { get; set; }
        
    }
}

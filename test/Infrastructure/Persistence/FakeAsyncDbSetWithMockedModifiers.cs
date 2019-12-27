using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using safnet.TestHelper.AsyncDbSet;

namespace safnet.Identity.Api.UnitTests.Infrastructure.Persistence
{
    public class aFakeAsyncDbSetWithMockedModifiers<TEntity> : FakeAsyncDbSet<TEntity>
        where TEntity : class
    {
        public IList<TEntity> Updated { get; } = new List<TEntity>();
        public IList<TEntity> Deleted { get; } = new List<TEntity>();

        public override EntityEntry<TEntity> Add(TEntity entity)
        {
            List.Add(entity);

            // Returning null here is only safe because we never want to
            // do anything with an EntityEntry in this application.
            return null;
        }

        public override EntityEntry<TEntity> Update(TEntity entity)
        {
            List.Add(entity);
            Updated.Add(entity);

            // Returning null here is only safe because we never want to
            // do anything with an EntityEntry in this application.
            return null;
        }

        public override EntityEntry<TEntity> Remove(TEntity entity)
        {
            List.Add(entity);
            Deleted.Add(entity);

            // Returning null here is only safe because we never want to
            // do anything with an EntityEntry in this application.
            return null;
        }
    }
}

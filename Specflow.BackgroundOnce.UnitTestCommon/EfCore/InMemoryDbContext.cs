using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Specflow.BackgroundOnce.EFCore;
using Specflow.BackgroundOnce.UnitTestCommon.Data;
using TechTalk.SpecFlow;
using IDatabase = Specflow.BackgroundOnce.UnitTestCommon.Repository.IDatabase;

namespace Specflow.BackgroundOnce.UnitTestCommon.EfCore
{
    public class InMemoryDbContext : DbContext, IDatabase
    {
        private readonly InMemoryDatabaseRoot _root;
        private readonly IInMemorySnapshotHelper _snapshotHelper;

        public InMemoryDbContext(InMemoryDatabaseRoot root, IInMemorySnapshotHelper snapshotHelper)
        {
            _root = root;
            _snapshotHelper = snapshotHelper;
        }

        public ICollection<Person> People => Get<Person>();
        public ICollection<Address> Addresses => Get<Address>();
        public ICollection<Department> Departments => Get<Department>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("Test", _root);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Person>().HasKey(x => new { x.Name });
            modelBuilder.Entity<Address>().HasKey(x => new { x.AddressCode });
            modelBuilder.Entity<Department>().HasKey(x => new { x.DepartmentCode });
        }

        public async Task SaveChangesAsync()
        {
            await base.SaveChangesAsync();
        }

        public ICollection<T> Get<T>()
            where T : class
        {
            return Set<T>().AsCollection();
        }

        public Task Remove<T>(Expression<Func<T, bool>> predicate)
            where T : class
        {
            var data = Set<T>();
            var toRemove = data.Where(predicate).ToArray();
            data.RemoveRange(toRemove);
            return Task.CompletedTask;
        }

        public async Task CreateSnapshot(FeatureContext featureContext)
        {
            if (_snapshotHelper.SnapshotExists(this, featureContext))
            {
                throw new InvalidOperationException("Snapshot has already been created, it might be intentional to replace the snapshot, but it's not expected");
            }

            await _snapshotHelper.CreateSnapshot(this, featureContext);
        }

        public async Task RestoreSnapshot(FeatureContext featureContext)
        {
            if (!_snapshotHelper.SnapshotExists(this, featureContext))
            {
                throw new InvalidOperationException("Snapshot hasn't been created, yet");
            }

            await _snapshotHelper.RestoreSnapshot(this, featureContext);
        }

        public bool SnapshotExists(FeatureContext featureContext)
        {
            return _snapshotHelper.SnapshotExists(this, featureContext);
        }
    }
}
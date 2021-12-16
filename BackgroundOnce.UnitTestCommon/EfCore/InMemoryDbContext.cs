using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BackgroundOnce.EFCore;
using BackgroundOnce.UnitTestCommon.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TechTalk.SpecFlow;

namespace BackgroundOnce.UnitTestCommon.EfCore
{
    public class InMemoryDbContext : DbContext, Repository.IDatabase
    {
        private readonly InMemoryDatabaseRoot _root;
        private readonly ISnapshotHelper _snapshotHelper;

        public InMemoryDbContext(InMemoryDatabaseRoot root, ISnapshotHelper snapshotHelper)
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
            await _snapshotHelper.CreateSnapshot(this, featureContext);
        }

        public async Task RestoreSnapshot(FeatureContext featureContext)
        {
            await _snapshotHelper.RestoreSnapshot(this, featureContext);
        }
    }
}
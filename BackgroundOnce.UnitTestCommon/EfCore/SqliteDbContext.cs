using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BackgroundOnce.EFCore;
using BackgroundOnce.EFCore.Sqlite;
using BackgroundOnce.UnitTestCommon.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TechTalk.SpecFlow;

namespace BackgroundOnce.UnitTestCommon.EfCore
{
    public class SqliteDbContext : DbContext, Repository.IDatabase
    {
        private readonly SqliteConnection _connection;
        private readonly ISnapshotHelper _snapshotHelper;

        [Obsolete("Don't use, only here so `dotnet ef migrations add` can construct the context")]
        public SqliteDbContext()
        {
        }

        public SqliteDbContext(SqliteConnection connection, SqliteSnapshotHelper snapshotHelper)
        {
            _connection = connection;
            _snapshotHelper = snapshotHelper;
        }

        public ICollection<Person> People => Get<Person>();
        public ICollection<Address> Addresses => Get<Address>();
        public ICollection<Department> Departments => Get<Department>();

        public ICollection<T> Get<T>() where T : class
        {
            return Set<T>().AsCollection();
        }

        public async Task Remove<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var set = Set<T>();
            var records = set.Where(predicate).ToArray();
            set.RemoveRange(records);
            await SaveChangesAsync();
        }

        public async Task CreateSnapshot(FeatureContext featureContext)
        {
            await _snapshotHelper.CreateSnapshot(this, featureContext);
        }

        public async Task RestoreSnapshot(FeatureContext featureContext)
        {
            await _snapshotHelper.RestoreSnapshot(this, featureContext);
        }

        public async Task ResetToInitial(FeatureContext featureContext)
        {
            await _snapshotHelper.ResetToInitial(this, featureContext);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connection ?? GetConnectionForUseWithMigrations());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Person>().HasKey(p => p.Name);
            modelBuilder.Entity<Address>().HasKey(p => p.AddressCode);
            modelBuilder.Entity<Department>().HasKey(p => p.DepartmentCode);
        }

        private static SqliteConnection GetConnectionForUseWithMigrations()
        {
            return new SqliteConnection("Filename=:memory:");
        }
    }
}
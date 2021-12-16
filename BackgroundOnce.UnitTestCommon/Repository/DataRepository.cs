using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BackgroundOnce.UnitTestCommon.Data;
using TechTalk.SpecFlow;

namespace BackgroundOnce.UnitTestCommon.Repository
{
    public class DataRepository : IDataRepository
    {
        private readonly Func<IDatabase> _getDatabase;

        public DataRepository(Func<IDatabase> getDatabase)
        {
            _getDatabase = getDatabase;
        }

        public async Task AddData(ReferenceData referenceData)
        {
            var database = _getDatabase();

            foreach (var person in referenceData.People ?? new List<Person>())
            {
                database.People.Add(person);
            }

            foreach (var address in referenceData.Addresses ?? new List<Address>())
            {
                database.Addresses.Add(address);
            }

            foreach (var department in referenceData.Departments ?? new List<Department>())
            {
                database.Departments.Add(department);
            }

            await database.SaveChangesAsync();
        }

        public async Task UpdateAddress(string code, Address newAddress)
        {
            var database = _getDatabase();
            var currentAddress = database.Addresses.SingleOrDefault(a => a.Name == code);

            if (currentAddress == null)
            {
                throw new InvalidOperationException("Address not found");
            }

            currentAddress.County = newAddress.County;
            currentAddress.House = newAddress.House;
            currentAddress.Street = newAddress.Street;
            await database.SaveChangesAsync();
        }

        public Task<PersonDetails> GetPersonDetails(string name)
        {
            var database = _getDatabase();
            var people = database.People;
            var departments = database.Departments;
            var addresses = database.Addresses;

            var details = (from person in people
                where person.Name == name
                join department in departments on person.DepartmentCode equals department.DepartmentCode
                join address in addresses on department.AddressCode equals address.AddressCode
                select new PersonDetails
                {
                    Name = person.Name,
                    Gender = person.Gender,
                    Department = department.Name,
                    Address = $"{address.House}, {address.Street}, {address.County}"
                }).SingleOrDefault();

            return Task.FromResult(details);
        }

        public int GetCount<T>()
            where T : class
        {
            var database = _getDatabase();
            return database.Get<T>().Count;
        }

        public async Task Remove<T>(Expression<Func<T, bool>> predicate)
            where T : class
        {
            var database = _getDatabase();
            await database.Remove(predicate);
        }

        public IEnumerable<T> Get<T>()
            where T : class
        {
            var database = _getDatabase();
            return database.Get<T>();
        }
    }
}
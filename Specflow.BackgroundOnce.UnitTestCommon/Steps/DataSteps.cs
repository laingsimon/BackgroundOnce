using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Specflow.BackgroundOnce.UnitTestCommon.Context;
using Specflow.BackgroundOnce.UnitTestCommon.Data;
using Specflow.BackgroundOnce.UnitTestCommon.Repository;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace Specflow.BackgroundOnce.UnitTestCommon.Steps
{
    [Binding]
    public abstract class DataSteps
    {
        private readonly RequestContext _requestContext;
        private readonly DataRepositoryFactory _dataRepositoryFactory;
        private readonly DataContext _dataContext;

        public DataSteps(
            DataContext dataContext,
            RequestContext requestContext,
            DataRepositoryFactory dataRepositoryFactory)
        {
            _requestContext = requestContext;
            _dataRepositoryFactory = dataRepositoryFactory;
            _dataContext = dataContext;
        }

        [Given("I am using the (.+) database")]
        public void IAmUsingTheDatabaseType(DatabaseType databaseType)
        {
            _dataContext.DatabaseType = databaseType;
        }

        [Given("there are the following people")]
        public async Task GivenTheTableHasTheFollowingPeople(Table data)
        {
            var repository = _dataRepositoryFactory.GetRepository(_dataContext);
            await repository.AddData(new ReferenceData
            {
                People = data.CreateSet<Person>().ToArray(),
            });
        }

        [Given("there are the following addresses")]
        public async Task GivenTheTableHasTheFollowingAddresses(Table data)
        {
            var repository = _dataRepositoryFactory.GetRepository(_dataContext);
            await repository.AddData(new ReferenceData
            {
                Addresses = data.CreateSet<Address>().ToArray(),
            });
        }

        [Then(@"there are (\d+) people")]
        [Given(@"there are (\d+) people")]
        public void VerifyPeopleCount(int count)
        {
            var repository = _dataRepositoryFactory.GetRepository(_dataContext);
            var actualCount = repository.GetCount<Person>();

            Assert.AreEqual(count, actualCount);
        }

        [Then(@"there are (\d+) addresses")]
        [Given(@"there are (\d+) addresses")]
        public void VerifyAddressCount(int count)
        {
            var repository = _dataRepositoryFactory.GetRepository(_dataContext);
            var actualCount = repository.GetCount<Address>();

            Assert.AreEqual(count, actualCount);
        }

        [Given(@"There are no people under the age of (\d+)")]
        public async Task GivenThereAreNoPeopleUnderTheAgeOf(int age)
        {
            var repository = _dataRepositoryFactory.GetRepository(_dataContext);
            await repository.Remove<Person>(p => p.Age < age);
        }

        [Given(@"There are no people over the age of (\d+)")]
        public async Task GivenThereAreNoPeopleOverTheAgeOf(int age)
        {
            var repository = _dataRepositoryFactory.GetRepository(_dataContext);
            await repository.Remove<Person>(p => p.Age > age);
        }

        [Given(@"There are only addresses in (.+)")]
        public async Task GivenThereAreOnlyAddressesFromCounty(string county)
        {
            var repository = _dataRepositoryFactory.GetRepository(_dataContext);
            await repository.Remove<Address>(a => a.County != county);
        }

        [When("I request all people")]
        public void WhenIRequestAllPeople()
        {
            var repository = _dataRepositoryFactory.GetRepository(_dataContext);
            _requestContext.RetrievedPeople = repository.Get<Person>().ToArray();
        }

        [When("I request all addresses")]
        public void WhenIRequestAllAddresses()
        {
            var repository = _dataRepositoryFactory.GetRepository(_dataContext);
            _requestContext.RetrievedAddresses = repository.Get<Address>().ToArray();
        }

        [Then("I receive the following people")]
        public void ThenIReceiveTheFollowingPeople(Table table)
        {
            var expectedPeople = table.CreateSet<Person>();
            var expectedNames = expectedPeople.Select(p => p.Name).ToArray();
            var actualNames = _requestContext.RetrievedPeople.Select(p => p.Name).ToArray();
            Assert.AreEquivalentTo(expectedNames, actualNames);
        }

        [Then("I receive the following addresses")]
        public void ThenIReceiveTheFollowingAddresses(Table table)
        {
            string FormatAddress(Address address)
            {
                return $"{address.House}, {address.Street}, {address.County}";
            }

            var expectedAddresses = table.CreateSet<Address>().Select(FormatAddress).ToArray();
            var actualAddresses = _requestContext.RetrievedAddresses.Select(FormatAddress).ToArray();

            Assert.AreEquivalentTo(expectedAddresses, actualAddresses);
        }

        [Given("the database engine is started")]
        public void TheDatabaseEngineIsStarted()
        {
            // start the database engine, wait for it to be ready
            // create the schema
            // grant access and permissions
        }

        [Given("standard reference data is created")]
        public void StandardReferenceDataIsCreated()
        {
            // create some data in the 'database' that is standard
            var repository = _dataRepositoryFactory.GetRepository(_dataContext);
            repository.AddData(new ReferenceData
            {
                Addresses = AddAddresses().ToArray(),
                People = AddPeople().ToArray(),
                Departments = AddDepartments().ToArray(),
            });
        }

        [When("I request the details of (.+)")]
        public async Task IRequestTheDetailsOfPerson(string name)
        {
            var repository = _dataRepositoryFactory.GetRepository(_dataContext);
            var details = await repository.GetPersonDetails(name);
            _requestContext.RetrievedPersonDetails = details;
        }

        [Given("I update the address labeled (.+) to")]
        public async Task IUpdateTheAddress(string name, Table newDetails)
        {
            var repository = _dataRepositoryFactory.GetRepository(_dataContext);
            var newAddress = newDetails.CreateInstance<Address>();
            await repository.UpdateAddress(name, newAddress);
        }

        [Then("I receive the following details")]
        public void IReceiveTheFollowingDetails(Table expectedPersonDetails)
        {
            var expected = expectedPersonDetails.CreateInstance<PersonDetails>();

            var actual = _requestContext.RetrievedPersonDetails;
            if (actual == null)
            {
                throw new InvalidOperationException("Person not found");
            }

            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.Gender, actual.Gender);
            Assert.AreEqual(expected.Department, actual.Department);
            Assert.AreEqual(expected.Address, actual.Address);
        }

        private static IEnumerable<Person> AddPeople()
        {
            var person = new Person
            {
                Name = "Joe Blogs",
                Gender = "Male",
                DepartmentCode = "HR",
            };

            yield return person;
        }

        private static IEnumerable<Department> AddDepartments()
        {
            var departments = new Department
            {
                DepartmentCode = "HR",
                Name = "Human Resources",
                AddressCode = "HeadQuarters",
            };

            yield return departments;
        }

        private static IEnumerable<Address> AddAddresses()
        {
            var address = new Address
            {
                AddressCode = "HeadQuarters",
                Name = "Head Quarters",
                House = "Dorothy House",
                Street = "Yellow Brick Road",
                County = "Kansas",
            };

            yield return address;
        }
    }
}
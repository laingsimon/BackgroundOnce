using System;
using System.Linq;
using Specflow.BackgroundOnce.UnitTestCommon.Context;
using Specflow.BackgroundOnce.UnitTestCommon.Data;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace Specflow.BackgroundOnce.UnitTestCommon.Steps
{
    [Binding]
    public abstract class DataSteps
    {
        private readonly RequestContext _requestContext;
        private readonly InMemoryDatabase _database;

        public DataSteps(
            DataContext dataContext,
            RequestContext requestContext)
        {
            _requestContext = requestContext;
            _database = dataContext.Database;
        }

        [Given("there are the following people")]
        public void GivenTheTableHasTheFollowingPeople(Table data)
        {
            var people = data.CreateSet<Person>();
            _database.GetTable<Person>().AddRecords(people);
        }

        [Given("there are the following addresses")]
        public void GivenTheTableHasTheFollowingAddresses(Table data)
        {
            var addresses = data.CreateSet<Address>();
            _database.GetTable<Address>().AddRecords(addresses);
        }

        [Then(@"there are (\d+) people")]
        [Given(@"there are (\d+) people")]
        public void VerifyPeopleCount(int count)
        {
            Assert.AreEqual(count, _database.GetTable<Person>().Count());
        }

        [Then(@"there are (\d+) addresses")]
        [Given(@"there are (\d+) addresses")]
        public void VerifyAddressCount(int count)
        {
            Assert.AreEqual(count, _database.GetTable<Address>().Count());
        }

        [Given(@"There are no people under the age of (\d+)")]
        public void GivenThereAreNoPeopleUnderTheAgeOf(int age)
        {
            _database.GetTable<Person>().RemoveRecords(p => p.Age < age);
        }

        [Given(@"There are no people over the age of (\d+)")]
        public void GivenThereAreNoPeopleOverTheAgeOf(int age)
        {
            _database.GetTable<Person>().RemoveRecords(p => p.Age > age);
        }

        [Given(@"There are only addresses in (.+)")]
        public void GivenThereAreOnlyAddressesFromCounty(string county)
        {
            _database.GetTable<Address>().RemoveRecords(a => a.County != county);
        }

        [When("I request all people")]
        public void WhenIRequestAllPeople()
        {
            _requestContext.RetrievedPeople = _database.GetTable<Person>().ToArray();
        }

        [When("I request all addresses")]
        public void WhenIRequestAllAddresses()
        {
            _requestContext.RetrievedAddresses = _database.GetTable<Address>().ToArray();
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
            AddPeople();
            AddDepartments();
            AddAddresses();
        }

        [When("I request the details of (.+)")]
        public void IRequestTheDetailsOfPerson(string name)
        {
            var people = _database.GetTable<Person>();
            var departments = _database.GetTable<Department>();
            var addresses = _database.GetTable<Address>();

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

            _requestContext.RetrievedPersonDetails = details;
        }

        [Given("I update the address labeled (.+) to")]
        public void IUpdateTheAddress(string name, Table newDetails)
        {
            var newAddress = newDetails.CreateInstance<Address>();
            var currentAddress = _database.GetTable<Address>().SingleOrDefault(a => a.Name == name);

            if (currentAddress == null)
            {
                throw new InvalidOperationException("Address not found");
            }

            currentAddress.County = newAddress.County;
            currentAddress.House = newAddress.House;
            currentAddress.Street = newAddress.Street;
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

        private void AddPeople()
        {
            var person = new Person
            {
                Name = "Joe Blogs",
                Gender = "Male",
                DepartmentCode = "HR",
            };

            _database.GetTable<Person>().AddRecords(new[] { person });
        }

        private void AddDepartments()
        {
            var departments = new Department
            {
                DepartmentCode = "HR",
                Name = "Human Resources",
                AddressCode = "HeadQuarters",
            };

            _database.GetTable<Department>().AddRecords(new[] { departments });
        }

        private void AddAddresses()
        {
            var address = new Address
            {
                AddressCode = "HeadQuarters",
                Name = "Head Quarters",
                House = "Dorothy House",
                Street = "Yellow Brick Road",
                County = "Kansas",
            };

            _database.GetTable<Address>().AddRecords(new[] { address });
        }
    }
}
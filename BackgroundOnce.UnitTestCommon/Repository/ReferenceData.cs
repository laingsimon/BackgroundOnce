using System.Collections.Generic;
using BackgroundOnce.UnitTestCommon.Data;

namespace BackgroundOnce.UnitTestCommon.Repository
{
    public class ReferenceData
    {
        public IReadOnlyCollection<Person> People { get; set; }
        public IReadOnlyCollection<Address> Addresses { get; set; }
        public IReadOnlyCollection<Department> Departments { get; set; }
    }
}
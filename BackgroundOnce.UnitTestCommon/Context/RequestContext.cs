using System.Collections.Generic;
using BackgroundOnce.UnitTestCommon.Data;

namespace BackgroundOnce.UnitTestCommon.Context
{
    public class RequestContext
    {
        public IReadOnlyCollection<Person> RetrievedPeople { get; set; }

        public IReadOnlyCollection<Address> RetrievedAddresses { get; set; }

        public PersonDetails RetrievedPersonDetails { get; set; }
    }
}
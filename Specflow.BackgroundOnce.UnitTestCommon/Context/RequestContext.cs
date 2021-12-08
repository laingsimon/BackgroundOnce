using System.Collections.Generic;
using Specflow.BackgroundOnce.UnitTestCommon.Data;

namespace Specflow.BackgroundOnce.UnitTestCommon.Context
{
    public class RequestContext
    {
        public IReadOnlyCollection<Person> RetrievedPeople { get; set; }

        public IReadOnlyCollection<Address> RetrievedAddresses { get; set; }

        public PersonDetails RetrievedPersonDetails { get; set; }
    }
}
using NUnit.Framework;
using Shouldly;
using System;

namespace MQuery.QueryString.Tests
{
    [TestFixture()]
    public class UtilsTests
    {
        public class Person
        {
            public Location Location { get; set; }
        }

        public class Location
        {
            public string Address { get; set; }
        }

        [Test()]
        public void StringToPropSelectorTest()
        {
            var selector = Utils.StringToPropSelector<Person>("location.address") ;
            var person = new Person { Location = new Location { Address = "abc" } };

            var func = selector.Compile() as Func<Person, string>;
            var address = func(person);

            address.ShouldBe("abc");
        }
    }
}


using FlightNode.Identity.Domain.Entities;
using Xunit;

namespace FlightNode.Identity.UnitTests.Domain.Entities
{
    public class UserTests
    {
        [Fact]
        public void ConfirmGetAndSetForActive()
        {
            var expected = "active";
            var system = new User();

            system.Active = expected;

            var actual = system.Active;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ConfirmGetAndSetPhoneNumber()
        {
            var expected = "(555) 555-5555";
            var system = new User();

            system.MobilePhoneNumber = expected;

            var actual = system.MobilePhoneNumber;

            Assert.Equal(expected, actual);
        }
    }
}

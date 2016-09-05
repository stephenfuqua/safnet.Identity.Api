using FlightNode.Identity.Domain.Logic;
using System;
using Xunit;

namespace FlightNode.Identity.UnitTests.Domain.Managers.UserDomainManagerTests
{

    public class ConstructorBehavior : Fixture
    {

        [Fact]
        public void ConfirmWithValidArgument()
        {
            Assert.NotNull(BuildSystem());
        }

        [Fact]
        public void ConfirmThatNullFirstArgumentIsNotAllowed()
        {
            Assert.Throws<ArgumentNullException>(() => new UserDomainManager(null, EmailFactoryMock.Object));
        }

        [Fact]
        public void ConfirmThatNullSecondArgumentIsNotAllowed()
        {
            Assert.Throws<ArgumentNullException>(() => new UserDomainManager(MockUserManager.Object, null));
        }
    }


}

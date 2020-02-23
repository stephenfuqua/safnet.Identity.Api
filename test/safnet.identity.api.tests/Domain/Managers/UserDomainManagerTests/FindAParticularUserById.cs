using FlightNode.Identity.Domain.Entities;
using FlightNode.Identity.Services.Models;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FlightNode.Identity.UnitTests.Domain.Managers.UserDomainManagerTests
{


    public class FindAParticularUserById : Fixture
    {

        [Fact]
        public void ConfirmWhenUserDoesNotExist()
        {
            var id = 1232;

            // TODO: confirm that EF is returning a non-null result

            MockUserManager.Setup(x => x.FindByIdAsync(It.Is<int>(y => y == id)))
                .ReturnsAsync(new User());

            var actual = BuildSystem().FindById(id);

            Assert.NotEqual(id, actual.UserId);
        }

        public class UserDoesExist : Fixture
        {

            private UserModel RunTheTest()
            {
                var user = new User
                {
                    Active = ActiveString,
                    MobilePhoneNumber = SecondaryPhoneNumber,
                    Id = UserId,
                    Email = Email,
                    PhoneNumber = PrimaryPhoneNumber,
                    UserName = UserName,
                    FamilyName = FamilyName,
                    GivenName = GivenName,
                    LockoutEnabled = LockedOut,
                    County = County,
                    City = City,
                    State = State,
                    ZipCode = ZipCode,
                    MailingAddress = Mailing
                };

                MockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<int>()))
                               .ReturnsAsync(user);
                MockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<int>()))
                    .ReturnsAsync(new List<string>() { RoleAdministrator });

                return BuildSystem().FindById(UserId);
            }

            [Fact]
            public void QueriesUsersById()
            {
                RunTheTest();

                MockUserManager.Verify(x => x.FindByIdAsync(It.Is<int>(y => y == UserId)));
            }

            [Fact]
            public void QueriesRolesById()
            {
                RunTheTest();

                MockUserManager.Verify(x => x.GetRolesAsync(It.Is<int>(y => y == UserId)));
            }

            [Fact]
            public void ConfirmMapsCounty()
            {
                Assert.Equal(County, RunTheTest().County);
            }

            [Fact]
            public void ConfirmMapsCity()
            {
                Assert.Equal(City, RunTheTest().City);
            }

            [Fact]
            public void ConfirmMapsState()
            {
                Assert.Equal(State, RunTheTest().State);
            }

            [Fact]
            public void ConfirmMapsZipCode()
            {
                Assert.Equal(ZipCode, RunTheTest().ZipCode);
            }

            [Fact]
            public void ConfirmMapsMailingAddress()
            {
                Assert.Equal(Mailing, RunTheTest().MailingAddress);
            }

            [Fact]
            public void ConfirmMapsLockout()
            {
                Assert.Equal(LockedOut, RunTheTest().LockedOut);
            }

            [Fact]
            public void ConfirmMapsRole()
            {
                Assert.Equal(RoleAdministrator, ((RoleEnum)RunTheTest().Role).ToString());
            }

            [Fact]
            public void ConfirmPasswordIsEmpty()
            {
                Assert.Equal(string.Empty, RunTheTest().Password);
            }

            [Fact]
            public void ConfirmMobileNumberIsMapped()
            {
                Assert.Equal(SecondaryPhoneNumber, RunTheTest().SecondaryPhoneNumber);
            }

            [Fact]
            public void ConfirmPhoneNumberIsMapped()
            {
                Assert.Equal(PrimaryPhoneNumber, RunTheTest().PrimaryPhoneNumber);
            }

            [Fact]
            public void ConfirmEmailIsMapped()
            {
                Assert.Equal(Email, RunTheTest().Email);
            }

            [Fact]
            public void ConfirmUserNameIsMapped()
            {
                Assert.Equal(UserName, RunTheTest().UserName);
            }

            [Fact]
            public void ConfirmUserIdIsMapped()
            {
                Assert.Equal(UserId, RunTheTest().UserId);
            }

            [Fact]
            public void ConfirmFamilylNameIsMapped()
            {
                Assert.Equal(FamilyName, RunTheTest().FamilyName);
            }

            [Fact]
            public void ConfirmGivenNameIsMapped()
            {
                Assert.Equal(GivenName, RunTheTest().GivenName);
            }
        }

    }
}

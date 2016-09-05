using FlightNode.Identity.Domain.Entities;
using FlightNode.Identity.Services.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FlightNode.Identity.UnitTests.Domain.Managers.UserDomainManagerTests
{


    public class FindAllUsers : Fixture
    {

        [Fact]
        public void ConfirmEmptyListWhenThereAreNoUsersInTheSystem()
        {
            MockUserManager.SetupGet(x => x.Users)
                .Returns(new List<User>().AsQueryable());

            var actual = BuildSystem().FindAll();

            Assert.Equal(0, actual.Count());
        }

        public class NoActiveUsers : Fixture
        {
            const int userId = 12312;
            const string email = "asd@asdfasd.com";
            const string phoneNumber = "(555) 555-5555";
            const string userName = "asdfasd";
            const string mobileNumber = "(555) 555-5554";
            const bool lockedOut = true;
            const string active = "inactive";

            private IEnumerable<UserModel> RunTheTest()
            {
                var user = new User
                {
                    Active = active,
                    MobilePhoneNumber = mobileNumber,
                    Id = userId,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    UserName = userName,
                    LockoutEnabled = lockedOut
                };

                MockUserManager.SetupGet(x => x.Users)
                    .Returns(new List<User>() {
                           user
                    }.AsQueryable());

                return BuildSystem().FindAll();
            }

            [Fact]
            public void ConfirmResultCount()
            {
                var results = RunTheTest();
                Assert.Equal(0, results.Count());
            }

        }

        public class SingleActiveUser : Fixture
        {
            const int userId = 12312;
            const string email = "asd@asdfasd.com";
            const string phoneNumber = "(555) 555-5555";
            const string userName = "asdfasd";
            const string mobileNumber = "(555) 555-5554";
            const string active = "active";
            const string familyName = "last";
            const string givenName = "first";
            const bool lockedOut = true;

            private IEnumerable<UserModel> RunTheTest()
            {
                var user = new User
                {
                    Active = active,
                    MobilePhoneNumber = mobileNumber,
                    Id = userId,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    UserName = userName,
                    FamilyName = familyName,
                    GivenName = givenName,
                    LockoutEnabled = lockedOut
                };

                MockUserManager.SetupGet(x => x.Users)
                    .Returns(new List<User>() {
                           user
                    }.AsQueryable());

                return BuildSystem().FindAll();
            }

            [Fact]
            public void ConfirmResultCount()
            {
                var results = RunTheTest();
                Assert.Equal(1, results.Count());
            }

            [Fact]
            public void ConfirmLockoutIsMapped()
            {
                Assert.Equal(lockedOut, RunTheTest().First().LockedOut);
            }


            [Fact]
            public void ConfirmMobileNumberIsMapped()
            {
                Assert.Equal(mobileNumber, RunTheTest().First().SecondaryPhoneNumber);
            }

            [Fact]
            public void ConfirmPhoneNumberIsMapped()
            {
                Assert.Equal(phoneNumber, RunTheTest().First().PrimaryPhoneNumber);
            }

            [Fact]
            public void ConfirmEmailIsMapped()
            {
                Assert.Equal(email, RunTheTest().First().Email);
            }

            [Fact]
            public void ConfirmUserNameIsMapped()
            {
                Assert.Equal(userName, RunTheTest().First().UserName);
            }

            [Fact]
            public void ConfirmUserIdIsMapped()
            {
                Assert.Equal(userId, RunTheTest().First().UserId);
            }

            [Fact]
            public void ConfirmFamilyNameIsMapped()
            {
                Assert.Equal(familyName, RunTheTest().First().FamilyName);
            }

            [Fact]
            public void ConfirmGivenNameIsMapped()
            {
                Assert.Equal(givenName, RunTheTest().First().GivenName);
            }
        }

        public class TwoActiveUsers : Fixture
        {
            const int userId = 12312;
            const string email = "asd@asdfasd.com";
            const string phoneNumber = "(555) 555-5555";
            const string userName = "asdfasd";
            const string mobileNumber = "(555) 555-5554";
            const string active = "active";
            const string familyName = "last";
            const string givenName = "first";

            private IEnumerable<UserModel> RunTheTest()
            {
                var user = new User
                {
                    Active = active,
                    MobilePhoneNumber = mobileNumber,
                    Id = userId,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    UserName = userName,
                    FamilyName = familyName,
                    GivenName = givenName
                };

                MockUserManager.SetupGet(x => x.Users)
                    .Returns(new List<User>() {
                            new User { Active = active },
                           user
                    }.AsQueryable());

                return BuildSystem().FindAll();
            }

            [Fact]
            public void ConfirmResultCount()
            {
                var results = RunTheTest();
                Assert.Equal(2, results.Count());
            }

            [Fact]
            public void ConfirmMobileNumberIsMappedForSecondUser()
            {
                Assert.Equal(mobileNumber, RunTheTest().Skip(1).First().SecondaryPhoneNumber);
            }

            [Fact]
            public void ConfirmPhoneNumberIsMappedForSecondUser()
            {
                Assert.Equal(phoneNumber, RunTheTest().Skip(1).First().PrimaryPhoneNumber);
            }

            [Fact]
            public void ConfirmEmailIsMappedForSecondUser()
            {
                Assert.Equal(email, RunTheTest().Skip(1).First().Email);
            }

            [Fact]
            public void ConfirmUserNameIsMappedForSecondUser()
            {
                Assert.Equal(userName, RunTheTest().Skip(1).First().UserName);
            }

            [Fact]
            public void ConfirmUserIdIsMappedForSecondUser()
            {
                Assert.Equal(userId, RunTheTest().Skip(1).First().UserId);
            }

            [Fact]
            public void ConfirmGivenNameIsMappedForSecondUser()
            {
                Assert.Equal(givenName, RunTheTest().Skip(1).First().GivenName);
            }

            [Fact]
            public void ConfirmFamilyNameIsMappedForSecondUser()
            {
                Assert.Equal(familyName, RunTheTest().Skip(1).First().FamilyName);
            }
        }
    }
}

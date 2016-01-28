﻿using FlightNode.Common.Exceptions;
using FlightNode.Identity.Domain.Entities;
using FlightNode.Identity.Domain.Interfaces;
using FlightNode.Identity.Domain.Logic;
using FlightNode.Identity.Infrastructure.Persistence;
using FlightNode.Identity.Services.Models;
using Microsoft.AspNet.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FlightNode.Identity.UnitTests.Domain.Logic
{


    public class UserDomainManagerTests
    {
        public class Fixture : IDisposable
        {
            protected Mock<Identity.Domain.Interfaces.IUserPersistence> mockUserManager = new Mock<Identity.Domain.Interfaces.IUserPersistence>(MockBehavior.Strict);

            protected UserDomainManager BuildSystem()
            {
                return new UserDomainManager(mockUserManager.Object);
            }

            public void Dispose()
            {
                mockUserManager.VerifyAll();
            }

        }

        public class ConstructorBehavior : Fixture
        {

            [Fact]
            public void ConfirmWithValidArgument()
            {
                Assert.NotNull(BuildSystem());
            }

            [Fact]
            public void ConfirmThatNullArgumentIsNotAllowed()
            {
                Assert.Throws<ArgumentNullException>(() => new UserDomainManager(null));
            }
        }
        
        public class CreateUser : Fixture
        {
            [Fact]
            public void NullObjectNotAllowed()
            {
                Assert.Throws<ArgumentNullException>(() =>
                {
                    BuildSystem().Create(null);
                });
            }

            const string givenName = "José";
            const string familyName = "Jalapeño";
            const string primaryPhoneNumber = "(512) 555-2391";
            const string secondaryPhoneNumber = "(651) 234-2349";
            const string userName = "josej";
            const string password = "bien gracias, y tú?";
            const string email = "jose@jalapenos.com";
            const int userId = 2342;
            const bool lockedOut = true;
            const string active = "pending";

            private UserModel RunTest()
            {
                var input = new UserModel
                {
                    Email = email,
                    FamilyName = familyName,
                    GivenName = givenName,
                    Password = password,
                    PrimaryPhoneNumber = primaryPhoneNumber,
                    SecondaryPhoneNumber = secondaryPhoneNumber,
                    UserName = userName,
                    Roles = new List<string>(),
                    LockedOut = lockedOut,
                    Active = active
                };

                return BuildSystem().Create(input);
            }

            [Fact]
            public void ConfirmUserIdIsSetInReturnObject()
            {
                mockUserManager.Setup(x => x.AddToRolesAsync(It.Is<int>(y => y == userId), It.Is<string[]>(y => y.Length == 0)))
                    .Returns(Task.Run(() => SuccessResult.Create()));

                mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.Is<string>(y => y == password)))
                    .Callback((User actual, string p) =>
                    {
                        Assert.Equal(givenName, actual.GivenName);
                        Assert.Equal(familyName, actual.FamilyName);
                        Assert.Equal(primaryPhoneNumber, actual.PhoneNumber);
                        Assert.Equal(secondaryPhoneNumber, actual.MobilePhoneNumber);
                        Assert.Equal(userName, actual.UserName);
                        Assert.Equal(email, actual.Email);
                        Assert.Equal(lockedOut, actual.LockoutEnabled);
                        Assert.Equal(active, actual.Active);
                    })
                    .Returns((User actual, string p) =>
                    {
                        actual.Id = userId;

                        return Task.Run(() => SuccessResult.Create());
                    });

                Assert.Equal(userId, RunTest().UserId);
            }


            [Fact]
            public void ConfirmExceptionIfRolesDoNotSave()
            {
                mockUserManager.Setup(x => x.AddToRolesAsync(It.Is<int>(y => y == userId), It.Is<string[]>(y => y.Length == 0)))
                    .Returns(Task.Run(() => SuccessResult.Failed("asdfasd")));

                mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.Is<string>(y => y == password)))
                    .Returns((User actual, string p) =>
                    {
                        actual.Id = userId;

                        return Task.Run(() => SuccessResult.Create());
                    });
                
                Assert.Throws<UserException>(() => RunTest());
            }

            [Fact]
            public void ConfirmErrorHandling()
            {
                mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.Is<string>(y => y == password)))
                    .Callback((User actual, string p) =>
                    {
                        Assert.Equal(givenName, actual.GivenName);
                        Assert.Equal(familyName, actual.FamilyName);
                        Assert.Equal(primaryPhoneNumber, actual.PhoneNumber);
                        Assert.Equal(secondaryPhoneNumber, actual.MobilePhoneNumber);
                        Assert.Equal(userName, actual.UserName);
                        Assert.Equal(email, actual.Email);
                        Assert.Equal(lockedOut, actual.LockoutEnabled);
                    })
                    .Returns((User actual, string p) =>
                    {
                        actual.Id = userId;

                        return Task.Run(() => new IdentityResult(new[] { "something bad happened" }));
                    });


                Assert.Throws<UserException>(() => RunTest().UserId);
            }
        }


        public class UpdateUser : Fixture
        {
            [Fact]
            public void NullObjectNotAllowed()
            {
                Assert.Throws<ArgumentNullException>(() =>
                {
                    BuildSystem().Update(null);
                });
            }

            const string givenName = "José";
            const string familyName = "Jalapeño";
            const string primaryPhoneNumber = "(512) 555-2391";
            const string secondaryPhoneNumber = "(651) 234-2349";
            const string userName = "josej";
            const string password = "bien gracias, y tú?";
            const string email = "jose@jalapenos.com";
            const int userId = 2342;
            const string oldRole = "Old";
            const string newRole = "new";
            const bool lockedOut = true;
            const string active = "pending";

            private void RunTest()
            {
                var input = new UserModel
                {
                    Email = email,
                    FamilyName = familyName,
                    GivenName = givenName,
                    Password = password,
                    PrimaryPhoneNumber = primaryPhoneNumber,
                    SecondaryPhoneNumber = secondaryPhoneNumber,
                    UserName = userName,
                    UserId = userId,
                    Roles = new List<string>() {  newRole },
                    LockedOut = lockedOut,
                    Active = active
                };

                BuildSystem().Update(input);
            }

            [Fact]
            public void ConfirmUserIdIsSet()
            {
                // Must retrieve the user first
                mockUserManager.Setup(x => x.FindByIdAsync(It.Is<int>(y => y == userId)))
                    .Returns(Task.Run(() => new User()));

                // Then update the user
                mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                    .Callback((User actual) =>
                    {
                        Assert.Equal(givenName, actual.GivenName);
                        Assert.Equal(familyName, actual.FamilyName);
                        Assert.Equal(primaryPhoneNumber, actual.PhoneNumber);
                        Assert.Equal(secondaryPhoneNumber, actual.MobilePhoneNumber);
                        Assert.Equal(userName, actual.UserName);
                        Assert.Equal(email, actual.Email);
                        Assert.Equal(active, actual.Active);
                        Assert.Equal(lockedOut, actual.LockoutEnabled);
                    })
                    .Returns((User actual) =>
                    {
                        actual.Id = userId;

                        return Task.Run(() => SuccessResult.Create());
                    });

                // For roles, must retrieve existing roles
                mockUserManager.Setup(x => x.GetRolesAsync(It.Is<int>(y => y == userId)))
                    .ReturnsAsync(new List<string>() { oldRole });

                // Then remove them
                mockUserManager.Setup(x => x.RemoveFromRolesAsync(It.Is<int>(y => y == userId),
                                                                   It.Is<string[]>(y => y[0] == oldRole)))
                            .ReturnsAsync(SuccessResult.Create());

                // And finally save new ones
                mockUserManager.Setup(x => x.AddToRolesAsync(It.Is<int>(y => y == userId),
                                                                   It.Is<string[]>(y => y[0] == newRole)))
                            .ReturnsAsync(SuccessResult.Create());

                // Finally we can run the test
                RunTest();
            }

            [Fact]
            public void ConfirmHandlingOfErrorWhenSavingUpdatedUserRecord()
            {
                mockUserManager.Setup(x => x.FindByIdAsync(It.Is<int>(y => y == userId)))
                       .Returns(Task.Run(() => new User()));


                mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                    .Returns((User actual) =>
                    {
                        actual.Id = userId;

                        return Task.Run(() => new IdentityResult(new[] { "something bad happened" }));
                    });

                Assert.Throws<UserException>(() => RunTest());
            }

            [Fact]
            public void ConfirmHandlingOfErrorWhenRemovingExistingRoles()
            {
                const string badStuffHappened = "Bad stuff happened";

                // Must retrieve the user first
                mockUserManager.Setup(x => x.FindByIdAsync(It.Is<int>(y => y == userId)))
                    .Returns(Task.Run(() => new User()));

                // Then update the user
                mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                    .Callback((User actual) =>
                    {
                        Assert.Equal(givenName, actual.GivenName);
                        Assert.Equal(familyName, actual.FamilyName);
                        Assert.Equal(primaryPhoneNumber, actual.PhoneNumber);
                        Assert.Equal(secondaryPhoneNumber, actual.MobilePhoneNumber);
                        Assert.Equal(userName, actual.UserName);
                        Assert.Equal(email, actual.Email);
                        Assert.Equal(active, actual.Active);
                        Assert.Equal(lockedOut, actual.LockoutEnabled);
                    })
                    .Returns((User actual) =>
                    {
                        actual.Id = userId;

                        return Task.Run(() => SuccessResult.Create());
                    });

                // For roles, must retrieve existing roles
                mockUserManager.Setup(x => x.GetRolesAsync(It.Is<int>(y => y == userId)))
                    .ReturnsAsync(new List<string>() { oldRole });

                // Then remove them
                mockUserManager.Setup(x => x.RemoveFromRolesAsync(It.Is<int>(y => y == userId),
                                                                   It.Is<string[]>(y => y[0] == oldRole)))
                            .ReturnsAsync(SuccessResult.Failed(badStuffHappened));


                // Finally we can run the test
                Assert.Throws<UserException>(() => RunTest());
            }

            [Fact]
            public void ConfirmHandlingOfErrorWhenRemovingAddingNewRoles()
            {
                const string badStuffHappened = "Bad stuff happened";

                // Must retrieve the user first
                mockUserManager.Setup(x => x.FindByIdAsync(It.Is<int>(y => y == userId)))
                    .Returns(Task.Run(() => new User()));

                // Then update the user
                mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                    .Callback((User actual) =>
                    {
                        Assert.Equal(givenName, actual.GivenName);
                        Assert.Equal(familyName, actual.FamilyName);
                        Assert.Equal(primaryPhoneNumber, actual.PhoneNumber);
                        Assert.Equal(secondaryPhoneNumber, actual.MobilePhoneNumber);
                        Assert.Equal(userName, actual.UserName);
                        Assert.Equal(email, actual.Email);
                        Assert.Equal(active, actual.Active);
                    })
                    .Returns((User actual) =>
                    {
                        actual.Id = userId;

                        return Task.Run(() => SuccessResult.Create());
                    });

                // For roles, must retrieve existing roles
                mockUserManager.Setup(x => x.GetRolesAsync(It.Is<int>(y => y == userId)))
                    .ReturnsAsync(new List<string>() { oldRole });

                // Then remove them
                mockUserManager.Setup(x => x.RemoveFromRolesAsync(It.Is<int>(y => y == userId),
                                                                   It.Is<string[]>(y => y[0] == oldRole)))
                            .ReturnsAsync(SuccessResult.Create());

                // And finally save new ones
                mockUserManager.Setup(x => x.AddToRolesAsync(It.Is<int>(y => y == userId),
                                                                   It.Is<string[]>(y => y[0] == newRole)))
                            .ReturnsAsync(SuccessResult.Failed(badStuffHappened));


                // Finally we can run the test
                Assert.Throws<UserException>(() => RunTest());
            }
        }

        public class FindAllUsers : Fixture
        {

            [Fact]
            public void ConfirmEmptyListWhenThereAreNoUsersInTheSystem()
            {
                mockUserManager.SetupGet(x => x.Users)
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

                    mockUserManager.SetupGet(x => x.Users)
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

                    mockUserManager.SetupGet(x => x.Users)
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

                    mockUserManager.SetupGet(x => x.Users)
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

        public class FindAParticularUserById : Fixture
        {

            [Fact]
            public void ConfirmWhenUserDoesNotExist()
            {
                var id = 1232;

                // TODO: confirm that EF is returning a non-null result

                mockUserManager.Setup(x => x.FindByIdAsync(It.Is<int>(y => y == id)))
                    .ReturnsAsync(new User());

                var actual = BuildSystem().FindById(id);

                Assert.NotEqual(id, actual.UserId);
            }

            public class UserDoesExist : Fixture
            {
                const int userId = 12312;
                const string email = "asd@asdfasd.com";
                const string phoneNumber = "(555) 555-5555";
                const string userName = "asdfasd";
                const string mobileNumber = "(555) 555-5554";
                const string active = "active";
                const string familyName = "last";
                const string givenName = "first";
                const string role = "Administrator";
                const bool lockedOut = true;

                private UserModel RunTheTest()
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

                    mockUserManager.Setup(x => x.FindByIdAsync(It.Is<int>(y => y == userId)))
                                   .ReturnsAsync(user);
                    mockUserManager.Setup(x => x.GetRolesAsync(It.Is<int>(y => y == userId)))
                        .ReturnsAsync(new List<string>() { role });

                    return BuildSystem().FindById(userId);
                }

                [Fact]
                public void ConfirmMapsLockout()
                {
                    Assert.Equal(lockedOut, RunTheTest().LockedOut);
                }

                [Fact]
                public void ConfirmMapsRole()
                {
                    Assert.Equal(role, RunTheTest().Roles.First());
                }

                [Fact]
                public void ConfirmPasswordIsEmpty()
                {
                    Assert.Equal(string.Empty, RunTheTest().Password);
                }

                [Fact]
                public void ConfirmMobileNumberIsMapped()
                {
                    Assert.Equal(mobileNumber, RunTheTest().SecondaryPhoneNumber);
                }

                [Fact]
                public void ConfirmPhoneNumberIsMapped()
                {
                    Assert.Equal(phoneNumber, RunTheTest().PrimaryPhoneNumber);
                }

                [Fact]
                public void ConfirmEmailIsMapped()
                {
                    Assert.Equal(email, RunTheTest().Email);
                }

                [Fact]
                public void ConfirmUserNameIsMapped()
                {
                    Assert.Equal(userName, RunTheTest().UserName);
                }

                [Fact]
                public void ConfirmUserIdIsMapped()
                {
                    Assert.Equal(userId, RunTheTest().UserId);
                }

                [Fact]
                public void ConfirmFamilylNameIsMapped()
                {
                    Assert.Equal(familyName, RunTheTest().FamilyName);
                }

                [Fact]
                public void ConfirmGivenNameIsMapped()
                {
                    Assert.Equal(givenName, RunTheTest().GivenName);
                }
            }

        }
    }

    public class SuccessResult : IdentityResult
    {
        public SuccessResult() : base(true) { }

        internal static IdentityResult Create()
        {
            return new SuccessResult();
        }
    }
}

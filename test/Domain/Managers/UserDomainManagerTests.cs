using FlightNode.Common.Exceptions;
using FlightNode.Common.UnitTests;
using FlightNode.Common.Utility;
using FlightNode.Identity.Domain.Entities;
using FlightNode.Identity.Domain.Interfaces;
using FlightNode.Identity.Domain.Logic;
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
            protected const string GivenName = "José";
            protected const string FamilyName = "Jalapeño";
            protected const string PrimaryPhoneNumber = "(512) 555-2391";
            protected const string SecondaryPhoneNumber = "(651) 234-2349";
            protected const string UserName = "josej";
            protected const string Password = "bien gracias, y tú?";
            protected const string Email = "jose@jalapenos.com";
            protected const int UserId = 2342;
            protected const bool LockedOut = true;
            protected const bool Active = true;
            protected const string ActiveString = "active";
            protected const string PendingString = "pending";
            protected const string County = "County";
            protected const string Mailing = "3334 Park Ave";
            protected const string City = "Ctyyy";
            protected const string State = "TX";
            protected const string ZipCode = "23423";
            protected const string OldRole = "Old";
            protected const string NewRole = "new";
            protected const string RoleAdministrator = "Administrator";

            protected MockRepository mockRepository = new MockRepository(MockBehavior.Strict);
            protected Mock<IUserPersistence> mockUserManager;
            protected Mock<IEmailFactory> emailFactoryMock;

            public Fixture()
            {
                mockUserManager = mockRepository.Create<IUserPersistence>();
                emailFactoryMock = mockRepository.Create<IEmailFactory>();
            }


            protected UserDomainManager BuildSystem()
            {
                return new UserDomainManager(mockUserManager.Object, emailFactoryMock.Object);
            }

            public void Dispose()
            {
                mockUserManager.VerifyAll();
            }


            protected Mock<IEmailNotifier> ExpectToSendEmail()
            {
                var notifierMock = this.mockRepository.Create<IEmailNotifier>();
                notifierMock.Setup(x => x.SendAsync(It.IsAny<NotificationModel>()));


                this.emailFactoryMock.Setup(x => x.CreateNotifier())
                    .Returns(notifierMock.Object);

                return notifierMock;
            }

            public static Task<IdentityResult> CreateSuccessResult()
            {
                return Task.Run(() => SuccessResult.Create());
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
            public void ConfirmThatNullFirstArgumentIsNotAllowed()
            {
                Assert.Throws<ArgumentNullException>(() => new UserDomainManager(null, emailFactoryMock.Object));
            }

            [Fact]
            public void ConfirmThatNullSecondArgumentIsNotAllowed()
            {
                Assert.Throws<ArgumentNullException>(() => new UserDomainManager(mockUserManager.Object, null));
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

            private UserModel RunTest()
            {
                var input = new UserModel
                {
                    Email = Email,
                    FamilyName = FamilyName,
                    GivenName = GivenName,
                    Password = Password,
                    PrimaryPhoneNumber = PrimaryPhoneNumber,
                    SecondaryPhoneNumber = SecondaryPhoneNumber,
                    UserName = UserName,
                    LockedOut = LockedOut,
                    Active = Active,
                    County = County,
                    MailingAddress = Mailing,
                    City = City,
                    State = State,
                    ZipCode = ZipCode
                };

                return BuildSystem().Create(input);
            }

            [Fact]
            public void ConfirmUserIdIsSetInReturnObject()
            {
                SetupMockExpectationsForSuccessfulSaveOfUserAndRoles();

                Assert.Equal(UserId, RunTest().UserId);
            }

            private void SetupMockExpectationsForSuccessfulSaveOfUserAndRoles()
            {
                ExpectSuccessfulSaveOfRoles();

                ExpectSuccessfulSaveOfUser();
            }

            private void ExpectSuccessfulSaveOfUser()
            {
                mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                                    .Returns((User actual, string p) =>
                                    {
                                        actual.Id = UserId;

                                        return CreateSuccessResult();
                                    });
            }

            private void ExpectSuccessfulSaveOfRoles()
            {
                mockUserManager.Setup(x => x.AddToRolesAsync(It.IsAny<int>(), It.IsAny<string[]>()))
                                                    .Returns(Task.Run(() => SuccessResult.Create()));
            }

            [Fact]
            public void ConfirmUserIdIsMappedToRole()
            {
                SetupMockExpectationsForSuccessfulSaveOfUserAndRoles();

                RunTest();

                mockUserManager.Verify(x => x.AddToRolesAsync(It.Is<int>(y => y == UserId), It.IsAny<string[]>()));
            }

            [Fact]
            public void ConfirmThereAreNoRoles()
            {
                SetupMockExpectationsForSuccessfulSaveOfUserAndRoles();

                RunTest();

                mockUserManager.Verify(x => x.AddToRolesAsync(It.IsAny<int>(), It.Is<string[]>(y => y.Length == 0)));
            }

            [Fact]
            public void ConfirmPasswordIsMapped()
            {
                SetupMockExpectationsForSuccessfulSaveOfUserAndRoles();

                RunTest();

                mockUserManager.Verify(x => x.CreateAsync(It.IsAny<User>(), It.Is<string>(y => y == Password)));
            }

            [Fact]
            public void ConfirmGivenNameIsMapped()
            {
                ConfirmFieldMapping(x => x.GivenName == GivenName);
            }

            [Fact]
            public void ConfirmFamilyNameIsMapped()
            {
                ConfirmFieldMapping(x => x.FamilyName == FamilyName);
            }

            [Fact]
            public void ConfirmPrimaryPhoneIsMapped()
            {
                ConfirmFieldMapping(x => x.PhoneNumber == PrimaryPhoneNumber);
            }

            [Fact]
            public void ConfirmSecondaryPhoneIsMapped()
            {
                ConfirmFieldMapping(x => x.MobilePhoneNumber == SecondaryPhoneNumber);
            }

            [Fact]
            public void ConfirmUserNameIsMapped()
            {
                ConfirmFieldMapping(x => x.UserName == UserName);
            }

            [Fact]
            public void ConfirmEmailIsMapped()
            {
                ConfirmFieldMapping(x => x.Email == Email);
            }

            [Fact]
            public void ConfirmLockedOutIsMapped()
            {
                ConfirmFieldMapping(x => x.LockoutEnabled == LockedOut);
            }

            [Fact]
            public void ConfirmActiveIsMapped()
            {
                ConfirmFieldMapping(x => x.Active == ActiveString);
            }

            [Fact]
            public void ConfirmCountyIsMapped()
            {
                ConfirmFieldMapping(x => x.County == County);
            }

            [Fact]
            public void ConfirmMailingAddressIsMapped()
            {
                ConfirmFieldMapping(x => x.MailingAddress == Mailing);
            }


            [Fact]
            public void ConfirmCityMapped()
            {
                ConfirmFieldMapping(x => x.City == City);
            }


            [Fact]
            public void ConfirmStateIsMapped()
            {
                ConfirmFieldMapping(x => x.State == State);
            }


            [Fact]
            public void ConfirmZipCodeIsMapped()
            {
                ConfirmFieldMapping(x => x.ZipCode == ZipCode);
            }


            private void ConfirmFieldMapping(Func<User, bool> assert)
            {

                SetupMockExpectationsForSuccessfulSaveOfUserAndRoles();

                RunTest();

                mockUserManager.Verify(x => x.CreateAsync(It.Is<User>(y => assert(y)), It.IsAny<string>()));
            }


            [Fact]
            public void ConfirmExceptionIfRolesDoNotSave()
            {
                mockUserManager.Setup(x => x.AddToRolesAsync(It.IsAny<int>(), It.IsAny<string[]>()))
                    .Returns(Task.Run(() => IdentityResult.Failed("asdfasd")));

                ExpectSuccessfulSaveOfUser();

                Assert.Throws<UserException>(() => RunTest());
            }

            [Fact]
            public void ConfirmErrorHandling()
            {
                mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                    .Returns((User actual, string p) =>
                    {
                        actual.Id = UserId;

                        return Task.Run(() => new IdentityResult(new[] { "something bad happened" }));
                    });


                Assert.Throws<UserException>(() => RunTest().UserId);
            }
        }

        public class CreatePendingUser : Fixture
        {
            [Fact]
            public void NullObjectNotAllowed()
            {
                Assert.Throws<ArgumentNullException>(() =>
                {
                    BuildSystem().Create(null);
                });
            }

            private void SetupMockExpectationsForSuccessfulSaveOfUserAndRoles()
            {
                ExpectToSendEmail();

                ExpectSuccessfulSaveOfRoles();

                ExpectSuccessfulSaveOfUser();
            }

            private void ExpectSuccessfulSaveOfUser()
            {
                mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                                    .Returns((User actual, string p) =>
                                    {
                                        actual.Id = UserId;

                                        return CreateSuccessResult();
                                    });
            }

            private void ExpectSuccessfulSaveOfRoles()
            {
                mockUserManager.Setup(x => x.AddToRolesAsync(It.IsAny<int>(), It.IsAny<string[]>()))
                                                    .Returns(Task.Run(() => SuccessResult.Create()));
            }

            private void ConfirmFieldMapping(Func<User, bool> assert)
            {
                ExpectToSendEmail();
                SetupMockExpectationsForSuccessfulSaveOfUserAndRoles();

                RunTest();

                mockUserManager.Verify(x => x.CreateAsync(It.Is<User>(y => assert(y)), It.IsAny<string>()));
            }


            private UserModel RunTest()
            {

                //
                // Arrange
                var input = new UserModel
                {
                    Email = Email,
                    FamilyName = FamilyName,
                    GivenName = GivenName,
                    Password = Password,
                    PrimaryPhoneNumber = PrimaryPhoneNumber,
                    SecondaryPhoneNumber = SecondaryPhoneNumber,
                    UserName = UserName,
                    County = County,
                    City = City,
                    MailingAddress = Mailing,
                    State = State,
                    ZipCode = ZipCode
                };

                //
                // Act
                return BuildSystem().CreatePending(input);
            }

            [Fact]
            public void ConfirmSendsEmailToNewUserWithCorrectNameAndEmail()
            {
                SetupMockExpectationsForSuccessfulSaveOfUserAndRoles();
                var emailFactoryMock = ExpectToSendEmail();

                RunTest();

                VerifyEmailParameter(emailFactoryMock, y => y.To == GivenName + " " + FamilyName + " <" + Email + ">");
            }

            [Fact]
            public void ConfirmSendsEmailToNewUserWithCorrectSubject()
            {
                SetupMockExpectationsForSuccessfulSaveOfUserAndRoles();
                var emailFactoryMock = ExpectToSendEmail();

                RunTest();

                VerifyEmailParameter(emailFactoryMock, y => y.Subject == UserDomainManager.PendingUserEmailSubject);
            }

            [Fact]
            public void ConfirmSendsEmailToNewUserWithCorrectMessage()
            {
                SetupMockExpectationsForSuccessfulSaveOfUserAndRoles();
                var emailFactoryMock = ExpectToSendEmail();
                var expected = @"Thank you for creating a new account at http://localhost. Your account will remain in a pending state until an administrator approves your registration, at which point you will receive an e-mail notification to alert you to the change in status.

Username: " + UserName + @"
Password: " + Password + @"

Please visit the website's Contact form to submit any questions to the administrators.
";

                RunTest();

                VerifyEmailParameter(emailFactoryMock, y => y.Body == expected);
            }

            private static void VerifyEmailParameter(Mock<IEmailNotifier> emailFactoryMock, Func<NotificationModel, bool> assert)
            {
                emailFactoryMock.Verify(x => x.SendAsync(It.Is<NotificationModel>(y => assert(y))));
            }


            [Fact]
            public void ConfirmUserIdIsSetInReturnObject()
            {
                SetupMockExpectationsForSuccessfulSaveOfUserAndRoles();

                var result = RunTest();

                Assert.Equal(UserId, result.UserId);
            }

            [Fact]
            public void ConfirmUserNameIsMapped()
            {

                ConfirmFieldMapping(x => x.UserName == UserName);
            }

            [Fact]
            public void ConfirmGivenNameIsMapped()
            {

                ConfirmFieldMapping(x => x.GivenName == GivenName);
            }

            [Fact]
            public void ConfirmFamilyNameIsMapped()
            {
                ConfirmFieldMapping(x => x.FamilyName == FamilyName);
            }

            [Fact]
            public void ConfirmPrimaryPhoneNumberIsMapped()
            {
                ConfirmFieldMapping(x => x.PhoneNumber == PrimaryPhoneNumber);
            }

            [Fact]
            public void ConfirmSecondaryPhoneNumberIsMapped()
            {
                ConfirmFieldMapping(x => x.MobilePhoneNumber == SecondaryPhoneNumber);
            }

            [Fact]
            public void ConfirmEmailPhoneNumberIsMapped()
            {
                ConfirmFieldMapping(x => x.Email == Email);
            }


            [Fact]
            public void ConfirmUserIsLockedOut()
            {
                ConfirmFieldMapping(x => x.LockoutEnabled == LockedOut);
            }

            [Fact]
            public void ConfirmStatusIsPending()
            {
                ConfirmFieldMapping(x => x.Active == PendingString);
            }


            [Fact]
            public void ConfirmCountyIsMapped()
            {
                ConfirmFieldMapping(x => x.County == County);
            }


            [Fact]
            public void ConfirmMailingAddressIsMapped()
            {
                ConfirmFieldMapping(x => x.MailingAddress == Mailing);
            }


            [Fact]
            public void ConfirmCityIsMapped()
            {
                ConfirmFieldMapping(x => x.City == City);
            }

            [Fact]
            public void ConfirStateIsMapped()
            {
                ConfirmFieldMapping(x => x.State == State);
            }

            [Fact]
            public void ConfirmZipCodeIsMapped()
            {
                ConfirmFieldMapping(x => x.ZipCode == ZipCode);
            }

            [Fact]
            public void ConfirmRoleIsSetForNewUser()
            {
                SetupMockExpectationsForSuccessfulSaveOfUserAndRoles();

                var result = RunTest();

                mockUserManager.Verify(x => x.AddToRolesAsync(It.Is<int>(y => y == UserId), It.IsAny<string[]>()));
            }

            [Fact]
            public void ConfirmRoleIsReporter()
            {
                SetupMockExpectationsForSuccessfulSaveOfUserAndRoles();

                var result = RunTest();

                mockUserManager.Verify(x => x.AddToRolesAsync(It.IsAny<int>(), It.Is<string[]>(y => y[0] == "Reporter")));
            }

            [Fact]
            public void ConfirmExceptionIfRolesDoNotSave()
            {
                mockUserManager.Setup(x => x.AddToRolesAsync(It.IsAny<int>(), It.IsAny<string[]>()))
                    .Returns(Task.Run(() => SuccessResult.Failed("asdfasd")));

                ExpectSuccessfulSaveOfUser();

                Assert.Throws<UserException>(() => RunTest());
            }

            [Fact]
            public void ConfirmErrorHandling()
            {
                mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                    .Returns((User actual, string p) =>
                    {
                        actual.Id = UserId;

                        return Task.Run(() => new IdentityResult(new[] { "something bad happened" }));
                    });


                Assert.Throws<UserException>(() => RunTest());
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

            private void RunTest()
            {
                var input = new UserModel
                {
                    Email = Email,
                    FamilyName = FamilyName,
                    GivenName = GivenName,
                    Password = Password,
                    PrimaryPhoneNumber = PrimaryPhoneNumber,
                    SecondaryPhoneNumber = SecondaryPhoneNumber,
                    UserName = UserName,
                    UserId = UserId,
                    LockedOut = LockedOut,
                    Active = Active,
                    City = City,
                    State = State,
                    ZipCode = ZipCode,
                    County = County,
                    MailingAddress = Mailing
                };
                input.Roles.Add(NewRole);

                BuildSystem().Update(input);
            }



            private void ExpectSuccessfulSaveOfUser()
            {
                mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                                    .Returns(CreateSuccessResult());
            }

            private void ExpectSuccessfulSaveOfRoles()
            {
                mockUserManager.Setup(x => x.AddToRolesAsync(It.IsAny<int>(), It.IsAny<string[]>()))
                                    .Returns(CreateSuccessResult());
            }

            private void ConfirmFieldMapping(Func<User, bool> assert)
            {

                ExpectSuccessfulSaveOfRoles();
                ExpectSuccessfulSaveOfUser();
                ExpectToQueryForExistingUserRecord();
                ExpectToRemoveOldRoles();
                ExpectToSaveNewRoles();
                ExpectUserIsAlreadyInARole();

                RunTest();

                mockUserManager.Verify(x => x.UpdateAsync(It.Is<User>(y => assert(y))));
            }
            [Fact]
            public void ConfirmUserNameIsMapped()
            {

                ConfirmFieldMapping(x => x.UserName == UserName);
            }

            [Fact]
            public void ConfirmGivenNameIsMapped()
            {

                ConfirmFieldMapping(x => x.GivenName == GivenName);
            }

            [Fact]
            public void ConfirmFamilyNameIsMapped()
            {
                ConfirmFieldMapping(x => x.FamilyName == FamilyName);
            }

            [Fact]
            public void ConfirmPrimaryPhoneNumberIsMapped()
            {
                ConfirmFieldMapping(x => x.PhoneNumber == PrimaryPhoneNumber);
            }

            [Fact]
            public void ConfirmSecondaryPhoneNumberIsMapped()
            {
                ConfirmFieldMapping(x => x.MobilePhoneNumber == SecondaryPhoneNumber);
            }

            [Fact]
            public void ConfirmEmailPhoneNumberIsMapped()
            {
                ConfirmFieldMapping(x => x.Email == Email);
            }


            [Fact]
            public void ConfirmUserIsLockedOut()
            {
                ConfirmFieldMapping(x => x.LockoutEnabled == LockedOut);
            }

            [Fact]
            public void ConfirmActiveStatus()
            {
                ConfirmFieldMapping(x => x.Active == ActiveString);
            }


            [Fact]
            public void ConfirmCountyIsMapped()
            {
                ConfirmFieldMapping(x => x.County == County);
            }


            [Fact]
            public void ConfirmMailingAddressIsMapped()
            {
                ConfirmFieldMapping(x => x.MailingAddress == Mailing);
            }


            [Fact]
            public void ConfirmCityIsMapped()
            {
                ConfirmFieldMapping(x => x.City == City);
            }

            [Fact]
            public void ConfirStateIsMapped()
            {
                ConfirmFieldMapping(x => x.State == State);
            }

            [Fact]
            public void ConfirmZipCodeIsMapped()
            {
                ConfirmFieldMapping(x => x.ZipCode == ZipCode);
            }


            [Fact]
            public void ConfirmSearchForExistingUserFirst()
            {
                SetupSuccessfulDatabaseExpectations();

                RunTest();


                mockUserManager.Verify(x => x.FindByIdAsync(It.Is<int>(y => y == UserId)));
            }

            private void SetupSuccessfulDatabaseExpectations()
            {
                ExpectToQueryForExistingUserRecord();
                ExpectSuccessfulSaveOfUser();
                ExpectUserIsAlreadyInARole();
                ExpectToRemoveOldRoles();
                ExpectToSaveNewRoles();
            }

            [Fact]
            public void ConfirmSearchForExistingRoles()
            {
                SetupSuccessfulDatabaseExpectations();

                RunTest();

                mockUserManager.Verify(x => x.GetRolesAsync(It.Is<int>(y => y == UserId)));
            }


            [Fact]
            public void ConfirmRemovalOfOldRoleForUser()
            {
                SetupSuccessfulDatabaseExpectations();

                RunTest();

                mockUserManager.Verify(x => x.RemoveFromRolesAsync(It.Is<int>(y => y == UserId),
                                                                   It.IsAny<string[]>()));
            }

            [Fact]
            public void ConfirmRemovalOfOldRoleByName()
            {
                SetupSuccessfulDatabaseExpectations();

                RunTest();

                mockUserManager.Verify(x => x.RemoveFromRolesAsync(It.IsAny<int>(),
                                                                   It.Is<string[]>(y => y[0] == OldRole)));
            }


            [Fact]
            public void ConfirmaddNewRoleForUser()
            {
                SetupSuccessfulDatabaseExpectations();

                RunTest();

                mockUserManager.Verify(x => x.AddToRolesAsync(It.Is<int>(y => y == UserId),
                                                              It.IsAny<string[]>()));
            }

            [Fact]
            public void ConfirmaddNewRoleByName()
            {
                SetupSuccessfulDatabaseExpectations();

                RunTest();

                mockUserManager.Verify(x => x.AddToRolesAsync(It.IsAny<int>(),
                                                              It.Is<string[]>(y => y[0] == NewRole)));
            }

            private void ExpectToRemoveOldRoles()
            {
                mockUserManager.Setup(x => x.RemoveFromRolesAsync(It.IsAny<int>(),
                                                                                  It.IsAny<string[]>()))
                                                    .Returns(CreateSuccessResult());
            }

            private void ExpectToSaveNewRoles()
            {
                mockUserManager.Setup(x => x.AddToRolesAsync(It.IsAny<int>(),
                                                                             It.IsAny<string[]>()))
                                                    .Returns(CreateSuccessResult());
            }

            private void ExpectUserIsAlreadyInARole()
            {
                mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<int>()))
                                    .ReturnsAsync(new List<string>() { OldRole });
            }

            private void ExpectToQueryForExistingUserRecord()
            {
                mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<int>()))
                                    .Returns(Task.Run(() => new User()));
            }

            [Fact]
            public void ConfirmHandlingOfErrorWhenSavingUpdatedUserRecord()
            {
                ExpectToQueryForExistingUserRecord();

                mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                    .Returns((User actual) =>
                    {
                        actual.Id = UserId;

                        return Task.Run(() => new IdentityResult(new[] { "something bad happened" }));
                    });

                Assert.Throws<UserException>(() => RunTest());
            }

            [Fact]
            public void ConfirmHandlingOfErrorWhenRemovingExistingRoles()
            {
                const string badStuffHappened = "Bad stuff happened";

                ExpectToQueryForExistingUserRecord();
                ExpectSuccessfulSaveOfUser();
                ExpectUserIsAlreadyInARole();


                mockUserManager.Setup(x => x.RemoveFromRolesAsync(It.IsAny<int>(),
                                                                  It.IsAny<string[]>()))
                            .ReturnsAsync(IdentityResult.Failed(badStuffHappened));


                // Act & assert
                Assert.Throws<UserException>(() => RunTest());
            }

            [Fact]
            public void ConfirmHandlingOfErrorWhenRemovingAddingNewRoles()
            {
                const string badStuffHappened = "Bad stuff happened";

                ExpectToQueryForExistingUserRecord();
                ExpectSuccessfulSaveOfUser();
                ExpectUserIsAlreadyInARole();
                ExpectToRemoveOldRoles();


                mockUserManager.Setup(x => x.AddToRolesAsync(It.IsAny<int>(),
                                                             It.IsAny<string[]>()))
                            .ReturnsAsync(IdentityResult.Failed(badStuffHappened));


                // Act & assert
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


        public class FindAllPendingUsers : Fixture
        {

            [Fact]
            public void ConfirmEmptyListWhenThereAreNoUsersInTheSystem()
            {
                mockUserManager.SetupGet(x => x.Users)
                    .Returns(new List<User>().AsQueryable());

                var actual = BuildSystem().FindAllPending();

                Assert.Equal(0, actual.Count());
            }

            public class NoPendingUsers : Fixture
            {
                const int userId = 12312;
                const string email = "asd@asdfasd.com";
                const string phoneNumber = "(555) 555-5555";
                const string userName = "asdfasd";
                const string mobileNumber = "(555) 555-5554";
                const bool lockedOut = true;
                const string active = "inactive";

                private IEnumerable<PendingUserModel> RunTheTest()
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

                    return BuildSystem().FindAllPending();
                }

                [Fact]
                public void ConfirmResultCount()
                {
                    var results = RunTheTest();
                    Assert.Equal(0, results.Count());
                }

            }

            public class SinglePendingUser : Fixture
            {
                const int userId = 12312;
                const string email = "asd@asdfasd.com";
                const string phoneNumber = "(555) 555-5555";
                const string userName = "asdfasd";
                const string mobileNumber = "(555) 555-5554";
                const string active = "pending";
                const string familyName = "last";
                const string givenName = "first";
                const string displayName = "first last";
                const bool lockedOut = true;

                private IEnumerable<PendingUserModel> RunTheTest()
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

                    return BuildSystem().FindAllPending();
                }

                [Fact]
                public void ConfirmResultCount()
                {
                    var results = RunTheTest();
                    Assert.Equal(1, results.Count());
                }


                [Fact]
                public void ConfirmSecondaryPhoneNumberIsMapped()
                {
                    Assert.Equal(mobileNumber, RunTheTest().First().SecondaryPhoneNumber);
                }

                [Fact]
                public void ConfirmPrimaryPhoneNumberIsMapped()
                {
                    Assert.Equal(phoneNumber, RunTheTest().First().PrimaryPhoneNumber);
                }

                [Fact]
                public void ConfirmEmailIsMapped()
                {
                    Assert.Equal(email, RunTheTest().First().Email);
                }

                [Fact]
                public void ConfirmUserIdIsMapped()
                {
                    Assert.Equal(userId, RunTheTest().First().UserId);
                }


                [Fact]
                public void ConfirmDisplayNameIsMapped()
                {
                    Assert.Equal(displayName, RunTheTest().First().DisplayName);
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

                    mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<int>()))
                                   .ReturnsAsync(user);
                    mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<int>()))
                        .ReturnsAsync(new List<string>() { RoleAdministrator });

                    return BuildSystem().FindById(UserId);
                }

                [Fact]
                public void QueriesUsersById()
                {
                    RunTheTest();

                    mockUserManager.Verify(x => x.FindByIdAsync(It.Is<int>(y => y == UserId)));
                }

                [Fact]
                public void QueriesRolesById()
                {
                    RunTheTest();

                    mockUserManager.Verify(x => x.GetRolesAsync(It.Is<int>(y => y == UserId)));
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
                    Assert.Equal(RoleAdministrator, RunTheTest().Roles.First());
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


        public class Approve : Fixture
        {
            [Fact]
            public void NullObjectNotAllowed()
            {
                Assert.Throws<ArgumentNullException>(() =>
                {
                    BuildSystem().Approve(null);
                });
            }

            private void RunTest(List<int> ids)
            {
                BuildSystem().Approve(ids);
            }

            [Fact]
            public void SendEmailUsingRegisteredAddressAndName()
            {
                // Arrange
                var ids = new List<int>() { UserId };
                var expected = GivenName + " " + FamilyName + " <" + Email + ">";

                ExpectToLookupUser(UserId);
                ExpectSuccessfulUserUpdate();
                var notifierMock = ExpectToSendEmail();

                // Act
                RunTest(ids);

                // Assert
                notifierMock.Verify(x => x.SendAsync(It.Is<NotificationModel>(y => y.To == expected)));
            }

            [Fact]
            public void SendEmailUsingCustomizedMessage()
            {
                // Arrange
                var ids = new List<int>() { UserId };
                const string expected = @"Your account registration at FlightNode has been approved, and you can now start entering data. 

Username: " + UserName + @"
";

                ExpectToLookupUser(UserId);
                ExpectSuccessfulUserUpdate();
                var notifierMock = ExpectToSendEmail();

                // Act
                RunTest(ids);

                // Assert
                notifierMock.Verify(x => x.SendAsync(It.Is<NotificationModel>(y => y.Body == expected)));
            }

            //" user registration has been approved";


            [Fact]
            public void SendEmailUsingCorrectSubject()
            {
                // Arrange
                var ids = new List<int>() { UserId };
                const string expected = "FlightNode user registration has been approved";

                ExpectToLookupUser(UserId);
                ExpectSuccessfulUserUpdate();
                var notifierMock = ExpectToSendEmail();

                // Act
                RunTest(ids);

                // Assert
                notifierMock.Verify(x => x.SendAsync(It.Is<NotificationModel>(y => y.Subject == expected)));
            }

            private void ExpectSuccessfulUserUpdate()
            {
                mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                    .Returns(Task.Run(() => SuccessResult.Create()));
            }

            private void ExpectToLookupUser(int id)
            {
                mockUserManager.Setup(x => x.FindByIdAsync(id))
                   .Returns(Task.Run(() => new User {
                       Id = id,
                       Active = "pending",
                       Email = Email,
                       GivenName = GivenName,
                       FamilyName = FamilyName,
                       UserName = UserName
                   }));
            }

            [Fact]
            public void ApproveTwoPeopleWhenBothExistSetsFirstStatusToActive()
            {
                //
                // Arrange
                var ids = new List<int>() { 14, 43 };

                ids.ForEach(x => ExpectToLookupUser(x));
                ExpectSuccessfulUserUpdate();
                ExpectToSendEmail();

                //
                // Act
                RunTest(ids);

                // Assert
                mockUserManager.Verify(x => x.UpdateAsync(It.Is<User>(y => y.Id == ids[0] && y.Active == "active")));
            }

            [Fact]
            public void ApproveTwoPeopleWhenBothExistSetsSecondStatusToActive()
            {
                //
                // Arrange
                var ids = new List<int>() { 14, 43 };

                ids.ForEach(x => ExpectToLookupUser(x));
                ExpectSuccessfulUserUpdate();

                ExpectToSendEmail();

                //
                // Act
                RunTest(ids);

                //
                // Assert
                mockUserManager.Verify(x => x.UpdateAsync(It.Is<User>(y => y.Id == ids[1] && y.Active == "active")));
            }


            [Fact]
            public void UnlocksAccount()
            {
                //
                // Arrange
                var ids = new List<int>() { UserId };

                ExpectToLookupUser(UserId);
                ExpectSuccessfulUserUpdate();

                ExpectToSendEmail();

                //
                // Act
                RunTest(ids);

                //
                // Assert
                mockUserManager.Verify(x => x.UpdateAsync(It.Is<User>(y => y.Id == UserId && !y.LockoutEnabled)));        
            }

            [Fact]
            public void ConfirmSavesSecondRecordEvenWhenFirstRecordDoesNotExist()
            {

                var ids = new List<int>() { 14, 43 };

                mockUserManager.Setup(x => x.FindByIdAsync(ids[0]))
                    .Returns(Task.Run(() => null as User));

                ExpectToLookupUser(ids[1]);
                ExpectSuccessfulUserUpdate();

                ExpectToSendEmail();

                //
                // Act
                RunTest(ids);

                //
                // Assert
                mockUserManager.Verify(x => x.UpdateAsync(It.Is<User>(y => y.Id == ids[1])));
            }

            [Fact]
            public void ConfirmSavesFirstRecordEvenWhenSecondRecordDoesNotExist()
            {

                var ids = new List<int>() { 14, 43 };

                mockUserManager.Setup(x => x.FindByIdAsync( ids[1]))
                    .Returns(Task.Run(() => null as User));
                
                ExpectToLookupUser(ids[0]);
                ExpectSuccessfulUserUpdate();

                ExpectToSendEmail();

                //
                // Act
                RunTest(ids);

                //
                // Assert
                mockUserManager.Verify(x => x.UpdateAsync(It.Is<User>(y => y.Id == ids[0])));
            }



            [Fact]
            public void ConfirmSavesSecondRecordEvenWhenFirstRecordFailsToUpdate()
            {

                var ids = new List<int>() { 14, 43 };

                ids.ForEach(x => ExpectToLookupUser(x));

                ExpectSuccessfulUserUpdate();
                mockUserManager.Setup(x => x.UpdateAsync(It.Is<User>(y=> y.Id == ids[0])))
                    .Returns(Task.Run(() => new IdentityResult("errorrrrrrr")));

                ExpectToSendEmail();

                //
                // Act
                RunTest(ids);

                //
                // Assert
                mockUserManager.Verify(x => x.UpdateAsync(It.Is<User>(y => y.Id == ids[1])));
            }



            [Fact]
            public void ConfirmSavesFirstRecordEvenWhenSecondRecordFailsToUpdate()
            {

                var ids = new List<int>() { 14, 43 };
                ids.ForEach(x => ExpectToLookupUser(x));

                ExpectSuccessfulUserUpdate();
                mockUserManager.Setup(x => x.UpdateAsync(It.Is<User>(y => y.Id == ids[1])))
                    .Returns(Task.Run(() => new IdentityResult("errorrrrrrr")));
                
                ExpectToSendEmail();

                //
                // Act
                RunTest(ids);

                //
                // Assert
                mockUserManager.Verify(x => x.UpdateAsync(It.Is<User>(y => y.Id == ids[0])));
            }


            // More detailed exception handling occurs in the controller class. In this case,
            // if one record succeeds and a subsequent record (from array of multiple id values)
            // fails, then it is acceptable that the first (or more) succeeded. That one+ will
            // be removed from the pending list as desired.

            [Fact]
            public void ConfirmDoesNotCatchExceptionOnFind()
            {
                var ids = new List<int>() { 14, 43 };

                mockUserManager.Setup(x => x.FindByIdAsync(It.Is<int>(y => y == ids[0])))
                    .Throws<InvalidOperationException>();

                //
                // Act & Assert
                Assert.Throws<InvalidOperationException>(() => RunTest(ids));
            }

            [Fact]
            public void ConfirmDoesNotCatchExceptionOnUpdate()
            {
                var ids = new List<int>() { 14, 43 };

                mockUserManager.Setup(x => x.FindByIdAsync(It.Is<int>(y => y == ids[0])))
                    .Returns(Task.Run(() => new User { Id = ids[0], Active = "pending" }));

                mockUserManager.Setup(x => x.UpdateAsync(It.Is<User>(y => y.Id == ids[0])))
                    .Throws<InvalidOperationException>();

                //
                // Act & Assert
                Assert.Throws<InvalidOperationException>(() => RunTest(ids));
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

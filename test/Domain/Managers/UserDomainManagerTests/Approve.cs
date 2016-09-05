using FlightNode.Common.Utility;
using FlightNode.Identity.Domain.Entities;
using Microsoft.AspNet.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FlightNode.Identity.UnitTests.Domain.Managers.UserDomainManagerTests
{

    public class Approve : Fixture
    {
        [Fact]
        public void NullObjectNotAllowed()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => BuildSystem().Approve(null));
        }

        private Task RunTest(List<int> ids)
        {
            return BuildSystem().Approve(ids);
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
            MockUserManager.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .Returns(Task.Run(() => SuccessResult.Create()));
        }

        private void ExpectToLookupUser(int id)
        {
            MockUserManager.Setup(x => x.FindByIdAsync(id))
               .Returns(Task.Run(() => new User
               {
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
            MockUserManager.Verify(x => x.UpdateAsync(It.Is<User>(y => y.Id == ids[0] && y.Active == "active")));
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
            MockUserManager.Verify(x => x.UpdateAsync(It.Is<User>(y => y.Id == ids[1] && y.Active == "active")));
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
            MockUserManager.Verify(x => x.UpdateAsync(It.Is<User>(y => y.Id == UserId && !y.LockoutEnabled)));
        }

        [Fact]
        public void ConfirmSavesSecondRecordEvenWhenFirstRecordDoesNotExist()
        {

            var ids = new List<int>() { 14, 43 };

            MockUserManager.Setup(x => x.FindByIdAsync(ids[0]))
                .Returns(Task.Run(() => null as User));

            ExpectToLookupUser(ids[1]);
            ExpectSuccessfulUserUpdate();

            ExpectToSendEmail();

            //
            // Act
            RunTest(ids);

            //
            // Assert
            MockUserManager.Verify(x => x.UpdateAsync(It.Is<User>(y => y.Id == ids[1])));
        }

        [Fact]
        public void ConfirmSavesFirstRecordEvenWhenSecondRecordDoesNotExist()
        {

            var ids = new List<int>() { 14, 43 };

            MockUserManager.Setup(x => x.FindByIdAsync(ids[1]))
                .Returns(Task.Run(() => null as User));

            ExpectToLookupUser(ids[0]);
            ExpectSuccessfulUserUpdate();

            ExpectToSendEmail();

            //
            // Act
            RunTest(ids);

            //
            // Assert
            MockUserManager.Verify(x => x.UpdateAsync(It.Is<User>(y => y.Id == ids[0])));
        }



        [Fact]
        public void ConfirmSavesSecondRecordEvenWhenFirstRecordFailsToUpdate()
        {

            var ids = new List<int>() { 14, 43 };

            ids.ForEach(x => ExpectToLookupUser(x));

            ExpectSuccessfulUserUpdate();
            MockUserManager.Setup(x => x.UpdateAsync(It.Is<User>(y => y.Id == ids[0])))
                .Returns(Task.Run(() => new IdentityResult("errorrrrrrr")));

            ExpectToSendEmail();

            //
            // Act
            RunTest(ids);

            //
            // Assert
            MockUserManager.Verify(x => x.UpdateAsync(It.Is<User>(y => y.Id == ids[1])));
        }



        [Fact]
        public void ConfirmSavesFirstRecordEvenWhenSecondRecordFailsToUpdate()
        {

            var ids = new List<int>() { 14, 43 };
            ids.ForEach(x => ExpectToLookupUser(x));

            ExpectSuccessfulUserUpdate();
            MockUserManager.Setup(x => x.UpdateAsync(It.Is<User>(y => y.Id == ids[1])))
                .Returns(Task.Run(() => new IdentityResult("errorrrrrrr")));

            ExpectToSendEmail();

            //
            // Act
            RunTest(ids);

            //
            // Assert
            MockUserManager.Verify(x => x.UpdateAsync(It.Is<User>(y => y.Id == ids[0])));
        }


        // More detailed exception handling occurs in the controller class. In this case,
        // if one record succeeds and a subsequent record (from array of multiple id values)
        // fails, then it is acceptable that the first (or more) succeeded. That one+ will
        // be removed from the pending list as desired.

        [Fact]
        public void ConfirmDoesNotCatchExceptionOnFind()
        {
            var ids = new List<int>() { 14, 43 };

            MockUserManager.Setup(x => x.FindByIdAsync(It.Is<int>(y => y == ids[0])))
                .Throws<InvalidOperationException>();

            //
            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(() => RunTest(ids));
        }

        [Fact]
        public void ConfirmDoesNotCatchExceptionOnUpdate()
        {
            var ids = new List<int>() { 14, 43 };

            MockUserManager.Setup(x => x.FindByIdAsync(ids[0]))
                .ReturnsAsync(new User { Id = ids[0], Active = "pending" });

            //mockUserManager.Setup(x => x.FindByIdAsync(ids[1]))
            //    .ReturnsAsync(new User { Id = ids[1], Active = "pending" });

            MockUserManager.Setup(x => x.UpdateAsync(It.Is<User>(y => y.Id == ids[0])))
                .ThrowsAsync(new InvalidOperationException());

            //
            // Act & Assert
            Assert.ThrowsAsync<AggregateException>(() => RunTest(ids));
        }
    }
}

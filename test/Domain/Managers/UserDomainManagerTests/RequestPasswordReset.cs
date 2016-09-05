using FlightNode.Common.Utility;
using FlightNode.Identity.Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FlightNode.Identity.UnitTests.Domain.Managers.UserDomainManagerTests
{
    public class RequestPasswordReset : Fixture
    {
        [Fact]
        public void NullArgumentNotAllowed()
        {
            try
            {
                var a = BuildSystem().RequestPasswordChange(null).Result;
            }
            catch (AggregateException ag)
            {
                Assert.IsType<ArgumentNullException>(ag.InnerException);
            }
        }

        [Fact]
        public void WhenEmailDoesNotExistThenReturnFalse()
        {
            // Arrange
            var input = "jdoe@example.com";

            MockUserManager.SetupGet(x => x.Users)
                .Returns(new List<User>().AsQueryable());

            // Act
            var result = BuildSystem().RequestPasswordChange(input).Result;

            // Assert
            Assert.False(result);
        }

        public class HappyPath : Fixture
        {

            const string emailAddress = "jdoe@example.com";
            const string givenName = "a";
            const string familyName = "b";
            const string expectedEmail = "a b <jdoe@example.com>";
            const string token = "asdfadfadfasdfa";
            const int id = 999993;
            const string expectedUrl = "http://localhost:9000/#/users/changepassword?token=asdfadfadfasdfa";

            private bool RunTest()
            {
                // .. setup existing user
                MockUserManager.SetupGet(x => x.Users)
                    .Returns(new List<User> { new User { Email = emailAddress, Id = id, GivenName = givenName, FamilyName = familyName } }.AsQueryable());


                // .. setup token generation
                MockUserManager.Setup(x => x.GeneratePasswordResetTokenAsync(id))
                    .Returns(Task.FromResult(token));

                // .. setup e-mail send
                EmailFactoryMock.Setup(x => x.CreateNotifier()).Returns(EmailNotifierMock.Object);
                EmailNotifierMock.Setup(x => x.SendAsync(It.IsAny<NotificationModel>())).Returns(Task.FromResult(true));


                // Act
                return BuildSystem().RequestPasswordChange(emailAddress).Result;
            }

            [Fact]
            public void ReturnsTrue()
            {
                var result = RunTest();

                Assert.True(result);
            }

            [Fact]
            public void SetsUrlInMessageBody()
            {
                RunTest();

                Func<NotificationModel, bool> verifier = actual =>
                {
                    Assert.True(actual.Body.Contains(expectedUrl));

                    return true;
                };

                EmailNotifierMock.Verify(x => x.SendAsync(It.Is<NotificationModel>(y => verifier(y))));
            }

            [Fact]
            public void SetsSubject()
            {
                RunTest();

                Func<NotificationModel, bool> verifier = actual =>
                {
                    Assert.Equal("FlightNode Password Change Request", actual.Subject);

                    return true;
                };

                EmailNotifierMock.Verify(x => x.SendAsync(It.Is<NotificationModel>(y => verifier(y))));
            }

            [Fact]
            public void SetsToAddress()
            {
                RunTest();

                Func<NotificationModel, bool> verifier = actual =>
                {
                    Assert.Equal(expectedEmail, actual.To);

                    return true;
                };

                EmailNotifierMock.Verify(x => x.SendAsync(It.Is<NotificationModel>(y => verifier(y))));

            }
        }
    }
}

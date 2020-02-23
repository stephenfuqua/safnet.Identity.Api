using FlightNode.Identity.Domain.Entities;
using FlightNode.Identity.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FlightNode.Identity.UnitTests.Domain.Managers.UserDomainManagerTests
{
    public class ChangeForgottenPassword : Fixture
    {
        [Fact]
        public void NullTokenNotAllowed()
        {
            var input = new ChangePasswordModel();

            try
            {
                var a = BuildSystem().ChangeForgottenPassword(null, input).Result;
            }
            catch (AggregateException agg)
            {
                Assert.IsType<ArgumentNullException>(agg.InnerException);
            }

        }


        [Fact]
        public void EmptyStringTokenNotAllowed()
        {
            var input = new ChangePasswordModel();

            try
            {
                var a = BuildSystem().ChangeForgottenPassword("    ", input).Result;
            }
            catch (AggregateException agg)
            {
                Assert.IsType<ArgumentException>(agg.InnerException);
            }
        }

        [Fact]
        public void NullInputModelNotAllowed()
        {
            const string token = "asdfA";

            try
            {
                var a = BuildSystem().ChangeForgottenPassword(token, null).Result;
            }
            catch (AggregateException agg)
            {
                Assert.IsType<ArgumentNullException>(agg.InnerException);
            }

        }

        [Fact]
        public void UserDoesNotExistthenReturnFalse()
        {
            // Arrange
            const string token = "asdfA";
            var input = new ChangePasswordModel
            {
                EmailAddress = "dd",
                Password = "ee"
            };

            // .. can't find the user
            MockUserManager.SetupGet(x => x.Users)
                .Returns(new List<User>().AsQueryable());

            // Act
            var result = BuildSystem().ChangeForgottenPassword(token, input).Result;

            // Assert
            Assert.Equal(result, ChangeForgottenPasswordResult.UserDoesNotExist);
        }

        [Fact]
        public void TokenIsNotValidThenReturnFalse()
        {
            // Arrange
            const string token = "asdfA";
            var input = new ChangePasswordModel
            {
                EmailAddress = "dd",
                Password = "ee"
            };
            var user = new User
            {
                Id = 2,
                Email = input.EmailAddress
            };

            // .. Find the user
            MockUserManager.SetupGet(x => x.Users)
                .Returns(new List<User> { user }.AsQueryable());

            // .. invalid token
            MockUserManager.Setup(x => x.ResetPasswordAsync(user.Id, token, input.Password))
                .Returns(Task.FromResult(new Microsoft.AspNet.Identity.IdentityResult("Invalid token.")));

            // Act
            var result = BuildSystem().ChangeForgottenPassword(token, input).Result;

            // Assert
            Assert.Equal(result, ChangeForgottenPasswordResult.BadToken);
        }

        [Fact]
        public void PasswordDoesNotMeetComplexityRequirements()
        {
            // Arrange
            const string token = "asdfA";
            var input = new ChangePasswordModel
            {
                EmailAddress = "dd",
                Password = "ee"
            };
            var user = new User
            {
                Id = 2,
                Email = input.EmailAddress
            };

            // .. Find the user
            MockUserManager.SetupGet(x => x.Users)
                .Returns(new List<User> { user }.AsQueryable());

            // .. invalid password
            MockUserManager.Setup(x => x.ResetPasswordAsync(user.Id, token, input.Password))
                .Returns(Task.FromResult(new Microsoft.AspNet.Identity.IdentityResult("Passwords must be at ...")));

            // Act
            var result = BuildSystem().ChangeForgottenPassword(token, input).Result;

            // Assert
            Assert.Equal(result, ChangeForgottenPasswordResult.InvalidPassword);
        }

        [Fact]
        public void HappyPath()
        {
            // Arrange
            const string token = "asdfA";
            var input = new ChangePasswordModel
            {
                EmailAddress = "dd",
                Password = "ee"
            };
            var user = new User
            {
                Id = 2,
                Email = input.EmailAddress
            };

            // .. Find the user
            MockUserManager.SetupGet(x => x.Users)
                .Returns(new List<User> { user }.AsQueryable());

            MockUserManager.Setup(x => x.ResetPasswordAsync(user.Id, token, input.Password))
                .Returns(Task.FromResult(SuccessResult.Create()));


            // Act
            var result = BuildSystem().ChangeForgottenPassword(token, input).Result;

            // Assert
            Assert.Equal(result, ChangeForgottenPasswordResult.Happy);
        }
    }
}

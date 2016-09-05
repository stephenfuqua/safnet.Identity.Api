using FlightNode.Common.Api.Models;
using FlightNode.Identity.Domain.Interfaces;
using FlightNode.Identity.Domain.Logic;
using FlightNode.Identity.Infrastructure.Persistence;
using FlightNode.Identity.Services.Models;
using FligthNode.Common.Api.Controllers;
using Flurl;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Collections.Generic;
using FlightNode.Common.Utility;
using System.Threading.Tasks;
using FlightNode.Identity.Domain.Entities;

namespace FligthNode.Identity.Services.Controllers
{
    /// <summary>
    /// API Controller for User records
    /// </summary>
    public class UsersController : LoggingController
    {
        private readonly IUserDomainManager _manager;

        private IUserDomainManager _managerWithTokenSetup;


        /// <summary>
        /// A property to retrieve a <see cref="IUserDomainManager"/> that can handle password changes.
        /// </summary>
        /// <remarks>
        /// Can be set by a unit test. This is a bit ugly, but the best I can come up with for now.
        /// </remarks>
        protected IUserDomainManager ManagerWithTokenSetup
        {
            get
            {
                if (_managerWithTokenSetup == null)
                {
                    var userManager = HttpContext.Current.GetOwinContext().GetUserManager<AppUserManager>();
                    _managerWithTokenSetup = new UserDomainManager(userManager, new EmailFactory());
                }
                return _managerWithTokenSetup;
            }
            set
            {
                _managerWithTokenSetup = value;
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="UsersController"/>
        /// </summary>
        /// <param name="manager">Instance of <see cref="IUserDomainManager"/></param>
        public UsersController(IUserDomainManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }

            _manager = manager;
        }


        /// <summary>
        /// Retrieves all active system users.
        /// </summary>
        /// <returns>Action result containing an array of users</returns>
        /// <example>
        /// GET: /api/v1/users
        /// </example>
        [Authorize(Roles = "Administrator, Coordinator")]
        public IHttpActionResult Get()
        {
            return WrapWithTryCatch(() =>
            {
                var all = _manager.FindAll();

                if (all.Any())
                {
                    return Ok(all);
                }

                return NotFound();
            });
        }

        /// <summary>
        /// Retrieves a single user by User Id value.
        /// </summary>
        /// <param name="id">User Id</param>
        /// <returns>Action result containing the requested user, or status code 404 "not found".</returns>
        /// <example>
        /// GET: /api/v1/users/1
        /// </example>
        [Authorize(Roles = "Administrator, Coordinator")]
        public IHttpActionResult Get(int id)
        {
            return WrapWithTryCatch(() =>
            {
                var result = _manager.FindById(id);
                if (result != null)
                {
                    return Ok(result);
                }
                else
                {
                    return NotFound();
                }
            });
        }

        /// <summary>
        /// Retrieves new users whose activation is pending.
        /// </summary>
        /// <returns>
        /// Action result containing <see cref="PendingUserModel"/> records.
        /// </returns>
        /// <example>
        /// GET: /api/v1/users/pending
        /// [
        ///   {
        ///     "userId": 34234,
        ///     "displayName": "Juana Coneja",
        ///     "email": "dirigible@asfddfsdfs.com",
        ///     "primaryPhoneNumber": "555-555-5555",
        ///     "secondaryPhoneNumber": "(555) 555-5554",
        ///   }
        /// ]
        /// </example>
        [Route("api/v1/users/pending")]
        [Authorize(Roles = "Administrator, Coordinator")]
        [HttpGet]
        public IHttpActionResult Pending()
        {
            return WrapWithTryCatch(() =>
            {
                var result = _manager.FindAllPending() ?? new List<PendingUserModel>();
                return Ok(result);
            });
        }

        /// <summary>
        /// Updates the registration status to "active" for the input User Id values.
        /// </summary>
        /// <param name="ids">List of ID values.</param>
        /// <returns>
        /// Action result with no content
        /// </returns>
        /// <example>
        /// POST /api/v1/users/pending
        /// [ 1, 2 ]
        /// </example>
        [Route("api/v1/users/pending")]
        [Authorize(Roles = "Administrator, Coordinator")]
        [HttpPost]
        public async Task<IHttpActionResult> Approve([FromBody]List<int> ids)
        {
            return await WrapWithTryCatchAsync(async () =>
            {
                await _manager.Approve(ids);
                return NoContent();
            });
        }

        /// <summary>
        /// Adds a new user to the system.
        /// </summary>
        /// <param name="user"><see cref="UserModel"/></param>
        /// <returns>Action result containing the created user record (including the new ID), with status code 201 "created".</returns>
        /// <example>
        /// POST: /api/v1/users
        /// {
        ///   "userName": "dirigible@asfddfsdfs.com",
        ///   "givenName": "Juana",
        ///   "familyName": "Coneja",
        ///   "email": "dirigible@asfddfsdfs.com",
        ///   "primaryPhoneNumber": "555-555-5555",
        ///   "secondaryPhoneNumber": "(555) 555-5554",
        ///   "password": "deerEatRabbits?"
        /// }
        /// </example>
        [Authorize(Roles = "Administrator, Coordinator")]
        public IHttpActionResult Post([FromBody]UserModel user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return WrapWithTryCatch(() =>
            {
                var result = _manager.Create(user);
                return Created(user);
            });
        }

        /// <summary>
        /// Adds a new user to the system via self-registration
        /// </summary>
        /// <param name="user"><see cref="UserModel"/></param>
        /// <returns>Action result containing the created user record (including the new ID), with status code 201 "created".</returns>
        /// <example>
        /// POST: /api/v1/users/register
        /// {
        ///   "userName": "dirigible@asfddfsdfs.com",
        ///   "givenName": "Juana",
        ///   "familyName": "Coneja",
        ///   "email": "dirigible@asfddfsdfs.com",
        ///   "primaryPhoneNumber": "555-555-5555",
        ///   "secondaryPhoneNumber": "(555) 555-5554",
        ///   "password": "deerEatRabbits?"
        /// }
        /// </example>
        [Route("api/v1/users/register")]
        public IHttpActionResult Register([FromBody]UserModel user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return WrapWithTryCatch(() =>
            {
                var result = _manager.CreatePending(user);
                return Created(result);
            });
        }

        private IHttpActionResult Created(UserModel result)
        {
            var location = Request.RequestUri
                                  .ToString()
                                  .AppendPathSegment(result.UserId.ToString());

            return Created(location, result);
        }

        /// <summary>
        /// Changes a user's password
        /// </summary>
        /// <param name="id">User Id</param>
        /// <param name="change"><see cref="PasswordModel"/></param>
        /// <returns>Action result with status code 204 "No content".</returns>
        /// <example>
        /// PUT: api/v1/users/1/changepassword
        /// {
        ///   "oldPassword": "deerEatRabbits?",
        ///   "newPassword": "notUsually."
        /// }
        /// </example>
        [HttpPut]
        [Authorize(Roles = "Administrator, Coordinator")]
        [Route("api/v1/users/{id:int}/changepassword")]
        public IHttpActionResult ChangePassword(int id, [FromBody]PasswordModel change)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return WrapWithTryCatch(() =>
            {
                var domainManager = ManagerWithTokenSetup;
                domainManager.ChangePassword(id, change);

                return NoContent();
            });

        }

        /// <summary>
        /// Update an existing system user.
        /// </summary>
        /// <param name="id">User Id</param>
        /// <param name="user"><see cref="UserModel"/></param>
        /// <returns>Action result with status code 204 "No content".</returns>
        /// <example>
        /// PUT: /api/v1/users/1
        /// {
        ///   "userId": 1,
        ///   "userName": "dirigible@asfddfsdfs.com",
        ///   "givenName": "Juana",
        ///   "familyName": "Coneja",
        ///   "email": "dirigible@asfddfsdfs.com",
        ///   "phoneNumber": "555-555-5555",
        ///   "mobilePhoneNumber": "(555) 555-5554",
        ///   "password": "will be set since this is not blank",
        ///   "active": true
        /// }
        /// </example>
        [Authorize(Roles = "Administrator, Coordinator")]
        public IHttpActionResult Put(int id, [FromBody]UserModel user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Update(id, user);
        }

        private IHttpActionResult Update(int id, UserModel user)
        {
            return WrapWithTryCatch(() =>
            {
                // For safety, override the message body's id with the input value
                user.UserId = id;
                _manager.Update(user);

                if (!string.IsNullOrWhiteSpace(user.Password))
                {
                    var domainManager = ManagerWithTokenSetup;
                    domainManager.AdministrativePasswordChange(user.UserId, user.Password);
                }

                return NoContent();
            });
        }


        /// <summary>
        /// Allows a user to update his/her own record
        /// </summary>
        /// <param name="id">User Id</param>
        /// <param name="user"><see cref="UserModel"/></param>
        /// <returns>Action result with status code 204 "No content".</returns>
        /// <example>
        /// PUT: /api/v1/users/profile/1
        /// {
        ///   "userId": 1,
        ///   "userName": "dirigible@asfddfsdfs.com",
        ///   "givenName": "Juana",
        ///   "familyName": "Coneja",
        ///   "email": "dirigible@asfddfsdfs.com",
        ///   "phoneNumber": "555-555-5555",
        ///   "mobilePhoneNumber": "(555) 555-5554",
        ///   "password": "will be set since this is not blank"
        /// }
        /// </example>
        [Authorize]
        [Route("api/v1/users/profile/{id:int}")]
        [HttpPut]
        public IHttpActionResult PutProfile(int id, [FromBody]UserModel user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // TODO: disallow editing of roles

            // Do not allow manipulation into someone else's userId
            if (id != RetrieveCurrentUserId())
            {
                return BadRequest();
            }

            return Update(id, user);
        }


        /// <summary>
        /// Retrieves the authenticated user's profile.
        /// </summary>
        /// <returns>Action result containing the requested user, or status code 404 "not found".</returns>
        /// <example>
        /// GET: /api/v1/users/profile
        /// Returns:
        /// {
        ///   "userId": 1,
        ///   "userName": "dirigible@asfddfsdfs.com",
        ///   "givenName": "Juana",
        ///   "familyName": "Coneja",
        ///   "email": "dirigible@asfddfsdfs.com",
        ///   "phoneNumber": "555-555-5555",
        ///   "mobilePhoneNumber": "(555) 555-5554",
        ///   "password": "will be set since this is not blank"
        /// }
        /// </example>
        [Authorize]
        [Route("api/v1/users/profile")]
        public IHttpActionResult GetProfile()
        {
            var id = RetrieveCurrentUserId();
            return Get(id);
        }

        private int RetrieveCurrentUserId()
        {
            return User.Identity.GetUserId<int>();
        }

        /// <summary>
        /// Soft-deletes (deactivates) a user from the system.
        /// </summary>
        /// <param name="id">User Id</param>
        /// <returns>Action result with status code 204 "No content".</returns>
        /// <example>
        /// DELETE: api/v1/users/1
        /// </example>
        [Authorize(Roles = "Administrator, Coordinator")]
        public IHttpActionResult Delete(int id)
        {
            return WrapWithTryCatch(() =>
            {
                return MethodNotAllowed();
            });
        }

        /// <summary>
        /// Retrieves a simplified list representation of all work type resources.
        /// </summary>
        /// <returns>Action result containing an enumeration of <see cref="SimpleListItem"/></returns>
        /// <example>
        /// GET: /api/v1/users/simplelist
        /// </example>
        [Authorize]
        [Route("api/v1/users/simplelist")]
        [HttpGet]
        public IHttpActionResult SimpleList()
        {
            return WrapWithTryCatch(() =>
            {
                var all = _manager.FindAll();

                var models = all.Select(x => new SimpleListItem
                {
                    Value = x.DisplayName,
                    Id = x.UserId
                });

                return Ok(models);
            });
        }

        /// <summary>
        /// Starts the password reset process, which will send an e-mail with a password reset link in it.
        /// </summary>
        /// <returns>
        /// 201 Created
        /// </returns>
        /// <example>
        /// POST api/v1/users/requestreset
        /// {
        ///     "emailAddress": "jdoe@example.com"
        /// }
        /// </example>
        [Route("api/v1/users/requestreset")]
        [HttpPost]
        public async Task<IHttpActionResult> RequestPasswordChange([FromBody]RequestPasswordResetModel request)
        {
            Validate(request ?? new RequestPasswordResetModel());
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await ManagerWithTokenSetup.RequestPasswordChange(request.EmailAddress);
            if (success)
            {
                return Ok();
            }

            return Unprocessable();
        }

        [Route("api/v1/users/changepassword")]
        [HttpPost]
        public async Task<IHttpActionResult> ChangeOwnPassword([FromUri] string token, [FromBody] ChangePasswordModel change)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("Url must provide a reset token");
            }

            Validate(change ?? new ChangePasswordModel());
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var result = await ManagerWithTokenSetup.ChangeForgottenPassword(token, change);

            switch (result)
            {
                case ChangeForgottenPasswordResult.BadToken:
                    return Unprocessable();
                case ChangeForgottenPasswordResult.Happy:
                    return Ok();
                case ChangeForgottenPasswordResult.InvalidPassword:
                    ModelState.AddModelError("password", "complexity");
                    return BadRequest(ModelState);
                case ChangeForgottenPasswordResult.UserDoesNotExist:
                    return NotFound();
                default:
                    // Shouldn't ever reach here, unless someone adds to the enum without handling it
                    return InternalServerError();
            }

        }

    }
}

﻿
using FlightNode.Common.Api.Models;
using FlightNode.Identity.Domain.Interfaces;
using FlightNode.Identity.Domain.Logic;
using FlightNode.Identity.Infrastructure.Persistence;
using FlightNode.Identity.Services.Models;
using FligthNode.Common.Api.Controllers;
using Flurl;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace FligthNode.Identity.Services.Controllers
{
    /// <summary>
    /// API Controller for User records
    /// </summary>
    public class UsersController : LoggingController
    {
        private readonly IUserDomainManager _manager;

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

                var location = Request.RequestUri
                                      .ToString()
                                      .AppendPathSegment(result.UserId.ToString());

                return Created(location, result);
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
                user.LockedOut = true;
                user.Active = "pending";

                var result = _manager.Create(user);

                var location = Request.RequestUri
                                      .ToString()
                                      .AppendPathSegment(result.UserId.ToString());

                return Created(location, result);
            });
        }

        /// <summary>
        /// Changes a user's password
        /// </summary>
        /// <param name="id">User Id</param>
        /// <param name="change"><see cref="PasswordModel"/></param>
        /// <returns>Action result with status code 204 "No content".</returns>
        /// <example>
        /// PUT: api/v1/users/changepassword/1
        /// {
        ///   "oldPassword": "deerEatRabbits?",
        ///   "newPassword": "notUsually."
        /// }
        /// </example>
        [HttpPut]
        [Authorize(Roles = "Administrator, Coordinator")]
        [Route("api/v1/user/changepassword/{id:int}")]
        public IHttpActionResult ChangePassword(int id, [FromBody]PasswordModel change)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return WrapWithTryCatch(() =>
            {
                var userManager = HttpContext.Current.GetOwinContext().GetUserManager<AppUserManager>();
                userManager.ChangePasswordAsync(id, change.CurrentPassword, change.NewPassword);

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
        ///   "password": "will be set since this is not blank"
        /// }
        /// </example>
        [Authorize(Roles = "Administrator, Coordinator")]
        public IHttpActionResult Put(int id, [FromBody]UserModel user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return WrapWithTryCatch(() =>
            {
                // For safety, override the message body's id with the input value
                user.UserId = id;
                _manager.Update(user);

                if (!string.IsNullOrWhiteSpace(user.Password))
                {
                    // TODO: check on back-end validation of the password complexity

                    var userManager = HttpContext.Current.GetOwinContext().GetUserManager<AppUserManager>();
                    var domainManager = new UserDomainManager(userManager);
                    domainManager.AdministrativePasswordChange(id, user.Password);
                }

                return NoContent();

                // TODO: return NotFound() if the user doesn't exist
            });
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

                // TODO: return NotFound() if the user doesn't exist
            });
        }
    }
}

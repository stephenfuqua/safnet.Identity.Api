using FlightNode.Common.Utility;
using FlightNode.Identity.Services.Models;
using FligthNode.Common.Api.Controllers;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace FlightNode.Identity.Services.Controllers
{
    /// <summary>
    /// Actions for route /api/v1/contact
    /// </summary>
    public class ContactController : LoggingController
    {
        private IEmailFactory _emailFactory;
        private ISanitizer _sanitizer;

        /// <summary>
        /// Creates a new instance of <see cref="ContactController"/>.
        /// </summary>
        /// <param name="emailFactory">An instance of <see cref="IEmailFactory"/>.</param>
        /// <param name="sanitizer">An instance of <see cref="ISanitizer"/>.</param>
        public ContactController(IEmailFactory emailFactory, ISanitizer sanitizer) : base()
        {
            if (emailFactory == null)
            {
                throw new ArgumentNullException(nameof(emailFactory));
            }
            if (sanitizer == null)
            {
                throw new ArgumentNullException(nameof(sanitizer));
            }

            _emailFactory = emailFactory;
            _sanitizer = sanitizer;
        }


        /// <summary>
        /// Generates a contact form e-mail message.
        /// </summary>
        /// <param name="input">Contents of the e-mail</param>
        /// <returns>
        /// Task action result with 204 Created status code
        /// or
        /// 400 Bad Request
        /// </returns>
        /// <example>
        /// POST /api/v1/contact
        /// {
        ///   "subject": "request to volunteer",
        ///   "body": "this is the message to the recipient. HTML will be removed",
        ///   "fromEmail": "someone@example.com",
        ///   "fromName": "Someone Example"
        /// }
        /// </example>
        public async Task<IHttpActionResult> Post([FromBody] EmailModel input)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var to = Properties.Settings.Default.ContactEmail;

            input.Sanitize(this._sanitizer);


            var message = new NotificationModel(to, input.Subject, input.Body);
            message.FromName = input.FromName;
            message.FromEmail = input.FromAddress;


            await _emailFactory.CreateNotifier()
                .SendAsync(message);

            return NoContent();
        }
    }
}

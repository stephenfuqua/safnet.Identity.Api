
using Microsoft.AspNet.Identity;

namespace FlightNode.Identity.UnitTests.Domain.Managers.UserDomainManagerTests
{

    public class SuccessResult : IdentityResult
    {
        public SuccessResult() : base(true) { }

        internal static IdentityResult Create()
        {
            return new SuccessResult();
        }
    }
}

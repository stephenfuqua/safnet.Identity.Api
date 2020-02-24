
namespace FlightNode.Identity.Domain.Entities
{

    public enum ChangeForgottenPasswordResult
    {
        BadToken,
        UserDoesNotExist,
        InvalidPassword,
        Happy
    }
}

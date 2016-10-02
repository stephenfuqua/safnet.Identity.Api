
namespace FlightNode.Identity.Domain.Entities
{
    public enum RoleEnum
    {
        // These must exactly match the Role table in the database.
        Administrator = 1,
        Reporter = 2,
        Coordinator = 3,
        Lead = 4
    }
}

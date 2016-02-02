namespace FlightNode.Identity.Migrations
{
    using System.Data.Entity.Migrations;

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public partial class UpdatedUser : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Users", "Active", c => c.String());
            
            base.Sql("UPDATE dbo.Users SET Active = CASE WHEN Active = 1 THEN 'active' ELSE 'inactive' END");
        }

        public override void Down()
        {
            AlterColumn("dbo.Users", "Active", c => c.Boolean(nullable: false));
        }
    }
}

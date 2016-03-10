namespace FlightNode.Identity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdditionalAddressFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "County", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.Users", "MailingAddress", c => c.String(maxLength: 100));
            AddColumn("dbo.Users", "City", c => c.String(maxLength: 50));
            AddColumn("dbo.Users", "State", c => c.String(maxLength: 2));
            AddColumn("dbo.Users", "ZipCode", c => c.String(maxLength: 10));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "ZipCode");
            DropColumn("dbo.Users", "State");
            DropColumn("dbo.Users", "City");
            DropColumn("dbo.Users", "MailingAddress");
            DropColumn("dbo.Users", "County");
        }
    }
}

namespace API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WebsiteExt2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Websites", "OwnerLogin", c => c.String());
            DropColumn("dbo.Websites", "Locale");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Websites", "Locale", c => c.Int(nullable: false));
            DropColumn("dbo.Websites", "OwnerLogin");
        }
    }
}

namespace API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WebsiteExt : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Websites", "BaseUrl", c => c.String());
            AddColumn("dbo.Websites", "ServerRelativeUrl", c => c.String());
            DropColumn("dbo.Websites", "Url");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Websites", "Url", c => c.String());
            DropColumn("dbo.Websites", "ServerRelativeUrl");
            DropColumn("dbo.Websites", "BaseUrl");
        }
    }
}

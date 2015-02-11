namespace API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WebsiteExt1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Urls", "SiteCreationEnabled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Urls", "SiteCreationEnabled");
        }
    }
}

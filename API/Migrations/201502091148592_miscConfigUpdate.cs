namespace API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class miscConfigUpdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Configs", "Miscellanous_AllowBetaFeatures", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Configs", "Miscellanous_AllowBetaFeatures");
        }
    }
}

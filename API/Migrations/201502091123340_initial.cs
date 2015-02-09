namespace API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Configs",
                c => new
                    {
                        ConfigID = c.Int(nullable: false, identity: true),
                        Miscellanous_CacheRefreshInterval = c.Time(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.ConfigID);
            
            CreateTable(
                "dbo.Urls",
                c => new
                    {
                        Path = c.String(nullable: false, maxLength: 128),
                        IsCritical = c.Boolean(nullable: false),
                        IsOffice365 = c.Boolean(nullable: false),
                        IsServiceUrl = c.Boolean(nullable: false),
                        Config_ConfigID = c.Int(),
                        Config_ConfigID1 = c.Int(),
                    })
                .PrimaryKey(t => t.Path)
                .ForeignKey("dbo.Configs", t => t.Config_ConfigID)
                .ForeignKey("dbo.Configs", t => t.Config_ConfigID1)
                .Index(t => t.Config_ConfigID)
                .Index(t => t.Config_ConfigID1);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Urls", "Config_ConfigID1", "dbo.Configs");
            DropForeignKey("dbo.Urls", "Config_ConfigID", "dbo.Configs");
            DropIndex("dbo.Urls", new[] { "Config_ConfigID1" });
            DropIndex("dbo.Urls", new[] { "Config_ConfigID" });
            DropTable("dbo.Urls");
            DropTable("dbo.Configs");
        }
    }
}

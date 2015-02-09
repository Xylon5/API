namespace API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initialSiteTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Websites",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        Title = c.String(),
                        Url = c.String(),
                        Type = c.Int(nullable: false),
                        IsOffice365 = c.Boolean(nullable: false),
                        Locale = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Websites");
        }
    }
}

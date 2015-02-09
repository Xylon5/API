namespace API.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using API.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<API.ConfigDBContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(API.ConfigDBContext context)
        {
            context.Configurations.AddOrUpdate(
                x => x.ConfigID,
                new Config()
                {
                    ConfigID = 1,
                    Urls =
                        new System.Collections.Generic.List<Url>()
                {
                    new Url(){ IsCritical=true, IsOffice365=false, IsServiceUrl=false, Path="http://vrmu009:8000"},
                    new Url(){ IsCritical=true, IsOffice365=false, IsServiceUrl=false, Path="http://intranet.vdrmu009.loc"}
                },
                    EndPoints =
                          new System.Collections.Generic.List<Url>()
                {
                    new Url(){ IsCritical=true, IsOffice365=true, IsServiceUrl=true, Path="sb://myServicebus"}
                },
                    Miscellanous = new MiscConfigs()
                    {
                        CacheRefreshInterval = new TimeSpan(0, 5, 0),
                        AllowBetaFeatures = true
                    }
                }
                );

            context.Websites.AddOrUpdate(
                x => x.ID,
                new Website() { ID = Guid.NewGuid(), CanBeDeleted = false, IsOffice365 = true, Locale = 1031, Title = "RMU Dev Site", Type = WebsiteType.Administration, Url = "https://rmumsdn.sharepoint.com/SitePages/DevHome.aspx" }
                );

            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}

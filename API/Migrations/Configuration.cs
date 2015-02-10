namespace API.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using API.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<API.ApiDBContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(API.ApiDBContext context)
        {
            context.Configurations.AddOrUpdate(
                x => x.ConfigID,
                new Config()
            {
                ConfigID = 1,
                Urls =
                        new System.Collections.Generic.List<Url>()
                {
                    new Url(){ IsCritical=true, IsOffice365=false, IsServiceUrl=false, Path="http://vrmu009:8000", SiteCreationEnabled=false},
                    new Url(){ IsCritical=true, IsOffice365=false, IsServiceUrl=false, Path="http://intranet.vdrmu009.loc", SiteCreationEnabled=true}
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
                new Website() { ID = new Guid("{DAD8B015-8E2C-41AF-AEC8-988247376AA4}"), CanBeDeleted = false, IsOffice365 = true, Locale = 1031, Title = "RMU Dev Site",
                    Type = WebsiteType.Administration, BaseUrl = "https://rmumsdn.sharepoint.com", ServerRelativeUrl = "/SitePages/DevHome.aspx" }
                );
        }
    }
}

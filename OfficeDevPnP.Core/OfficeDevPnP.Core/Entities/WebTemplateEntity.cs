﻿namespace OfficeDevPnP.Core.Entities
{
    public class WebTemplateEntity
    {

        /// <summary>
        /// Language code, use 'all' or leave empty when not relevant
        /// </summary>
        public string LanguageCode
        {
            get;
            set;
        }

        /// <summary>
        /// Template name in format of BLOG#0
        /// </summary>
        public string TemplateName
        {
            get;
            set;
        }
    }
}

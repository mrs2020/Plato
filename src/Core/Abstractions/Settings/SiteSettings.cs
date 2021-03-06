﻿using Microsoft.AspNetCore.Routing;
using PlatoCore.Abstractions.Routing;

namespace PlatoCore.Abstractions.Settings
{
    public sealed class SiteSettings : Serializable, ISiteSettings
    {

        public string SiteName { get; set; } = "Plato";

        public string SiteSalt { get; set; }

        public string Calendar { get; set; }

        public string Culture { get; set; } = "en-US";

        public int MaxPagedCount { get; set; }

        public int MaxPageSize { get; set; }

        public int PageSize { get; set; }

        public string PageTitleSeparator { get; set; }
   
        public string SuperUser { get; set; }

        public string TimeZone { get; set; }

        public string DateTimeFormat { get; set; } = "G";

        public string BaseUrl { get; set; }

        public bool UseCdn { get; set; }

        public HomeRoute HomeRoute { get; set; }

        public string HomeAlias { get; set; } = "support";

        public string Theme { get; set; }

        public string ApiKey { get; set; }

    }

}

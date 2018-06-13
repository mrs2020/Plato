﻿using Microsoft.AspNetCore.Routing;

namespace Plato.Internal.Abstractions.Settings
{
    public interface ISiteSettings : ISerializable
    {

        string SiteName { get; set; }

        string SiteSalt { get; set; }
        
        string PageTitleSeparator { get; set; }

        string SuperUser { get; set; }

        string Culture { get; set; }

        string Calendar { get; set; }

        string TimeZone { get; set; }

        //ResourceDebugMode ResourceDebugMode { get; set; }

        bool UseCdn { get; set; }

        int PageSize { get; set; }

        int MaxPageSize { get; set; }

        int MaxPagedCount { get; set; }

        string BaseUrl { get; set; }

        RouteValueDictionary HomeRoute { get; set; }

        string ThemeName { get; set; }

    }
}
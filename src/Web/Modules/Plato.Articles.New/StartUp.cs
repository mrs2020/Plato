﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PlatoCore.Models.Shell;
using PlatoCore.Hosting.Abstractions;
using PlatoCore.Layout.ViewAdapters.Abstractions;
using Plato.Articles.New.ViewAdapters;

namespace Plato.Articles.New
{
    public class Startup : StartupBase
    {
        private readonly IShellSettings _shellSettings;

        public Startup(IShellSettings shellSettings)
        {
            _shellSettings = shellSettings;
        }

        public override void ConfigureServices(IServiceCollection services)
        {

            // Register view adapters        
            services.AddScoped<IViewAdapterProvider, ArticleListItemViewAdapter>();
            services.AddScoped<IViewAdapterProvider, ArticleViewAdapter>();
            //services.AddScoped<IViewAdapterProvider, ArticleListViewAdapter>();
            //services.AddScoped<IViewAdapterProvider, ArticleCommentListViewAdapter>();
            //services.AddScoped<IViewAdapterProvider, ArticleCommentListItemViewAdapter>();

        }

        public override void Configure(
            IApplicationBuilder app,
            IRouteBuilder routes,
            IServiceProvider serviceProvider)
        {
        }

    }

}
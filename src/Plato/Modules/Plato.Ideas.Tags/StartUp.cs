﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Plato.Ideas.Models;
using Plato.Ideas.Tags.Badges;
using Plato.Ideas.Tags.Models;
using Plato.Ideas.Tags.Navigation;
using Plato.Ideas.Tags.Tasks;
using Plato.Ideas.Tags.ViewAdapters;
using Plato.Ideas.Tags.ViewProviders;
using Plato.Internal.Badges.Abstractions;
using Plato.Internal.Models.Shell;
using Plato.Internal.Hosting.Abstractions;
using Plato.Internal.Layout.ViewAdapters;
using Plato.Internal.Layout.ViewProviders;
using Plato.Internal.Models.Badges;
using Plato.Internal.Navigation.Abstractions;
using Plato.Internal.Notifications;
using Plato.Internal.Notifications.Abstractions;
using Plato.Internal.Tasks.Abstractions;
using Plato.Tags.Repositories;
using Plato.Tags.Services;
using Plato.Tags.Stores;

namespace Plato.Ideas.Tags
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
            
            // Register navigation provider
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<INavigationProvider, SiteMenu>();
            services.AddScoped<INavigationProvider, QuestionFooterMenu>();
            services.AddScoped<INavigationProvider, IdeaCommentFooterMenu>();
            
            // Discuss view providers
            services.AddScoped<IViewProviderManager<Idea>, ViewProviderManager<Idea>>();
            services.AddScoped<IViewProvider<Idea>, QuestionViewProvider>();
            services.AddScoped<IViewProviderManager<IdeaComment>, ViewProviderManager<IdeaComment>>();
            services.AddScoped<IViewProvider<IdeaComment>, AnswerViewProvider>();
        
            // Admin view providers
            services.AddScoped<IViewProviderManager<TagAdmin>, ViewProviderManager<TagAdmin>>();
            services.AddScoped<IViewProvider<TagAdmin>, AdminViewProvider>();

            // Tag view providers
            services.AddScoped<IViewProviderManager<Tag>, ViewProviderManager<Tag>>();
            services.AddScoped<IViewProvider<Tag>, TagViewProvider>();
         
            // Register view adapters
            services.AddScoped<IViewAdapterProvider, QuestionListItemViewAdapter>();

            // Badge providers
            services.AddScoped<IBadgesProvider<Badge>, TagBadges>();

            // Background tasks
            services.AddScoped<IBackgroundTaskProvider, TagBadgesAwarder>();

            // Notification manager
            services.AddScoped<INotificationManager<Badge>, NotificationManager<Badge>>();

            // Data access
            services.AddScoped<ITagRepository<Tag>, TagRepository<Tag>>();
            services.AddScoped<ITagStore<Tag>, TagStore<Tag>>();
            services.AddScoped<ITagService<Tag>, TagService<Tag>>();
            services.AddScoped<ITagManager<Tag>, TagManager<Tag>>();
            
        }

        public override void Configure(
            IApplicationBuilder app,
            IRouteBuilder routes,
            IServiceProvider serviceProvider)
        {

            // Tag Index
            routes.MapAreaRoute(
                name: "QuestionsTagIndex",
                areaName: "Plato.Ideas.Tags",
                template: "questions/tags/{pager.offset:int?}/{opts.search?}",
                defaults: new { controller = "Home", action = "Index" }
            );

            // Tag Entities
            routes.MapAreaRoute(
                name: "QuestionsTagDisplay",
                areaName: "Plato.Ideas.Tags",
                template: "questions/tag/{opts.tagId:int}/{opts.alias}/{pager.offset:int?}",
                defaults: new { controller = "Home", action = "Display" }
            );
            
        }

    }

}
﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plato.Docs.Models;
using Plato.Entities.Services;
using Plato.Entities.Stores;
using Plato.Entities.ViewModels;
using PlatoCore.Layout.Views.Abstractions;
using PlatoCore.Navigation.Abstractions;
using PlatoCore.Security.Abstractions;

namespace Plato.Docs.ViewComponents
{

    public class GetDocCommentListViewComponent : ViewComponentBase
    {
        
        private readonly IEntityReplyService<DocComment> _replyService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IEntityStore<Doc> _entityStore;

        public GetDocCommentListViewComponent(
            IEntityReplyService<DocComment> replyService, 
            IAuthorizationService authorizationService,
            IEntityStore<Doc> entityStore)
        {
            _authorizationService = authorizationService;
            _replyService = replyService;
            _entityStore = entityStore;   
        }

        public async Task<IViewComponentResult> InvokeAsync(EntityOptions options, PagerOptions pager)
        {

            if (options == null)
            {
                options = new EntityOptions();
            }

            if (pager == null)
            {
                pager = new PagerOptions();
            }
            
            return View(await GetViewModel(options, pager));

        }

        async Task<EntityViewModel<Doc, DocComment>> GetViewModel(EntityOptions options, PagerOptions pager)
        {

            var topic = await _entityStore.GetByIdAsync(options.Id);
            if (topic == null)
            {
                throw new ArgumentNullException();
            }

            var results = await _replyService
                .ConfigureQuery(async q =>
                {

                    // Hide private?
                    if (!await _authorizationService.AuthorizeAsync(HttpContext.User,
                        Permissions.ViewHiddenDocs))
                    {
                        q.HideHidden.True();
                    }

                    // Hide spam?
                    if (!await _authorizationService.AuthorizeAsync(HttpContext.User,
                        Permissions.ViewSpamDocs))
                    {
                        q.HideSpam.True();
                    }

                    // Hide deleted?
                    if (!await _authorizationService.AuthorizeAsync(HttpContext.User,
                        Permissions.ViewDeletedDocs))
                    {
                        q.HideDeleted.True();
                    }



                })
                .GetResultsAsync(options, pager);

            // Set total on pager
            pager.SetTotal(results?.Total ?? 0);

            // Return view model
            return new EntityViewModel<Doc, DocComment>
            {
                Options = options,
                Pager = pager,
                Entity = topic,
                Replies = results
            };

        }

    }

}
  
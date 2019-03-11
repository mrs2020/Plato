﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Plato.Internal.Hosting.Abstractions;
using Plato.Internal.Models.Users;

namespace Plato.Users.Middleware
{

    public class AuthenticatedUserMiddleware
    {
        static readonly object SyncLock = new object();
  
        private readonly RequestDelegate _next;

        public AuthenticatedUserMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context)
        {

            // If the request is not authenticated move along
            if (!context.User.Identity.IsAuthenticated)
            {
                await _next.Invoke(context);
            }
            
            // Hydrate HttpContext.Features with our user
            await HydrateHttpContextFeature(context);

            // Return next delegate
            await _next.Invoke(context);
            
        }

        #region "Private Methods"

        async Task HydrateHttpContextFeature(HttpContext context)
        {

            // Get context facade
            var contextFacade = context.RequestServices.GetRequiredService<IContextFacade>();
            if (contextFacade == null)
            {
                return;
            }

            // Attempt tto get user from data store
            var user = await contextFacade.GetAuthenticatedUserAsync();

            // User not found
            if (user == null)
            {
                return;
            }

            //lock (SyncLock)
            //{
                // Add authenticated user to features for subsequent use
                context.Features[typeof(User)] = user;
            //}

        }
        
        #endregion

    }

}

﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Plato.Internal.Layout.ViewProviders;
using Plato.Users.reCAPTCHA2.Models;
using Plato.Users.reCAPTCHA2.Services;
using Plato.Users.reCAPTCHA2.Stores;
using Plato.Users.reCAPTCHA2.ViewModels;
using Plato.Users.ViewModels;

namespace Plato.Users.reCAPTCHA2.ViewProviders
{
    public class LoginViewProvider : BaseViewProvider<LoginViewModel>
    {

        private readonly IReCaptchaService _recaptchaService;
        private readonly HttpRequest _request;
        private readonly IReCaptchaSettingsStore<ReCaptchaSettings> _recaptchaSettingsStore;

        public LoginViewProvider(
            IHttpContextAccessor httpContextAccessor,
            IReCaptchaService recaptchaService,
            IReCaptchaSettingsStore<ReCaptchaSettings> recaptchaSettingsStore)
        {
            _recaptchaService = recaptchaService;
            _recaptchaSettingsStore = recaptchaSettingsStore;
            _request = httpContextAccessor.HttpContext.Request;
        }

        public override async Task<IViewProviderResult> BuildIndexAsync(LoginViewModel viewModel,
            IViewProviderContext context)
        {

            // Get reCAPTCHA settings
            var settings = await _recaptchaSettingsStore.GetAsync();

            // Build view model
            var recaptchaViewModel = new ReCaptchaViewModel()
            {
                SiteKey = settings?.SiteKey ?? "",
                Secret = settings?.Secret ?? ""
            };

            // Build view
            return Views(
                View<ReCaptchaViewModel>("Register.Google.reCAPTCHA2", model => recaptchaViewModel).Zone("content")
                    .Order(int.MaxValue)
            );

        }

        public override Task<IViewProviderResult> BuildDisplayAsync(LoginViewModel viewModel,
            IViewProviderContext context)
        {
            return Task.FromResult(default(IViewProviderResult));
        }

        public override Task<IViewProviderResult> BuildEditAsync(LoginViewModel viewModel, IViewProviderContext context)
        {
            return Task.FromResult(default(IViewProviderResult));
        }

        public override async Task<IViewProviderResult> BuildUpdateAsync(LoginViewModel viewModel,
            IViewProviderContext context)
        {

            if (await ValidateModelAsync(viewModel, context.Updater))
            {

                var recaptchaResponse = _request.Form["g-recaptcha-response"];

                // No valid posted - user has probably not ticked check box
                if (String.IsNullOrEmpty(recaptchaResponse))
                {
                    context.Updater.ModelState.AddModelError(string.Empty, "You must indicate your not a robot!");
                    return await BuildIndexAsync(viewModel, context);
                }

                // Validate posted recaptcha response
                var response = await _recaptchaService.Validate(recaptchaResponse);

                // No response received
                if (response == null)
                {
                    context.Controller.ModelState.AddModelError(string.Empty,
                        "Sorry but a problem occurred communicating with the Google reCaptcha service. Please try again!");
                    return await BuildIndexAsync(viewModel, context);
                }

                // Response was invalid
                if (response.ErrorCodes.Count > 0)
                {
                    context.Controller.ModelState.AddModelError(string.Empty,
                        "Sorry we could not validate you are human. Response from Google: " +
                        String.Join(Environment.NewLine, response.ErrorCodes.Select(c => c).ToArray())); 
                    return await BuildIndexAsync(viewModel, context);
                }

            }
            
            return await BuildIndexAsync(viewModel, context);

        }
        
    }

}

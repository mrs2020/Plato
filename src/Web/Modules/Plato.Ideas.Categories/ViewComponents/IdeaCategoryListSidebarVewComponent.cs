﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Plato.Categories.Services;
using Plato.Categories.ViewModels;
using Plato.Ideas.Categories.Models;
using PlatoCore.Navigation.Abstractions;

namespace Plato.Ideas.Categories.ViewComponents
{

    public class IdeaCategoryListSidebarViewComponent : ViewComponent
    {

        private readonly ICategoryService<Category> _categoryService;
        
        public IdeaCategoryListSidebarViewComponent(ICategoryService<Category> categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IViewComponentResult> InvokeAsync(CategoryIndexOptions options)
        {

            if (options == null)
            {
                options = new CategoryIndexOptions();
            }
            
            return View(await GetIndexModel(options));

        }
        
        async Task<CategoryListViewModel<Category>> GetIndexModel(CategoryIndexOptions options)
        {

            // Get categories
            var categories = await _categoryService
                .GetResultsAsync(new CategoryIndexOptions()
                {
                    FeatureId = options.FeatureId,
                    CategoryId = 0
                }, new PagerOptions()
                {
                    Page = 1,
                    Size = int.MaxValue,
                    CountTotal = false
                });
            
            return new CategoryListViewModel<Category>()
            {
                Options = options,
                Categories = categories?.Data?.Where(c => c.ParentId == 0)
            };
        }


    }


}

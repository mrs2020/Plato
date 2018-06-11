﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Plato.Layout.ViewAdaptors;

namespace Plato.Layout.Views
{
    public class ViewDisplayContext
    {
        public IServiceProvider ServiceProvider { get; set; }

        public IViewDisplayHelper DisplayAsync { get; set; }

        public ViewContext ViewContext { get; set; }

        public IEnumerable<IViewAdaptorResult> ViewAdaptorResults { get; set; }

        public object Value { get; set; }
        
        public ViewDescriptor ViewDescriptor { get; set; }
    }
}
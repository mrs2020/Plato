﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;

namespace Plato.Internal.Layout.ViewAdaptors
{
    public interface IViewAdaptorResult
    {

        IViewAdaptorBuilder Builder { get; set; }

        IList<Func<IHtmlContent, IHtmlContent>> OutputAlterations { get; set; }

        IList<string> ViewAlterations { get; set; }

        IList<Func<object, object>> ModelAlterations { get; set; }

    }

}
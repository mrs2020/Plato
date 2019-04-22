﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;

namespace Plato.Entities.Models
{

    public class EntityTreeOptions
    {
         
        public IEnumerable<IEntity> Entities { get; set; }

        public int SelectedEntity { get; set; }

        public string HtmlName { get; set; }

        public bool EnableCheckBoxes { get; set; }

        public string EditMenuViewName { get; set; }

        public string CssClass { get; set; }

        public RouteValueDictionary RouteValues { get; set; }

    }

}
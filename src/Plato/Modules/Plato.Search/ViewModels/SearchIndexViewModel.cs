﻿using System.Collections.Generic;
using System.Runtime.Serialization;
using Plato.Entities.Models;
using Plato.Internal.Data.Abstractions;
using Plato.Internal.Navigation;

namespace Plato.Search.ViewModels
{
    public class SearchIndexViewModel
    {
        
        public IPagedResults<Entity> Results { get; set; }

        public PagerOptions Pager { get; set; } = new PagerOptions();

        public SearchIndexOptions Options { get; set; } = new SearchIndexOptions();

        public IList<SortColumn> SortColumns { get; set; }

        public IList<SortOrder> SortOrder { get; set; }

        public IList<Filter> Filters { get; set; }
        
    }
    
    public class SearchIndexOptions
    {

        [DataMember(Name = "search")]
        public string Search { get; set; }

        [DataMember(Name = "feature")]
        public int FeatureId { get; set; }

        [DataMember(Name = "within")]
        public string Within { get; set; }
        
        [DataMember(Name = "filter")]
        public FilterBy Filter { get; set; } = FilterBy.All;
        
        [DataMember(Name = "sort")]
        public SortBy Sort { get; set; } = SortBy.Auto;

        [DataMember(Name = "order")]
        public OrderBy Order { get; set; } = OrderBy.Desc;
        
    }

    public class SortColumn
    {
        public string Text { get; set; }

        public SortBy Value { get; set; }

    }

    public class SortOrder
    {
        public string Text { get; set; }

        public OrderBy Value { get; set; }

    }

    public class Filter
    {
        public string Text { get; set; }

        public FilterBy Value { get; set; }

    }

    public enum SortBy
    {
        Auto = 0,
        Rank = 1,
        LastReply = 2,
        Replies = 3,
        Views = 4,
        Participants = 5,
        Reactions = 6,
        Created = 7,
        Modified = 8
    }

    public enum FilterBy
    {
        All = 0,
        MyTopics = 1,
        Participated = 2,
        Following = 3,
        Starred = 4,
        Unanswered = 5,
        NoReplies = 6
    }

}

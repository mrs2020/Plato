﻿using System;
using PlatoCore.Abstractions;
using PlatoCore.Models.Users;

namespace Plato.Docs.Categories.Models
{

    public class CategoryDetails : Serializable
    {
        public int TotalEntities { get; set; }

        public int TotalReplies { get; set; }

        public LatestPost LatestEntity { get; set; } = new LatestPost();

        public LatestPost LatestReply { get; set; } = new LatestPost();

        public bool Closed { get; set; }

    }
    
    public class LatestPost
    {
        public int Id { get; set; }

        public string Alias { get; set; }

        public ISimpleUser CreatedBy { get; set; } = new SimpleUser();
        
        public DateTimeOffset? CreatedDate { get; set; }

    }

}

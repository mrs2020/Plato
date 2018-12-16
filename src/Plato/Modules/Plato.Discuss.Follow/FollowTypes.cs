﻿using System.Collections.Generic;
using Plato.Follows.Models;
using Plato.Follows.Services;

namespace Plato.Discuss.Follow
{

    public class FollowTypes : IFollowTypeProvider
    {
        
        public static readonly FollowType Topic =
            new FollowType(
                "Topic",
                "Follow Topic",
                "Folow this topic to get notified when replies are posted...",
                "Unsubscribe",
                "You are already following this topic. Unsubscribe below...");

        public IEnumerable<IFollowType> GetFollowTypes()
        {
            return new[]
            {
                Topic
            };
        }

    }
}
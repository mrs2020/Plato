﻿using System.Collections.Generic;
using Plato.Internal.Badges.Abstractions;
using Plato.Internal.Models.Badges;

namespace Plato.Issues.Badges
{
    public class IssueCommentBadges : IBadgesProvider<Badge>
    {

        public static readonly Badge First =
            new Badge("First Comment", "Posted an issue comment", "fal fa-comment", BadgeLevel.Bronze, 1, 0);

        public static readonly Badge Bronze =
            new Badge("Assistant", "Posted several issue comments", "fal fa-comment", BadgeLevel.Bronze, 25, 5);

        public static readonly Badge Silver =
            new Badge("Resolver", "Contributed to several issues", "fal fa-comment", BadgeLevel.Silver, 50, 10);
        
        public static readonly Badge Gold =
            new Badge("Diligent Resolver", "Contributed to dozens of issues", "far fa-comment", BadgeLevel.Gold, 100, 20);

        public IEnumerable<Badge> GetBadges()
        {
            return new[]
            {
                First,
                Bronze,
                Silver,
                Gold
            };

        }
        
    }

}
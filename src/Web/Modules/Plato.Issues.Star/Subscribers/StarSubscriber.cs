﻿using System;
using System.Threading.Tasks;
using Plato.Issues.Models;
using Plato.Entities.Stores;
using PlatoCore.Messaging.Abstractions;
using PlatoCore.Reputations.Abstractions;

namespace Plato.Issues.Star.Subscribers
{

    /// <summary>
    /// Awards reputation & updates star count for docs when users star or delete doc stars.
    /// </summary>
    public class StarSubscriber : IBrokerSubscriber
    {
         
        private readonly IUserReputationAwarder _reputationAwarder;
        private readonly IEntityStore<Issue> _entityStore;
        private readonly IBroker _broker;

        public StarSubscriber(
            IUserReputationAwarder reputationAwarder,
            IEntityStore<Issue> entityStore,
            IBroker broker)
        {           
            _reputationAwarder = reputationAwarder;
            _entityStore = entityStore;
            _broker = broker;
        }

        public void Subscribe()
        {
 
            // Created
            _broker.Sub<Stars.Models.Star>(new MessageOptions()
            {
                Key = "StarCreated"
            }, async message => await StarCreated(message.What));
            
            // Updated
            _broker.Sub<Stars.Models.Star>(new MessageOptions()
            {
                Key = "StarDeleted"
            }, async message => await StarDeleted(message.What));

        }

        public void Unsubscribe()
        {
            // Created
            _broker.Unsub<Stars.Models.Star>(new MessageOptions()
            {
                Key = "StarCreated"
            }, async message => await StarCreated(message.What));

            // Updated
            _broker.Unsub<Stars.Models.Star>(new MessageOptions()
            {
                Key = "StarDeleted"
            }, async message => await StarDeleted(message.What));

        }

        private async Task<Stars.Models.Star> StarCreated(Stars.Models.Star star)
        {

            if (star == null)
            {
                return null;
            }

            // Is this a doc star?
            if (!star.Name.Equals(StarTypes.Issue.Name, StringComparison.OrdinalIgnoreCase))
            {
                return star;
            }
            
            // Ensure the entity we are starring exists
            var entity = await _entityStore.GetByIdAsync(star.ThingId);
            if (entity == null)
            {
                return star;
            }

            // Update total stars
            entity.TotalStars = entity.TotalStars + 1;

            // Persist changes
            var updatedEntity = await _entityStore.UpdateAsync(entity);
            if (updatedEntity != null)
            {
                // Award reputation to user starring the entity
                await _reputationAwarder.AwardAsync(Reputations.StarIssue, star.CreatedUserId, "Starred an issue");

                // Award reputation to entity author when there entity is starred
                await _reputationAwarder.AwardAsync(Reputations.StarredIssue, entity.CreatedUserId, "Someone starred my issue");

            }

            return star;

        }

        private async Task<Stars.Models.Star> StarDeleted(Stars.Models.Star star)
        {

            if (star == null)
            {
                return null;
            }

            // Is this a doc star?
            if (!star.Name.Equals(StarTypes.Issue.Name, StringComparison.OrdinalIgnoreCase))
            {
                return star;
            }

            // Ensure the entity we are starring exists
            var entity = await _entityStore.GetByIdAsync(star.ThingId);
            if (entity == null)
            {
                return star;
            }

            // Update total stars
            entity.TotalStars = entity.TotalStars - 1;
        
            // Ensure we don't go negative
            if (entity.TotalStars < 0)
            {
                entity.TotalStars = 0;
            }
            
            // Persist changes
            var updatedEntity = await _entityStore.UpdateAsync(entity);
            if (updatedEntity != null)
            {
                // Revoke reputation from user removing the entity star
                await _reputationAwarder.RevokeAsync(Reputations.StarIssue, star.CreatedUserId, "Unstarred an issue");

                // Revoke reputation from entity author for user removing there entity star
                await _reputationAwarder.RevokeAsync(Reputations.StarredIssue, entity.CreatedUserId, "A user unstarred my issue");

            }
            
            return star;

        }

    }

}

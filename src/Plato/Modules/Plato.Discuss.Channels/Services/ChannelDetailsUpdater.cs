﻿using System.Linq;
using System.Threading.Tasks;
using Plato.Categories.Models;
using Plato.Categories.Services;
using Plato.Categories.Stores;
using Plato.Discuss.Channels.Models;
using Plato.Discuss.Models;
using Plato.Entities.Stores;
using Plato.Internal.Data.Abstractions;

namespace Plato.Discuss.Channels.Services
{
    
    public class ChannelDetailsUpdater : IChannelDetailsUpdater
    {

        private readonly ICategoryManager<Channel> _channelManager;
        private readonly ICategoryStore<Channel> _channelStore;
        private readonly IEntityReplyStore<Reply> _replyStore;
        private readonly IEntityStore<Topic> _topicStore;

        public ChannelDetailsUpdater(
            ICategoryStore<Channel> channelStore, 
            ICategoryManager<Channel> channelManager,
            IEntityStore<Topic> topicStore, 
            IEntityReplyStore<Reply> replyStore)
        {
            _channelStore = channelStore;
            _channelManager = channelManager;
            _topicStore = topicStore;
            _replyStore = replyStore;
        }
        
        public async Task UpdateAsync(int channelId)
        {
            
            // Get supplied channel and all parent channels
            var parents = await _channelStore.GetParentsByIdAsync(channelId);

            // Update details within current and all parents
            foreach (var parent in parents)
            {
                
                // Get all children for current channel
                var children = await _channelStore.GetChildrenByIdAsync(parent.Id);

                // Get latest topic & total topic count for current channel
                var topics = await _topicStore.QueryAsync()
                    .Take(1, 1) // we only need the latest topic
                    .Select<EntityQueryParams>(q =>
                    {

                        // Include entities from child channels?
                        if (children != null)
                        {
                            var channelIds = children
                                .Select(c => c.Id)
                                .Append(parent.Id)
                                .ToArray();
                            q.CategoryId.IsIn(channelIds);
                        }
                        else
                        {
                            // Get topics for current channel
                            q.CategoryId.Equals(parent.Id);
                        }

                        q.HideSpam.True();
                        q.HidePrivate.True();
                        q.HideDeleted.True();
                    })
                    .OrderBy("LastReplyDate", OrderBy.Desc)
                    .ToList();

                // Get latest reply & total reply count for current channel
                var replies = await _replyStore.QueryAsync()
                    .Take(1, 1) // we only need the latest reply
                    .Select<EntityReplyQueryParams>(q =>
                    {

                        // Include entities from child channels?
                        if (children != null)
                        {
                            var channelIds = children
                                .Select(c => c.Id)
                                .Append(parent.Id)
                                .ToArray();
                            q.CategoryId.IsIn(channelIds);
                        }
                        else
                        {
                            // Get topics for current channel
                            q.CategoryId.Equals(parent.Id);
                        }

                        q.HideSpam.True();
                        q.HidePrivate.True();
                        q.HideDeleted.True();
                    })
                    .OrderBy("CreatedDate", OrderBy.Desc)
                    .ToList();
                
                var totalEntities = 0;
                Topic latestEntity = null;
                if (topics?.Data != null)
                {
                    totalEntities = topics.Total;
                    latestEntity = topics.Data[0];
                }

                var totalReplies = 0;
                Reply latestReply = null;
                if (replies?.Data != null)
                {
                    totalReplies = replies.Total;
                    latestReply = replies.Data[0];
                }

                // Update channel details with latest entity details
                var details = parent.GetOrCreate<ChannelDetails>();
                details.TotalEntities = totalEntities;
                details.TotalReplies = totalReplies;

                if (latestEntity != null)
                {
                    details.LatestEntity.Id = latestEntity.Id;
                    details.LatestEntity.Alias = latestEntity.Alias;
                    details.LatestEntity.CreatedBy = latestEntity.CreatedBy;
                    details.LatestEntity.CreatedDate = latestEntity.CreatedDate;
                }

                if (latestReply != null)
                {
                    details.LatestReply.Id = latestReply.Id;
                    details.LatestReply.CreatedBy = latestReply.CreatedBy;
                    details.LatestReply.CreatedDate = latestReply.CreatedDate;
                }

                parent.AddOrUpdate<ChannelDetails>(details);

                // Save the updated details 
                await _channelManager.UpdateAsync(parent);

            }
            
        }

    }

}

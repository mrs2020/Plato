﻿using System.Threading.Tasks;
using Plato.Discuss.Models;
using Plato.Entities.Services;
using Plato.Internal.Abstractions;
using Plato.Internal.Hosting.Abstractions;

namespace Plato.Discuss.Services
{
    public class TopicManager : IPostManager<Topic>
    {

        private readonly IEntityManager<Topic> _entityManager;
        private readonly IContextFacade _contextFacade;

        public TopicManager(
            IEntityManager<Topic> entityManager, 
            IContextFacade contextFacade)
        {
            _entityManager = entityManager;
            _contextFacade = contextFacade;
        }

        public async Task<IActivityResult<Topic>> CreateAsync(Topic model)
        {
            if (model.FeatureId == 0)
            {
                var feature = await _contextFacade.GetFeatureByAreaAsync();
                if (feature != null)
                {
                    model.FeatureId = feature.Id;
                }
            }

            return await _entityManager.CreateAsync(model);
        }

        public async Task<IActivityResult<Topic>> UpdateAsync(Topic model)
        {

            if (model.FeatureId == 0)
            {
                var feature = await _contextFacade.GetFeatureByAreaAsync();
                if (feature != null)
                {
                    model.FeatureId = feature.Id;
                }
            }

            return await _entityManager.UpdateAsync(model);
        }

        public async Task<IActivityResult<Topic>> DeleteAsync(int id)
        {
            return await _entityManager.DeleteAsync(id);
        }
    }
}
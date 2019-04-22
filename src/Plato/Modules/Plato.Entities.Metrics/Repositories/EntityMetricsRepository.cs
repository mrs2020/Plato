﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plato.Entities.Metrics.Models;
using Plato.Internal.Abstractions.Extensions;
using Plato.Internal.Data.Abstractions;

namespace Plato.Entities.Metrics.Repositories
{
    
    public class EntityMetricsRepository : IEntityMetricsRepository<EntityMetric>
    {

        private readonly IDbContext _dbContext;
        private readonly ILogger<EntityMetricsRepository> _logger;

        public EntityMetricsRepository(
            IDbContext dbContext,
            ILogger<EntityMetricsRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        #region "Implementation"

        public async Task<EntityMetric> InsertUpdateAsync(EntityMetric entityMetric)
        {
            if (entityMetric == null)
            {
                throw new ArgumentNullException(nameof(entityMetric));
            }

            var id = await InsertUpdateInternal(
                entityMetric.Id,
                entityMetric.EntityId,
                entityMetric.IpV4Address,
                entityMetric.IpV6Address,
                entityMetric.UserAgent,
                entityMetric.CreatedUserId,
                entityMetric.CreatedDate);

            if (id > 0)
            {
                // return
                return await SelectByIdAsync(id);
            }

            return null;
        }

        public async Task<EntityMetric> SelectByIdAsync(int id)
        {
            EntityMetric output = null;
            using (var context = _dbContext)
            {
                output = await context.ExecuteReaderAsync<EntityMetric>(
                    CommandType.StoredProcedure,
                    "SelectEntityMetricById",
                    async reader =>
                    {
                        if ((reader != null) && (reader.HasRows))
                        {
                            await reader.ReadAsync();
                            output = new EntityMetric();
                            output.PopulateModel(reader);
                        }

                        return output;
                    },
                    id);

            }

            return output;

        }

        public async Task<IPagedResults<EntityMetric>> SelectAsync(params object[] inputParams)
        {
            IPagedResults<EntityMetric> output = null;
            using (var context = _dbContext)
            {
                output = await context.ExecuteReaderAsync<IPagedResults<EntityMetric>>(
                    CommandType.StoredProcedure,
                    "SelectEntityMetricsPaged",
                    async reader =>
                    {
                        if ((reader != null) && (reader.HasRows))
                        {
                            output = new PagedResults<EntityMetric>();
                            while (await reader.ReadAsync())
                            {
                                var metric = new EntityMetric();
                                metric.PopulateModel(reader);
                                output.Data.Add(metric);
                            }

                            if (await reader.NextResultAsync())
                            {
                                if (reader.HasRows)
                                {
                                    await reader.ReadAsync();
                                    output.PopulateTotal(reader);
                                }
                            }

                        }

                        return output;
                    },
                    inputParams);
            
            }

            return output;

        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation($"Deleting metric with id: {id}");
            }

            var success = 0;
            using (var context = _dbContext)
            {
                success = await context.ExecuteScalarAsync<int>(
                    CommandType.StoredProcedure,
                    "DeleteEntityMetricById", id);
            }

            return success > 0 ? true : false;
        }
        
        #endregion

        #region "Private Methods"

        async Task<int> InsertUpdateInternal(
            int id,
            int entityId,
            string ipV4Address,
            string ipV6Address,
            string userAgent,
            int createdUserId,
            DateTimeOffset? createdDate)
        {

            var emailId = 0;
            using (var context = _dbContext)
            {
                emailId = await context.ExecuteScalarAsync<int>(
                    CommandType.StoredProcedure,
                    "InsertUpdateEntityMetric",
                    id,
                    entityId,
                    ipV4Address.TrimToSize(20).ToEmptyIfNull(),
                    ipV6Address.TrimToSize(50).ToEmptyIfNull(),
                    userAgent.TrimToSize(255).ToEmptyIfNull(),
                    createdUserId,
                    createdDate.ToDateIfNull(),
                    new DbDataParameter(DbType.Int32, ParameterDirection.Output)
                );
            }

            return emailId;

        }

        #endregion

    }

}
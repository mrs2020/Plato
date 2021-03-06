﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plato.Stars.Models;
using PlatoCore.Abstractions.Extensions;
using PlatoCore.Data.Abstractions;

namespace Plato.Stars.Repositories
{

    public class StarRepository : IStarRepository<Star>
    {

        private readonly IDbContext _dbContext;
        private readonly ILogger<StarRepository> _logger;

        public StarRepository(
            IDbContext dbContext,
            ILogger<StarRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        
        #region "Implementation"

        public async Task<Star> InsertUpdateAsync(Star star)
        {
            if (star == null)
            {
                throw new ArgumentNullException(nameof(star));
            }
                
            var id = await InsertUpdateInternal(
                star.Id,
                star.Name,
                star.ThingId,
                star.CancellationToken,
                star.CreatedUserId,
                star.CreatedDate);

            if (id > 0)
            {
                // return
                return await SelectByIdAsync(id);
            }

            return null;
        }

        public async Task<Star> SelectByIdAsync(int id)
        {
            Star output = null;
            using (var context = _dbContext)
            {
                output = await context.ExecuteReaderAsync<Star>(
                    CommandType.StoredProcedure,
                    "SelectStarById",
                    async reader =>
                    {
                        if ((reader != null) && (reader.HasRows))
                        {
                            await reader.ReadAsync();
                            output = new Star();
                            output.PopulateModel(reader);
                        }

                        return output;
                    }, new IDbDataParameter[]
                    {
                        new DbParam("Id", DbType.Int32, id)
                    });

            }

            return output;
        }

        public async Task<IPagedResults<Star>> SelectAsync(IDbDataParameter[] dbParams)
        {
            IPagedResults<Star> results = null;
            using (var context = _dbContext)
            {
                results = await context.ExecuteReaderAsync<IPagedResults<Star>>(
                    CommandType.StoredProcedure,
                    "SelectStarsPaged",
                    async reader =>
                    {
                        if ((reader != null) && (reader.HasRows))
                        {
                            var output = new PagedResults<Star>();
                            while (await reader.ReadAsync())
                            {
                                var entity = new Star();
                                entity.PopulateModel(reader);
                                output.Data.Add(entity);
                            }

                            if (await reader.NextResultAsync())
                            {
                                await reader.ReadAsync();
                                output.PopulateTotal(reader);
                            }

                            return output;
                        }

                        return null;
                    },
                    dbParams);
                
            }

            return results;

        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation($"Deleting star with id: {id}");
            }

            var success = 0;
            using (var context = _dbContext)
            {
                success = await context.ExecuteScalarAsync<int>(
                    CommandType.StoredProcedure,
                    "DeleteStarById", new IDbDataParameter[]
                    {
                        new DbParam("Id", DbType.Int32, id)
                    });
            }

            return success > 0 ? true : false;
        }

        public async Task<IEnumerable<Star>> SelectByNameAndThingId(string name, int thingId)
        {
            IList<Star> results = null;
            using (var context = _dbContext)
            {
                results = await context.ExecuteReaderAsync<List<Star>>(
                    CommandType.StoredProcedure,
                    "SelectStarsByNameAndThingId",
                    async reader =>
                    {
                        if ((reader != null) && (reader.HasRows))
                        {
                            var output = new List<Star>();
                            while (await reader.ReadAsync())
                            {
                                var star = new Star();
                                star.PopulateModel(reader);
                                output.Add(star);
                            }

                            return output;
                        }

                        return null;

                    }, new IDbDataParameter[]
                    {
                        new DbParam("Name", DbType.String, 255, name),
                        new DbParam("ThingId", DbType.Int32, thingId),
                    });

            }

            return results;
        }

        public async Task<Star> SelectByNameThingIdAndCreatedUserId(string name, int thingId, int createdUserId)
        {
            Star output = null;
            using (var context = _dbContext)
            {
                output = await context.ExecuteReaderAsync(
                    CommandType.StoredProcedure,
                    "SelectStarByNameThingIdAndCreatedUserId",
                    async reader =>
                    {
                        if ((reader != null) && (reader.HasRows))
                        {
                            await reader.ReadAsync();
                            output = new Star();
                            output.PopulateModel(reader);
                        }

                        return output;
                    },
                    new IDbDataParameter[]
                    {
                        new DbParam("Name", DbType.String, 255, name),
                        new DbParam("ThingId", DbType.Int32, thingId),
                        new DbParam("CreatedUserId", DbType.Int32, createdUserId),
                    });

            }

            return output;
        }

        #endregion

        #region "Private Methods"

        async Task<int> InsertUpdateInternal(
            int id,
            string name,
            int thingId,
            string cancellationToken,
            int createdUserId,
            DateTimeOffset? createdDate)
        {

            var output = 0;
            using (var context = _dbContext)
            {
                output = await context.ExecuteScalarAsync<int>(
                    CommandType.StoredProcedure,
                    "InsertUpdateStar",
                    new IDbDataParameter[]
                    {
                        new DbParam("Id", DbType.Int32, id),
                        new DbParam("Name", DbType.String, 255, name),
                        new DbParam("ThingId", DbType.Int32, thingId),
                        new DbParam("CancellationToken", DbType.String, 255, cancellationToken),
                        new DbParam("CreatedUserId", DbType.Int32, createdUserId),
                        new DbParam("CreatedDate", DbType.DateTimeOffset, createdDate.ToDateIfNull()),
                        new DbParam("UniqueId", DbType.Int32, ParameterDirection.Output)
                    });
            }

            return output;

        }

        #endregion
        
    }

}

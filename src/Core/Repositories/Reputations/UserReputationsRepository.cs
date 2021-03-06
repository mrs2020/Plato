﻿using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlatoCore.Abstractions.Extensions;
using PlatoCore.Data.Abstractions;
using PlatoCore.Models.Reputations;

namespace PlatoCore.Repositories.Reputations
{
    public class UserReputationsRepository : IUserReputationsRepository<UserReputation>
    {
        
        private readonly ILogger<UserReputationsRepository> _logger;
        private readonly IDbContext _dbContext;

        public UserReputationsRepository(
            ILogger<UserReputationsRepository> logger,
            IDbContext dbContext)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        #region "Implementation"

        public async Task<UserReputation> InsertUpdateAsync(UserReputation model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var id = await InsertUpdateInternal(
                model.Id,
                model.FeatureId,
                model.Name,
                model.Description,
                model.Points,
                model.CreatedUserId,
                model.CreatedDate
            );

            if (id > 0)
            {
                return await SelectByIdAsync(id);
            }

            return null;
        }

        public async Task<UserReputation> SelectByIdAsync(int id)
        {
            UserReputation userReputation = null;
            using (var context = _dbContext)
            {
                userReputation = await context.ExecuteReaderAsync(
                    CommandType.StoredProcedure,
                    "SelectUserReputationById",
                    async reader =>
                    {
                        if ((reader != null) && (reader.HasRows))
                        {
                            userReputation = new UserReputation();
                            await reader.ReadAsync();
                            userReputation.PopulateModel(reader);
                        }

                        return userReputation;
                    }, new IDbDataParameter[]
                    {
                        new DbParam("Id", DbType.Int32, id)
                    });


            }

            return userReputation;
        }

        public async Task<IPagedResults<UserReputation>> SelectAsync(IDbDataParameter[] dbParams)
        {
            IPagedResults<UserReputation> output = null;
            using (var context = _dbContext)
            {
                output = await context.ExecuteReaderAsync(
                    CommandType.StoredProcedure,
                    "SelectUserReputationsPaged",
                    async reader =>
                    {
                        if ((reader != null) && (reader.HasRows))
                        {
                            output = new PagedResults<UserReputation>();
                            while (await reader.ReadAsync())
                            {
                                var userReputation = new UserReputation();
                                userReputation.PopulateModel(reader);
                                output.Data.Add(userReputation);
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
                    dbParams);


            }

            return output;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation($"Deleting user reputation with id: {id}");
            }

            var success = 0;
            using (var context = _dbContext)
            {
                success = await context.ExecuteScalarAsync<int>(
                    CommandType.StoredProcedure,
                    "DeleteUserReputationById",
                    new IDbDataParameter[]
                    {
                        new DbParam("Id", DbType.Int32, id)
                    });
            }

            return success > 0 ? true : false;
        }

        #endregion

        #region "Private Methods"

        async Task<int> InsertUpdateInternal(
            int id,
            int featureId,
            string name,
            string description,
            int points,
            int createdUserId,
            DateTimeOffset? createdDate)
        {

            var output = 0;
            using (var context = _dbContext)
            {
                output = await context.ExecuteScalarAsync<int>(
                    CommandType.StoredProcedure,
                    "InsertUpdateUserReputation",
                    new IDbDataParameter[]
                    {
                        new DbParam("Id", DbType.Int32, id),
                        new DbParam("FeatureId", DbType.Int32, featureId),
                        new DbParam("Name", DbType.String, 255, name),
                        new DbParam("Description", DbType.String, 255, description),
                        new DbParam("Points", DbType.Int32, points),
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

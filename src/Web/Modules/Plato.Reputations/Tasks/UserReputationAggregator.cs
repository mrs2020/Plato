﻿using System.Threading.Tasks;
using PlatoCore.Cache.Abstractions;
using PlatoCore.Data.Abstractions;
using PlatoCore.Stores.Users;
using PlatoCore.Tasks.Abstractions;

namespace Plato.Reputations.Tasks
{

    /// <summary>
    /// Polls the UserReputations tables and aggregates total reputation points into the Users.Reputation column..
    /// </summary>
    public class UserReputationAggregator : IBackgroundTaskProvider
    {

        // Selects all users who have been awarded reputation within the last 24 hours
        // and recalculate the total sum of all reputation awareded to any found user
        private const string Sql = @"
                    DECLARE @dirty bit = 0;
                    DECLARE @date datetimeoffset = SYSDATETIMEOFFSET(); 
                    DECLARE @yesterday DATETIME = DATEADD(day, -1, @date);                           
                    DECLARE @userId int;

                    DECLARE MSGCURSOR CURSOR FOR SELECT TOP 200 Id FROM {prefix}_Users ORDER BY ReputationUpdatedDate;                    
                    OPEN MSGCURSOR FETCH NEXT FROM MSGCURSOR INTO @userId;                    
                    WHILE @@FETCH_STATUS = 0
                    BEGIN	                   
                        UPDATE {prefix}_Users SET 
                            Reputation = IsNull((SELECT SUM(Points) FROM {prefix}_UserReputations WHERE CreatedUserId = @userId), 0),
                            ReputationUpdatedDate = @date
                        WHERE Id = @userId;
                        SET @dirty = 1;
	                    FETCH NEXT FROM MSGCURSOR INTO @userId;	                    
                    END;
                    CLOSE MSGCURSOR;
                    DEALLOCATE MSGCURSOR;
                    SELECT @dirty;";

        public int IntervalInSeconds => 240;
        
        private readonly ICacheManager _cacheManager;
        private readonly IDbHelper _dbHelper;

        public UserReputationAggregator(
            IDbHelper dbHelper, 
            ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
            _dbHelper = dbHelper;
        }

        public async Task ExecuteAsync(object sender, SafeTimerEventArgs args)
        {

            var dirty = await _dbHelper.ExecuteScalarAsync<bool>(Sql);
            if (dirty)
            {
                _cacheManager.CancelTokens(typeof(PlatoUserStore));
            }
            
        }

    }
    
}

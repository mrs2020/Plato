﻿using System.Threading.Tasks;
using PlatoCore.Cache.Abstractions;
using PlatoCore.Data.Abstractions;
using PlatoCore.Stores.Users;
using PlatoCore.Tasks.Abstractions;


namespace Plato.Reputations.Tasks
{
    
    public class UserRankAggregator : IBackgroundTaskProvider
    {
        
        private const string Sql = @"
                    DECLARE @dirty bit = 0;
                    DECLARE @date datetimeoffset = SYSDATETIMEOFFSET(); 
                    DECLARE @yesterday DATETIME = DATEADD(day, -1, @date);                           
                    DECLARE @userId int;

                    DECLARE @temp TABLE
                    (
	                    [Rank] int IDENTITY (1, 1) NOT NULL PRIMARY KEY,
	                    UserID int	                               
                    );

                    -- Order users by points adding to temp table                    
                    INSERT INTO @temp (UserID) 
	                    SELECT Id FROM {prefix}_Users ORDER BY Reputation DESC;

                    -- Now we have a ranked list update rank for last 200 users
                    DECLARE MSGCURSOR CURSOR FOR SELECT TOP 200 Id FROM {prefix}_Users ORDER BY RankUpdatedDate;
                    
                    OPEN MSGCURSOR FETCH NEXT FROM MSGCURSOR INTO @userId;                    
                    WHILE @@FETCH_STATUS = 0
                    BEGIN	                   
                        UPDATE {prefix}_Users SET
                            [Rank] = (SELECT [Rank] FROM @temp WHERE UserId = @userId),
                            RankUpdatedDate = @date
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

        public UserRankAggregator(
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

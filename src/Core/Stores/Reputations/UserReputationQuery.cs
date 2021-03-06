﻿using System;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using PlatoCore.Data.Abstractions;
using PlatoCore.Models.Reputations;
using PlatoCore.Stores.Abstractions;

namespace PlatoCore.Stores.Reputations
{

    #region "UserReputationsQuery"

    public class UserReputationQuery : DefaultQuery<UserReputation>
    {

        private readonly IQueryableStore<UserReputation> _store;

        public UserReputationQuery(IQueryableStore<UserReputation> store)
        {
            _store = store;
        }

        public UserReputationsQueryParams Params { get; set; }

        public override IQuery<UserReputation> Select<T>(Action<T> configure)
        {
            var defaultParams = new T();
            configure(defaultParams);
            Params = (UserReputationsQueryParams)Convert.ChangeType(defaultParams, typeof(UserReputationsQueryParams));
            return this;
        }

        public override async Task<IPagedResults<UserReputation>> ToList()
        {

            var builder = new UserReputationsQueryBuilder(this);
            var populateSql = builder.BuildSqlPopulate();
            var countSql = builder.BuildSqlCount();
            var keywords = Params?.Keywords?.Value ?? string.Empty;
            
            return await _store.SelectAsync(new IDbDataParameter[]
            {
                new DbParam("PageIndex", DbType.Int32, PageIndex),
                new DbParam("PageSize", DbType.Int32, PageSize),
                new DbParam("SqlPopulate", DbType.String, populateSql),
                new DbParam("SqlCount", DbType.String, countSql),
                new DbParam("Keywords", DbType.String, keywords)
            });

        }

    }

    #endregion

    #region "UserReputationsQueryParams"

    public class UserReputationsQueryParams
    {
        
        private WhereInt _id;
        private WhereString _keywords;
        private WhereInt _userId;

        public WhereInt Id
        {
            get => _id ?? (_id = new WhereInt());
            set => _id = value;
        }

        public WhereString Keywords
        {
            get => _keywords ?? (_keywords = new WhereString());
            set => _keywords = value;
        }

        public WhereInt UserId
        {
            get => _userId ?? (_userId = new WhereInt());
            set => _userId = value;
        }

    }

    #endregion

    #region "UserReputationsQueryBuilder"

    public class UserReputationsQueryBuilder : IQueryBuilder
    {

        #region "Constructor"

        private readonly string _userReputationssTableName;

        private readonly UserReputationQuery _query;

        public UserReputationsQueryBuilder(UserReputationQuery query)
        {
            _query = query;
            _userReputationssTableName = GetTableNameWithPrefix("UserReputationss");
        }

        #endregion

        #region "Implementation"
        
        public string BuildSqlPopulate()
        {
            var whereClause = BuildWhereClause();
            var orderBy = BuildOrderBy();
            var sb = new StringBuilder();
            sb.Append("SELECT ")
                .Append(BuildPopulateSelect())
                .Append(" FROM ")
                .Append(BuildTables());
            if (!string.IsNullOrEmpty(whereClause))
                sb.Append(" WHERE (").Append(whereClause).Append(")");
            // Order only if we have something to order by
            sb.Append(" ORDER BY ").Append(!string.IsNullOrEmpty(orderBy)
                ? orderBy
                : "(SELECT NULL)");
            // Limit results only if we have a specific page size
            if (!_query.IsDefaultPageSize)
                sb.Append(" OFFSET @RowIndex ROWS FETCH NEXT @PageSize ROWS ONLY;");
            return sb.ToString();
        }

        public string BuildSqlCount()
        {
            if (!_query.CountTotal)
                return string.Empty;
            var whereClause = BuildWhereClause();
            var sb = new StringBuilder();
            sb.Append("SELECT COUNT(ur.Id) FROM ")
                .Append(BuildTables());
            if (!string.IsNullOrEmpty(whereClause))
                sb.Append(" WHERE (").Append(whereClause).Append(")");
            return sb.ToString();
        }

        #endregion

        #region "Private Methods"

        private string BuildPopulateSelect()
        {
            var sb = new StringBuilder();
            sb.Append("ur.*");
            return sb.ToString();
        }

        private string BuildTables()
        {
            var sb = new StringBuilder();
            sb.Append(_userReputationssTableName)
                .Append(" ub ");
            return sb.ToString();
        }

        private string GetTableNameWithPrefix(string tableName)
        {
            return !string.IsNullOrEmpty(_query.Options.TablePrefix)
                ? _query.Options.TablePrefix + tableName
                : tableName;
        }
        
        private string BuildWhereClause()
        {

            if (_query.Params == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            // Id
            if (_query.Params.Id.Value > -1)
            {
                if (!string.IsNullOrEmpty(sb.ToString()))
                    sb.Append(_query.Params.Id.Operator);
                sb.Append(_query.Params.Id.ToSqlString("ur.Id"));
            }

            // Name
            if (!String.IsNullOrEmpty(_query.Params.Keywords.Value))
            {
                if (!string.IsNullOrEmpty(sb.ToString()))
                    sb.Append(_query.Params.Keywords.Operator);
                sb.Append(_query.Params.Keywords.ToSqlString("[Name]", "Keywords"));
            }

            // UserId
            if (_query.Params.UserId.Value > -1)
            {
                if (!string.IsNullOrEmpty(sb.ToString()))
                    sb.Append(_query.Params.Id.Operator);
                sb.Append(_query.Params.UserId.ToSqlString("UserId"));
            }

            return sb.ToString();

        }
        
        private string GetQualifiedColumnName(string columnName)
        {
            if (columnName == null)
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            return columnName.IndexOf('.') >= 0
                ? columnName
                : "ur." + columnName;
        }

        private string BuildOrderBy()
        {
            if (_query.SortColumns.Count == 0) return null;
            var sb = new StringBuilder();
            var i = 0;
            foreach (var sortColumn in _query.SortColumns)
            {
                sb.Append(GetQualifiedColumnName(sortColumn.Key));
                if (sortColumn.Value != OrderBy.Asc)
                    sb.Append(" DESC");
                if (i < _query.SortColumns.Count - 1)
                    sb.Append(", ");
                i += 1;
            }

            return sb.ToString();

        }

        #endregion

    }

    #endregion
    
}

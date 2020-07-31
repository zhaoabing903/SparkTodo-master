﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WeihanLi.Common.Models;
using WeihanLi.Extensions;

namespace WeihanLi.Common.Data
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : new()
    {
        private static readonly Type EntityType = typeof(TEntity);

        private static readonly Dictionary<string, string> ColumnMappings = CacheUtil.TypePropertyCache.GetOrAdd(typeof(TEntity), t => t.GetProperties())
             .Where(_ => !_.IsDefined(typeof(NotMappedAttribute)))
             .Select(p => new KeyValuePair<string, string>(p.GetColumnName(), p.Name))
             .ToDictionary(p => p.Key, p => p.Value);

        private static readonly string SelectColumnsString = CacheUtil.TypePropertyCache.GetOrAdd(typeof(TEntity), t => t.GetProperties())
            .Where(_ => !_.IsDefined(typeof(NotMappedAttribute))).Select(_ => $"{_.GetColumnName()} AS {_.Name}").StringJoin(",");

        private static readonly Lazy<Dictionary<string, string>> InsertColumnMappings = new Lazy<Dictionary<string, string>>(() => CacheUtil.TypePropertyCache.GetOrAdd(typeof(TEntity), t => t.GetProperties())
            .Where(_ => !_.IsDefined(typeof(NotMappedAttribute)) && !_.IsDefined(typeof(DatabaseGeneratedAttribute)))
            .Select(p => new KeyValuePair<string, string>(p.GetColumnName(), p.Name))
            .ToDictionary(_ => _.Key, _ => _.Value));

        private static readonly string TableName = typeof(TEntity).IsDefined(typeof(TableAttribute))
            ? EntityType.GetCustomAttribute<TableAttribute>().Name
            : EntityType.Name;

        protected readonly Lazy<DbConnection> _dbConnection;

        public Repository(Func<DbConnection> dbConnectionFunc)
        {
            _dbConnection = new Lazy<DbConnection>(dbConnectionFunc, true);
        }

        public virtual int Count(Expression<Func<TEntity, bool>> whereExpression)
        {
            var whereSql = SqlExpressionParser.ParseWhereExpression(whereExpression, ColumnMappings);

            var sql = $@"
SELECT COUNT(1) FROM {TableName}
{whereSql.SqlText}
";
            return _dbConnection.Value.ExecuteScalarTo<int>(sql, whereSql.Parameters);
        }

        public virtual Task<int> CountAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
        {
            var whereSql = SqlExpressionParser.ParseWhereExpression(whereExpression, ColumnMappings);

            var sql = $@"
SELECT COUNT(1) FROM {TableName}
{whereSql.SqlText}
";
            return _dbConnection.Value.ExecuteScalarToAsync<int>(sql, whereSql.Parameters, cancellationToken: cancellationToken);
        }

        public virtual long LongCount(Expression<Func<TEntity, bool>> whereExpression)
        {
            var whereSql = SqlExpressionParser.ParseWhereExpression(whereExpression, ColumnMappings);

            var sql = $@"
SELECT COUNT(1) FROM {TableName}
{whereSql.SqlText}
";
            return _dbConnection.Value.ExecuteScalarTo<long>(sql, whereSql.Parameters);
        }

        public virtual Task<long> LongCountAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
        {
            var whereSql = SqlExpressionParser.ParseWhereExpression(whereExpression, ColumnMappings);

            var sql = $@"
SELECT COUNT(1) FROM {TableName}
{whereSql.SqlText}
";
            return _dbConnection.Value.ExecuteScalarToAsync<long>(sql, whereSql.Parameters, cancellationToken: cancellationToken);
        }

        public virtual bool Exist(Expression<Func<TEntity, bool>> whereExpression)
        {
            var whereSql = SqlExpressionParser.ParseWhereExpression(whereExpression, ColumnMappings);
            var sql = $@"SELECT CAST(IIF(EXISTS (SELECT TOP(1) 1 FROM {TableName} {whereSql.SqlText}), 1, 0) AS BIT)";
            return _dbConnection.Value.ExecuteScalarTo<bool>(sql, whereSql.Parameters);
        }

        public virtual Task<bool> ExistAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
        {
            var whereSql = SqlExpressionParser.ParseWhereExpression(whereExpression, ColumnMappings);
            var sql = $@"SELECT CAST(IIF(EXISTS (SELECT TOP(1) 1 FROM {TableName} {whereSql.SqlText}), 1, 0) AS BIT)";
            return _dbConnection.Value.ExecuteScalarToAsync<bool>(sql, whereSql.Parameters, cancellationToken: cancellationToken);
        }

        public virtual TEntity Fetch(Expression<Func<TEntity, bool>> whereExpression)
        {
            var whereSql = SqlExpressionParser.ParseWhereExpression(whereExpression, ColumnMappings);
            var sql = $@"
SELECT TOP(1) {SelectColumnsString} FROM {TableName}
{whereSql.SqlText}
";
            return _dbConnection.Value.Fetch<TEntity>(sql, whereSql.Parameters);
        }

        public virtual Task<TEntity> FetchAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
        {
            var whereSql = SqlExpressionParser.ParseWhereExpression(whereExpression, ColumnMappings);
            var sql = $@"
SELECT TOP 1 {SelectColumnsString} FROM {TableName}
{whereSql.SqlText}
";
            return _dbConnection.Value.FetchAsync<TEntity>(sql, whereSql.Parameters, cancellationToken: cancellationToken);
        }

        public virtual TEntity Fetch<TProperty>(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> orderByExpression, bool ascending = false)
        {
            var whereSql = SqlExpressionParser.ParseWhereExpression(whereExpression, ColumnMappings);
            var sql = $@"
SELECT TOP(1) {SelectColumnsString} FROM {TableName}
{whereSql.SqlText}
ORDER BY {GetColumnName(orderByExpression.GetMemberName())}  {(ascending ? "" : "DESC")}
";
            return _dbConnection.Value.Fetch<TEntity>(sql, whereSql.Parameters);
        }

        public virtual Task<TEntity> FetchAsync<TProperty>(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> orderByExpression, bool ascending = false, CancellationToken cancellationToken = default)
        {
            var whereSql = SqlExpressionParser.ParseWhereExpression(whereExpression, ColumnMappings);
            var sql = $@"
SELECT TOP(1) {SelectColumnsString} FROM {TableName}
{whereSql.SqlText}
ORDER BY {GetColumnName(orderByExpression.GetMemberName())}  {(ascending ? "" : "DESC")}
";
            return _dbConnection.Value.FetchAsync<TEntity>(sql, whereSql.Parameters, cancellationToken: cancellationToken);
        }

        public virtual List<TEntity> Select(Expression<Func<TEntity, bool>> whereExpression)
        {
            var whereSql = SqlExpressionParser.ParseWhereExpression(whereExpression, ColumnMappings);
            var sql = $@"
SELECT {SelectColumnsString} FROM {TableName}
{whereSql.SqlText}
";
            return _dbConnection.Value.Select<TEntity>(sql, whereSql.Parameters).ToList();
        }

        public virtual Task<List<TEntity>> SelectAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
        {
            var whereSql = SqlExpressionParser.ParseWhereExpression(whereExpression, ColumnMappings);
            var sql = $@"
SELECT {SelectColumnsString} FROM {TableName}
{whereSql.SqlText}
";
            return _dbConnection.Value.SelectAsync<TEntity>(sql, whereSql.Parameters, cancellationToken: cancellationToken).ContinueWith(r => r.Result.ToList(), cancellationToken);
        }

        public virtual List<TEntity> Select<TProperty>(int count, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> orderByExpression, bool ascending = false)
        {
            var whereSql = SqlExpressionParser.ParseWhereExpression(whereExpression, ColumnMappings);
            var sql = $@"
SELECT TOP({count}) {SelectColumnsString} FROM {TableName}
{whereSql.SqlText}
ORDER BY {GetColumnName(orderByExpression.GetMemberName())} {(ascending ? "" : "DESC")}
";
            return _dbConnection.Value.Select<TEntity>(sql, whereSql.Parameters).ToList();
        }

        public virtual Task<List<TEntity>> SelectAsync<TProperty>(int count, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> orderByExpression, bool ascending = false, CancellationToken cancellationToken = default)
        {
            var whereSql = SqlExpressionParser.ParseWhereExpression(whereExpression, ColumnMappings);
            var sql = $@"
SELECT TOP({count}) {SelectColumnsString} FROM {TableName}
{whereSql.SqlText}
ORDER BY {GetColumnName(orderByExpression.GetMemberName())} {(ascending ? "" : "DESC")}
";
            return _dbConnection.Value.SelectAsync<TEntity>(sql, whereSql.Parameters, cancellationToken: cancellationToken).ContinueWith(_ => _.Result.ToList(), cancellationToken);
        }

        public virtual IPagedListResult<TEntity> Paged<TProperty>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> orderByExpression, bool ascending = false)
        {
            var whereSql = SqlExpressionParser.ParseWhereExpression(whereExpression, ColumnMappings);
            if (pageNumber <= 0)
            {
                pageNumber = 1;
            }
            if (pageSize <= 0)
            {
                pageSize = 10;
            }
            var sql = $@"
SELECT COUNT(1) FROM {TableName}
{whereSql.SqlText}
";
            var total = _dbConnection.Value.ExecuteScalarTo<int>(sql, whereSql.Parameters);
            if (total == 0)
            {
                return PagedListResult<TEntity>.Empty;
            }

            var offset = (pageNumber - 1) * pageSize;

            sql = $@"
SELECT {SelectColumnsString} FROM {TableName}
{whereSql.SqlText}
ORDER BY {GetColumnName(orderByExpression.GetMemberName())}{(ascending ? "" : " DESC")}
OFFSET {offset} ROWS
FETCH NEXT {pageSize} ROWS ONLY
";

            return _dbConnection.Value.Select<TEntity>(sql, whereSql.Parameters).ToPagedList(pageNumber, pageSize, total);
        }

        public virtual async Task<IPagedListResult<TEntity>> PagedAsync<TProperty>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> orderByExpression, bool ascending = false, CancellationToken cancellationToken = default)
        {
            var whereSql = SqlExpressionParser.ParseWhereExpression(whereExpression, ColumnMappings);
            if (pageNumber <= 0)
            {
                pageNumber = 1;
            }
            if (pageSize <= 0)
            {
                pageSize = 10;
            }
            var sql = $@"
SELECT COUNT(1) FROM {TableName}
{whereSql.SqlText}
";
            var total = await _dbConnection.Value.ExecuteScalarToAsync<int>(sql, whereSql.Parameters, cancellationToken: cancellationToken);
            if (total == 0)
            {
                return PagedListResult<TEntity>.Empty;
            }

            var offset = (pageNumber - 1) * pageSize;

            sql = $@"
SELECT {SelectColumnsString} FROM {TableName}
{whereSql.SqlText}
ORDER BY {GetColumnName(orderByExpression.GetMemberName())}{(ascending ? "" : " DESC")}
OFFSET {offset} ROWS
FETCH NEXT {pageSize} ROWS ONLY
";

            return (await _dbConnection.Value.SelectAsync<TEntity>(sql, whereSql.Parameters, cancellationToken: cancellationToken)).ToPagedList(pageNumber, pageSize, total);
        }

        public virtual int Insert(TEntity entity)
        {
            var paramDictionary = new Dictionary<string, object>();
            var sqlBuilder = new StringBuilder($@"INSERT INTO {TableName}");
            sqlBuilder.AppendLine();
            sqlBuilder.AppendLine("(");
            sqlBuilder.AppendLine($"{InsertColumnMappings.Value.Keys.Select(_ => _).StringJoin($",{Environment.NewLine}")}");
            sqlBuilder.AppendLine(")");
            sqlBuilder.AppendLine("VALUES");
            sqlBuilder.AppendLine("(");
            sqlBuilder.AppendLine($"{InsertColumnMappings.Value.Keys.Select(_ => $"@{_}").StringJoin($",{Environment.NewLine}")}");
            sqlBuilder.AppendLine(")");
            foreach (var field in InsertColumnMappings.Value.Keys)
            {
                paramDictionary.Add($"{field}", entity.GetPropertyValue(InsertColumnMappings.Value[field]));
            }
            var sql = sqlBuilder.ToString();

            return _dbConnection.Value.Execute(sql, paramDictionary);
        }

        public virtual Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var paramDictionary = new Dictionary<string, object>();

            var sqlBuilder = new StringBuilder($@"INSERT INTO {TableName}");
            sqlBuilder.AppendLine("(");
            sqlBuilder.AppendLine($"{InsertColumnMappings.Value.Keys.Select(_ => _).StringJoin($",{Environment.NewLine}")}");
            sqlBuilder.AppendLine(")");
            sqlBuilder.AppendLine("VALUES (");
            sqlBuilder.AppendLine($"{InsertColumnMappings.Value.Keys.Select(_ => $"@{_}").StringJoin($",{Environment.NewLine}")}");
            foreach (var field in InsertColumnMappings.Value.Keys)
            {
                paramDictionary.Add($"{field}", entity.GetPropertyValue(InsertColumnMappings.Value[field]));
            }

            sqlBuilder.AppendLine(")");
            return _dbConnection.Value.ExecuteAsync(sqlBuilder.ToString(), paramDictionary, cancellationToken: cancellationToken);
        }

        public virtual int Insert(IEnumerable<TEntity> entities)
        {
            var count = entities?.Count() ?? 0;
            if (count == 0)
            {
                return 0;
            }
            if (count > 1000)
            {
                return -1; // too large, not supported
            }
            var paramDictionary = new Dictionary<string, object>();
            var sqlBuilder = new StringBuilder($@"INSERT INTO {TableName}");
            sqlBuilder.AppendLine("(");
            sqlBuilder.AppendLine($"{InsertColumnMappings.Value.Keys.Select(_ => _).StringJoin($",{Environment.NewLine}")}");
            sqlBuilder.AppendLine(")");
            sqlBuilder.AppendLine("VALUES");

            for (var i = 0; i < count; i++)
            {
                sqlBuilder.AppendLine();
                sqlBuilder.AppendLine("(");
                sqlBuilder.AppendLine($"{InsertColumnMappings.Value.Keys.Select(_ => $"@{_}_{i}").StringJoin($",{Environment.NewLine}")}");
                foreach (var field in InsertColumnMappings.Value.Keys)
                {
                    paramDictionary.Add($"{field}_{i}", EntityType.GetPropertyValue(InsertColumnMappings.Value[field]));
                }
                sqlBuilder.Append("),");
            }
            sqlBuilder.Remove(sqlBuilder.Length - 2, 1);

            return _dbConnection.Value.Execute(sqlBuilder.ToString(), paramDictionary);
        }

        public virtual Task<int> InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            var count = entities?.Count() ?? 0;
            if (count == 0)
            {
                return Task.FromResult(0);
            }
            if (count > 1000)
            {
                return Task.FromResult(-1); // too large, not supported
            }
            var paramDictionary = new Dictionary<string, object>();
            var sqlBuilder = new StringBuilder($@"INSERT INTO {TableName}");
            sqlBuilder.AppendLine("(");
            sqlBuilder.AppendLine($"{InsertColumnMappings.Value.Keys.Select(_ => _).StringJoin($",{Environment.NewLine}")}");
            sqlBuilder.AppendLine(")");
            sqlBuilder.AppendLine("VALUES");

            for (var i = 0; i < count; i++)
            {
                sqlBuilder.AppendLine();
                sqlBuilder.AppendLine("(");
                sqlBuilder.AppendLine($"{InsertColumnMappings.Value.Keys.Select(_ => $"@{_}_{i}").StringJoin($",{Environment.NewLine}")}");
                foreach (var field in InsertColumnMappings.Value.Keys)
                {
                    paramDictionary.Add($"{field}_{i}", EntityType.GetPropertyValue(InsertColumnMappings.Value[field]));
                }
                sqlBuilder.Append("),");
            }
            sqlBuilder.Remove(sqlBuilder.Length - 2, 1);

            return _dbConnection.Value.ExecuteAsync(sqlBuilder.ToString(), paramDictionary, cancellationToken: cancellationToken);
        }

        public virtual int Update<TProperty>(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> propertyExpression, object value)
        {
            var whereSql = SqlExpressionParser.ParseWhereExpression(whereExpression, ColumnMappings);
            var propertyName = propertyExpression.GetMemberName();
            var sql = $@"
UPDATE {TableName}
SET {GetColumnName(propertyName)} = @set_{propertyName}
{whereSql.SqlText}
";
            whereSql.Parameters.Add($"set_{propertyName}", value);
            return _dbConnection.Value.Execute(sql, whereSql.Parameters);
        }

        public virtual Task<int> UpdateAsync<TProperty>(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> propertyExpression, object value, CancellationToken cancellationToken = default)
        {
            var whereSql = SqlExpressionParser.ParseWhereExpression(whereExpression, ColumnMappings);
            var propertyName = propertyExpression.GetMemberName();
            var sql = $@"
UPDATE {TableName}
SET {GetColumnName(propertyName)} = @set_{propertyName}
{whereSql.SqlText}
";
            whereSql.Parameters.Add($"set_{propertyName}", value);
            return _dbConnection.Value.ExecuteAsync(sql, whereSql.Parameters, cancellationToken: cancellationToken);
        }

        public virtual int Update(Expression<Func<TEntity, bool>> whereExpression, IDictionary<string, object> propertyValues)
        {
            if (propertyValues == null || propertyValues.Count == 0)
            {
                return 0;
            }
            var whereSql = SqlExpressionParser.ParseWhereExpression(whereExpression, ColumnMappings);
            var sql = $@"
UPDATE {TableName}
SET {propertyValues.Keys.Select(p => $"{GetColumnName(p)}=@set_{p}").StringJoin($",{Environment.NewLine}")}
{whereSql.SqlText}
";
            foreach (var propertyValue in propertyValues)
            {
                whereSql.Parameters.Add($"set_{propertyValue.Key}", propertyValue.Value);
            }
            return _dbConnection.Value.Execute(sql, whereSql.Parameters);
        }

        public virtual Task<int> UpdateAsync(Expression<Func<TEntity, bool>> whereExpression, IDictionary<string, object> propertyValues, CancellationToken cancellationToken = default)
        {
            if (propertyValues == null || propertyValues.Count == 0)
            {
                return Task.FromResult(0);
            }
            var whereSql = SqlExpressionParser.ParseWhereExpression(whereExpression, ColumnMappings);
            var sql = $@"
UPDATE {TableName}
SET {propertyValues.Keys.Select(p => $"{GetColumnName(p)}=@set_{p}").StringJoin($",{Environment.NewLine}")}
{whereSql.SqlText}
";
            foreach (var propertyValue in propertyValues)
            {
                whereSql.Parameters.Add($"set_{propertyValue.Key}", propertyValue.Value);
            }
            return _dbConnection.Value.ExecuteAsync(sql, whereSql.Parameters, cancellationToken: cancellationToken);
        }

        public virtual int Delete(Expression<Func<TEntity, bool>> whereExpression)
        {
            var whereSql = SqlExpressionParser.ParseWhereExpression(whereExpression, ColumnMappings);
            var sql = $@"
DELETE FROM {TableName}
{whereSql.SqlText}
";
            return _dbConnection.Value.Execute(sql, whereSql.Parameters);
        }

        public virtual Task<int> DeleteAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
        {
            var whereSql = SqlExpressionParser.ParseWhereExpression(whereExpression, ColumnMappings);
            var sql = $@"
DELETE FROM {TableName}
{whereSql.SqlText}
";
            return _dbConnection.Value.ExecuteAsync(sql, whereSql.Parameters, cancellationToken: cancellationToken);
        }

        public virtual int Execute(string sqlStr, object param = null)
        => _dbConnection.Value.Execute(sqlStr, paramInfo: param);

        public virtual Task<int> ExecuteAsync(string sqlStr, object param = null, CancellationToken cancellationToken = default)
        => _dbConnection.Value.ExecuteAsync(sqlStr, paramInfo: param, cancellationToken: cancellationToken);

        public virtual TResult ExecuteScalar<TResult>(string sqlStr, object param = null)

        => _dbConnection.Value.ExecuteScalarTo<TResult>(sqlStr, paramInfo: param);

        public virtual Task<TResult> ExecuteScalarAsync<TResult>(string sqlStr, object param = null, CancellationToken cancellationToken = default)

        => _dbConnection.Value.ExecuteScalarToAsync<TResult>(sqlStr, paramInfo: param, cancellationToken: cancellationToken);

        private static string GetColumnName(string propertyName)
        {
            return ColumnMappings.FirstOrDefault(_ => _.Value == propertyName).Key ?? propertyName;
        }
    }
}

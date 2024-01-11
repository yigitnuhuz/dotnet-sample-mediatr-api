using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

namespace Core.Helpers;


  public interface ISqlHelper
    {
        #region Health Check

        Task<bool> HealthCheckAsync(CancellationToken cancellationToken);

        Task<bool> HealthCheckReadOnlyAsync(CancellationToken cancellationToken);

        #endregion

        #region Requests

        Task<IEnumerable<T>> QueryAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default);

        Task<T> QueryFirstOrDefaultAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default);

        Task<T> QuerySingleOrDefaultAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default);

        Task<int> ExecuteAsync(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default);

        Task<T> ExecuteScalarAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default);

        Task<IDataReader> ExecuteReaderAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default);

        Task QueryMultipleAsync(string sql, Action<SqlMapper.GridReader> map, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default);

        #endregion

        #region Readonly Requests

        Task<IEnumerable<T>> QueryReadOnlyAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default);

        Task<T> QueryFirstOrDefaultReadOnlyAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default);

        Task<T> QuerySingleOrDefaultReadOnlyAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default);

        Task<int> ExecuteReadOnlyAsync(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default);

        Task<T> ExecuteScalarReadOnlyAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default);

        Task<IDataReader> ExecuteReaderReadOnlyAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default);

        Task QueryMultipleReadOnlyAsync(string sql, Action<SqlMapper.GridReader> map, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default);

        #endregion
    }

    public class SqlHelper : ISqlHelper
    {
        private readonly string _connectionString;

        private readonly string _readOnlyConnectionString;

        protected SqlHelper(string connectionString, string readOnlyConnectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("DbConnection_string_has_not_found");
            }

            if (string.IsNullOrEmpty(readOnlyConnectionString))
            {
                throw new ArgumentException("DbReadOnlyConnection_string_has_not_found");
            }

            _connectionString = connectionString;
            _readOnlyConnectionString = readOnlyConnectionString;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        private SqlConnection GetReadOnlyConnection()
        {
            return new SqlConnection(_readOnlyConnectionString);
        }

        #region Health Check

        public async Task<bool> HealthCheckAsync(CancellationToken cancellationToken)
        {
            await using var connection = GetConnection();

            const string query = "select newid()";

            var cmd = new CommandDefinition(query, cancellationToken);

            var result = await connection.ExecuteScalarAsync<Guid>(cmd);

            return result != Guid.Empty;
        }

        public async Task<bool> HealthCheckReadOnlyAsync(CancellationToken cancellationToken)
        {
            await using var onlyConnection = GetReadOnlyConnection();

            const string query = "select newid()";

            var cmd = new CommandDefinition(query, cancellationToken);

            var result = await onlyConnection.ExecuteScalarAsync<Guid>(cmd);

            return result != Guid.Empty;
        }

        #endregion

        #region Requests

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default)
        {
            await using var connection = GetConnection();

            var command = new CommandDefinition(sql, parameters, transaction, commandTimeout, commandType, flags, cancellationToken);

            return await connection.QueryAsync<T>(command);
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default)
        {
            await using var connection = GetConnection();

            var command = new CommandDefinition(sql, parameters, transaction, commandTimeout, commandType, flags, cancellationToken);

            return await connection.QueryFirstOrDefaultAsync<T>(command);
        }

        public async Task<T> QuerySingleOrDefaultAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default)
        {
            await using var connection = GetConnection();

            var command = new CommandDefinition(sql, parameters, transaction, commandTimeout, commandType, flags, cancellationToken);

            return await connection.QuerySingleOrDefaultAsync<T>(command);
        }

        public async Task<int> ExecuteAsync(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default)
        {
            await using var connection = GetConnection();

            var command = new CommandDefinition(sql, parameters, transaction, commandTimeout, commandType, flags, cancellationToken);

            return await connection.ExecuteAsync(command);
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default)
        {
            await using var connection = GetConnection();

            var command = new CommandDefinition(sql, parameters, transaction, commandTimeout, commandType, flags, cancellationToken);

            return await connection.ExecuteScalarAsync<T>(command);
        }

        public async Task<IDataReader> ExecuteReaderAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default)
        {
            await using var connection = GetConnection();

            var command = new CommandDefinition(sql, parameters, transaction, commandTimeout, commandType, flags, cancellationToken);

            return await connection.ExecuteReaderAsync(command);
        }

        public async Task QueryMultipleAsync(string sql, Action<SqlMapper.GridReader> map, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default)
        {
            await using var connection = GetConnection();

            var command = new CommandDefinition(sql, parameters, transaction, commandTimeout, commandType, flags, cancellationToken);

            var result = await connection.QueryMultipleAsync(command);

            map(result);
        }

        #endregion

        #region Readonly Requests

        public async Task<IEnumerable<T>> QueryReadOnlyAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default)
        {
            await using var connection = GetReadOnlyConnection();

            var command = new CommandDefinition(sql, parameters, transaction, commandTimeout, commandType, flags, cancellationToken);

            return await connection.QueryAsync<T>(command);
        }

        public async Task<T> QueryFirstOrDefaultReadOnlyAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default)
        {
            await using var connection = GetReadOnlyConnection();

            var command = new CommandDefinition(sql, parameters, transaction, commandTimeout, commandType, flags, cancellationToken);

            return await connection.QueryFirstOrDefaultAsync<T>(command);
        }

        public async Task<T> QuerySingleOrDefaultReadOnlyAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default)
        {
            await using var connection = GetReadOnlyConnection();

            var command = new CommandDefinition(sql, parameters, transaction, commandTimeout, commandType, flags, cancellationToken);

            return await connection.QuerySingleOrDefaultAsync<T>(command);
        }

        public async Task<int> ExecuteReadOnlyAsync(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default)
        {
            await using var connection = GetReadOnlyConnection();

            var command = new CommandDefinition(sql, parameters, transaction, commandTimeout, commandType, flags, cancellationToken);

            return await connection.ExecuteAsync(command);
        }


        public async Task<T> ExecuteScalarReadOnlyAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default)
        {
            await using var connection = GetReadOnlyConnection();

            var command = new CommandDefinition(sql, parameters, transaction, commandTimeout, commandType, flags, cancellationToken);

            return await connection.ExecuteScalarAsync<T>(command);
        }

        public async Task<IDataReader> ExecuteReaderReadOnlyAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default)
        {
            await using var connection = GetReadOnlyConnection();

            var command = new CommandDefinition(sql, parameters, transaction, commandTimeout, commandType, flags, cancellationToken);

            return await connection.ExecuteReaderAsync(command);
        }

        public async Task QueryMultipleReadOnlyAsync(string sql, Action<SqlMapper.GridReader> map, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default)
        {
            await using var connection = GetReadOnlyConnection();

            var command = new CommandDefinition(sql, parameters, transaction, commandTimeout, commandType, flags, cancellationToken);

            var result = await connection.QueryMultipleAsync(command);

            result.Dispose();

            map(result);
        }

        #endregion
    }
using System.Data;
using Microsoft.Data.SqlClient;

namespace ProductInventoryAPI.Infrastructure.Repositories.Contracts;

/// <summary>
/// Interface for executing stored procedures and raw SQL commands
/// </summary>
public interface IStoredProcedureRepository
{
    /// <summary>
    /// Execute a stored procedure with parameters and return a DataTable
    /// </summary>
    /// <param name="storedProcedureName">Name of the stored procedure</param>
    /// <param name="parameters">SQL parameters for the stored procedure</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>DataTable containing the results</returns>
    Task<DataTable> ExecuteStoredProcedureAsync(string storedProcedureName, SqlParameter[]? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a stored procedure with parameters and return strongly typed results
    /// </summary>
    /// <typeparam name="T">Type to map the results to</typeparam>
    /// <param name="storedProcedureName">Name of the stored procedure</param>
    /// <param name="parameters">SQL parameters for the stored procedure</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of strongly typed objects</returns>
    Task<List<T>> ExecuteStoredProcedureAsync<T>(string storedProcedureName, SqlParameter[]? parameters = null, CancellationToken cancellationToken = default) where T : class, new();

    /// <summary>
    /// Execute a stored procedure that doesn't return data (INSERT, UPDATE, DELETE)
    /// </summary>
    /// <param name="storedProcedureName">Name of the stored procedure</param>
    /// <param name="parameters">SQL parameters for the stored procedure</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of rows affected</returns>
    Task<int> ExecuteStoredProcedureNonQueryAsync(string storedProcedureName, SqlParameter[]? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a stored procedure and return a scalar value
    /// </summary>
    /// <typeparam name="T">Type of the scalar value</typeparam>
    /// <param name="storedProcedureName">Name of the stored procedure</param>
    /// <param name="parameters">SQL parameters for the stored procedure</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Scalar value of type T</returns>
    Task<T?> ExecuteStoredProcedureScalarAsync<T>(string storedProcedureName, SqlParameter[]? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute raw SQL query and return a DataTable
    /// </summary>
    /// <param name="sql">SQL query to execute</param>
    /// <param name="parameters">SQL parameters for the query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>DataTable containing the results</returns>
    Task<DataTable> ExecuteRawSqlAsync(string sql, SqlParameter[]? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute raw SQL query and return strongly typed results
    /// </summary>
    /// <typeparam name="T">Type to map the results to</typeparam>
    /// <param name="sql">SQL query to execute</param>
    /// <param name="parameters">SQL parameters for the query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of strongly typed objects</returns>
    Task<List<T>> ExecuteRawSqlAsync<T>(string sql, SqlParameter[]? parameters = null, CancellationToken cancellationToken = default) where T : class, new();

    /// <summary>
    /// Execute raw SQL command that doesn't return data
    /// </summary>
    /// <param name="sql">SQL command to execute</param>
    /// <param name="parameters">SQL parameters for the command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of rows affected</returns>
    Task<int> ExecuteRawSqlNonQueryAsync(string sql, SqlParameter[]? parameters = null, CancellationToken cancellationToken = default);
}
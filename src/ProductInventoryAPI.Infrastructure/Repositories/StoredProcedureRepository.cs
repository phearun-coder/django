using System.Data;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductInventoryAPI.Infrastructure.Data;
using ProductInventoryAPI.Infrastructure.Repositories.Contracts;

namespace ProductInventoryAPI.Infrastructure.Repositories;

/// <summary>
/// Implementation of stored procedure repository for executing stored procedures and raw SQL
/// </summary>
public class StoredProcedureRepository : IStoredProcedureRepository
{
    private readonly ProductInventoryDbContext _context;
    private readonly ILogger<StoredProcedureRepository> _logger;

    public StoredProcedureRepository(ProductInventoryDbContext context, ILogger<StoredProcedureRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DataTable> ExecuteStoredProcedureAsync(string storedProcedureName, SqlParameter[]? parameters = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing stored procedure: {StoredProcedureName}", storedProcedureName);

        var dataTable = new DataTable();
        
        using var connection = _context.Database.GetDbConnection() as SqlConnection;
        if (connection == null)
            throw new InvalidOperationException("Database connection is not a SQL Server connection");

        await connection.OpenAsync(cancellationToken);

        using var command = new SqlCommand(storedProcedureName, connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        if (parameters != null)
        {
            command.Parameters.AddRange(parameters);
        }

        using var adapter = new SqlDataAdapter(command);
        adapter.Fill(dataTable);

        _logger.LogInformation("Stored procedure {StoredProcedureName} completed. Returned {RowCount} rows", 
            storedProcedureName, dataTable.Rows.Count);

        return dataTable;
    }

    public async Task<List<T>> ExecuteStoredProcedureAsync<T>(string storedProcedureName, SqlParameter[]? parameters = null, CancellationToken cancellationToken = default) where T : class, new()
    {
        var dataTable = await ExecuteStoredProcedureAsync(storedProcedureName, parameters, cancellationToken);
        return MapDataTableToList<T>(dataTable);
    }

    public async Task<int> ExecuteStoredProcedureNonQueryAsync(string storedProcedureName, SqlParameter[]? parameters = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing non-query stored procedure: {StoredProcedureName}", storedProcedureName);

        using var connection = _context.Database.GetDbConnection() as SqlConnection;
        if (connection == null)
            throw new InvalidOperationException("Database connection is not a SQL Server connection");

        await connection.OpenAsync(cancellationToken);

        using var command = new SqlCommand(storedProcedureName, connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        if (parameters != null)
        {
            command.Parameters.AddRange(parameters);
        }

        var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogInformation("Stored procedure {StoredProcedureName} completed. {RowsAffected} rows affected", 
            storedProcedureName, rowsAffected);

        return rowsAffected;
    }

    public async Task<T?> ExecuteStoredProcedureScalarAsync<T>(string storedProcedureName, SqlParameter[]? parameters = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing scalar stored procedure: {StoredProcedureName}", storedProcedureName);

        using var connection = _context.Database.GetDbConnection() as SqlConnection;
        if (connection == null)
            throw new InvalidOperationException("Database connection is not a SQL Server connection");

        await connection.OpenAsync(cancellationToken);

        using var command = new SqlCommand(storedProcedureName, connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        if (parameters != null)
        {
            command.Parameters.AddRange(parameters);
        }

        var result = await command.ExecuteScalarAsync(cancellationToken);

        _logger.LogInformation("Scalar stored procedure {StoredProcedureName} completed", storedProcedureName);

        return result == null || result == DBNull.Value ? default : (T)Convert.ChangeType(result, typeof(T));
    }

    public async Task<DataTable> ExecuteRawSqlAsync(string sql, SqlParameter[]? parameters = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing raw SQL query");

        var dataTable = new DataTable();

        using var connection = _context.Database.GetDbConnection() as SqlConnection;
        if (connection == null)
            throw new InvalidOperationException("Database connection is not a SQL Server connection");

        await connection.OpenAsync(cancellationToken);

        using var command = new SqlCommand(sql, connection)
        {
            CommandType = CommandType.Text,
            CommandTimeout = 30
        };

        if (parameters != null)
        {
            command.Parameters.AddRange(parameters);
        }

        using var adapter = new SqlDataAdapter(command);
        adapter.Fill(dataTable);

        _logger.LogInformation("Raw SQL query completed. Returned {RowCount} rows", dataTable.Rows.Count);

        return dataTable;
    }

    public async Task<List<T>> ExecuteRawSqlAsync<T>(string sql, SqlParameter[]? parameters = null, CancellationToken cancellationToken = default) where T : class, new()
    {
        var dataTable = await ExecuteRawSqlAsync(sql, parameters, cancellationToken);
        return MapDataTableToList<T>(dataTable);
    }

    public async Task<int> ExecuteRawSqlNonQueryAsync(string sql, SqlParameter[]? parameters = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing raw SQL non-query");

        using var connection = _context.Database.GetDbConnection() as SqlConnection;
        if (connection == null)
            throw new InvalidOperationException("Database connection is not a SQL Server connection");

        await connection.OpenAsync(cancellationToken);

        using var command = new SqlCommand(sql, connection)
        {
            CommandType = CommandType.Text,
            CommandTimeout = 30
        };

        if (parameters != null)
        {
            command.Parameters.AddRange(parameters);
        }

        var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogInformation("Raw SQL non-query completed. {RowsAffected} rows affected", rowsAffected);

        return rowsAffected;
    }

    /// <summary>
    /// Maps a DataTable to a list of strongly typed objects using reflection
    /// </summary>
    private static List<T> MapDataTableToList<T>(DataTable dataTable) where T : class, new()
    {
        var list = new List<T>();
        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToArray();

        foreach (DataRow row in dataTable.Rows)
        {
            var item = new T();
            
            foreach (var property in properties)
            {
                if (dataTable.Columns.Contains(property.Name))
                {
                    var value = row[property.Name];
                    if (value != null && value != DBNull.Value)
                    {
                        // Handle nullable types
                        var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                        var convertedValue = Convert.ChangeType(value, targetType);
                        property.SetValue(item, convertedValue);
                    }
                }
            }
            
            list.Add(item);
        }

        return list;
    }
}
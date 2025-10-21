/*
 - Database Service for PostgreSQL Operations
 - This service handles all database interactions using the connection string from app settings
 - It follows the Repository pattern to separate data access from business logic
 */
using Npgsql;
using Microsoft.Extensions.Logging;
using VisitorsReg.Models;

namespace VisitorsReg.Services;

public class PostgresService
{
    private readonly string _connectionString;
    private readonly ILogger<PostgresService> _logger;

    // Constructor receives connection string and logger via Dependency Injection
    public PostgresService(string connectionString, ILogger<PostgresService> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    /*
     - Saves a visitor to the database
     - Returns true if successful, false if failed
     */
    public async Task<bool> SaveVisitorAsync(Visitor visitor)
    {
        try
        {
            // Using statement ensures database connection is properly closed and disposed
            using var connection = new NpgsqlConnection(_connectionString);

            // Open connection to PostgreSQL database
            await connection.OpenAsync();

            // SQL command to insert new visitor - parameterized to prevent SQL injection
            var sql = @"
                INSERT INTO visitors (name, email, registered_at) 
                VALUES (@name, @email, @registeredAt)";

            using var command = new NpgsqlCommand(sql, connection);

            // Add parameters to the command - this is SECURE against SQL injection
            command.Parameters.AddWithValue("name", visitor.Name);
            command.Parameters.AddWithValue("email", visitor.Email);
            command.Parameters.AddWithValue("registeredAt", DateTime.UtcNow); // Use UTC for consistency

            // Execute the command and get number of rows affected
            var rowsAffected = await command.ExecuteNonQueryAsync();

            // Log successful registration
            _logger.LogInformation("Visitor {Name} with email {Email} registered successfully",
                visitor.Name, visitor.Email);

            // Return true if exactly one row was inserted
            return rowsAffected == 1;
        }
        catch (Exception ex)
        {
            // Log the error with details for troubleshooting
            _logger.LogError(ex, "Error saving visitor {Name} with email {Email} to database",
                visitor.Name, visitor.Email);
            return false;
        }
    }

    /*
     - Checks if a visitor with the same email already exists
     - Prevents duplicate registrations
     */
    public async Task<bool> VisitorExistsAsync(string email)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // SQL to check if email already exists in database
            var sql = "SELECT COUNT(1) FROM visitors WHERE email = @email";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("email", email);

            // ExecuteScalar returns the first column of the first row
            var count = await command.ExecuteScalarAsync();

            // If count > 0, email already exists
            return Convert.ToInt32(count) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if visitor with email {Email} exists", email);
            // If we can't check, assume it doesn't exist to allow registration attempt
            return false;
        }
    }
}

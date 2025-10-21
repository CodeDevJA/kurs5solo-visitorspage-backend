/*
 - Main HTTP Trigger Function for Visitor Registration
 - This function handles POST requests from the frontend form
 - It validates input, checks for duplicates, and saves to database
 */
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VisitorsReg.Models;
using VisitorsReg.Services;

namespace VisitorsReg;

public class HttpTriggerVisitorFunction
{
    private readonly ILogger<HttpTriggerVisitorFunction> _logger;
    private readonly PostgresService _postgresService;

    // Constructor with Dependency Injection - Azure Functions automatically injects these services
    public HttpTriggerVisitorFunction(ILogger<HttpTriggerVisitorFunction> logger, PostgresService postgresService)
    {
        _logger = logger;
        _postgresService = postgresService;
    }

    [Function("HttpTriggerVisitorFunction")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processing visitor registration request.");

        try
        {
            // Read and parse the JSON request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<dynamic>(requestBody);

            // Extract name and email from request
            string? name = data?.name;
            string? email = data?.email;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Missing name or email in request");
                return new BadRequestObjectResult(new { error = "Name and email are required" });
            }

            // Basic validation to match frontend rules
            if (!System.Text.RegularExpressions.Regex.IsMatch(name, @"^[A-Za-z]+$"))
            {
                _logger.LogWarning("Invalid name format: {Name}", name);
                return new BadRequestObjectResult(new { error = "Invalid name: please enter a single word with letters only." });
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
            {
                _logger.LogWarning("Invalid email format: {Email}", email);
                return new BadRequestObjectResult(new { error = "Invalid email: please enter a valid email address." });
            }

            // Check if visitor with this email already exists
            bool visitorExists = await _postgresService.VisitorExistsAsync(email);
            if (visitorExists)
            {
                _logger.LogWarning("Duplicate registration attempt for email: {Email}", email);
                return new ConflictObjectResult(new { error = "A visitor with this email already exists." });
            }

            // Create new visitor object
            var visitor = new Visitor
            {
                Name = name.Trim(),
                Email = email.Trim().ToLower() // Store email in lowercase for consistency
            };

            // Save visitor to database
            bool saveResult = await _postgresService.SaveVisitorAsync(visitor);

            if (saveResult)
            {
                _logger.LogInformation("Successfully registered visitor: {Name} ({Email})", name, email);
                return new OkObjectResult(new { message = "Registration successful! Data saved to database." });
            }
            else
            {
                _logger.LogError("Failed to save visitor to database: {Name} ({Email})", name, email);
                return new StatusCodeResult(500); // Internal Server Error
            }
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "Invalid JSON in request body");
            return new BadRequestObjectResult(new { error = "Invalid JSON format" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing visitor registration");
            return new StatusCodeResult(500); // Internal Server Error
        }
    }
}

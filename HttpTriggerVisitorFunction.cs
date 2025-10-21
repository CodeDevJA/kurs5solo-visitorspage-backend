using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace VisitorsReg;

public class HttpTriggerVisitorFunction
{
    private readonly ILogger<HttpTriggerVisitorFunction> _logger;

    public HttpTriggerVisitorFunction(ILogger<HttpTriggerVisitorFunction> logger)
    {
        _logger = logger;
    }

    [Function("HttpTriggerVisitorFunction")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Welcome to Azure Functions!");
    }
}
namespace SwipeSwap.WebApi.Middleware;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var request = await FormatRequest(context.Request);
        _logger.LogInformation("Request: {Request}", request);

        var originalBodyStream = context.Response.Body;
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            throw; 
        }
        finally
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            _logger.LogInformation("Response: {Status} {Body}", context.Response.StatusCode, responseText);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);

            context.Response.Body = originalBodyStream;
            await responseBody.DisposeAsync();
        }
    }

    private static async Task<string> FormatRequest(HttpRequest request)
    {
        request.EnableBuffering();
        var body = await new StreamReader(request.Body).ReadToEndAsync();
        request.Body.Position = 0;
        return $"Method: {request.Method}, Path: {request.Path}, Query: {request.QueryString}, Body: {body}";
    }
}
namespace SwipeSwap.WebApi.Middleware;

public class LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        var request = await FormatRequest(context.Request);
        logger.LogInformation("Request: {Request}", request);

        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occurred");
            throw;
        }
        finally
        {
            var response = await FormatResponse(context.Response);
            logger.LogInformation("Response: {Response}", response);
            
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private static async Task<string> FormatRequest(HttpRequest request)
    {
        request.EnableBuffering();
        var body = await new StreamReader(request.Body).ReadToEndAsync();
        request.Body.Position = 0;

        return $"Method: {request.Method}, Path: {request.Path}, Query: {request.QueryString}, Body: {body}";
    }

    private static async Task<string> FormatResponse(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(response.Body).ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);

        return $"Status: {response.StatusCode}, Body: {body}";
    }
}
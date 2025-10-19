using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var startTime = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation(
                "[{Time}] Handling {RequestName} with payload {@Request}",
                startTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                requestName,
                request);

            var response = await next();

            stopwatch.Stop();
            var endTime = DateTime.UtcNow;

            _logger.LogInformation(
                "[{Time}] Handled {RequestName} with response {@Response} in {Elapsed} ms",
                endTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                requestName,
                response,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "[{Time}] Error handling {RequestName} after {Elapsed} ms with payload {@Request}",
                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                requestName,
                stopwatch.ElapsedMilliseconds,
                request);
            throw;
        }
    }
}
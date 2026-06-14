using Bpm.Core.Models;
using Bpm.Core.Services;
using Microsoft.Extensions.Logging;

namespace CaseMgmt.Server.Bpm;

/// <summary>
/// Logs BPM action execution results to the standard logger.
/// </summary>
public class ConsoleActionLog : IActionLog
{
    private readonly ILogger<ConsoleActionLog> _logger;

    public ConsoleActionLog(ILogger<ConsoleActionLog> logger) => _logger = logger;

    public Task LogAsync(TransitionContext context, TransitionActionResult result, CancellationToken ct = default)
    {
        foreach (var step in result.StepResults)
        {
            _logger.LogInformation(
                "BPM [{Action}] {Activity}: {Outcome} ({Duration:F1}ms){Error}",
                result.DefinitionName, step.ActivityType, step.Outcome,
                step.Duration.TotalMilliseconds,
                step.Error is not null ? $" — {step.Error}" : "");
        }
        return Task.CompletedTask;
    }
}

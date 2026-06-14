using Bpm.Core.Models;
using Bpm.Core.Services;

namespace CaseMgmt.Server.Bpm;

/// <summary>
/// In-memory action definitions for the Case entity.
/// In a real app this could be backed by a database or config file.
/// </summary>
public class CaseActionStore : ITransitionActionStore
{
    private static readonly List<TransitionActionDefinition> Definitions = new()
    {
        new()
        {
            Name = "Create follow-up on triage",
            EntityName = "Case",
            FieldName = "Status",
            FromValue = "New",
            ToValue = "Triaged",
            Steps = new()
            {
                new ActionStep
                {
                    ActivityType = "CreateRecord",
                    Config = new()
                    {
                        ["entity"] = "Activity",
                        ["field.Subject"] = "Triage follow-up for {{RecordId}}",
                        ["field.Type"] = "Task",
                        ["field.Status"] = "Open",
                        ["field.Priority"] = "Normal"
                    }
                },
                new ActionStep
                {
                    ActivityType = "SendNotification",
                    Config = new()
                    {
                        ["to"] = "support-team",
                        ["template"] = "case-triaged",
                        ["subject"] = "Case triaged: {{RecordId}}"
                    }
                }
            }
        },
        new()
        {
            Name = "Notify on case closed",
            EntityName = "Case",
            FieldName = "Status",
            FromValue = null,
            ToValue = "Closed",
            Steps = new()
            {
                new ActionStep
                {
                    ActivityType = "SendNotification",
                    Config = new()
                    {
                        ["to"] = "customer",
                        ["template"] = "case-closed",
                        ["subject"] = "Your case has been resolved"
                    }
                }
            }
        }
    };

    public Task<IReadOnlyList<TransitionActionDefinition>> GetByTriggerAsync(
        string entityName, string fieldName, CancellationToken ct = default)
    {
        var matches = Definitions
            .Where(d => d.EntityName == entityName && d.FieldName == fieldName)
            .ToList();
        return Task.FromResult<IReadOnlyList<TransitionActionDefinition>>(matches);
    }
}

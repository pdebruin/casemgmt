using System.Text.Json;
using Bpm.Core.Models;
using Bpm.Core.Services;
using Xrm.Core.Models;
using Xrm.Core.Services;

namespace CaseMgmt.Server.Bpm;

/// <summary>
/// XRM lifecycle handler that bridges field changes to BPM's dispatcher.
/// </summary>
public class BpmLifecycleHandler : IRecordLifecycleHandler
{
    private readonly TransitionActionDispatcher _dispatcher;

    public BpmLifecycleHandler(TransitionActionDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task OnUpdatedAsync(Record record, string oldDataJson, EntityDefinition entity, CancellationToken ct = default)
    {
        using var oldDoc = JsonDocument.Parse(oldDataJson);
        using var newDoc = JsonDocument.Parse(record.DataJson);

        foreach (var prop in newDoc.RootElement.EnumerateObject())
        {
            var newVal = prop.Value.ToString();
            var oldVal = oldDoc.RootElement.TryGetProperty(prop.Name, out var ov) ? ov.ToString() : null;

            if (newVal != oldVal)
            {
                var context = new TransitionContext
                {
                    EntityName = entity.Name,
                    RecordId = record.Id,
                    FieldName = prop.Name,
                    OldValue = oldVal,
                    NewValue = newVal,
                    UserId = "system"
                };

                await _dispatcher.DispatchAsync(context, ct);
            }
        }
    }
}

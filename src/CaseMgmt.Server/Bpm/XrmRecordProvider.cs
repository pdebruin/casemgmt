using System.Text.Json;
using Bpm.Core.Activities;
using Microsoft.Extensions.DependencyInjection;
using Xrm.Core.Services;

namespace CaseMgmt.Server.Bpm;

/// <summary>
/// Bridges BPM's IRecordProvider to XRM's IRecordService.
/// Uses IServiceProvider to break the circular DI dependency.
/// </summary>
public class XrmRecordProvider : IRecordProvider
{
    private readonly IServiceProvider _sp;

    public XrmRecordProvider(IServiceProvider sp)
    {
        _sp = sp;
    }

    public async Task CreateRecordAsync(string entityName, Dictionary<string, string> fields, CancellationToken ct = default)
    {
        using var scope = _sp.CreateScope();
        var entities = scope.ServiceProvider.GetRequiredService<IEntityService>();
        var records = scope.ServiceProvider.GetRequiredService<IRecordService>();

        var entity = (await entities.GetAllAsync()).First(e => e.Name == entityName);
        var json = JsonSerializer.Serialize(fields);
        await records.CreateAsync(entity.Id, json);
    }

    public async Task UpdateFieldAsync(string entityName, Guid recordId, string fieldName, string value, CancellationToken ct = default)
    {
        using var scope = _sp.CreateScope();
        var entities = scope.ServiceProvider.GetRequiredService<IEntityService>();
        var records = scope.ServiceProvider.GetRequiredService<IRecordService>();

        var entity = (await entities.GetAllAsync()).First(e => e.Name == entityName);
        var record = await records.GetByIdAsync(entity.Id, recordId);
        if (record is null) return;

        var data = JsonSerializer.Deserialize<Dictionary<string, object>>(record.DataJson)!;
        data[fieldName] = value;
        await records.UpdateAsync(entity.Id, recordId, JsonSerializer.Serialize(data));
    }
}

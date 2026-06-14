using System.Text.Json;
using Bpm.Core.Activities;
using Xrm.Core.Services;

namespace CaseMgmt.Server.Bpm;

/// <summary>
/// Bridges BPM's IRecordProvider to XRM's IRecordService.
/// </summary>
public class XrmRecordProvider : IRecordProvider
{
    private readonly IRecordService _records;
    private readonly IEntityService _entities;

    public XrmRecordProvider(IRecordService records, IEntityService entities)
    {
        _records = records;
        _entities = entities;
    }

    public async Task CreateRecordAsync(string entityName, Dictionary<string, string> fields, CancellationToken ct = default)
    {
        var entities = await _entities.GetAllAsync();
        var entity = entities.First(e => e.Name == entityName);
        var json = JsonSerializer.Serialize(fields);
        await _records.CreateAsync(entity.Id, json);
    }

    public async Task UpdateFieldAsync(string entityName, Guid recordId, string fieldName, string value, CancellationToken ct = default)
    {
        var entities = await _entities.GetAllAsync();
        var entity = entities.First(e => e.Name == entityName);
        var record = await _records.GetByIdAsync(entity.Id, recordId);
        if (record is null) return;

        var data = JsonSerializer.Deserialize<Dictionary<string, object>>(record.DataJson)!;
        data[fieldName] = value;
        await _records.UpdateAsync(entity.Id, recordId, JsonSerializer.Serialize(data));
    }
}

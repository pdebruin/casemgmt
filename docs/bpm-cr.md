# BPM Change Requests from CaseMgmt

| # | Feature | Impact | Blocking? | Status |
|---|---------|--------|-----------|--------|
| CR-001 | LinkRecord activity + StepContext | BPM creates a record and links it back to the triggering record via a named relationship | Yes — created Activities were orphaned without it | ✅ Done |
| CR-002 | Remove IRecordProvider, use XRM directly | Activities resolve XRM services via IServiceProvider | Yes (needed by CR-001) | ✅ Done |
| CR-003 | Process definitions visible via XRM UI | ProcessDefinition is now an XRM entity — viewable/editable in standard UI and API | No | ✅ Done |
| CR-004 | Record field templates (`{{Record.FieldName}}`) | Activity configs show GUIDs where human-readable values are needed | No | Open |

---

## CR-004 Detail: Record field templates

### Problem

Activity config templates can only reference transition metadata:
- `{{RecordId}}` — GUID
- `{{EntityName}}`, `{{FieldName}}`, `{{OldValue}}`, `{{NewValue}}`

There's no access to the triggering record's actual data fields. This produces
subjects like "Triage follow-up for 54eb5fc5-b842-4142-affc-8dff922c2b56"
instead of "Triage follow-up for CS00003".

### Proposed change

Add `{{Record.FieldName}}` template syntax. At dispatch time, the dispatcher
loads the triggering record's DataJson (it already has RecordId + EntityName)
and makes all fields available for substitution.

**Template resolution order:**
1. `{{StepContext.Key}}` — step-to-step output (existing)
2. `{{Record.FieldName}}` — triggering record's data fields (new)
3. `{{RecordId}}`, `{{EntityName}}`, etc. — transition metadata (existing)

### Implementation scope

`TransitionActionDispatcher.DispatchAsync`:
- After building `TransitionContext`, load record via `IRecordService.GetByIdAsync`
- Parse `DataJson` into a dictionary
- Pass the dictionary to activity template resolution alongside existing context

Activities' `Resolve()` helper adds one loop:
```csharp
foreach (var (key, value) in recordFields)
    template = template.Replace($"{{{{Record.{key}}}}}", value?.ToString() ?? "");
```

### Example

Before:
```json
{ "field.Subject": "Triage follow-up for {{RecordId}}" }
→ "Triage follow-up for 54eb5fc5-b842-4142-affc-8dff922c2b56"
```

After:
```json
{ "field.Subject": "Triage follow-up for {{Record.CaseNumber}}" }
→ "Triage follow-up for CS00003"
```

### Dependencies

None. The dispatcher already has access to IServiceProvider → IRecordService.
Record is loaded once per dispatch (not per step), so no performance concern.

### Acceptance criteria

- [ ] `{{Record.FieldName}}` resolves to the field value from the triggering record
- [ ] Unknown field names resolve to empty string (no crash)
- [ ] Existing `{{RecordId}}` etc. continue to work unchanged
- [ ] Unit test covers template resolution with record fields


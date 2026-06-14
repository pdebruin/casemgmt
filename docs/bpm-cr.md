# BPM Change Requests from CaseMgmt

| # | Feature | Impact | Blocking? | Status |
|---|---------|--------|-----------|--------|
| CR-001 | LinkRecord activity + StepContext | BPM creates a record and links it back to the triggering record via a named relationship | Yes — created Activities were orphaned without it | ✅ Done |
| CR-002 | Remove IRecordProvider, use XRM directly | Activities resolve XRM services via IServiceProvider | Yes (needed by CR-001) | ✅ Done |
| CR-003 | Process definitions visible via XRM UI | ProcessDefinition is now an XRM entity — viewable/editable in standard UI and API | No | ✅ Done |
| CR-004 | Record field templates (`{{Record.FieldName}}`) | Activity configs show GUIDs where human-readable values are needed | No | Open |
| CR-005 | Step editor Blazor component | StepsJson needs a visual card-based editor, not raw JSON editing | No (depends on XRM CR-001: pluggable field renderers) | Open |

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


---

## CR-005 Detail: Step editor Blazor component

### Problem

ProcessDefinition.StepsJson stores a JSON array of action steps. The default
XRM RichText renderer shows raw JSON — unusable for business users who need to
create, edit, and reorder process steps.

### Proposed change

BPM ships a Blazor component (`BpmStepEditor`) that registers as a custom
field renderer for ProcessDefinition.StepsJson (via XRM CR-001).

**UI design (inspired by Power Automate / Betty Blocks):**

```
┌─────────────────────────────────────────────┐
│ Steps                              [+ Add]  │
├─────────────────────────────────────────────┤
│ ⋮ 1. Create Record                    [✎🗑] │
│     Entity: Activity                        │
│     Subject: Triage follow-up for {{...}}   │
│     Type: Task | Status: Open               │
├─────────────────────────────────────────────┤
│ ⋮ 2. Link Record                      [✎🗑] │
│     Relationship: CaseActivities            │
│     Parent: {{RecordId}}                    │
│     Child: {{StepContext.LastCreatedRecordId}} │
├─────────────────────────────────────────────┤
│ ⋮ 3. Send Notification                [✎🗑] │
│     To: support-team                        │
│     Subject: Case triaged: {{...}}          │
└─────────────────────────────────────────────┘
```

**Interactions:**
- **Add step**: click [+], pick activity type from dropdown, fill config form
- **Edit step**: click [✎] to expand inline config form with labeled fields
- **Delete step**: click [🗑] with confirmation
- **Reorder**: drag handle (⋮) for drag-and-drop reordering
- **Template picker**: when editing a text field, dropdown shows available
  variables (`{{RecordId}}`, `{{Record.CaseNumber}}`, `{{StepContext.X}}`)

**Activity type metadata:**
BPM provides a registry of activity types with their config schemas:
```csharp
public class ActivityTypeInfo
{
    public string TypeName { get; set; }         // "CreateRecord"
    public string DisplayName { get; set; }      // "Create Record"
    public string Description { get; set; }      // "Creates a new XRM record"
    public List<ConfigFieldInfo> ConfigFields { get; set; }
}

public class ConfigFieldInfo
{
    public string Key { get; set; }              // "entity"
    public string Label { get; set; }            // "Target Entity"
    public bool Required { get; set; }
    public bool SupportsTemplates { get; set; }  // show variable picker?
}
```

### Dependencies

- **XRM CR-001** (pluggable field renderers) — must land first
- **BPM CR-004** (record field templates) — needed for the template picker to
  show `{{Record.X}}` options

### Acceptance criteria

- [ ] Steps render as visual cards with summary text (not raw JSON)
- [ ] Users can add a new step by selecting an activity type
- [ ] Step config is editable via form fields (not text editing)
- [ ] Steps can be reordered via drag-and-drop
- [ ] Steps can be deleted
- [ ] Template variable picker shows available placeholders
- [ ] Component serializes back to valid StepsJson on save
- [ ] Read-only mode shows step summaries without edit controls

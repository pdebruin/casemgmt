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
- **Reorder**: move up/down buttons (primary), drag handle as enhancement
- **Template picker**: when editing a text field, dropdown shows available
  variables (`{{RecordId}}`, `{{Record.CaseNumber}}`, `{{StepContext.X}}`)

**Activity type metadata:**
BPM provides a registry (`IActivityTypeRegistry`) of activity types with their
config schemas. Host packages register custom activities via
`AddBpmActivityType<T>(ActivityTypeInfo)`.

```csharp
public class ActivityTypeInfo
{
    public string TypeName { get; set; }         // "CreateRecord"
    public string DisplayName { get; set; }      // "Create Record"
    public string Description { get; set; }      // "Creates a new XRM record"
    public string SummaryTemplate { get; set; }  // "Create {{entity}} record"
    public List<ConfigFieldInfo> ConfigFields { get; set; }
    public List<string> OutputKeys { get; set; } // e.g. ["LastCreatedRecordId"]
    public bool AllowDynamicConfigKeys { get; set; } // for "field.*" patterns
}

public class ConfigFieldInfo
{
    public string Key { get; set; }              // "entity"
    public string Label { get; set; }            // "Target Entity"
    public ConfigFieldType FieldType { get; set; } // Text, Dropdown, EntityPicker, etc.
    public bool Required { get; set; }
    public bool SupportsTemplates { get; set; }  // show variable picker?
    public string? DefaultValue { get; set; }
    public List<string>? Options { get; set; }   // for Dropdown type
    public string? DependsOn { get; set; }       // dynamic options based on another field
}

public enum ConfigFieldType { Text, Dropdown, EntityPicker, FieldPicker, RelationshipPicker, Boolean }
```

**Template variable providers:**
The template picker dynamically assembles available variables:
1. Transition metadata: `{{RecordId}}`, `{{EntityName}}`, `{{FieldName}}`, etc.
2. Record fields: `{{Record.X}}` — derived from the ProcessDefinition's EntityName
   (looks up that entity's fields from XRM)
3. Step outputs: `{{StepContext.X}}` — derived from OutputKeys of preceding steps

### Error handling

**Invalid existing StepsJson:**
- On parse failure, show error banner with the raw JSON in read-only text area
- Preserve original value; do not overwrite with `[]`
- Provide explicit "Reset to empty" action with confirmation

**Unknown activity types:**
- Render as "Unknown: {TypeName}" card with raw config displayed
- Allow reorder and delete, but not config editing
- Preserve unknown steps exactly on save (no data loss)

**Validation:**
- Required config fields block save when empty
- Report validation state to parent form via XRM CR-001's `ValidationChanged`
- Invalid steps highlighted with inline error messages

### Editing lifecycle

- Component edits in-memory only; persistence via parent XRM form Save
- Parent Cancel discards all step edits (original Value restored by XRM)
- No auto-save or independent persistence

### Dependencies

- **XRM CR-001** (pluggable field renderers) — must land first
- **BPM CR-004** (record field templates) — needed for the template picker to
  show `{{Record.X}}` options

### Acceptance criteria

- [ ] Steps render as visual cards with summary text (not raw JSON)
- [ ] Users can add a new step by selecting an activity type
- [ ] Step config is editable via form fields (not text editing)
- [ ] Steps can be reordered (up/down buttons; drag-and-drop as enhancement)
- [ ] Steps can be deleted with confirmation
- [ ] Template variable picker shows context-aware placeholders
- [ ] Component serializes back to valid StepsJson on save
- [ ] Read-only mode shows step summaries without edit controls
- [ ] Invalid existing JSON shows error state, preserves original value
- [ ] Unknown activity types preserved as-is (no data loss)
- [ ] Host-registered custom activity types appear in the editor
- [ ] Invalid/incomplete steps block parent form save
- [ ] Cancel discards all unsaved step edits

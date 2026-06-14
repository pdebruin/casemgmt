# XRM Change Requests from CaseMgmt

| # | Feature | Impact | Blocking? | Status |
|---|---------|--------|-----------|--------|
| CR-001 | Pluggable field renderers | Packages can register custom Blazor components for specific fields | No (UI enhancement) | Open |

---

## CR-001 Detail: Pluggable field renderers

### Problem

XRM renders all fields using built-in components (TextInput, RichTextEditor,
ChoiceDropdown, etc.) based on `FieldDataType`. There's no way for an external
package (like BPM) to provide a custom editor for a specific field.

The `StepsJson` field on ProcessDefinition is stored as RichText but contains
structured JSON that should be edited with a purpose-built step editor — not a
raw text box.

### Proposed change

Allow packages to register custom Blazor render components for specific
entity/field combinations.

**Registration API:**
```csharp
builder.Services.AddXrmFieldRenderer(
    entityName: "ProcessDefinition",
    fieldName: "StepsJson",
    componentType: typeof(BpmStepEditorComponent));
```

**Resolution in Blazor UI:**
When XRM's `FieldEditor` component renders a field, it checks the registry:
1. If a custom renderer is registered for this entity+field → render that component
2. Otherwise → fall back to the default renderer for the field's DataType

**Component contract:**
```csharp
// Custom renderers receive these parameters
[Parameter] public string Value { get; set; }          // current field value
[Parameter] public EventCallback<string> ValueChanged { get; set; }  // update callback
[Parameter] public FieldDefinition Field { get; set; } // field metadata
[Parameter] public EntityDefinition Entity { get; set; } // parent entity metadata
[Parameter] public string RecordDataJson { get; set; }  // full record data (for context)
[Parameter] public bool ReadOnly { get; set; }
[Parameter] public EventCallback<string?> ValidationChanged { get; set; } // null=valid, string=error
```

### Scope

- `IFieldRendererRegistry` interface + in-memory implementation
- `AddXrmFieldRenderer()` extension method on `IServiceCollection`
- Extract reusable `FieldEditor` component from current inline rendering in
  `RecordDetail.razor` (prerequisite refactor)
- Update `FieldEditor` to check registry before default rendering
- No changes to data model or API

### Design decisions (from peer review)

**Render modes:** CR-001 applies to edit mode only. Read/detail and list views
continue to use default display rendering unless a future CR extends this.

**Validation:** Custom renderers report validity via `ValidationChanged`. The
parent form disables Save when any renderer reports a non-null error. Renderers
are responsible for enforcing field constraints (IsRequired, MaxLength) or
delegating to XRM's record service validation on save.

**Duplicate registration:** Throws `InvalidOperationException` at startup if
two packages register the same entity+field combination. Explicit
`AddXrmFieldRenderer(..., replace: true)` allows intentional override.

**Security:** Only trusted host-installed assemblies may register renderers.
Renderer types are never resolved from database metadata or user input.
Registered types must implement `IComponent`; validated at startup.

**Case sensitivity:** Entity/field name matching is case-insensitive (matching
XRM's existing behavior for entity lookups).

### Acceptance criteria

- [ ] Packages can register a custom component for a specific entity+field
- [ ] Custom component receives current value, entity context, and record data
- [ ] Custom component can report validation state to parent form
- [ ] Parent form blocks save when any renderer reports invalid state
- [ ] Unregistered fields continue to use default renderers
- [ ] Duplicate registration throws at startup (unless explicit replace)
- [ ] Registered types validated as IComponent at startup
- [ ] Read/list views unaffected (use default rendering)

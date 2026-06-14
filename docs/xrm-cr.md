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
[Parameter] public EventCallback<string> ValueChanged { get; set; }  // save callback
[Parameter] public FieldDefinition Field { get; set; } // field metadata
[Parameter] public bool ReadOnly { get; set; }
```

### Scope

- `IFieldRendererRegistry` interface + in-memory implementation
- `AddXrmFieldRenderer()` extension method on `IServiceCollection`
- Update `FieldEditor.razor` to check registry before default rendering
- No changes to data model or API

### Acceptance criteria

- [ ] Packages can register a custom component for a specific entity+field
- [ ] Custom component receives current value and can save changes
- [ ] Unregistered fields continue to use default renderers
- [ ] Multiple packages can register renderers for different fields without conflict

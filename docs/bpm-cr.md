# BPM Change Requests from CaseMgmt

| # | Feature | Impact | Blocking? | Status |
|---|---------|--------|-----------|--------|
| CR-001 | LinkRecord activity or auto-link in CreateRecord | When BPM creates a record as a side effect, it should optionally link it back to the triggering record via a named relationship | Yes — created Activities are orphaned without it | Open |
| CR-002 | IRecordProvider.LinkRecordAsync method | New method: `LinkRecordAsync(string relationshipName, Guid parentId, Guid childId)` | Yes (needed by CR-001) | Open |
| CR-003 | API to list transition action definitions | `GET /api/bpm/actions` — returns all registered TransitionActionDefinitions from the store, so users/admins can see what flows are configured | No | Open |
| CR-004 | Template: resolve display field instead of raw GUID | `{{RecordId}}` in subjects shows a GUID. Support `{{Record.CaseNumber}}` or similar to show the display value | No | Open |

## CR-001 Detail

Current behavior: `CreateRecord` activity creates a record but doesn't link it to the triggering record.

Expected: config option to auto-link:
```json
{
  "ActivityType": "CreateRecord",
  "Config": {
    "entity": "Activity",
    "field.Subject": "Follow-up",
    "linkTo.relationship": "CaseActivities",
    "linkTo.as": "child"
  }
}
```

Or a separate `LinkRecord` activity step:
```json
{
  "ActivityType": "LinkRecord",
  "Config": {
    "relationship": "CaseActivities",
    "parentId": "{{RecordId}}",
    "childId": "{{LastCreatedRecordId}}"
  }
}
```

The second option requires step-to-step context passing (the ID of the newly created record).


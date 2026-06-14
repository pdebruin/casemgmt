# CaseMgmt — Current State

A customer service case management application built on the XRM framework.

## Entities

| Entity | Description | Domain |
|--------|-------------|--------|
| **Account** | Customer organizations with service tier | Customers |
| **Contact** | People associated with accounts | Customers |
| **Activity** | Calls, meetings, emails, tasks | Activities |
| **Case** | Support tickets with priority, channel, category | Service |

## Relationships

```
Account ──OneToMany──→ Contact
Account ──OneToMany──→ Case
Account ──OneToMany──→ Activity
Contact ──OneToMany──→ Case
Case    ──OneToMany──→ Activity (cascade delete)
```

## State Machines

### Activity.Status

```
Open → In Progress → Completed
  │         │
  └→ Cancelled ←┘
     (can reopen → Open)
```

### Case.Status

```
New → Triaged → In Progress → Waiting on Customer → Resolved → Closed
 │       │          │                │                  │
 └→Closed └→Closed   ├→Closed         └→Closed          └→In Progress
                     └→Waiting on Customer
```

Terminal state: **Closed**

## Fields

### Account
- Name (required), Industry (choice), Phone, Email, Website, City, Country, Service Tier (Standard/Premium/Enterprise)

### Contact
- First Name (required), Last Name (required), Email, Phone, Job Title, Primary Contact (bool)

### Activity
- Subject (required), Type (Call/Meeting/Email/Task/Note), Due Date, Priority (Low/Normal/High/Urgent), Status (state machine), Notes

### Case
- Case Number (auto: CS00001), Title (required), Description, Priority (Low/Normal/High/Critical), Status (state machine), Channel (Phone/Email/Web/Chat/Social), Category (Bug/Feature Request/Question/Complaint/Billing/Other), Opened Date, Resolved Date, Resolution Notes

## Sample Data

| Entity | Records |
|--------|---------|
| Account | Acme Corp (Enterprise), Globex Industries (Premium), Initech Solutions (Standard) |
| Contact | Alice Johnson @ Acme, Bob Martinez @ Globex, Carol Davis @ Initech |
| Case | CS00001 (In Progress), CS00002 (Triaged), CS00003 (New) |
| Activity | Triage call (Completed), Follow-up email (Open) |

## Tech Stack

- .NET 10 / ASP.NET Core
- XRM framework (Xrm.Core + Xrm.Blazor) via project reference
- SQLite (`casemgmt.db`)
- Blazor Server UI (from XRM RCL)
- Swagger API docs in development mode

## Running

```bash
cd src/CaseMgmt.Server
dotnet run
```

http://localhost:5200

## Next Step

Integrate BPM Stage 1 to add transition actions on Case.Status changes.
See [bpm/docs/samples/1-xrm-for-cases.md](https://github.com/pdebruin/bpm/blob/main/docs/samples/1-xrm-for-cases.md).

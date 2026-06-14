# CaseMgmt — Customer Service Case Management

An XRM implementation for customer service scenarios: accounts, contacts,
activities, and support cases with state machine transitions.

Built on [XRM](https://github.com/pdebruin/xrm) as a reusable framework.
Intended as the starting point for [BPM](https://github.com/pdebruin/bpm) integration.

## Entities

- **Account** — customer organizations with service tier
- **Contact** — people associated with accounts
- **Activity** — calls, meetings, emails, tasks (with state machine)
- **Case** — support tickets with priority, channel, category, and a
  multi-step state machine: New → Triaged → In Progress → Waiting on Customer → Resolved → Closed

## Running

```bash
cd src/CaseMgmt.Server
dotnet run
```

Opens at http://localhost:5200

## Prerequisites

- .NET 10 SDK
- XRM source at `../xrm` (sibling directory)

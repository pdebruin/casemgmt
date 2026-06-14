using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Xrm.Core.Data;
using Xrm.Core.Models;

namespace CaseMgmt.Server;

public class CaseDataSeeder : IDataSeeder
{
    public async Task SeedAsync(XrmDbContext db)
    {
        if (await db.EntityDefinitions.AnyAsync())
            return;

        // --- Entity Definitions ---
        var account = new EntityDefinition
        {
            Id = Guid.NewGuid(), Name = "Account", DisplayName = "Account", PluralName = "Accounts",
            Description = "Customer organizations", Icon = "building", IsHomeEntity = true, SortOrder = 1,
            Domain = "Customers", DomainSortOrder = 1
        };
        var contact = new EntityDefinition
        {
            Id = Guid.NewGuid(), Name = "Contact", DisplayName = "Contact", PluralName = "Contacts",
            Description = "People associated with accounts", Icon = "person", SortOrder = 2,
            Domain = "Customers", DomainSortOrder = 2
        };
        var activity = new EntityDefinition
        {
            Id = Guid.NewGuid(), Name = "Activity", DisplayName = "Activity", PluralName = "Activities",
            Description = "Calls, meetings, emails, tasks", Icon = "calendar", SortOrder = 3,
            Domain = "Activities", DomainSortOrder = 1
        };
        var caseEntity = new EntityDefinition
        {
            Id = Guid.NewGuid(), Name = "Case", DisplayName = "Case", PluralName = "Cases",
            Description = "Customer service cases and support tickets", Icon = "clipboard", SortOrder = 4,
            Domain = "Service", DomainSortOrder = 1
        };

        db.EntityDefinitions.AddRange(account, contact, activity, caseEntity);

        // --- Field Definitions ---
        var fields = new List<FieldDefinition>
        {
            // Account fields
            new() { Id = Guid.NewGuid(), EntityDefinitionId = account.Id, Name = "Name", DisplayName = "Account Name", DataType = FieldDataType.Text, IsRequired = true, MaxLength = 200, SortOrder = 1 },
            new() { Id = Guid.NewGuid(), EntityDefinitionId = account.Id, Name = "Industry", DisplayName = "Industry", DataType = FieldDataType.Choice, OptionsJson = Json(new[] { "Technology", "Finance", "Healthcare", "Manufacturing", "Retail", "Government", "Education", "Other" }), SortOrder = 2 },
            new() { Id = Guid.NewGuid(), EntityDefinitionId = account.Id, Name = "Phone", DisplayName = "Phone", DataType = FieldDataType.Phone, SortOrder = 3 },
            new() { Id = Guid.NewGuid(), EntityDefinitionId = account.Id, Name = "Email", DisplayName = "Email", DataType = FieldDataType.Email, SortOrder = 4 },
            new() { Id = Guid.NewGuid(), EntityDefinitionId = account.Id, Name = "Website", DisplayName = "Website", DataType = FieldDataType.Url, SortOrder = 5 },
            new() { Id = Guid.NewGuid(), EntityDefinitionId = account.Id, Name = "City", DisplayName = "City", DataType = FieldDataType.Text, MaxLength = 100, SortOrder = 6 },
            new() { Id = Guid.NewGuid(), EntityDefinitionId = account.Id, Name = "Country", DisplayName = "Country", DataType = FieldDataType.Text, MaxLength = 100, SortOrder = 7 },
            new() { Id = Guid.NewGuid(), EntityDefinitionId = account.Id, Name = "ServiceTier", DisplayName = "Service Tier", DataType = FieldDataType.Choice, OptionsJson = Json(new[] { "Standard", "Premium", "Enterprise" }), SortOrder = 8 },

            // Contact fields
            new() { Id = Guid.NewGuid(), EntityDefinitionId = contact.Id, Name = "FirstName", DisplayName = "First Name", DataType = FieldDataType.Text, IsRequired = true, MaxLength = 100, SortOrder = 1 },
            new() { Id = Guid.NewGuid(), EntityDefinitionId = contact.Id, Name = "LastName", DisplayName = "Last Name", DataType = FieldDataType.Text, IsRequired = true, MaxLength = 100, SortOrder = 2 },
            new() { Id = Guid.NewGuid(), EntityDefinitionId = contact.Id, Name = "Email", DisplayName = "Email", DataType = FieldDataType.Email, SortOrder = 3 },
            new() { Id = Guid.NewGuid(), EntityDefinitionId = contact.Id, Name = "Phone", DisplayName = "Phone", DataType = FieldDataType.Phone, SortOrder = 4 },
            new() { Id = Guid.NewGuid(), EntityDefinitionId = contact.Id, Name = "JobTitle", DisplayName = "Job Title", DataType = FieldDataType.Text, MaxLength = 100, SortOrder = 5 },
            new() { Id = Guid.NewGuid(), EntityDefinitionId = contact.Id, Name = "IsMainContact", DisplayName = "Primary Contact", DataType = FieldDataType.Boolean, DefaultValue = "false", SortOrder = 6 },

            // Activity fields
            new() { Id = Guid.NewGuid(), EntityDefinitionId = activity.Id, Name = "Subject", DisplayName = "Subject", DataType = FieldDataType.Text, IsRequired = true, MaxLength = 200, SortOrder = 1 },
            new() { Id = Guid.NewGuid(), EntityDefinitionId = activity.Id, Name = "Type", DisplayName = "Type", DataType = FieldDataType.Choice, OptionsJson = Json(new[] { "Call", "Meeting", "Email", "Task", "Note" }), SortOrder = 2 },
            new() { Id = Guid.NewGuid(), EntityDefinitionId = activity.Id, Name = "DueDate", DisplayName = "Due Date", DataType = FieldDataType.DateTime, SortOrder = 3 },
            new() { Id = Guid.NewGuid(), EntityDefinitionId = activity.Id, Name = "Priority", DisplayName = "Priority", DataType = FieldDataType.Choice, OptionsJson = Json(new[] { "Low", "Normal", "High", "Urgent" }), SortOrder = 4 },
            new() { Id = Guid.NewGuid(), EntityDefinitionId = activity.Id, Name = "Status", DisplayName = "Status", DataType = FieldDataType.Choice,
                OptionsJson = Json(new[] { "Open", "In Progress", "Completed", "Cancelled" }),
                TransitionsJson = """{"Open":["In Progress","Cancelled"],"In Progress":["Completed","Cancelled"],"Cancelled":["Open"]}""",
                SortOrder = 5 },
            new() { Id = Guid.NewGuid(), EntityDefinitionId = activity.Id, Name = "Notes", DisplayName = "Notes", DataType = FieldDataType.RichText, SortOrder = 6 },

            // Case fields
            new() { Id = Guid.NewGuid(), EntityDefinitionId = caseEntity.Id, Name = "CaseNumber", DisplayName = "Case Number", DataType = FieldDataType.AutoNumber, DefaultValue = """{"prefix":"CS","width":5}""", SortOrder = 1 },
            new() { Id = Guid.NewGuid(), EntityDefinitionId = caseEntity.Id, Name = "Title", DisplayName = "Title", DataType = FieldDataType.Text, IsRequired = true, MaxLength = 300, SortOrder = 2 },
            new() { Id = Guid.NewGuid(), EntityDefinitionId = caseEntity.Id, Name = "Description", DisplayName = "Description", DataType = FieldDataType.RichText, SortOrder = 3 },
            new() { Id = Guid.NewGuid(), EntityDefinitionId = caseEntity.Id, Name = "Priority", DisplayName = "Priority", DataType = FieldDataType.Choice, OptionsJson = Json(new[] { "Low", "Normal", "High", "Critical" }), SortOrder = 4 },
            new() { Id = Guid.NewGuid(), EntityDefinitionId = caseEntity.Id, Name = "Status", DisplayName = "Status", DataType = FieldDataType.Choice,
                OptionsJson = Json(new[] { "New", "Triaged", "In Progress", "Waiting on Customer", "Resolved", "Closed" }),
                TransitionsJson = """{"New":["Triaged","Closed"],"Triaged":["In Progress","Closed"],"In Progress":["Waiting on Customer","Resolved","Closed"],"Waiting on Customer":["In Progress","Closed"],"Resolved":["Closed","In Progress"]}""",
                SortOrder = 5 },
            new() { Id = Guid.NewGuid(), EntityDefinitionId = caseEntity.Id, Name = "Channel", DisplayName = "Channel", DataType = FieldDataType.Choice, OptionsJson = Json(new[] { "Phone", "Email", "Web", "Chat", "Social" }), SortOrder = 6 },
            new() { Id = Guid.NewGuid(), EntityDefinitionId = caseEntity.Id, Name = "Category", DisplayName = "Category", DataType = FieldDataType.Choice, OptionsJson = Json(new[] { "Bug", "Feature Request", "Question", "Complaint", "Billing", "Other" }), SortOrder = 7 },
            new() { Id = Guid.NewGuid(), EntityDefinitionId = caseEntity.Id, Name = "OpenedDate", DisplayName = "Opened", DataType = FieldDataType.DateTime, SortOrder = 8 },
            new() { Id = Guid.NewGuid(), EntityDefinitionId = caseEntity.Id, Name = "ResolvedDate", DisplayName = "Resolved", DataType = FieldDataType.DateTime, SortOrder = 9 },
            new() { Id = Guid.NewGuid(), EntityDefinitionId = caseEntity.Id, Name = "Resolution", DisplayName = "Resolution Notes", DataType = FieldDataType.RichText, SortOrder = 10 },
        };
        db.FieldDefinitions.AddRange(fields);

        // Set primary display fields
        account.PrimaryFieldId = fields.First(f => f.EntityDefinitionId == account.Id && f.Name == "Name").Id;
        contact.PrimaryFieldId = fields.First(f => f.EntityDefinitionId == contact.Id && f.Name == "FirstName").Id;
        activity.PrimaryFieldId = fields.First(f => f.EntityDefinitionId == activity.Id && f.Name == "Subject").Id;
        caseEntity.PrimaryFieldId = fields.First(f => f.EntityDefinitionId == caseEntity.Id && f.Name == "CaseNumber").Id;

        // --- Relationship Definitions ---
        var accountContacts = new RelationshipDefinition
        {
            Id = Guid.NewGuid(), Name = "AccountContacts", DisplayName = "Account → Contacts",
            ParentEntityId = account.Id, ChildEntityId = contact.Id,
            RelationshipType = RelationshipType.OneToMany, CascadeBehavior = CascadeBehavior.RemoveLink
        };
        var accountCases = new RelationshipDefinition
        {
            Id = Guid.NewGuid(), Name = "AccountCases", DisplayName = "Account → Cases",
            ParentEntityId = account.Id, ChildEntityId = caseEntity.Id,
            RelationshipType = RelationshipType.OneToMany, CascadeBehavior = CascadeBehavior.RemoveLink
        };
        var contactCases = new RelationshipDefinition
        {
            Id = Guid.NewGuid(), Name = "ContactCases", DisplayName = "Contact → Cases",
            ParentEntityId = contact.Id, ChildEntityId = caseEntity.Id,
            RelationshipType = RelationshipType.OneToMany, CascadeBehavior = CascadeBehavior.RemoveLink
        };
        var caseActivities = new RelationshipDefinition
        {
            Id = Guid.NewGuid(), Name = "CaseActivities", DisplayName = "Case → Activities",
            ParentEntityId = caseEntity.Id, ChildEntityId = activity.Id,
            RelationshipType = RelationshipType.OneToMany, CascadeBehavior = CascadeBehavior.Cascade
        };
        var accountActivities = new RelationshipDefinition
        {
            Id = Guid.NewGuid(), Name = "AccountActivities", DisplayName = "Account → Activities",
            ParentEntityId = account.Id, ChildEntityId = activity.Id,
            RelationshipType = RelationshipType.OneToMany, CascadeBehavior = CascadeBehavior.RemoveLink
        };

        db.RelationshipDefinitions.AddRange(accountContacts, accountCases, contactCases, caseActivities, accountActivities);

        // --- Sample Records ---
        var acme = new Record { Id = Guid.NewGuid(), EntityDefinitionId = account.Id, DataJson = JsonSerializer.Serialize(new Dictionary<string, object> { ["Name"] = "Acme Corp", ["Industry"] = "Technology", ["Phone"] = "+1-555-0100", ["Email"] = "support@acme.example", ["City"] = "Seattle", ["Country"] = "USA", ["ServiceTier"] = "Enterprise" }) };
        var globex = new Record { Id = Guid.NewGuid(), EntityDefinitionId = account.Id, DataJson = JsonSerializer.Serialize(new Dictionary<string, object> { ["Name"] = "Globex Industries", ["Industry"] = "Manufacturing", ["Phone"] = "+1-555-0200", ["Email"] = "info@globex.example", ["City"] = "Portland", ["Country"] = "USA", ["ServiceTier"] = "Premium" }) };
        var initech = new Record { Id = Guid.NewGuid(), EntityDefinitionId = account.Id, DataJson = JsonSerializer.Serialize(new Dictionary<string, object> { ["Name"] = "Initech Solutions", ["Industry"] = "Finance", ["Phone"] = "+1-555-0300", ["Email"] = "hello@initech.example", ["City"] = "Austin", ["Country"] = "USA", ["ServiceTier"] = "Standard" }) };

        var alice = new Record { Id = Guid.NewGuid(), EntityDefinitionId = contact.Id, DataJson = JsonSerializer.Serialize(new Dictionary<string, object> { ["FirstName"] = "Alice", ["LastName"] = "Johnson", ["Email"] = "alice@acme.example", ["Phone"] = "+1-555-0101", ["JobTitle"] = "VP Engineering", ["IsMainContact"] = true }) };
        var bob = new Record { Id = Guid.NewGuid(), EntityDefinitionId = contact.Id, DataJson = JsonSerializer.Serialize(new Dictionary<string, object> { ["FirstName"] = "Bob", ["LastName"] = "Martinez", ["Email"] = "bob@globex.example", ["Phone"] = "+1-555-0201", ["JobTitle"] = "IT Manager", ["IsMainContact"] = true }) };
        var carol = new Record { Id = Guid.NewGuid(), EntityDefinitionId = contact.Id, DataJson = JsonSerializer.Serialize(new Dictionary<string, object> { ["FirstName"] = "Carol", ["LastName"] = "Davis", ["Email"] = "carol@initech.example", ["Phone"] = "+1-555-0301", ["JobTitle"] = "Operations Director", ["IsMainContact"] = true }) };

        var case1 = new Record { Id = Guid.NewGuid(), EntityDefinitionId = caseEntity.Id, DataJson = JsonSerializer.Serialize(new Dictionary<string, object> { ["CaseNumber"] = "CS00001", ["Title"] = "Login fails after password reset", ["Priority"] = "High", ["Status"] = "In Progress", ["Channel"] = "Email", ["Category"] = "Bug", ["OpenedDate"] = "2026-06-10T09:00:00" }) };
        var case2 = new Record { Id = Guid.NewGuid(), EntityDefinitionId = caseEntity.Id, DataJson = JsonSerializer.Serialize(new Dictionary<string, object> { ["CaseNumber"] = "CS00002", ["Title"] = "Request for bulk export feature", ["Priority"] = "Normal", ["Status"] = "Triaged", ["Channel"] = "Web", ["Category"] = "Feature Request", ["OpenedDate"] = "2026-06-11T14:30:00" }) };
        var case3 = new Record { Id = Guid.NewGuid(), EntityDefinitionId = caseEntity.Id, DataJson = JsonSerializer.Serialize(new Dictionary<string, object> { ["CaseNumber"] = "CS00003", ["Title"] = "Invoice discrepancy for June", ["Priority"] = "Critical", ["Status"] = "New", ["Channel"] = "Phone", ["Category"] = "Billing", ["OpenedDate"] = "2026-06-13T08:15:00" }) };

        var act1 = new Record { Id = Guid.NewGuid(), EntityDefinitionId = activity.Id, DataJson = JsonSerializer.Serialize(new Dictionary<string, object> { ["Subject"] = "Initial triage call with Alice", ["Type"] = "Call", ["Priority"] = "High", ["Status"] = "Completed", ["Notes"] = "Confirmed issue reproduces on Chrome. Escalated to dev team." }) };
        var act2 = new Record { Id = Guid.NewGuid(), EntityDefinitionId = activity.Id, DataJson = JsonSerializer.Serialize(new Dictionary<string, object> { ["Subject"] = "Follow up on export request", ["Type"] = "Email", ["Priority"] = "Normal", ["Status"] = "Open", ["DueDate"] = "2026-06-15T10:00:00" }) };

        db.Records.AddRange(acme, globex, initech, alice, bob, carol, case1, case2, case3, act1, act2);

        // --- Record Links ---
        db.RecordLinks.AddRange(
            new RecordLink { Id = Guid.NewGuid(), RelationshipDefinitionId = accountContacts.Id, ParentRecordId = acme.Id, ChildRecordId = alice.Id },
            new RecordLink { Id = Guid.NewGuid(), RelationshipDefinitionId = accountContacts.Id, ParentRecordId = globex.Id, ChildRecordId = bob.Id },
            new RecordLink { Id = Guid.NewGuid(), RelationshipDefinitionId = accountContacts.Id, ParentRecordId = initech.Id, ChildRecordId = carol.Id },
            new RecordLink { Id = Guid.NewGuid(), RelationshipDefinitionId = accountCases.Id, ParentRecordId = acme.Id, ChildRecordId = case1.Id },
            new RecordLink { Id = Guid.NewGuid(), RelationshipDefinitionId = accountCases.Id, ParentRecordId = globex.Id, ChildRecordId = case2.Id },
            new RecordLink { Id = Guid.NewGuid(), RelationshipDefinitionId = accountCases.Id, ParentRecordId = initech.Id, ChildRecordId = case3.Id },
            new RecordLink { Id = Guid.NewGuid(), RelationshipDefinitionId = contactCases.Id, ParentRecordId = alice.Id, ChildRecordId = case1.Id },
            new RecordLink { Id = Guid.NewGuid(), RelationshipDefinitionId = contactCases.Id, ParentRecordId = bob.Id, ChildRecordId = case2.Id },
            new RecordLink { Id = Guid.NewGuid(), RelationshipDefinitionId = contactCases.Id, ParentRecordId = carol.Id, ChildRecordId = case3.Id },
            new RecordLink { Id = Guid.NewGuid(), RelationshipDefinitionId = caseActivities.Id, ParentRecordId = case1.Id, ChildRecordId = act1.Id },
            new RecordLink { Id = Guid.NewGuid(), RelationshipDefinitionId = caseActivities.Id, ParentRecordId = case2.Id, ChildRecordId = act2.Id }
        );

        await db.SaveChangesAsync();
    }

    private static string Json<T>(T value) => JsonSerializer.Serialize(value);
}

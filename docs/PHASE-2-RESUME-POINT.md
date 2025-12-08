# Phase 2 - Resume Point: Generate Migrations

**Last Updated**: 2025-12-04
**Current Step**: Step 5 - Generate Migrations
**Progress**: 80% Complete (Steps 1-4 done)

---

## 🎯 WHERE WE LEFT OFF

We completed Steps 1-4 of Phase 2:
- ✅ Installed EF Core packages
- ✅ Created 3 DbContexts (PosDbContext, ManagementDbContext, SharedDbContext)
- ✅ Configured connection strings (appsettings.json)
- ✅ Registered DbContexts with Dependency Injection
- ✅ Secured passwords in .gitignore
- ✅ Solution builds successfully: 0 Errors, 0 Warnings

---

## 🚀 NEXT: GENERATE MIGRATIONS (Step 5)

### What Are Migrations?

Migrations are **code files** that tell EF Core how to create your database tables. Think of them as construction blueprints.

**Your Entities (C# classes)** → **Migration (blueprint)** → **SQL Server (tables)**

### Why 3 Separate Migrations?

We have 3 DbContexts, so we need 3 migrations:

1. **PosDbContext** → Creates POS operational tables (Transactions, Payments)
2. **ManagementDbContext** → Creates master data tables (Products, Debtors, Pricelists)
3. **SharedDbContext** → Creates gatekeeping tables (ErrorLogs, AuditLogs)

---

## 📋 COMMANDS TO RUN

Run these 3 commands from the **project root directory**:

```bash
# Navigate to project root
cd C:\Users\Jason\Documents\Mine\projects\Personal\Project420
```

### 1. Generate PosDbContext Migration

```bash
dotnet ef migrations add InitialCreate --project src/Modules/Retail/POS/Project420.Retail.POS.DAL --startup-project src/Modules/Retail/POS/Project420.Retail.POS.UI.Blazor --context PosDbContext
```

**What this does:**
- Creates `Migrations/` folder in Retail.POS.DAL
- Generates migration files with SQL to create:
  - Products table
  - Debtors table
  - Pricelists table
  - PricelistItems table
  - TransactionHeaders table
  - TransactionDetails table
  - Payments table

### 2. Generate ManagementDbContext Migration

```bash
dotnet ef migrations add InitialCreate --project src/Modules/Management/Project420.Management.DAL --startup-project src/Modules/Retail/POS/Project420.Retail.POS.UI.Blazor --context ManagementDbContext
```

**What this does:**
- Creates `Migrations/` folder in Management.DAL
- Generates migration files with SQL to create:
  - Debtors table (master)
  - Products table (master)
  - Pricelists table (master)
  - UserProfiles table

### 3. Generate SharedDbContext Migration

```bash
dotnet ef migrations add InitialCreate --project src/Shared/Project420.Shared.Database --startup-project src/Modules/Retail/POS/Project420.Retail.POS.UI.Blazor --context SharedDbContext
```

**What this does:**
- Creates `Migrations/` folder in Shared.Database
- Generates migration files with SQL to create:
  - ErrorLogs table
  - AuditLogs table

---

## 🔍 WHAT TO EXPECT

After running each command, you'll see:

```
Build started...
Build succeeded.
Done. To undo this action, use 'ef migrations remove'
```

And a new `Migrations/` folder will appear with files like:
- `20251204_InitialCreate.cs` (the migration)
- `20251204_InitialCreate.Designer.cs` (metadata)
- `PosDbContextModelSnapshot.cs` (current state)

---

## ⚠️ TROUBLESHOOTING

**If you get connection errors:**
- Check SQL Server is running
- Verify connection string in appsettings.json
- Server: `JASON\SQLDEVED`
- User: `sa`
- Password: `ZAQxsw123`

**If migrations already exist:**
```bash
# Remove existing migrations
dotnet ef migrations remove --project [project-path] --context [ContextName]
```

---

## 📊 DATABASE ARCHITECTURE REMINDER

**Two Databases Will Be Created:**

### Project420_Dev (Business Data)
- Used by: PosDbContext + ManagementDbContext
- Contains: Products, Debtors, Transactions, Payments, Pricelists, UserProfiles

### Project420_Shared (Gatekeeping/Infrastructure)
- Used by: SharedDbContext
- Contains: ErrorLogs, AuditLogs
- Future: Users, Roles, Permissions, Stations (POS terminals)

**Why separate?**
- Security isolation (auth separate from business data)
- Different scaling needs (read-heavy auth vs write-heavy transactions)
- Audit trail protection (can't delete your own logs)
- Compliance (auditors can access logs without seeing business data)

---

## 📝 AFTER MIGRATIONS ARE GENERATED

**Step 6: Apply Migrations (Creates Actual Databases)**

Run these commands to create the databases:

```bash
# Create POS tables in Project420_Dev
dotnet ef database update --project src/Modules/Retail/POS/Project420.Retail.POS.DAL --startup-project src/Modules/Retail/POS/Project420.Retail.POS.UI.Blazor --context PosDbContext

# Create Management tables in Project420_Dev
dotnet ef database update --project src/Modules/Management/Project420.Management.DAL --startup-project src/Modules/Retail/POS/Project420.Retail.POS.UI.Blazor --context ManagementDbContext

# Create Project420_Shared database
dotnet ef database update --project src/Shared/Project420.Shared.Database --startup-project src/Modules/Retail/POS/Project420.Retail.POS.UI.Blazor --context SharedDbContext
```

**Result:** Your SQL Server will have 2 new databases with all tables created!

---

## ✅ SUCCESS CRITERIA

You'll know it worked when:
- ✅ 3 `Migrations/` folders created
- ✅ No errors in migration generation
- ✅ Migration files contain `CreateTable` statements
- ✅ (After Step 6) Databases visible in SQL Server Management Studio

---

## 🎓 LEARNING RESOURCES

**Key Concepts:**
- **Migration**: Code file describing database changes
- **Up()**: Creates tables/columns (forward)
- **Down()**: Drops tables/columns (rollback)
- **ModelSnapshot**: Current state of your entities

**EF Core Migrations Docs:**
https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/

---

## 📞 QUICK REFERENCE

**Current Solution Status:**
- ✅ All projects build successfully
- ✅ 3 DbContexts configured
- ✅ Connection strings set up
- ✅ DI registration complete
- ⏳ Ready to generate migrations

**Next Session Start Here:**
1. Open terminal in project root
2. Run the 3 migration commands above
3. Review generated migration files
4. Proceed to Step 6 (apply migrations)

---

**Good luck! You're 80% through Phase 2!** 🚀

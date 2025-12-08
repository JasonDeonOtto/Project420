# Multi-Tenant Architecture Implementation Guide

**Created**: 2025-12-05
**Status**: IN PROGRESS - Ready for Implementation
**Phase**: Phase 2 - Database Architecture Correction

---

## 📋 TABLE OF CONTENTS

1. [Architecture Overview](#architecture-overview)
2. [Current State](#current-state)
3. [The Problem](#the-problem)
4. [The Solution](#the-solution)
5. [Questions to Answer](#questions-to-answer)
6. [Implementation Steps](#implementation-steps)
7. [Code Snippets](#code-snippets)
8. [Testing Checklist](#testing-checklist)

---

## 🏗️ ARCHITECTURE OVERVIEW

### **Multi-Tenant Architecture Goal**

Build a system where:
- **Multiple companies** can use the same application
- **Each company** has its own isolated database
- **Stations/PCs** can access one or more companies
- **Users** belong to specific companies (database-specific)
- **Authentication** happens against the selected company's database

### **Database Structure**

```
┌─────────────────────────────────────┐
│   Project420_Shared (Gateway DB)    │
│                                     │
│  - StationConnections               │ ← Which PCs access which companies?
│  - ErrorLogs (cross-system)         │ ← Global error tracking
│  - AuditLogs (cross-system)         │ ← Global compliance logs
└─────────────────────────────────────┘
            │
            ├──→ Authorized connections
            │
    ┌───────┴────────┬─────────────────┬──────────────────┐
    │                │                 │                  │
    ▼                ▼                 ▼                  ▼
┌─────────┐    ┌─────────┐      ┌─────────┐      ┌─────────┐
│Company A│    │Company B│      │Company C│      │Company N│
│  (Dev)  │    │ (Dev2)  │      │ (Dev3)  │      │  (...)  │
└─────────┘    └─────────┘      └─────────┘      └─────────┘
    │
    └──→ Each contains:
         - Users (company-specific)
         - UserProfiles
         - Stations (terminal config)
         - StationPeripherals (printers, etc.)
         - Products, Debtors, Pricelists
         - Transactions, Payments
```

### **Login Flow**

```
1. App Starts
   ↓
2. Get Machine Hostname: "POS-STORE-01"
   ↓
3. Query Shared DB: "Which companies can this PC access?"
   → Returns: ["Company A", "Company B"]
   ↓
4. User Selects Company: "Company A"
   ↓
5. App Connects to Project420_Dev (Company A's database)
   ↓
6. User Enters Username/Password
   ↓
7. Authenticate Against Project420_Dev.Users table
   ↓
8. Success → Access Company A's business data
```

---

## 📍 CURRENT STATE

### ✅ **What's Already Done**

1. **Database Created**: `Project420_Shared` with ErrorLogs, AuditLogs
2. **Database Created**: `Project420_Dev` with Products, Debtors, Pricelists, etc.
3. **Migration Applied**: Users + UserPermissions tables **INCORRECTLY created in Shared DB**
4. **Seed Data**: 5 users seeded **in wrong location** (Shared DB instead of Business DB)
5. **Build Status**: Solution builds successfully (0 errors, 0 warnings)

### ❌ **The Mistake**

**Users table was created in `Project420_Shared`** but it should be in **`Project420_Dev`** (and future business databases).

**Why this is wrong:**
- Users belong to a specific company (database)
- Different companies have different users
- Authentication should happen against the selected company's database
- UserProfile links to User (both should be in same DB)

### 📦 **Current Table Locations**

| Table | Current Location | Correct Location |
|-------|-----------------|------------------|
| Users | ❌ Project420_Shared | ✅ Project420_Dev |
| UserPermissions | ❌ Project420_Shared | ✅ Project420_Dev |
| UserProfiles | ✅ Project420_Dev | ✅ Project420_Dev |
| Products | ✅ Project420_Dev | ✅ Project420_Dev |
| Debtors | ✅ Project420_Dev | ✅ Project420_Dev |
| Pricelists | ✅ Project420_Dev | ✅ Project420_Dev |
| TransactionHeaders | ✅ Project420_Dev | ✅ Project420_Dev |
| TransactionDetails | ✅ Project420_Dev | ✅ Project420_Dev |
| Payments | ✅ Project420_Dev | ✅ Project420_Dev |
| ErrorLogs | ✅ Project420_Shared | ✅ Project420_Shared |
| AuditLogs | ✅ Project420_Shared | ✅ Project420_Shared |
| StationConnections | ❌ NOT CREATED YET | ✅ Project420_Shared |

---

## 🔥 THE PROBLEM

### **Issue 1: Users in Wrong Database**

**Current Code** (`SharedDbContext.cs` lines 95-104):
```csharp
public DbSet<User> Users { get; set; } = null!;
public DbSet<UserPermission> UserPermissions { get; set; } = null!;
```

**Current Migration**: `20251205043641_AddUsersAndPermissions` applied to Shared DB

**Impact:**
- All companies would share the same users (WRONG!)
- Can't isolate company data
- Defeats multi-tenant architecture

### **Issue 2: Missing Station Connection Infrastructure**

We need:
- `StationConnection` entity
- Connection routing logic
- Dynamic DbContext creation per company

---

## ✅ THE SOLUTION

### **Phase 1: Fix User Location**
1. Move Users/UserPermissions from Shared DB to Management DB
2. Re-seed user data in correct location

### **Phase 2: Add Connection Infrastructure**
1. Create StationConnection entity
2. Add to SharedDbContext
3. Seed connection metadata

### **Phase 3: Add Station Details**
1. Create Station entity (operational config)
2. Create StationPeripheral entity
3. Add to ManagementDbContext

### **Phase 4: Implement Authentication Service**
1. Company selection service
2. Dynamic connection service
3. Multi-tenant authentication

---

## ❓ QUESTIONS TO ANSWER

### **Question 1: Station Identification Method**

How should we identify the connecting station/PC?

**Option A: Machine Name** (Simplest)
```csharp
string hostName = Environment.MachineName; // "POS-STORE-01"
```
- ✅ Simple to implement
- ✅ Human-readable
- ❌ Can be spoofed (rename PC)
- ❌ Not unique if cloned machines

**Option B: IP Address**
```csharp
string hostName = GetLocalIPAddress(); // "192.168.1.100"
```
- ✅ Network-level identifier
- ❌ Changes with DHCP
- ❌ Same IP across different networks

**Option C: MAC Address** (Most Secure)
```csharp
string hostName = GetMACAddress(); // "00:1B:44:11:3A:B7"
```
- ✅ Hardware-level unique
- ✅ Cannot be easily changed
- ❌ More complex to manage
- ❌ Need to handle multiple NICs

**Option D: Hardware ID / License Key**
```csharp
string hostName = GetHardwareId(); // "HW-ABC123-XYZ789"
```
- ✅ Most secure
- ✅ Supports licensing model
- ❌ Complex to implement
- ❌ Requires activation process

**YOUR DECISION:** ✅ **Machine Name (Phase 1) → Hardware ID (Phase 4)**
- Start with Environment.MachineName for simplicity (POC + Desktop terminals)
- Design HostName field to accept any identifier (future-proof)
- Upgrade to Hardware ID licensing system in Phase 4
- Migration path: "POS-STORE-01" → "HW-ABC123-XYZ789"

---

### **Question 2: Connection String Encryption**

How should we encrypt/decrypt database connection strings?

**Option A: DPAPI (Windows Data Protection API)**
```csharp
// Built into Windows, uses machine/user keys
byte[] encrypted = ProtectedData.Protect(plaintext, entropy, DataProtectionScope.LocalMachine);
```
- ✅ Built-in, no additional packages
- ✅ Secure
- ❌ Windows-only
- ❌ Tied to machine (can't move encrypted data)

**Option B: AES Encryption with Secure Key Storage**
```csharp
// AES-256 with key in appsettings or Azure Key Vault
```
- ✅ Cross-platform
- ✅ Flexible key management
- ❌ Need to secure the key
- ❌ More complex

**Option C: Azure Key Vault**
```csharp
// Store connection strings in Azure Key Vault
```
- ✅ Enterprise-grade security
- ✅ Centralized management
- ❌ Requires Azure subscription
- ❌ Adds external dependency

**Option D: Environment Variables** (Least Secure)
```csharp
// Store in environment variables per machine
string connectionString = Environment.GetEnvironmentVariable("COMPANY_A_CONN");
```
- ✅ Simple
- ✅ No encryption code needed
- ❌ Less secure
- ❌ Manual setup per machine

**YOUR DECISION:** ✅ **AES-256 Encryption (Option B)**
- Cross-platform support (Web API, Android, Desktop)
- Flexible key management (appsettings for dev, Azure Key Vault for production)
- Implement encryption/decryption service in Shared.Infrastructure
- Secure key storage strategy per environment

---

### **Question 3: Multi-Company User Scenario**

Can a single user access multiple companies?

**Scenario A: User per Company** (Recommended)
- User "john" in Company A is different from user "john" in Company B
- Different UserIDs, different passwords
- Complete isolation
- Regional manager has separate accounts per franchise

**Scenario B: Shared User Identity**
- User "john" exists once in Shared DB
- Links to multiple companies via junction table
- Same password across all companies
- Role/permissions per company

**Scenario C: Hybrid**
- User created per company (separate accounts)
- Optional: Link accounts with "master profile" in Shared DB for reporting

**YOUR DECISION:** ✅ **Option 1: User per Company**
- John at Company A = separate account in Company A's database
- John at Company B = separate account in Company B's database
- Complete data isolation per company (security + compliance)
- Different UserIDs, different passwords, independent permissions
- Tradeoff: More admin work, but maximum security and audit clarity

---

### **Question 4: Station Management UI**

Where will you manage StationConnections (which PCs access which companies)?

**Option A: Super-Admin Tool**
- Separate standalone app
- Connects directly to Shared DB
- Full control over connections
- Used by system administrators only

**Option B: Management Module**
- Part of existing back-office
- Requires special "System Admin" role
- Needs to connect to both Shared and Business DB

**Option C: Database Direct**
- Manage via SQL scripts / SSMS
- No UI needed initially
- Technical staff only

**YOUR DECISION:** ✅ **Super-Admin Tool under Management Module (Option A + B Hybrid)**
- Part of Management module (integrated, not standalone)
- Requires special "SuperAdmin" role
- Used for initial setup and deployments (pre-database population)
- Prevents site deviation by avoiding direct SQL script management
- Connects to both Shared and Business databases with elevated privileges

---

### **Question 5: Station Details Scope**

What information belongs in Station vs StationConnection?

**StationConnection (Shared DB):**
- Hostname/Hardware ID
- Company access list
- Connection strings
- Authorization flags

**Station (Business DB):**
- Station Name (e.g., "Main Counter POS")
- Station Type (POS Terminal, Mobile, Kiosk, Back Office)
- ??? (Need your input)

**What else should be in Station entity?**
- ✅ **Phase 1 (Now)**: Name, Type, Location, Department
- ✅ **Phase 2 (Later)**: DefaultPricelist, AllowOfflineMode, ReceiptTemplate
- ✅ **Phase 3 (Later)**: ShiftManagement, CashFloatAmount, MaxDiscountPercent

**StationPeripheral (Business DB):**
- Printer model, IP address, port
- Receipt template path
- Cash drawer serial port
- Scanner settings
- ✅ **Phase 1 (Now)**: Basic structure (PeripheralType, Model, ConnectionType)
- ✅ **Phase 2 (Later)**: IPAddress, Port, Settings (JSON config)
- ✅ **Phase 3 (Later)**: Status monitoring, error logging, auto-recovery

---

## 🚀 IMPLEMENTATION STEPS

### **PHASE 1: Fix Users Location (CRITICAL)**

#### **Step 1.1: Remove Users from Shared DB**

**Location:** `src/Shared/Project420.Shared.Database/`

**Actions:**
1. Open `SharedDbContext.cs`
2. Remove lines 95-104 (Users and UserPermissions DbSets)
3. Remove lines 167-340 (User and UserPermission configuration)
4. Save file

**Files Modified:**
- `src/Shared/Project420.Shared.Database/SharedDbContext.cs`

**Command:**
```bash
cd C:\Users\Jason\Documents\Mine\projects\Personal\Project420

# Create migration to drop Users tables from Shared DB
dotnet ef migrations add RemoveUsersFromShared ^
  --project src/Shared/Project420.Shared.Database ^
  --startup-project src/Modules/Retail/POS/Project420.Retail.POS.UI.Blazor ^
  --context SharedDbContext

# Apply migration (drops Users and UserPermissions tables)
dotnet ef database update ^
  --project src/Shared/Project420.Shared.Database ^
  --startup-project src/Modules/Retail/POS/Project420.Retail.POS.UI.Blazor ^
  --context SharedDbContext
```

**Expected Result:**
- `Users` table dropped from Project420_Shared
- `UserPermissions` table dropped from Project420_Shared
- Migration created: `YYYYMMDDHHMMSS_RemoveUsersFromShared.cs`

---

#### **Step 1.2: Add Users to Management DB**

**Location:** `src/Modules/Management/Project420.Management.DAL/`

**Actions:**
1. Open `ManagementDbContext.cs`
2. Add after line 128 (after PricelistItems DbSet):

```csharp
/// <summary>
/// Users table - Authentication and user identity (COMPANY-SPECIFIC)
/// </summary>
/// <remarks>
/// Each company database has its own Users table.
/// Users belong to a specific company.
/// Authentication happens against this table after company selection.
/// </remarks>
public DbSet<User> Users { get; set; } = null!;

/// <summary>
/// User permissions table - Granular permission assignments
/// </summary>
/// <remarks>
/// Company-specific user permissions.
/// Phase 2: Table exists but not used (permissions hardcoded by role)
/// Phase 4: Stores user-specific permission grants/denies
/// </remarks>
public DbSet<UserPermission> UserPermissions { get; set; } = null!;
```

3. Add configuration in `OnModelCreating` method (before closing brace of method):

```csharp
// ===========================
// USER CONFIGURATION
// ===========================
modelBuilder.Entity<User>(entity =>
{
    // Indexes for performance and uniqueness
    entity.HasIndex(u => u.Username)
        .IsUnique()
        .HasDatabaseName("IX_Users_Username");

    entity.HasIndex(u => u.Email)
        .IsUnique()
        .HasDatabaseName("IX_Users_Email");

    entity.HasIndex(u => u.IsActive)
        .HasDatabaseName("IX_Users_IsActive");

    entity.HasIndex(u => u.Role)
        .HasDatabaseName("IX_Users_Role");

    entity.HasIndex(u => u.LastLoginAt)
        .HasDatabaseName("IX_Users_LastLogin");

    // Soft delete query filter (POPIA compliance)
    entity.HasQueryFilter(u => !u.IsDeleted);

    // Navigation property configuration (UserPermissions)
    entity.HasMany(u => u.UserPermissions)
        .WithOne(up => up.User)
        .HasForeignKey(up => up.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    // Relationship to UserProfile (one-to-one)
    entity.HasOne<UserProfile>()
        .WithOne(up => up.User)
        .HasForeignKey<UserProfile>(up => up.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    // ===========================
    // SEED DATA - Default Users
    // ===========================
    // Password for all seed users: "Project420!Pass"
    // In production, these should be changed immediately after first login
    const string seedPasswordHash = "$2a$11$EK5kC8qGqJH5rZvK.YjJGuH7Z6uYF6NjN9XoYdQdH3xRZLqKxhE/G";
    var seedDate = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc);

    entity.HasData(
        // 1. SuperAdmin - Full system access
        new User
        {
            Id = 1,
            Username = "superadmin",
            Email = "superadmin@project420.local",
            PasswordHash = seedPasswordHash,
            FirstName = "Super",
            LastName = "Admin",
            Role = "SuperAdmin",
            IsActive = true,
            IsLocked = false,
            FailedLoginAttempts = 0,
            TwoFactorEnabled = false,
            CreatedAt = seedDate,
            CreatedBy = "SYSTEM",
            IsDeleted = false
        },

        // 2. Admin - Administrative access
        new User
        {
            Id = 2,
            Username = "admin",
            Email = "admin@project420.local",
            PasswordHash = seedPasswordHash,
            FirstName = "System",
            LastName = "Administrator",
            Role = "Admin",
            IsActive = true,
            IsLocked = false,
            FailedLoginAttempts = 0,
            TwoFactorEnabled = false,
            CreatedAt = seedDate,
            CreatedBy = "SYSTEM",
            IsDeleted = false
        },

        // 3. Manager - Store management
        new User
        {
            Id = 3,
            Username = "manager",
            Email = "manager@project420.local",
            PasswordHash = seedPasswordHash,
            FirstName = "Store",
            LastName = "Manager",
            Role = "Manager",
            IsActive = true,
            IsLocked = false,
            FailedLoginAttempts = 0,
            TwoFactorEnabled = false,
            CreatedAt = seedDate,
            CreatedBy = "SYSTEM",
            IsDeleted = false
        },

        // 4. Cashier - POS operations
        new User
        {
            Id = 4,
            Username = "cashier",
            Email = "cashier@project420.local",
            PasswordHash = seedPasswordHash,
            FirstName = "POS",
            LastName = "Cashier",
            Role = "Cashier",
            IsActive = true,
            IsLocked = false,
            FailedLoginAttempts = 0,
            TwoFactorEnabled = false,
            CreatedAt = seedDate,
            CreatedBy = "SYSTEM",
            IsDeleted = false
        },

        // 5. Inventory User - Inventory management
        new User
        {
            Id = 5,
            Username = "inventory",
            Email = "inventory@project420.local",
            PasswordHash = seedPasswordHash,
            FirstName = "Inventory",
            LastName = "Manager",
            Role = "Inventory",
            IsActive = true,
            IsLocked = false,
            FailedLoginAttempts = 0,
            TwoFactorEnabled = false,
            CreatedAt = seedDate,
            CreatedBy = "SYSTEM",
            IsDeleted = false
        }
    );
});

// ===========================
// USERPERMISSION CONFIGURATION
// ===========================
modelBuilder.Entity<UserPermission>(entity =>
{
    // Indexes for performance
    entity.HasIndex(up => up.UserId)
        .HasDatabaseName("IX_UserPermissions_UserId");

    entity.HasIndex(up => up.Permission)
        .HasDatabaseName("IX_UserPermissions_Permission");

    entity.HasIndex(up => new { up.UserId, up.Permission })
        .IsUnique()
        .HasDatabaseName("IX_UserPermissions_UserId_Permission");

    entity.HasIndex(up => up.IsActive)
        .HasDatabaseName("IX_UserPermissions_IsActive");

    entity.HasIndex(up => up.ExpiresAt)
        .HasDatabaseName("IX_UserPermissions_ExpiresAt");

    // Soft delete query filter (POPIA compliance)
    entity.HasQueryFilter(up => !up.IsDeleted);

    // Relationship configuration already defined in User entity
});
```

4. Save file

**Files Modified:**
- `src/Modules/Management/Project420.Management.DAL/ManagementDbContext.cs`

**Command:**
```bash
# Generate migration for Management DB
dotnet ef migrations add AddUsersToManagement ^
  --project src/Modules/Management/Project420.Management.DAL ^
  --startup-project src/Modules/Retail/POS/Project420.Retail.POS.UI.Blazor ^
  --context ManagementDbContext

# Apply migration (creates Users and UserPermissions in Project420_Dev)
dotnet ef database update ^
  --project src/Modules/Management/Project420.Management.DAL ^
  --startup-project src/Modules/Retail/POS/Project420.Retail.POS.UI.Blazor ^
  --context ManagementDbContext
```

**Expected Result:**
- `Users` table created in Project420_Dev with 5 seed users
- `UserPermissions` table created in Project420_Dev
- Migration created: `YYYYMMDDHHMMSS_AddUsersToManagement.cs`

---

#### **Step 1.3: Verify Users Location Fix**

**Actions:**
1. Open SQL Server Management Studio (SSMS)
2. Connect to `JASON\SQLDEVED`
3. Expand `Project420_Shared` database
   - ✅ Should see: ErrorLogs, AuditLogs
   - ❌ Should NOT see: Users, UserPermissions
4. Expand `Project420_Dev` database
   - ✅ Should see: Users (with 5 rows), UserPermissions, UserProfiles, Products, Debtors, etc.

**Query to verify:**
```sql
-- Check Shared DB (should return 0)
USE Project420_Shared;
SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users';
-- Expected: 0

-- Check Business DB (should return 1)
USE Project420_Dev;
SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users';
-- Expected: 1

-- Verify seed data
SELECT Id, Username, Email, Role FROM Users;
-- Expected: 5 rows (superadmin, admin, manager, cashier, inventory)
```

**Build Verification:**
```bash
dotnet build
# Expected: Build succeeded, 0 Errors, 0 Warnings
```

---

### **PHASE 2: Add Station Connection Infrastructure**

#### **Step 2.1: Create StationConnection Entity**

**Location:** `src/Shared/Project420.Shared.Core/Entities/`

**Create new file:** `StationConnection.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace Project420.Shared.Core.Entities
{
    /// <summary>
    /// Represents which stations (PCs/terminals) can access which company databases.
    /// Multi-tenant routing metadata stored in Shared database.
    /// </summary>
    /// <remarks>
    /// Purpose:
    /// - Maps hostnames/hardware IDs to authorized company databases
    /// - Stores encrypted connection strings per company
    /// - Enables multi-tenant architecture with data isolation
    ///
    /// Usage Flow:
    /// 1. App starts on station "POS-STORE-01"
    /// 2. Query StationConnections: WHERE HostName = 'POS-STORE-01'
    /// 3. Returns list of companies this station can access
    /// 4. User selects company from dropdown
    /// 5. App uses EncryptedConnectionString to connect to that company's database
    /// 6. User authenticates against selected company's Users table
    ///
    /// Multi-Tenant Examples:
    /// - POS-STORE-01 → Can access "Main Store" and "Branch A"
    /// - POS-STORE-02 → Can only access "Branch B"
    /// - MOBILE-REP-01 → Can access all companies (regional manager)
    ///
    /// Security:
    /// - Connection strings encrypted at rest (DPAPI, AES, or Key Vault)
    /// - Decrypted only when establishing database connection
    /// - Different SQL credentials per company for additional isolation
    /// - IsAuthorized flag for quick enable/disable access
    ///
    /// POPIA Compliance:
    /// - Audit trail via AuditableEntity (who authorized this station)
    /// - Soft delete (preserve authorization history)
    /// - 7-year retention for compliance audits
    /// </remarks>
    public class StationConnection : AuditableEntity
    {
        // ============================================================
        // STATION IDENTIFICATION
        // ============================================================

        /// <summary>
        /// Hostname or unique identifier of the connecting station/PC
        /// </summary>
        /// <remarks>
        /// Identification Methods:
        /// - Machine Name: Environment.MachineName (e.g., "POS-STORE-01")
        /// - IP Address: 192.168.1.100 (not recommended - changes with DHCP)
        /// - MAC Address: 00:1B:44:11:3A:B7 (most secure)
        /// - Hardware ID: HW-ABC123-XYZ789 (license key based)
        ///
        /// Selection depends on Question 1 answer.
        ///
        /// Case Sensitivity:
        /// - Store uppercase for consistency (queries use .ToUpper())
        /// - Handle variations (e.g., "pos-store-01" vs "POS-STORE-01")
        ///
        /// Examples:
        /// - "POS-STORE-01" (physical terminal)
        /// - "MOBILE-REP-05" (tablet/laptop)
        /// - "BACKOFFICE-ADMIN" (admin workstation)
        /// - "00:1B:44:11:3A:B7" (MAC address)
        /// </remarks>
        [Required(ErrorMessage = "Station hostname is required")]
        [MaxLength(100, ErrorMessage = "Hostname cannot exceed 100 characters")]
        [Display(Name = "Station Hostname")]
        public string HostName { get; set; } = string.Empty;

        // ============================================================
        // COMPANY IDENTIFICATION
        // ============================================================

        /// <summary>
        /// Friendly company name displayed to user during company selection
        /// </summary>
        /// <remarks>
        /// User Interface:
        /// - Shown in company selection dropdown
        /// - Should be human-readable and recognizable
        ///
        /// Examples:
        /// - "Project420 - Main Store"
        /// - "Project420 - Branch A (Sandton)"
        /// - "Project420 - Franchise #5"
        /// - "ABC Cannabis - Johannesburg"
        ///
        /// Not unique: Multiple stations can access same company
        /// </remarks>
        [Required(ErrorMessage = "Company name is required")]
        [MaxLength(200, ErrorMessage = "Company name cannot exceed 200 characters")]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Physical database name for this company
        /// </summary>
        /// <remarks>
        /// Database Naming Convention:
        /// - Project420_Dev (first company / development)
        /// - Project420_Dev2 (second company)
        /// - Project420_CompanyA (named by company)
        /// - Project420_Franchise_001 (franchise model)
        ///
        /// Used to:
        /// - Identify which database to connect to
        /// - Build connection string dynamically
        /// - Track which company's data is being accessed
        ///
        /// Same Schema:
        /// - All company databases have identical schema
        /// - Migrations applied to all company databases
        /// - Only data differs per company
        /// </remarks>
        [Required(ErrorMessage = "Database name is required")]
        [MaxLength(100, ErrorMessage = "Database name cannot exceed 100 characters")]
        [Display(Name = "Database Name")]
        public string DatabaseName { get; set; } = string.Empty;

        // ============================================================
        // CONNECTION CONFIGURATION
        // ============================================================

        /// <summary>
        /// Encrypted connection string for this company's database
        /// </summary>
        /// <remarks>
        /// Security (CRITICAL):
        /// - NEVER store plain text connection strings
        /// - Encrypt before storing in database
        /// - Decrypt only when establishing connection
        /// - Different SQL credentials per company recommended
        ///
        /// Encryption Methods (based on Question 2 answer):
        /// - DPAPI (Windows): ProtectedData.Protect()
        /// - AES-256: Custom encryption with secure key
        /// - Azure Key Vault: Reference to secret
        /// - Environment Variables: Less secure, not recommended
        ///
        /// Example Plain Text (NEVER STORE THIS):
        /// "Server=JASON\\SQLDEVED;Database=Project420_Dev;User Id=company_a_user;Password=SecurePass123;TrustServerCertificate=True;"
        ///
        /// Example Encrypted (stored in this field):
        /// "AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAA..." (DPAPI)
        ///
        /// Connection String Components:
        /// - Server: Same or different SQL Server per company
        /// - Database: DatabaseName field value
        /// - Credentials: Company-specific SQL login (not sa!)
        /// - Additional: Connection pooling, timeout, encryption settings
        ///
        /// Decryption happens in:
        /// - AuthenticationService.LoginAsync()
        /// - CompanySelectionService.GetConnectionString()
        /// </remarks>
        [Required(ErrorMessage = "Connection string is required")]
        [MaxLength(1000, ErrorMessage = "Encrypted connection string too long")]
        [Display(Name = "Encrypted Connection String")]
        public string EncryptedConnectionString { get; set; } = string.Empty;

        // ============================================================
        // AUTHORIZATION & ACCESS CONTROL
        // ============================================================

        /// <summary>
        /// Whether this station is currently authorized to access this company
        /// </summary>
        /// <remarks>
        /// Use Cases:
        /// - Enable/disable station access without deleting record
        /// - Temporary suspension (e.g., during investigation)
        /// - Emergency lockout (security incident)
        /// - Deactivate old terminals
        ///
        /// Different from IsDeleted:
        /// - IsActive: Temporary authorization flag (easily reversed)
        /// - IsDeleted: Soft delete (permanent removal)
        ///
        /// Security:
        /// - Set to false = station immediately loses access
        /// - User sees "Access denied" at company selection
        /// - Logged in users kicked out on next auth check
        ///
        /// Audit:
        /// - Changes tracked via ModifiedAt/ModifiedBy
        /// - Alert on authorization changes
        /// </remarks>
        [Required]
        [Display(Name = "Is Authorized")]
        public bool IsAuthorized { get; set; } = true;

        /// <summary>
        /// Optional: Date when station access to this company expires
        /// </summary>
        /// <remarks>
        /// Temporary Access Scenarios:
        /// - Mobile rep visiting location for 1 week
        /// - Temporary staff terminal (seasonal workers)
        /// - Trial period for new franchise
        /// - Contractor access (external auditors)
        ///
        /// Behavior:
        /// - If null: Access never expires (permanent)
        /// - If past date: Access expired (treat as unauthorized)
        /// - If future date: Access valid until that date
        ///
        /// System Monitoring:
        /// - Background job checks for expiring access (alert at 7/3/1 days before)
        /// - Automatic deactivation on expiry (set IsAuthorized = false)
        /// - Email notifications to admins
        ///
        /// Examples:
        /// - 2025-12-31 23:59:59 (expires end of year)
        /// - 2025-12-15 (temporary holiday staff)
        /// </remarks>
        [Display(Name = "Access Expires At")]
        public DateTime? AccessExpiresAt { get; set; }

        // ============================================================
        // METADATA & NOTES
        // ============================================================

        /// <summary>
        /// Optional: Administrative notes about this station connection
        /// </summary>
        /// <remarks>
        /// Documentation:
        /// - Why does this station have access to this company?
        /// - Who approved the access?
        /// - Temporary or permanent?
        /// - Special circumstances?
        ///
        /// Examples:
        /// - "Main POS terminal for Store 01"
        /// - "Regional manager mobile device - access to all franchises"
        /// - "Temporary access for year-end stocktake (approved by CEO)"
        /// - "Backup terminal - only used when POS-STORE-01 is down"
        ///
        /// Audit Value:
        /// - Helps during security reviews
        /// - Explains unusual access patterns
        /// - Documents business justification
        /// </remarks>
        [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        [Display(Name = "Authorization Notes")]
        public string? Notes { get; set; }

        // ============================================================
        // COMPUTED PROPERTIES
        // ============================================================

        /// <summary>
        /// Computed: Is this station connection currently valid?
        /// </summary>
        /// <remarks>
        /// Combines multiple authorization checks:
        /// - IsAuthorized must be true
        /// - IsDeleted must be false (inherited from AuditableEntity)
        /// - AccessExpiresAt must be null or future date
        ///
        /// Usage:
        /// var validConnections = await _context.StationConnections
        ///     .Where(sc => sc.HostName == hostname && sc.IsValidConnection)
        ///     .ToListAsync();
        /// </remarks>
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        [Display(Name = "Is Valid Connection")]
        public bool IsValidConnection =>
            IsAuthorized &&
            !IsDeleted &&
            (AccessExpiresAt == null || AccessExpiresAt > DateTime.UtcNow);
    }
}
```

**Files Created:**
- `src/Shared/Project420.Shared.Core/Entities/StationConnection.cs`

---

#### **Step 2.2: Add StationConnection to SharedDbContext**

**Location:** `src/Shared/Project420.Shared.Database/SharedDbContext.cs`

**Actions:**
1. Open `SharedDbContext.cs`
2. Add after AuditLogs DbSet (around line 77):

```csharp
/// <summary>
/// Station connections table - Multi-tenant routing metadata
/// </summary>
/// <remarks>
/// Maps stations (PCs/terminals) to authorized company databases.
/// Enables multi-tenant architecture with company selection.
///
/// Usage:
/// - Station boots → query by hostname → list available companies
/// - User selects company → decrypt connection string → connect
/// - Authenticate against selected company's Users table
/// </remarks>
public DbSet<StationConnection> StationConnections { get; set; } = null!;
```

3. Add configuration in `OnModelCreating` (before closing brace):

```csharp
// ===========================
// STATIONCONNECTION CONFIGURATION
// ===========================
modelBuilder.Entity<StationConnection>(entity =>
{
    // Indexes for performance
    entity.HasIndex(sc => sc.HostName)
        .HasDatabaseName("IX_StationConnections_HostName");

    entity.HasIndex(sc => sc.DatabaseName)
        .HasDatabaseName("IX_StationConnections_DatabaseName");

    entity.HasIndex(sc => sc.IsAuthorized)
        .HasDatabaseName("IX_StationConnections_IsAuthorized");

    entity.HasIndex(sc => sc.AccessExpiresAt)
        .HasDatabaseName("IX_StationConnections_AccessExpiresAt");

    // Composite index for common query (hostname + authorized)
    entity.HasIndex(sc => new { sc.HostName, sc.IsAuthorized })
        .HasDatabaseName("IX_StationConnections_HostName_IsAuthorized");

    // Soft delete query filter (POPIA compliance)
    entity.HasQueryFilter(sc => !sc.IsDeleted);

    // ===========================
    // SEED DATA - Example Connections
    // ===========================
    // TODO: Replace with actual station data after answering Question 1 & 2
    // For now, using placeholder with plain text connection string (NOT PRODUCTION SAFE!)

    var seedDate = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc);

    entity.HasData(
        // Example 1: POS-STORE-01 can access Main Store (Project420_Dev)
        new StationConnection
        {
            Id = 1,
            HostName = "POS-STORE-01",
            CompanyName = "Project420 - Main Store",
            DatabaseName = "Project420_Dev",
            // TODO: Encrypt this connection string based on Question 2 answer
            EncryptedConnectionString = "Server=JASON\\SQLDEVED;Database=Project420_Dev;User Id=sa;Password=ZAQxsw123;TrustServerCertificate=True;",
            IsAuthorized = true,
            AccessExpiresAt = null, // Permanent access
            Notes = "Main POS terminal for Store 01",
            CreatedAt = seedDate,
            CreatedBy = "SYSTEM",
            IsDeleted = false
        },

        // Example 2: Same station can access Branch Store (multi-company access)
        new StationConnection
        {
            Id = 2,
            HostName = "POS-STORE-01",
            CompanyName = "Project420 - Branch Store",
            DatabaseName = "Project420_Dev2",
            // TODO: Encrypt this connection string
            EncryptedConnectionString = "Server=JASON\\SQLDEVED;Database=Project420_Dev2;User Id=sa;Password=ZAQxsw123;TrustServerCertificate=True;",
            IsAuthorized = true,
            AccessExpiresAt = null,
            Notes = "Backup access to Branch Store for emergencies",
            CreatedAt = seedDate,
            CreatedBy = "SYSTEM",
            IsDeleted = false
        },

        // Example 3: Mobile device with expiring access
        new StationConnection
        {
            Id = 3,
            HostName = "MOBILE-REP-01",
            CompanyName = "Project420 - Main Store",
            DatabaseName = "Project420_Dev",
            EncryptedConnectionString = "Server=JASON\\SQLDEVED;Database=Project420_Dev;User Id=sa;Password=ZAQxsw123;TrustServerCertificate=True;",
            IsAuthorized = true,
            AccessExpiresAt = new DateTime(2025, 12, 31, 23, 59, 59, DateTimeKind.Utc),
            Notes = "Regional manager tablet - temporary access for Q4 2025",
            CreatedAt = seedDate,
            CreatedBy = "SYSTEM",
            IsDeleted = false
        }
    );
});
```

4. Save file

**Files Modified:**
- `src/Shared/Project420.Shared.Database/SharedDbContext.cs`

**Command:**
```bash
# Generate migration
dotnet ef migrations add AddStationConnections ^
  --project src/Shared/Project420.Shared.Database ^
  --startup-project src/Modules/Retail/POS/Project420.Retail.POS.UI.Blazor ^
  --context SharedDbContext

# Apply migration
dotnet ef database update ^
  --project src/Shared/Project420.Shared.Database ^
  --startup-project src/Modules/Retail/POS/Project420.Retail.POS.UI.Blazor ^
  --context SharedDbContext
```

**Expected Result:**
- `StationConnections` table created in Project420_Shared
- 3 seed records created (POS-STORE-01 with 2 companies, MOBILE-REP-01 with 1 company)

---

### **PHASE 3: Add Station Details (Business-Specific Config)**

#### **Step 3.1: Create Station Entity**

**Location:** `src/Modules/Management/Project420.Management.Models/Entities/`

**Create new file:** `Station.cs`

**Content:** (To be defined based on Question 5 answers)

```csharp
using System.ComponentModel.DataAnnotations;
using Project420.Shared.Core.Entities;

namespace Project420.Management.Models.Entities
{
    /// <summary>
    /// Represents a physical station/terminal in this specific company
    /// </summary>
    /// <remarks>
    /// Company-Specific Configuration:
    /// - This is NOT the global connection metadata (that's StationConnection in Shared DB)
    /// - This is the business-specific configuration for a terminal in THIS company
    ///
    /// Example:
    /// - StationConnection: "POS-STORE-01 can access Company A and Company B"
    /// - Station (Company A): "POS-STORE-01 is configured as 'Main Counter' with Epson printer"
    /// - Station (Company B): "POS-STORE-01 is configured as 'Backup Terminal' with different printer"
    ///
    /// Managed by: Management module (back-office)
    /// Used by: POS module (reads configuration at startup)
    /// </remarks>
    public class Station : AuditableEntity
    {
        /// <summary>
        /// Station name (user-friendly identifier)
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Display(Name = "Station Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Station type / category
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Display(Name = "Station Type")]
        public string StationType { get; set; } = "POS"; // POS, Mobile, Kiosk, BackOffice

        // TODO: Add more properties based on Question 5 answers
        // Examples:
        // - Location (string)
        // - Department (string)
        // - DefaultPricelist (int? - FK to Pricelist)
        // - AllowOfflineMode (bool)
        // - ReceiptTemplate (string - path or name)
        // - etc.

        /// <summary>
        /// Navigation property to peripherals configured for this station
        /// </summary>
        public virtual ICollection<StationPeripheral>? Peripherals { get; set; }
    }
}
```

**Files Created:**
- `src/Modules/Management/Project420.Management.Models/Entities/Station.cs`

---

#### **Step 3.2: Create StationPeripheral Entity**

**Location:** `src/Modules/Management/Project420.Management.Models/Entities/`

**Create new file:** `StationPeripheral.cs`

**Content:** (To be defined based on Question 5 answers)

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.Shared.Core.Entities;

namespace Project420.Management.Models.Entities
{
    /// <summary>
    /// Represents a peripheral device configured for a station
    /// </summary>
    public class StationPeripheral : AuditableEntity
    {
        /// <summary>
        /// Foreign key to Station
        /// </summary>
        [Required]
        [Display(Name = "Station ID")]
        public int StationId { get; set; }

        /// <summary>
        /// Type of peripheral
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Display(Name = "Peripheral Type")]
        public string PeripheralType { get; set; } = string.Empty; // Printer, Scanner, CashDrawer, CardReader

        // TODO: Add more properties based on Question 5 answers
        // Examples:
        // - Model (string)
        // - IPAddress (string)
        // - Port (string)
        // - ConnectionType (string - USB, Network, Serial)
        // - Settings (string - JSON configuration)
        // - etc.

        /// <summary>
        /// Navigation property to Station
        /// </summary>
        [ForeignKey(nameof(StationId))]
        public virtual Station? Station { get; set; }
    }
}
```

**Files Created:**
- `src/Modules/Management/Project420.Management.Models/Entities/StationPeripheral.cs`

---

#### **Step 3.3: Add Stations to ManagementDbContext**

**Location:** `src/Modules/Management/Project420.Management.DAL/ManagementDbContext.cs`

**Actions:**
1. Add DbSets after UserProfiles (around line 117):

```csharp
/// <summary>
/// Stations table - Terminal configuration (company-specific)
/// </summary>
public DbSet<Station> Stations { get; set; } = null!;

/// <summary>
/// Station peripherals table - Peripheral device configuration
/// </summary>
public DbSet<StationPeripheral> StationPeripherals { get; set; } = null!;
```

2. Add configuration in `OnModelCreating`:

```csharp
// ===========================
// STATION CONFIGURATION
// ===========================
modelBuilder.Entity<Station>(entity =>
{
    entity.HasIndex(s => s.Name)
        .HasDatabaseName("IX_Stations_Name");

    entity.HasIndex(s => s.StationType)
        .HasDatabaseName("IX_Stations_StationType");

    entity.HasQueryFilter(s => !s.IsDeleted);

    // Relationship to peripherals
    entity.HasMany(s => s.Peripherals)
        .WithOne(sp => sp.Station)
        .HasForeignKey(sp => sp.StationId)
        .OnDelete(DeleteBehavior.Cascade);
});

// ===========================
// STATIONPERIPHERAL CONFIGURATION
// ===========================
modelBuilder.Entity<StationPeripheral>(entity =>
{
    entity.HasIndex(sp => sp.StationId)
        .HasDatabaseName("IX_StationPeripherals_StationId");

    entity.HasIndex(sp => sp.PeripheralType)
        .HasDatabaseName("IX_StationPeripherals_PeripheralType");

    entity.HasQueryFilter(sp => !sp.IsDeleted);
});
```

3. Save file

**Files Modified:**
- `src/Modules/Management/Project420.Management.DAL/ManagementDbContext.cs`

**Command:**
```bash
# Generate migration
dotnet ef migrations add AddStations ^
  --project src/Modules/Management/Project420.Management.DAL ^
  --startup-project src/Modules/Retail/POS/Project420.Retail.POS.UI.Blazor ^
  --context ManagementDbContext

# Apply migration
dotnet ef database update ^
  --project src/Modules/Management/Project420.Management.DAL ^
  --startup-project src/Modules/Retail/POS/Project420.Retail.POS.UI.Blazor ^
  --context ManagementDbContext
```

**Expected Result:**
- `Stations` table created in Project420_Dev
- `StationPeripherals` table created in Project420_Dev

---

### **PHASE 4: Implement Authentication Service (Future)**

**NOTE:** This phase depends on answers to Questions 1-4. Implementation details TBD.

**Placeholder Files to Create:**

1. `src/Shared/Project420.Shared.Infrastructure/Services/IAuthenticationService.cs`
2. `src/Shared/Project420.Shared.Infrastructure/Services/AuthenticationService.cs`
3. `src/Shared/Project420.Shared.Infrastructure/Services/ICompanySelectionService.cs`
4. `src/Shared/Project420.Shared.Infrastructure/Services/CompanySelectionService.cs`

**Functionality:**
- Get available companies for a station
- Decrypt connection strings
- Authenticate user against selected company
- Manage authentication state
- Handle multi-company switching

---

## 💻 CODE SNIPPETS

### **Helper: Get Machine Hostname**

```csharp
// Option A: Machine Name
public static string GetStationIdentifier()
{
    return Environment.MachineName.ToUpperInvariant();
}

// Option B: MAC Address
public static string GetStationIdentifier()
{
    var macAddr =
        (
            from nic in NetworkInterface.GetAllNetworkInterfaces()
            where nic.OperationalStatus == OperationalStatus.Up
            select nic.GetPhysicalAddress().ToString()
        ).FirstOrDefault();

    return macAddr ?? "UNKNOWN";
}
```

### **Helper: Encrypt/Decrypt Connection String**

```csharp
// Option A: DPAPI (Windows)
using System.Security.Cryptography;

public static string EncryptConnectionString(string plainText)
{
    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
    byte[] entropy = Encoding.UTF8.GetBytes("Project420Salt"); // Additional protection
    byte[] encryptedBytes = ProtectedData.Protect(plainBytes, entropy, DataProtectionScope.LocalMachine);
    return Convert.ToBase64String(encryptedBytes);
}

public static string DecryptConnectionString(string encryptedText)
{
    byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
    byte[] entropy = Encoding.UTF8.GetBytes("Project420Salt");
    byte[] plainBytes = ProtectedData.Unprotect(encryptedBytes, entropy, DataProtectionScope.LocalMachine);
    return Encoding.UTF8.GetString(plainBytes);
}
```

### **Service: Get Available Companies**

```csharp
public async Task<List<CompanyDto>> GetAvailableCompaniesAsync(string hostName)
{
    // Query Shared DB for authorized connections
    var connections = await _sharedContext.StationConnections
        .Where(sc =>
            sc.HostName.ToUpper() == hostName.ToUpper() &&
            sc.IsAuthorized &&
            !sc.IsDeleted &&
            (sc.AccessExpiresAt == null || sc.AccessExpiresAt > DateTime.UtcNow))
        .Select(sc => new CompanyDto
        {
            CompanyName = sc.CompanyName,
            DatabaseName = sc.DatabaseName,
            EncryptedConnectionString = sc.EncryptedConnectionString
        })
        .ToListAsync();

    return connections;
}
```

---

## ✅ TESTING CHECKLIST

### **Phase 1 Testing: Users Location Fix**

- [ ] Shared DB has NO Users table
- [ ] Shared DB has NO UserPermissions table
- [ ] Business DB HAS Users table with 5 seed users
- [ ] Business DB HAS UserPermissions table (empty)
- [ ] Query returns 5 users: `SELECT * FROM Project420_Dev.dbo.Users`
- [ ] All users have hashed passwords (not plain text)
- [ ] Solution builds: 0 errors, 0 warnings
- [ ] UserProfile.UserId can reference Users.Id (same database)

### **Phase 2 Testing: Station Connections**

- [ ] Shared DB has StationConnections table
- [ ] StationConnections has 3 seed records
- [ ] Query by hostname returns multiple companies: `SELECT * FROM StationConnections WHERE HostName = 'POS-STORE-01'`
- [ ] IsValidConnection computed property works
- [ ] Expired access is filtered out
- [ ] Solution builds successfully

### **Phase 3 Testing: Station Details**

- [ ] Business DB has Stations table
- [ ] Business DB has StationPeripherals table
- [ ] Foreign key relationship works (Station → StationPeripherals)
- [ ] Solution builds successfully

### **Phase 4 Testing: Authentication (Future)**

- [ ] Company selection UI shows available companies for station
- [ ] User can select company from dropdown
- [ ] Connection string decrypts successfully
- [ ] Authentication validates against correct company database
- [ ] User context includes company information
- [ ] Audit trails record which company user is accessing

---

## 📝 NOTES

### **Current Seed Password**
All seed users have password: **`Project420!Pass`**
BCrypt Hash: `$2a$11$EK5kC8qGqJH5rZvK.YjJGuH7Z6uYF6NjN9XoYdQdH3xRZLqKxhE/G`

### **Connection String (Development)**
```
Server=JASON\SQLDEVED;Database={DatabaseName};User Id=sa;Password=ZAQxsw123;TrustServerCertificate=True;
```

**WARNING:** Using `sa` account is NOT production safe. Create company-specific SQL logins.

### **Migration Commands Reference**

```bash
# Generate migration
dotnet ef migrations add {MigrationName} ^
  --project {ProjectPath} ^
  --startup-project src/Modules/Retail/POS/Project420.Retail.POS.UI.Blazor ^
  --context {ContextName}

# Apply migration
dotnet ef database update ^
  --project {ProjectPath} ^
  --startup-project src/Modules/Retail/POS/Project420.Retail.POS.UI.Blazor ^
  --context {ContextName}

# Remove last migration (if not applied)
dotnet ef migrations remove ^
  --project {ProjectPath} ^
  --context {ContextName}

# Remove last migration (even if applied) - DANGEROUS!
dotnet ef migrations remove ^
  --project {ProjectPath} ^
  --context {ContextName} ^
  --force
```

---

## 🎯 NEXT STEPS

### **Immediate (Answer Questions)**

1. Answer Question 1: Station identification method
2. Answer Question 2: Connection string encryption method
3. Answer Question 3: Multi-company user scenario
4. Answer Question 4: Station management UI location
5. Answer Question 5: Station/StationPeripheral properties

### **After Questions Answered**

1. Update StationConnection entity with chosen identification method
2. Implement encryption/decryption based on chosen method
3. Complete Station and StationPeripheral entities
4. Run Phase 1 implementation steps
5. Run Phase 2 implementation steps
6. Run Phase 3 implementation steps
7. Test each phase thoroughly
8. Plan Phase 4 authentication service implementation

---

## 📞 CONTACT INFORMATION

**File Location**: `docs/MULTI-TENANT-ARCHITECTURE-IMPLEMENTATION.md`

**When Ready to Continue:**
1. Answer all questions in this document
2. Start new conversation
3. Reference this file: "Let's implement Phase 1 from MULTI-TENANT-ARCHITECTURE-IMPLEMENTATION.md"
4. Claude will pick up where we left off

---

**Document End** - Ready for step-by-step implementation

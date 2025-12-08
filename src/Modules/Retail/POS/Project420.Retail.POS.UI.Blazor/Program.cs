using Microsoft.EntityFrameworkCore;
using Project420.Retail.POS.DAL;
using Project420.Retail.POS.DAL.Repositories;
using Project420.Management.DAL;
using Project420.Shared.Database;
using Project420.Shared.Database.Repositories;
using Project420.Retail.POS.UI.Blazor.Components;
using Project420.Retail.POS.BLL.Services;
using Project420.Shared.Infrastructure.Services;
using Project420.Shared.Infrastructure.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// DATABASE CONTEXT REGISTRATION (Dependency Injection)
// ========================================

// Get connection strings from appsettings.json
var businessConnection = builder.Configuration.GetConnectionString("BusinessConnection");
var sharedConnection = builder.Configuration.GetConnectionString("SharedConnection");

// Register PosDbContext (Operational POS data)
// Uses BusinessConnection → Project420_Dev database
builder.Services.AddDbContext<PosDbContext>(options =>
    options.UseSqlServer(businessConnection));

// Register ManagementDbContext (Master data)
// Uses BusinessConnection → Project420_Dev database (different schema)
builder.Services.AddDbContext<ManagementDbContext>(options =>
    options.UseSqlServer(businessConnection));

// Register SharedDbContext (Gatekeeping: Errors, Audits, Users, Stations)
// Uses SharedConnection → Project420_Shared database (separate DB)
builder.Services.AddDbContext<SharedDbContext>(options =>
    options.UseSqlServer(sharedConnection));

// Register DatabaseSeeder
builder.Services.AddScoped<DatabaseSeeder>();

// ========================================
// DATA ACCESS LAYER (Repositories)
// ========================================

// Transaction Repository - POS transaction database operations
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// Payment Repository - Payment reconciliation and cash drawer operations
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// Transaction Number Repository - Persistent transaction number sequences
builder.Services.AddScoped<ITransactionNumberRepository, TransactionNumberRepository>();

// ========================================
// BUSINESS LOGIC SERVICES (BLL)
// ========================================

// Register POSCalculationService for VAT calculations
builder.Services.AddScoped<IPOSCalculationService, POSCalculationService>();

// Transaction Service - Complete POS checkout workflow
builder.Services.AddScoped<ITransactionService, TransactionService>();

// Refund Service - Process refunds with validation and manager approval
builder.Services.AddScoped<IRefundService, RefundService>();

// Payment Reconciliation Service - Cash drawer management and reconciliation
builder.Services.AddScoped<IPaymentReconciliationService, PaymentReconciliationService>();

// Transaction Search Service - Advanced search and reporting
builder.Services.AddScoped<ITransactionSearchService, TransactionSearchService>();

// ========================================
// SHARED SERVICES (Universal across all modules)
// ========================================

// VAT Calculation Service - Universal transaction logic (POS, GRV, RTS, Invoices, Credits)
builder.Services.AddScoped<IVATCalculationService, VATCalculationService>();

// Transaction Number Generator - Auto-generate unique transaction numbers
builder.Services.AddScoped<ITransactionNumberGeneratorService, TransactionNumberGeneratorService>();

// Audit Log Service - POPIA compliance, Cannabis Act tracking, SARS reporting
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

// ========================================
// BLAZOR SERVICES
// ========================================

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Seed database on startup (Development only)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAllAsync();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

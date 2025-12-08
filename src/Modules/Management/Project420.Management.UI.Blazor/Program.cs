using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Project420.Management.UI.Blazor.Components;
using Project420.Management.DAL;
using Project420.Management.BLL.Sales.SalesCommon.Services;
using Project420.Management.BLL.Sales.SalesCommon.DTOs;
using Project420.Management.BLL.Sales.SalesCommon.Validators;
using Project420.Management.BLL.StockManagement.Services;
using Project420.Management.BLL.StockManagement.DTOs;
using Project420.Management.BLL.StockManagement.Validators;
using Project420.Shared.Core.Services.DataProtection;
using Project420.Shared.Core.Compliance.Services;
using Project420.Management.DAL.Repositories.Sales.SalesCommon;
using Project420.Management.DAL.Repositories.StockManagement;
using Project420.Management.DAL.Repositories.ProductManagement;
using Project420.Management.DAL.Repositories.Sales.Retail;
using Project420.Management.BLL.Sales.Retail.Services;
using Project420.Management.BLL.Sales.Retail.DTOs;
using Project420.Management.BLL.Sales.Retail.Validators;
using Project420.Cultivation.DAL;
using Project420.Production.DAL;
using Project420.Inventory.DAL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ============================================================
// DATABASE CONFIGURATION
// ============================================================
builder.Services.AddDbContext<ManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BusinessConnection")));

// Phase 5 MVP Modules - Seed-to-Sale Traceability
builder.Services.AddDbContext<CultivationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BusinessConnection")));

builder.Services.AddDbContext<ProductionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BusinessConnection")));

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BusinessConnection")));

// ============================================================
// REPOSITORY REGISTRATION
// ============================================================
// Sales/SalesCommon Repositories
builder.Services.AddScoped<IDebtorRepository, DebtorRepository>();
builder.Services.AddScoped<IDebtorCategoryRepository, DebtorCategoryRepository>();

// StockManagement Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();

// Sales/Retail Repositories
builder.Services.AddScoped<IRetailPricelistRepository, RetailPricelistRepository>();
builder.Services.AddScoped<IRetailPricelistItemRepository, RetailPricelistItemRepository>();

// ============================================================
// SERVICE REGISTRATION
// ============================================================
// Sales/SalesCommon Services
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<IDebtorCategoryService, DebtorCategoryService>();

// StockManagement Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();

// Sales/Retail Services
builder.Services.AddScoped<IPricelistService, PricelistService>();

// ============================================================
// SHARED SERVICES (Across all modules)
// ============================================================
builder.Services.AddSingleton<IDataProtectionService, PopiaDataMaskingService>();
builder.Services.AddScoped<ICannabisComplianceService, CannabisComplianceService>();

// ============================================================
// VALIDATOR REGISTRATION (FluentValidation)
// ============================================================
// Sales/SalesCommon Validators
builder.Services.AddScoped<IValidator<CustomerRegistrationDto>, CustomerRegistrationValidator>();
builder.Services.AddScoped<IValidator<CreateDebtorCategoryDto>, CreateDebtorCategoryValidator>();
builder.Services.AddScoped<IValidator<UpdateDebtorCategoryDto>, UpdateDebtorCategoryValidator>();

// StockManagement Validators
builder.Services.AddScoped<IValidator<CreateProductDto>, CreateProductValidator>();
builder.Services.AddScoped<IValidator<UpdateProductDto>, UpdateProductValidator>();
builder.Services.AddScoped<IValidator<CreateProductCategoryDto>, CreateProductCategoryValidator>();
builder.Services.AddScoped<IValidator<UpdateProductCategoryDto>, UpdateProductCategoryValidator>();

// Sales/Retail Validators
builder.Services.AddScoped<IValidator<CreatePricelistDto>, CreatePricelistValidator>();
builder.Services.AddScoped<IValidator<UpdatePricelistDto>, UpdatePricelistValidator>();
builder.Services.AddScoped<IValidator<CreatePricelistItemDto>, CreatePricelistItemValidator>();
builder.Services.AddScoped<IValidator<UpdatePricelistItemDto>, UpdatePricelistItemValidator>();

var app = builder.Build();

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

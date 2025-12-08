using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Project420.API.WebApi.Configuration;
using Project420.API.WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ===== Configuration =====

// Load JWT settings from appsettings.json
var jwtSettings = new JwtSettings();
builder.Configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);
builder.Services.AddSingleton(jwtSettings);

// ===== Services =====

builder.Services.AddControllers();

// Database Contexts
builder.Services.AddDbContext<Project420.Cultivation.DAL.CultivationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<Project420.Production.DAL.ProductionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<Project420.Inventory.DAL.InventoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cultivation Module - Repositories & Services
builder.Services.AddScoped<Project420.Cultivation.DAL.Repositories.IPlantRepository, Project420.Cultivation.DAL.Repositories.PlantRepository>();
builder.Services.AddScoped<Project420.Cultivation.DAL.Repositories.IGrowCycleRepository, Project420.Cultivation.DAL.Repositories.GrowCycleRepository>();
builder.Services.AddScoped<Project420.Cultivation.DAL.Repositories.IGrowRoomRepository, Project420.Cultivation.DAL.Repositories.GrowRoomRepository>();
builder.Services.AddScoped<Project420.Cultivation.DAL.Repositories.IHarvestBatchRepository, Project420.Cultivation.DAL.Repositories.HarvestBatchRepository>();
builder.Services.AddScoped<Project420.Cultivation.BLL.Services.IPlantService, Project420.Cultivation.BLL.Services.PlantService>();
builder.Services.AddScoped<Project420.Cultivation.BLL.Services.IGrowCycleService, Project420.Cultivation.BLL.Services.GrowCycleService>();
builder.Services.AddScoped<Project420.Cultivation.BLL.Services.IGrowRoomService, Project420.Cultivation.BLL.Services.GrowRoomService>();
builder.Services.AddScoped<Project420.Cultivation.BLL.Services.IHarvestBatchService, Project420.Cultivation.BLL.Services.HarvestBatchService>();

// Production Module - Repositories & Services
builder.Services.AddScoped<Project420.Production.DAL.Repositories.IProductionBatchRepository, Project420.Production.DAL.Repositories.ProductionBatchRepository>();
builder.Services.AddScoped<Project420.Production.DAL.Repositories.IProcessingStepRepository, Project420.Production.DAL.Repositories.ProcessingStepRepository>();
builder.Services.AddScoped<Project420.Production.DAL.Repositories.IQualityControlRepository, Project420.Production.DAL.Repositories.QualityControlRepository>();
builder.Services.AddScoped<Project420.Production.DAL.Repositories.ILabTestRepository, Project420.Production.DAL.Repositories.LabTestRepository>();
builder.Services.AddScoped<Project420.Production.BLL.Services.IProductionBatchService, Project420.Production.BLL.Services.ProductionBatchService>();
builder.Services.AddScoped<Project420.Production.BLL.Services.IProcessingStepService, Project420.Production.BLL.Services.ProcessingStepService>();
builder.Services.AddScoped<Project420.Production.BLL.Services.IQualityControlService, Project420.Production.BLL.Services.QualityControlService>();
builder.Services.AddScoped<Project420.Production.BLL.Services.ILabTestService, Project420.Production.BLL.Services.LabTestService>();

// Inventory Module - Repositories & Services
builder.Services.AddScoped<Project420.Inventory.DAL.Repositories.IStockMovementRepository, Project420.Inventory.DAL.Repositories.StockMovementRepository>();
builder.Services.AddScoped<Project420.Inventory.DAL.Repositories.IStockTransferRepository, Project420.Inventory.DAL.Repositories.StockTransferRepository>();
builder.Services.AddScoped<Project420.Inventory.DAL.Repositories.IStockAdjustmentRepository, Project420.Inventory.DAL.Repositories.StockAdjustmentRepository>();
builder.Services.AddScoped<Project420.Inventory.DAL.Repositories.IStockCountRepository, Project420.Inventory.DAL.Repositories.StockCountRepository>();
builder.Services.AddScoped<Project420.Inventory.BLL.Services.IStockMovementService, Project420.Inventory.BLL.Services.StockMovementService>();
builder.Services.AddScoped<Project420.Inventory.BLL.Services.IStockTransferService, Project420.Inventory.BLL.Services.StockTransferService>();
builder.Services.AddScoped<Project420.Inventory.BLL.Services.IStockAdjustmentService, Project420.Inventory.BLL.Services.StockAdjustmentService>();
builder.Services.AddScoped<Project420.Inventory.BLL.Services.IStockCountService, Project420.Inventory.BLL.Services.StockCountService>();

// FluentValidation - Cultivation Module
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Cultivation.BLL.DTOs.CreatePlantDto>, Project420.Cultivation.BLL.Validators.CreatePlantValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Cultivation.BLL.DTOs.UpdatePlantDto>, Project420.Cultivation.BLL.Validators.UpdatePlantValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Cultivation.BLL.DTOs.CreateGrowCycleDto>, Project420.Cultivation.BLL.Validators.CreateGrowCycleValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Cultivation.BLL.DTOs.UpdateGrowCycleDto>, Project420.Cultivation.BLL.Validators.UpdateGrowCycleValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Cultivation.BLL.DTOs.CreateGrowRoomDto>, Project420.Cultivation.BLL.Validators.CreateGrowRoomValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Cultivation.BLL.DTOs.UpdateGrowRoomDto>, Project420.Cultivation.BLL.Validators.UpdateGrowRoomValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Cultivation.BLL.DTOs.CreateHarvestBatchDto>, Project420.Cultivation.BLL.Validators.CreateHarvestBatchValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Cultivation.BLL.DTOs.UpdateHarvestBatchDto>, Project420.Cultivation.BLL.Validators.UpdateHarvestBatchValidator>();

// FluentValidation - Production Module
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Production.BLL.DTOs.CreateProductionBatchDto>, Project420.Production.BLL.Validators.CreateProductionBatchValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Production.BLL.DTOs.UpdateProductionBatchDto>, Project420.Production.BLL.Validators.UpdateProductionBatchValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Production.BLL.DTOs.CreateProcessingStepDto>, Project420.Production.BLL.Validators.CreateProcessingStepValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Production.BLL.DTOs.UpdateProcessingStepDto>, Project420.Production.BLL.Validators.UpdateProcessingStepValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Production.BLL.DTOs.CreateQualityControlDto>, Project420.Production.BLL.Validators.CreateQualityControlValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Production.BLL.DTOs.UpdateQualityControlDto>, Project420.Production.BLL.Validators.UpdateQualityControlValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Production.BLL.DTOs.CreateLabTestDto>, Project420.Production.BLL.Validators.CreateLabTestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Production.BLL.DTOs.UpdateLabTestDto>, Project420.Production.BLL.Validators.UpdateLabTestValidator>();

// FluentValidation - Inventory Module
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Inventory.BLL.DTOs.CreateStockMovementDto>, Project420.Inventory.BLL.Validators.CreateStockMovementValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Inventory.BLL.DTOs.UpdateStockMovementDto>, Project420.Inventory.BLL.Validators.UpdateStockMovementValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Inventory.BLL.DTOs.CreateStockTransferDto>, Project420.Inventory.BLL.Validators.CreateStockTransferValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Inventory.BLL.DTOs.UpdateStockTransferDto>, Project420.Inventory.BLL.Validators.UpdateStockTransferValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Inventory.BLL.DTOs.CreateStockAdjustmentDto>, Project420.Inventory.BLL.Validators.CreateStockAdjustmentValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Inventory.BLL.DTOs.UpdateStockAdjustmentDto>, Project420.Inventory.BLL.Validators.UpdateStockAdjustmentValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Inventory.BLL.DTOs.CreateStockCountDto>, Project420.Inventory.BLL.Validators.CreateStockCountValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Inventory.BLL.DTOs.UpdateStockCountDto>, Project420.Inventory.BLL.Validators.UpdateStockCountValidator>();

// FluentValidation - Management Module - Sales (Retail)
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Management.BLL.Sales.Retail.DTOs.CreatePricelistDto>, Project420.Management.BLL.Sales.Retail.Validators.CreatePricelistValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Management.BLL.Sales.Retail.DTOs.UpdatePricelistDto>, Project420.Management.BLL.Sales.Retail.Validators.UpdatePricelistValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Management.BLL.Sales.Retail.DTOs.CreatePricelistItemDto>, Project420.Management.BLL.Sales.Retail.Validators.CreatePricelistItemValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Management.BLL.Sales.Retail.DTOs.UpdatePricelistItemDto>, Project420.Management.BLL.Sales.Retail.Validators.UpdatePricelistItemValidator>();

// FluentValidation - Management Module - Sales (Common)
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Management.BLL.Sales.SalesCommon.DTOs.CustomerRegistrationDto>, Project420.Management.BLL.Sales.SalesCommon.Validators.CustomerRegistrationValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Management.BLL.Sales.SalesCommon.DTOs.CreateDebtorCategoryDto>, Project420.Management.BLL.Sales.SalesCommon.Validators.CreateDebtorCategoryValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Management.BLL.Sales.SalesCommon.DTOs.UpdateDebtorCategoryDto>, Project420.Management.BLL.Sales.SalesCommon.Validators.UpdateDebtorCategoryValidator>();

// FluentValidation - Management Module - Stock Management
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Management.BLL.StockManagement.DTOs.CreateProductDto>, Project420.Management.BLL.StockManagement.Validators.CreateProductValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Management.BLL.StockManagement.DTOs.UpdateProductDto>, Project420.Management.BLL.StockManagement.Validators.UpdateProductValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Management.BLL.StockManagement.DTOs.CreateProductCategoryDto>, Project420.Management.BLL.StockManagement.Validators.CreateProductCategoryValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Project420.Management.BLL.StockManagement.DTOs.UpdateProductCategoryDto>, Project420.Management.BLL.StockManagement.Validators.UpdateProductCategoryValidator>();

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.Zero // Remove default 5 minute clock skew
    };
});

builder.Services.AddAuthorization();

// CORS
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Project420 Online Ordering API",
        Version = "v1",
        Description = "Cannabis Click & Collect API - POPIA & Cannabis Act Compliant",
        Contact = new OpenApiContact
        {
            Name = "Project420",
            Email = "support@project420.co.za"
        }
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ===== Middleware Pipeline =====

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Project420 API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

// Compliance middleware (exception handling, rate limiting, audit logging, age verification)
app.UseComplianceMiddleware();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

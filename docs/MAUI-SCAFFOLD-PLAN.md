# Project420 MAUI Application - Complete Scaffold Plan

**Date**: 2025-12-08
**Status**: Planning Phase
**Target Framework**: .NET 9.0
**Architecture**: MVVM with CommunityToolkit.Mvvm

---

## Overview

Complete scaffold structure for Project420 mobile application using .NET MAUI. The scaffold establishes a consistent architecture pattern that will be replicated across all vertical slices (Authentication, Products, Cart, Orders, Account, Management).

---

## Project Structure

### Root Level
```
Project420.UI.Maui/
├── MauiProgram.cs              ← DI container & service registration
├── App.xaml                    ← Application resources & theme
├── AppShell.xaml               ← Shell navigation structure
└── Project420.UI.Maui.csproj  ← Project file
```

### Infrastructure Layer
```
Infrastructure/
├── ViewModelBase.cs            ← Base class for all ViewModels
├── NavigationService.cs        ← Navigation abstraction
├── DialogService.cs            ← Alerts, prompts, toasts
├── SecureStorageService.cs     ← POPIA-compliant token storage
└── Converters/                 ← Value converters
    ├── BoolToColorConverter.cs
    ├── DateTimeToStringConverter.cs
    └── DecimalToCurrencyConverter.cs
```

### Services Layer (Business Logic)
```
Services/
├── ApiClient/                  ← HTTP client services
│   ├── HttpClientService.cs    (Base HTTP client with auth headers)
│   ├── IAuthApiService.cs
│   ├── AuthApiService.cs
│   ├── IProductApiService.cs
│   ├── ProductApiService.cs
│   ├── IOrderApiService.cs
│   ├── OrderApiService.cs
│   ├── ICustomerApiService.cs
│   └── CustomerApiService.cs
├── Authentication/
│   ├── IAuthenticationService.cs
│   └── AuthenticationService.cs
├── Cart/
│   ├── ICartService.cs
│   └── CartService.cs
├── Compliance/
│   ├── IAgeVerificationService.cs
│   └── AgeVerificationService.cs
└── Notifications/
    ├── INotificationService.cs
    └── NotificationService.cs
```

### ViewModels Layer (MVVM)
```
ViewModels/
├── Authentication/
│   ├── LoginViewModel.cs
│   ├── RegisterViewModel.cs
│   └── AgeVerificationViewModel.cs
├── Products/
│   ├── ProductListViewModel.cs
│   ├── ProductDetailViewModel.cs
│   └── ProductSearchViewModel.cs
├── Cart/
│   ├── CartViewModel.cs
│   └── CheckoutViewModel.cs
├── Orders/
│   ├── OrderHistoryViewModel.cs
│   ├── OrderDetailViewModel.cs
│   └── OrderTrackingViewModel.cs
├── Account/
│   ├── AccountViewModel.cs
│   └── ProfileViewModel.cs
└── Management/               ← Admin features
    ├── Dashboard/
    │   └── DashboardViewModel.cs
    ├── Products/
    │   ├── ProductManagementViewModel.cs
    │   └── ProductFormViewModel.cs
    ├── Customers/
    │   ├── CustomerManagementViewModel.cs
    │   └── CustomerFormViewModel.cs
    └── Orders/
        ├── OrderManagementViewModel.cs
        └── OrderProcessingViewModel.cs
```

### Views Layer (XAML UI)
```
Views/
├── Authentication/
│   ├── LoginPage.xaml
│   ├── RegisterPage.xaml
│   └── AgeVerificationPage.xaml
├── Products/
│   ├── ProductListPage.xaml
│   ├── ProductDetailPage.xaml
│   └── ProductSearchPage.xaml
├── Cart/
│   ├── CartPage.xaml
│   └── CheckoutPage.xaml
├── Orders/
│   ├── OrderHistoryPage.xaml
│   ├── OrderDetailPage.xaml
│   └── OrderTrackingPage.xaml
├── Account/
│   ├── AccountPage.xaml
│   └── ProfilePage.xaml
└── Management/               ← Admin UI
    ├── Dashboard/
    │   └── DashboardPage.xaml
    ├── Products/
    │   ├── ProductManagementPage.xaml
    │   └── ProductFormPage.xaml
    ├── Customers/
    │   ├── CustomerManagementPage.xaml
    │   └── CustomerFormPage.xaml
    └── Orders/
        ├── OrderManagementPage.xaml
        └── OrderProcessingPage.xaml
```

### Controls Layer (Reusable Components)
```
Controls/
├── AgeVerificationBanner.xaml      ← Cannabis Act compliance banner
├── ProductCard.xaml                ← Product display card
├── OrderStatusBadge.xaml           ← Order status indicator
└── ComplianceFooter.xaml           ← Legal disclaimer footer
```

### Resources Layer
```
Resources/
├── AppIcon/                        ← App icons (various sizes)
├── Splash/                         ← Splash screen
├── Images/                         ← Image assets
├── Fonts/                          ← Custom fonts
├── Styles/                         ← Shared styles
│   ├── Colors.xaml                 (Color palette)
│   ├── Styles.xaml                 (Component styles)
│   └── AppTheme.xaml               (Light/dark theme)
└── Strings/                        ← Localization
    ├── AppResources.resx           (English)
    ├── AppResources.af.resx        (Afrikaans)
    └── AppResources.zu.resx        (Zulu)
```

---

## Vertical Slice Pattern

Each feature follows a consistent vertical slice pattern:

### Example: Product Management Slice
```
1. View (XAML)
   └── Views/Management/Products/ProductManagementPage.xaml

2. ViewModel (MVVM)
   └── ViewModels/Management/Products/ProductManagementViewModel.cs

3. Service (Business Logic)
   └── Services/ApiClient/ProductApiService.cs

4. API Integration
   └── Calls → Project420.API.WebApi/Controllers/ProductsController.cs

5. Backend Services
   └── Uses → Project420.Management.BLL/Services/ProductService.cs
```

### Vertical Slice Checklist
For each feature, implement in order:

1. **Service Layer**
   - Create API service interface (`IProductApiService.cs`)
   - Implement API service with HTTP calls (`ProductApiService.cs`)
   - Register in DI container (`MauiProgram.cs`)

2. **ViewModel Layer**
   - Create ViewModel extending `ViewModelBase`
   - Implement ObservableProperties (CommunityToolkit.Mvvm)
   - Implement Commands (RelayCommand)
   - Inject API service via constructor

3. **View Layer**
   - Create XAML page
   - Set ViewModel as BindingContext
   - Bind UI elements to ViewModel properties
   - Bind buttons to ViewModel commands

4. **Navigation**
   - Register page route in `AppShell.xaml`
   - Add navigation menu item
   - Test navigation flow

5. **Testing**
   - Manual testing on Android emulator
   - Manual testing on iOS simulator (if available)
   - Test offline behavior
   - Test error scenarios

---

## NuGet Package Dependencies

### Required Packages
```xml
<ItemGroup>
  <!-- MVVM Framework -->
  <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
  <PackageReference Include="CommunityToolkit.Maui" Version="7.0.0" />

  <!-- HTTP Client -->
  <PackageReference Include="Microsoft.Extensions.Http" Version="9.0.0" />

  <!-- JSON Serialization -->
  <PackageReference Include="System.Text.Json" Version="9.0.0" />

  <!-- Image Caching -->
  <PackageReference Include="FFImageLoading.Maui" Version="1.2.0" />

  <!-- Localization -->
  <PackageReference Include="Microsoft.Extensions.Localization" Version="9.0.0" />

  <!-- Secure Storage (built into MAUI) -->
  <!-- No package needed - use Microsoft.Maui.Storage.SecureStorage -->
</ItemGroup>
```

### Project References
```xml
<ItemGroup>
  <!-- Reference existing BLL projects for DTOs -->
  <ProjectReference Include="..\Modules\Management\Project420.Management.BLL\Project420.Management.BLL.csproj" />
  <ProjectReference Include="..\Modules\OnlineOrders\Project420.OnlineOrders.BLL\Project420.OnlineOrders.BLL.csproj" />
  <ProjectReference Include="..\Modules\Retail\POS\Project420.Retail.POS.BLL\Project420.Retail.POS.BLL.csproj" />
  <ProjectReference Include="..\Shared\Project420.Shared.Core\Project420.Shared.Core.csproj" />
</ItemGroup>
```

---

## MauiProgram.cs Configuration

```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register Infrastructure Services
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<IDialogService, DialogService>();
        builder.Services.AddSingleton<ISecureStorageService, SecureStorageService>();

        // Register HTTP Client
        builder.Services.AddHttpClient<HttpClientService>(client =>
        {
            client.BaseAddress = new Uri("https://localhost:7001/api/"); // TODO: Update with production URL
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Register API Services
        builder.Services.AddScoped<IAuthApiService, AuthApiService>();
        builder.Services.AddScoped<IProductApiService, ProductApiService>();
        builder.Services.AddScoped<IOrderApiService, OrderApiService>();
        builder.Services.AddScoped<ICustomerApiService, CustomerApiService>();

        // Register Business Services
        builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
        builder.Services.AddSingleton<ICartService, CartService>();
        builder.Services.AddSingleton<IAgeVerificationService, AgeVerificationService>();
        builder.Services.AddSingleton<INotificationService, NotificationService>();

        // Register ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<AgeVerificationViewModel>();
        builder.Services.AddTransient<ProductListViewModel>();
        builder.Services.AddTransient<ProductDetailViewModel>();
        builder.Services.AddTransient<ProductSearchViewModel>();
        builder.Services.AddTransient<CartViewModel>();
        builder.Services.AddTransient<CheckoutViewModel>();
        builder.Services.AddTransient<OrderHistoryViewModel>();
        builder.Services.AddTransient<OrderDetailViewModel>();
        builder.Services.AddTransient<OrderTrackingViewModel>();
        builder.Services.AddTransient<AccountViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();

        // Register Views (Pages)
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<AgeVerificationPage>();
        builder.Services.AddTransient<ProductListPage>();
        builder.Services.AddTransient<ProductDetailPage>();
        builder.Services.AddTransient<ProductSearchPage>();
        builder.Services.AddTransient<CartPage>();
        builder.Services.AddTransient<CheckoutPage>();
        builder.Services.AddTransient<OrderHistoryPage>();
        builder.Services.AddTransient<OrderDetailPage>();
        builder.Services.AddTransient<OrderTrackingPage>();
        builder.Services.AddTransient<AccountPage>();
        builder.Services.AddTransient<ProfilePage>();

        return builder.Build();
    }
}
```

---

## AppShell.xaml Navigation Structure

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<Shell
    x:Class="Project420.UI.Maui.AppShell"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:views="clr-namespace:Project420.UI.Maui.Views"
    Shell.FlyoutBehavior="Disabled">

    <!-- Tab Bar -->
    <TabBar>
        <!-- Home Tab -->
        <ShellContent
            Title="Home"
            ContentTemplate="{DataTemplate views:Products.ProductListPage}"
            Icon="home.png"
            Route="products" />

        <!-- Cart Tab -->
        <ShellContent
            Title="Cart"
            ContentTemplate="{DataTemplate views:Cart.CartPage}"
            Icon="cart.png"
            Route="cart" />

        <!-- Orders Tab -->
        <ShellContent
            Title="Orders"
            ContentTemplate="{DataTemplate views:Orders.OrderHistoryPage}"
            Icon="orders.png"
            Route="orders" />

        <!-- Account Tab -->
        <ShellContent
            Title="Account"
            ContentTemplate="{DataTemplate views:Account.AccountPage}"
            Icon="account.png"
            Route="account" />
    </TabBar>

    <!-- Define Routes (for navigation) -->
    <Shell.Resources>
        <ResourceDictionary>
            <!-- Authentication -->
            <DataTemplate x:Key="LoginPage">
                <views:Authentication.LoginPage />
            </DataTemplate>
            <DataTemplate x:Key="RegisterPage">
                <views:Authentication.RegisterPage />
            </DataTemplate>
            <DataTemplate x:Key="AgeVerificationPage">
                <views:Authentication.AgeVerificationPage />
            </DataTemplate>

            <!-- Products -->
            <DataTemplate x:Key="ProductDetailPage">
                <views:Products.ProductDetailPage />
            </DataTemplate>
            <DataTemplate x:Key="ProductSearchPage">
                <views:Products.ProductSearchPage />
            </DataTemplate>

            <!-- Checkout -->
            <DataTemplate x:Key="CheckoutPage">
                <views:Cart.CheckoutPage />
            </DataTemplate>

            <!-- Order Details -->
            <DataTemplate x:Key="OrderDetailPage">
                <views:Orders.OrderDetailPage />
            </DataTemplate>
            <DataTemplate x:Key="OrderTrackingPage">
                <views:Orders.OrderTrackingPage />
            </DataTemplate>

            <!-- Account -->
            <DataTemplate x:Key="ProfilePage">
                <views:Account.ProfilePage />
            </DataTemplate>
        </ResourceDictionary>
    </Shell.Resources>
</Shell>
```

---

## Cannabis Act Compliance (Mobile)

### Age Verification Flow
1. **App Launch**: Show age gate ("Are you 18+?")
2. **Registration**: Verify DOB (must be 18+)
3. **Every Session**: Display persistent age verification banner
4. **Product Pages**: Show THC/CBD content, lab test results
5. **Checkout**: Remind about ID verification at pickup

### POPIA Compliance
- **Consent**: Explicit opt-in during registration
- **Data Access**: Allow users to view/delete personal data
- **Secure Storage**: Use `SecureStorage` API for tokens/PII
- **Audit Trail**: Log all data access (API handles this)

### UI Elements
- **Age Verification Banner**: Persistent green banner at top ("Age Verified: 18+")
- **Compliance Footer**: Legal disclaimer on all pages
- **Cannabis Warning**: "Cannabis is a Schedule 5 substance. Use responsibly."

---

## Development Milestones

### Phase 1: Scaffold Setup (Week 1)
- [ ] Create MAUI project
- [ ] Install NuGet packages
- [ ] Create folder structure
- [ ] Setup DI container (MauiProgram.cs)
- [ ] Create AppShell navigation
- [ ] Create base infrastructure (ViewModelBase, services)
- [ ] Setup shared resources (colors, fonts, styles)

### Phase 2: Authentication Vertical Slice (Week 2)
- [ ] LoginViewModel + LoginPage
- [ ] RegisterViewModel + RegisterPage
- [ ] AgeVerificationViewModel + AgeVerificationPage
- [ ] AuthenticationService
- [ ] AuthApiService
- [ ] Test full authentication flow

### Phase 3: Product Catalog Vertical Slice (Week 3)
- [ ] ProductListViewModel + ProductListPage
- [ ] ProductDetailViewModel + ProductDetailPage
- [ ] ProductSearchViewModel + ProductSearchPage
- [ ] ProductApiService
- [ ] Test product browsing

### Phase 4: Cart & Checkout Vertical Slice (Week 4)
- [ ] CartViewModel + CartPage
- [ ] CheckoutViewModel + CheckoutPage
- [ ] CartService (local storage)
- [ ] OrderApiService
- [ ] Payment integration (Yoco/PayFast/Ozow)
- [ ] Test order placement

### Phase 5: Order Management Vertical Slice (Week 5)
- [ ] OrderHistoryViewModel + OrderHistoryPage
- [ ] OrderDetailViewModel + OrderDetailPage
- [ ] OrderTrackingViewModel + OrderTrackingPage
- [ ] Test order tracking

### Phase 6: Account Management Vertical Slice (Week 6)
- [ ] AccountViewModel + AccountPage
- [ ] ProfileViewModel + ProfilePage
- [ ] Test profile editing

### Phase 7: Admin/Management Slices (Week 7+)
- [ ] Dashboard (sales, inventory overview)
- [ ] Product Management (CRUD)
- [ ] Customer Management (view, search)
- [ ] Order Processing (fulfill orders, mark ready for pickup)

---

## Next Steps

1. Wait for `dotnet workload install maui` to complete
2. Create MAUI project: `dotnet new maui -n Project420.UI.Maui`
3. Create complete folder structure
4. Implement Phase 1: Scaffold Setup
5. Begin Phase 2: Authentication Vertical Slice

---

**Status**: Awaiting MAUI workload installation
**Est. Completion**: Phase 1 by end of Week 1

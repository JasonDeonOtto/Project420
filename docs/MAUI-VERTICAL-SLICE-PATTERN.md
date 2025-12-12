# MAUI Vertical Slice Pattern - Template

**Purpose**: Define the exact pattern to replicate for every feature
**Strategy**: Establish once (Login), replicate efficiently (all other features)
**Status**: Pattern Definition

---

## Pattern Overview

Every vertical slice follows this 5-step structure:

1. **Service** - API integration (HTTP calls to backend)
2. **ViewModel** - Business logic + data binding (MVVM)
3. **View** - UI (XAML + code-behind)
4. **DI Registration** - Register services + ViewModels
5. **Navigation** - Shell routing

---

## Step 1: Service Layer (API Integration)

### File: `Services/ApiClient/I{Feature}ApiService.cs`

```csharp
namespace Project420.UI.Maui.Services.ApiClient;

/// <summary>
/// Interface for {Feature} API calls
/// </summary>
public interface I{Feature}ApiService
{
    /// <summary>
    /// {Description of method}
    /// </summary>
    Task<{ResponseDto}> {MethodName}Async({RequestDto} request);
}
```

### File: `Services/ApiClient/{Feature}ApiService.cs`

```csharp
using System.Net.Http.Json;
using Project420.{Module}.BLL.DTOs.Request;
using Project420.{Module}.BLL.DTOs.Response;

namespace Project420.UI.Maui.Services.ApiClient;

/// <summary>
/// Implementation of {Feature} API service
/// </summary>
public class {Feature}ApiService : I{Feature}ApiService
{
    private readonly HttpClient _httpClient;

    public {Feature}ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// {Description of method}
    /// </summary>
    public async Task<{ResponseDto}> {MethodName}Async({RequestDto} request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/{controller}/{action}",
                request
            );

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<{ResponseDto}>()
                ?? throw new Exception("Failed to deserialize response");
        }
        catch (HttpRequestException ex)
        {
            // Log error
            throw new Exception($"API call failed: {ex.Message}", ex);
        }
    }
}
```

### Pattern Checklist - Service:
- [ ] Create interface `I{Feature}ApiService.cs`
- [ ] Create implementation `{Feature}ApiService.cs`
- [ ] Inject `HttpClient` via constructor
- [ ] Implement methods with try-catch error handling
- [ ] Use existing DTOs from BLL projects (don't recreate)
- [ ] Register in `MauiProgram.cs` DI container

---

## Step 2: ViewModel Layer (MVVM)

### File: `ViewModels/{Module}/{Feature}ViewModel.cs`

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Project420.UI.Maui.Services.ApiClient;
using Project420.UI.Maui.Infrastructure;
using Project420.{Module}.BLL.DTOs.Request;
using Project420.{Module}.BLL.DTOs.Response;

namespace Project420.UI.Maui.ViewModels.{Module};

/// <summary>
/// ViewModel for {Feature} page
/// </summary>
public partial class {Feature}ViewModel : ViewModelBase
{
    private readonly I{Feature}ApiService _{featureApi};
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

    public {Feature}ViewModel(
        I{Feature}ApiService {featureApi},
        INavigationService navigationService,
        IDialogService dialogService)
    {
        _{featureApi} = {featureApi};
        _navigationService = navigationService;
        _dialogService = dialogService;
    }

    #region Observable Properties

    [ObservableProperty]
    private string _propertyName = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    #endregion

    #region Commands

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            // Call API service
            var request = new {RequestDto}
            {
                // Map properties
            };

            var response = await _{featureApi}.{MethodName}Async(request);

            if (response.Success)
            {
                // Update UI properties
                await _dialogService.ShowAlertAsync("Success", response.Message);
            }
            else
            {
                ErrorMessage = response.ErrorMessage ?? "An error occurred";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
            await _dialogService.ShowAlertAsync("Error", ErrorMessage);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToDetailsAsync(int id)
    {
        await _navigationService.NavigateToAsync($"///{Feature}Detail?id={id}");
    }

    #endregion

    #region Initialization

    public override async Task InitializeAsync()
    {
        await LoadDataAsync();
    }

    #endregion
}
```

### Pattern Checklist - ViewModel:
- [ ] Extend `ViewModelBase`
- [ ] Use `[ObservableProperty]` for bindable properties
- [ ] Use `[RelayCommand]` for commands
- [ ] Inject API service, NavigationService, DialogService
- [ ] Implement `IsBusy` flag for loading states
- [ ] Implement `ErrorMessage` property for error display
- [ ] Wrap API calls in try-catch with error handling
- [ ] Override `InitializeAsync()` if needed
- [ ] Register in `MauiProgram.cs` DI container

---

## Step 3: View Layer (XAML + Code-Behind)

### File: `Views/{Module}/{Feature}Page.xaml`

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
    x:Class="Project420.UI.Maui.Views.{Module}.{Feature}Page"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:viewmodels="clr-namespace:Project420.UI.Maui.ViewModels.{Module}"
    x:DataType="viewmodels:{Feature}ViewModel"
    Title="{Feature}">

    <!-- Activity Indicator (Loading Spinner) -->
    <ContentPage.Resources>
        <ResourceDictionary>
            <Style TargetType="ActivityIndicator">
                <Setter Property="IsRunning" Value="{Binding IsBusy}" />
                <Setter Property="IsVisible" Value="{Binding IsBusy}" />
                <Setter Property="Color" Value="{StaticResource Primary}" />
            </Style>
        </ResourceDictionary>
    </ContentPage.Resources>

    <Grid>
        <!-- Main Content -->
        <ScrollView>
            <VerticalStackLayout Padding="20" Spacing="15">

                <!-- Header -->
                <Label
                    Text="{Feature} Page"
                    Style="{StaticResource HeaderLabelStyle}"
                    HorizontalOptions="Center" />

                <!-- Input Fields (example) -->
                <Entry
                    Placeholder="Enter value"
                    Text="{Binding PropertyName}"
                    IsEnabled="{Binding IsBusy, Converter={StaticResource InvertedBoolConverter}}" />

                <!-- Error Message -->
                <Label
                    Text="{Binding ErrorMessage}"
                    TextColor="Red"
                    IsVisible="{Binding ErrorMessage, Converter={StaticResource StringIsNotNullOrEmptyConverter}}" />

                <!-- Action Button -->
                <Button
                    Text="Submit"
                    Command="{Binding LoadDataCommand}"
                    IsEnabled="{Binding IsBusy, Converter={StaticResource InvertedBoolConverter}}" />

            </VerticalStackLayout>
        </ScrollView>

        <!-- Loading Overlay -->
        <Grid
            IsVisible="{Binding IsBusy}"
            BackgroundColor="#80000000">
            <ActivityIndicator
                IsRunning="{Binding IsBusy}"
                HorizontalOptions="Center"
                VerticalOptions="Center" />
        </Grid>
    </Grid>

</ContentPage>
```

### File: `Views/{Module}/{Feature}Page.xaml.cs`

```csharp
using Project420.UI.Maui.ViewModels.{Module};

namespace Project420.UI.Maui.Views.{Module};

public partial class {Feature}Page : ContentPage
{
    public {Feature}Page({Feature}ViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is {Feature}ViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }
}
```

### Pattern Checklist - View:
- [ ] Set `x:DataType` for compiled bindings
- [ ] Bind `Title` property
- [ ] Use `ScrollView` for scrollable content
- [ ] Implement loading overlay with `ActivityIndicator`
- [ ] Bind buttons to ViewModel commands
- [ ] Disable inputs when `IsBusy` is true
- [ ] Display error messages when present
- [ ] Inject ViewModel via constructor in code-behind
- [ ] Set `BindingContext = viewModel`
- [ ] Call `InitializeAsync()` in `OnAppearing()`
- [ ] Register in `MauiProgram.cs` DI container

---

## Step 4: DI Registration (MauiProgram.cs)

### Add to `MauiProgram.cs`:

```csharp
// Register API Service
builder.Services.AddScoped<I{Feature}ApiService, {Feature}ApiService>();

// Register ViewModel
builder.Services.AddTransient<{Feature}ViewModel>();

// Register View (Page)
builder.Services.AddTransient<{Feature}Page>();
```

### Pattern Checklist - DI:
- [ ] Register API service as `Scoped` (new instance per request)
- [ ] Register ViewModel as `Transient` (new instance per navigation)
- [ ] Register Page as `Transient` (new instance per navigation)

---

## Step 5: Navigation (AppShell.xaml)

### Option A: Tab Navigation (Main Pages)

```xml
<TabBar>
    <ShellContent
        Title="{Feature}"
        ContentTemplate="{DataTemplate views:{Module}.{Feature}Page}"
        Icon="{feature}.png"
        Route="{feature}" />
</TabBar>
```

### Option B: Route Registration (Detail Pages)

```xml
<Shell.Resources>
    <ResourceDictionary>
        <DataTemplate x:Key="{Feature}Page">
            <views:{Module}.{Feature}Page />
        </DataTemplate>
    </ResourceDictionary>
</Shell.Resources>
```

### Navigation from ViewModel:

```csharp
// Navigate to route
await _navigationService.NavigateToAsync("///{feature}");

// Navigate with parameter
await _navigationService.NavigateToAsync($"///{feature}Detail?id={id}");

// Navigate back
await _navigationService.GoBackAsync();
```

### Pattern Checklist - Navigation:
- [ ] Add route to `AppShell.xaml` (tab or route)
- [ ] Test navigation from other pages
- [ ] Test parameter passing (if applicable)

---

## Complete Vertical Slice Checklist

For each feature, complete in order:

### 1. Service Layer
- [ ] Create `I{Feature}ApiService.cs` interface
- [ ] Create `{Feature}ApiService.cs` implementation
- [ ] Register in DI container

### 2. ViewModel Layer
- [ ] Create `{Feature}ViewModel.cs`
- [ ] Add `[ObservableProperty]` for bindable data
- [ ] Add `[RelayCommand]` for actions
- [ ] Inject services via constructor
- [ ] Implement error handling
- [ ] Register in DI container

### 3. View Layer
- [ ] Create `{Feature}Page.xaml`
- [ ] Create `{Feature}Page.xaml.cs`
- [ ] Setup data binding
- [ ] Inject ViewModel via constructor
- [ ] Call `InitializeAsync()` in `OnAppearing()`
- [ ] Register in DI container

### 4. Navigation
- [ ] Add route to `AppShell.xaml`
- [ ] Test navigation

### 5. Testing
- [ ] Run on Android emulator
- [ ] Test loading state (IsBusy)
- [ ] Test error handling
- [ ] Test navigation flow

---

## Example: Replicating Login Pattern for Register

Once Login is working, replicate for Register:

1. **Copy** `LoginApiService.cs` → `RegisterApiService.cs`
2. **Rename** class, methods, DTOs
3. **Copy** `LoginViewModel.cs` → `RegisterViewModel.cs`
4. **Rename** class, properties, commands
5. **Copy** `LoginPage.xaml` → `RegisterPage.xaml`
6. **Modify** UI fields (DOB picker, consent checkboxes)
7. **Update** `MauiProgram.cs` (register new services)
8. **Update** `AppShell.xaml` (add route)
9. **Test** the new page

**Time Estimate**: 15-30 minutes per page (after pattern established)

---

## Pattern Files Summary

Every vertical slice creates these files:

```
Services/ApiClient/
├── I{Feature}ApiService.cs      (interface)
└── {Feature}ApiService.cs       (implementation)

ViewModels/{Module}/
└── {Feature}ViewModel.cs        (MVVM logic)

Views/{Module}/
├── {Feature}Page.xaml           (UI)
└── {Feature}Page.xaml.cs        (code-behind)
```

Plus updates to:
- `MauiProgram.cs` (DI registration)
- `AppShell.xaml` (navigation routes)

---

**Next Step**: Implement Login as the reference pattern, then replicate!

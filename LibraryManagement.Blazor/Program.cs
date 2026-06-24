using LibraryManagement.Blazor.Components;
using LibraryManagement.Blazor.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddControllers();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<HomeClientService>();
builder.Services.AddScoped<BooksClientService>();
builder.Services.AddScoped<CatalogClientService>();
builder.Services.AddScoped<AuditClientService>();
builder.Services.AddScoped<AIClientService>();
builder.Services.AddScoped<CirculationClientService>();
builder.Services.AddScoped<PaymentClientService>();
builder.Services.AddScoped<InventoryClientService>();
builder.Services.AddScoped<UserActionClientService>();
builder.Services.AddScoped<UserClientService>();
builder.Services.AddScoped<ReviewClientService>();
builder.Services.AddScoped<NotificationClientService>();
builder.Services.AddScoped<ReservationClientService>();
builder.Services.AddScoped<SystemSettingsClientService>();
builder.Services.AddScoped<ReportsClientService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/auth/logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });
builder.Services.AddAuthorization();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithRedirects("/not-found");
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseSession();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

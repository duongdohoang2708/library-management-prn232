using LibraryManagement.BLL.Services;
using LibraryManagement.BLL.Services.Interface;
using LibraryManagement.BLL.Services.Jobs;
using LibraryManagement.DAL.Data;
using LibraryManagement.DAL.Repositories;
using LibraryManagementDAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Quartz;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddOData(options => options
        .Select()
        .Filter()
        .OrderBy()
        .Expand()
        .Count()
        .SetMaxTop(100)
        .AddRouteComponents("odata", GetEdmModel()))
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuthenRepository>();
builder.Services.AddScoped<BookRepository>();
builder.Services.AddScoped<CatalogManagementRepository>();
builder.Services.AddScoped<InventoryRepository>();
builder.Services.AddScoped<CirculationRepository>();
builder.Services.AddScoped<ReservationRepository>();
builder.Services.AddScoped<UserManagementRepository>();
builder.Services.AddScoped<NotificationRepository>();
builder.Services.AddScoped<ReviewRepository>();
builder.Services.AddScoped<DashboardRepository>();
builder.Services.AddScoped<AuditLogRepository>();
builder.Services.AddScoped<SystemSettingRepository>();
builder.Services.AddScoped<ReportRepository>();
builder.Services.AddScoped<ReminderRepository>();
builder.Services.AddScoped<BookCatalogService>();
builder.Services.AddScoped<CatalogManagementService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<CirculationService>();
builder.Services.AddScoped<ReservationService>();
builder.Services.AddScoped<UserManagementService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<SystemSettingService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<DueReminderService>();
builder.Services.AddScoped<PasswordHasher<Account>>();
builder.Services.AddQuartz(options =>
{
    var jobKey = new JobKey("DueReminderJob");
    options.AddJob<DueReminderJob>(job => job.WithIdentity(jobKey));
    options.AddTrigger(trigger => trigger
        .ForJob(jobKey)
        .WithIdentity("DueReminderJob-trigger")
        .WithCronSchedule(builder.Configuration["Jobs:DueReminderCron"] ?? "0 0 8 * * ?"));
});
builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

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

        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey!)
        )
    };
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static IEdmModel GetEdmModel()
{
    var builder = new ODataConventionModelBuilder();

    builder.EntitySet<Book>("Books");
    builder.EntitySet<BookCopy>("BookCopies");
    builder.EntitySet<Author>("Authors");
    builder.EntitySet<Category>("Categories");
    builder.EntitySet<Publisher>("Publishers");

    return builder.GetEdmModel();
}

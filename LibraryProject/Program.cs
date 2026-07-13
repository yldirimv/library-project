using Hangfire;
using LibraryProject;
using LibraryProject.Business.Interfaces;
using LibraryProject.Business.Services;
using LibraryProject.Data.Context;
using LibraryProject.Data.Queries;
using LibraryProject.Data.Repositories;
using LibraryProject.Data.SeedData;
using LibraryProject.Hubs;
using LibraryProject.Model.Entities;
using LibraryProject.Model.Interfaces;
using LibraryProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();


builder.Services.AddDbContext<LibraryDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));


builder.Services.AddIdentity<AppUser, IdentityRole>(opt =>
{
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequiredLength = 6;
    opt.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<LibraryDbContext>()
.AddDefaultTokenProviders();

// Giriş yapmamış -> Login'e; yetkisi yetmeyen -> AccessDenied'a
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Account/Login";
    opt.AccessDeniedPath = "/Account/AccessDenied";
});

//hangfire arka plan işleri icin
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddHangfireServer();

//di kayıtları
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<ILoyaltyService, LoyaltyService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IQrService, QrService>();
builder.Services.AddScoped<ICheckInService, CheckInService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IReservationMaintenanceService, ReservationMaintenanceService>();
builder.Services.AddScoped<DashboardQueries>(); // Dapper sorguları
builder.Services.AddScoped<ILoyaltyMaintenanceService, LoyaltyMaintenanceService>();
builder.Services.AddScoped<INoiseReportService, NoiseReportService>();
builder.Services.AddScoped<ISeatNotifier, SignalRSeatNotifier>();
builder.Services.AddScoped<IChatbotService, ChatbotService>();
builder.Services.AddSignalR();

builder.Services.AddControllersWithViews();

var app = builder.Build();


//seed+job kaydı
using (var scope = app.Services.CreateScope())
{
    await DbInitializer.SeedAsync(scope.ServiceProvider);
}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();   // önce: sen kimsin?
app.UseAuthorization();    // sonra: buna yetkin var mı?

// Hangfire paneli (auth'tan sonra: Admin filtresi çalışabilsin)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAdminFilter() }
});

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<NotificationHub>("/hubs/notification");

RecurringJob.AddOrUpdate<IReservationMaintenanceService>(
    "reservation-maintenance",
    service => service.RunAsync(),
    "* * * * *"); // cron: her dakika

RecurringJob.AddOrUpdate<ILoyaltyMaintenanceService>(
    "weekly-loyalty-bonus",
    service => service.GrantWeeklyBonusesAsync(),
    "5 0 * * 1"); // her pazartesi 00:05

app.Run();
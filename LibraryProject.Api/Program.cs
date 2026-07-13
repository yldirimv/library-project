using System.Text;
using LibraryProject.Business.Interfaces;
using LibraryProject.Business.Services;
using LibraryProject.Data.Context;
using LibraryProject.Data.Repositories;
using LibraryProject.Model.Entities;
using LibraryProject.Model.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---- DbContext (web ile aynı veritabanı) ----
builder.Services.AddDbContext<LibraryDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// ---- Identity (cookie'siz: sadece kullanıcı/rol yönetimi için) ----
builder.Services.AddIdentityCore<AppUser>(opt =>
{
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequiredLength = 6;
    opt.Password.RequireUppercase = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<LibraryDbContext>();

// ---- JWT doğrulama ----
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!))
        };
    });

builder.Services.AddAuthorization();

// ---- Uygulama servisleri (web'dekiyle aynı kayıtlar) ----
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IQrService, QrService>();
builder.Services.AddScoped<ICheckInService, CheckInService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<ILoyaltyService, LoyaltyService>();
builder.Services.AddScoped<INoiseReportService, NoiseReportService>();

// SeatNotifier: API'de SignalR hub'ı yok; boş implementasyon
builder.Services.AddScoped<ISeatNotifier, NoOpSeatNotifier>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// CheckInService'in istediği notifier'ın "hiçbir şey yapma" versiyonu
public class NoOpSeatNotifier : ISeatNotifier
{
    public Task NotifySeatChangedAsync() => Task.CompletedTask;
}
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartBank.API.DTOs.Auth;
using SmartBank.API.DTOs.Accounts;
using SmartBank.API.DTOs.Transactions;
using SmartBank.API.DTOs.Loans;
using SmartBank.API.DTOs.Support;
using SmartBank.API.Validators;
using SmartBank.API.Middleware;
using SmartBank.API.Services;
using SmartBank.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SmartBankDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtKey = builder.Configuration["JwtSettings:SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey missing.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IValidator<RegisterRequestDto>, RegisterValidator>();
builder.Services.AddScoped<IValidator<LoginRequestDto>, LoginValidator>();

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IValidator<AccountCreateDto>, AccountCreateValidator>();

builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IValidator<DepositWithdrawDto>, DepositWithdrawValidator>();
builder.Services.AddScoped<IValidator<TransferDto>, TransferValidator>();

builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<ISupportService, SupportService>();
builder.Services.AddScoped<IValidator<LoanApplyDto>, LoanApplyValidator>();
builder.Services.AddScoped<IValidator<TicketCreateDto>, TicketCreateValidator>();

builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartBank API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your token}"
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

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("SmartBankPolicy", policy =>
    {
        policy
            .WithOrigins(
                "https://smartbank-mvc-prod-fhckddgcghbnfncf.centralindia-01.azurewebsites.net",
                "http://localhost:5112"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();


app.UseMiddleware<ExceptionMiddleware>(); 

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSession();
app.UseHttpsRedirection();
app.UseCors("SmartBankPolicy");
app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers();

app.Run();



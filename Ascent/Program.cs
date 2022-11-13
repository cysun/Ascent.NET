using Ascent.Models;
using Ascent.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var environment = builder.Environment;
var configuration = builder.Configuration;
var services = builder.Services;

// In production, this app will sit behind a Nginx reverse proxy with HTTPS
if (!environment.IsDevelopment())
    builder.WebHost.UseUrls("http://localhost:5002");

builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));

// Configure Services

services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB (default 30MB)
});

services.AddControllersWithViews();

services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie(opt =>
{
    opt.AccessDeniedPath = "/Home/AccessDenied";
    opt.Cookie.MaxAge = TimeSpan.FromDays(90);
})
.AddOpenIdConnect("oidc", options =>
{
    configuration.Bind("OIDC", options);
    options.ResponseType = "code";
    options.Scope.Add("email"); // only openid and profile scopes are requested by default
    options.Scope.Add("ascent-webapp");

    options.Events = new OpenIdConnectEvents
    {
        OnRemoteFailure = context =>
        {
            context.Response.Redirect("/");
            context.HandleResponse();
            return Task.FromResult(0);
        }
    };
});

services.AddAuthorization(options =>
{
    options.AddPolicy(Constants.Policy.CanRead, policy => policy.RequireClaim(Constants.ReadClaim));
    options.AddPolicy(Constants.Policy.CanWrite, policy => policy.RequireClaim(Constants.WriteClaim));
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireClaim(Constants.ReadClaim).Build();
});

services.AddRouting(options => options.LowercaseUrls = true);

services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

services.AddAutoMapper(config => config.AddProfile<MapperProfile>());

services.Configure<MinioSettings>(configuration.GetSection("Minio"));
services.AddSingleton<MinioService>();
services.AddScoped<FileService>();

services.Configure<EmailSettings>(configuration.GetSection("Email"));
services.AddScoped<EmailSender>();

services.AddScoped<PageService>();
services.AddScoped<PersonService>();
services.AddScoped<MessageService>();
services.AddScoped<GroupService>();
services.AddScoped<CourseService>();
services.AddScoped<SectionService>();
services.AddScoped<EnrollmentService>();
services.AddScoped<MftService>();
services.AddScoped<ProgramService>();
services.AddScoped<SurveyService>();
services.AddScoped<RubricService>();

// Build App

var app = builder.Build();

// Configure Middleware Pipeline

app.UseForwardedHeaders();

if (!environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Exception");
}

app.UseSerilogRequestLogging();

app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Run App

app.Run();

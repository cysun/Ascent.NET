using Ascent.Security;
using Ascent.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
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

var canvasSettings = configuration.GetSection("Canvas").Get<CanvasSettings>();
services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Constants.AuthenticationScheme.Oidc;
})
.AddCookie(opt =>
{
    opt.AccessDeniedPath = "/Home/AccessDenied";
    opt.Cookie.MaxAge = TimeSpan.FromDays(90);
})
.AddCookie(Constants.AuthenticationScheme.CanvasCookie, opt =>
{
    opt.AccessDeniedPath = "/Home/AccessDenied";
    opt.Cookie.MaxAge = TimeSpan.FromDays(90);

    opt.Events.OnValidatePrincipal = CanvasUtils.ProxyEvent(
        CanvasUtils.OnValidatePrincipal(canvasSettings),
        opt.Events.OnValidatePrincipal);
})
.AddOpenIdConnect(Constants.AuthenticationScheme.Oidc, options =>
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
})
.AddOAuth(Constants.AuthenticationScheme.Canvas, options =>
{
    options.AuthorizationEndpoint = canvasSettings.AuthorizationEndpoint;
    options.TokenEndpoint = canvasSettings.TokenEndpoint;
    options.ClientId = canvasSettings.ClientId;
    options.ClientSecret = canvasSettings.ClientSecret;
    options.SignInScheme = Constants.AuthenticationScheme.CanvasCookie;
    options.CallbackPath = new PathString("/signin-canvas");
    options.SaveTokens = true;
});

services.AddAuthorization(options =>
{
    options.AddPolicy(Constants.Policy.CanRead, policyBuilder => policyBuilder.RequireClaim(Constants.Claim.Read));
    options.AddPolicy(Constants.Policy.CanWrite, policyBuilder => policyBuilder.RequireClaim(Constants.Claim.Write));
    options.AddPolicy(Constants.Policy.CanManageProject,
        policyBuilder => policyBuilder.AddRequirements(new CanManageProjectRequirement()));
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireClaim(Constants.Claim.Read).Build();
});

services.AddScoped<IAuthorizationHandler, CanManageProjectHandler>();

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
services.AddScoped<CourseJournalService>();
services.AddScoped<SectionService>();
services.AddScoped<EnrollmentService>();
services.AddScoped<MftService>();
services.AddScoped<ProgramService>();
services.AddScoped<SurveyService>();
services.AddScoped<RubricService>();
services.AddScoped<ProjectService>();
services.AddScoped<RubricDataService>();
services.AddScoped<SurveyDataService>();
services.AddScoped<CourseTemplateService>();
services.AddScoped<AssignmentTemplateService>();

// Services for Canvas API
services.AddHttpContextAccessor();
services.AddScoped<CanvasHttpMessageHandler>();
services.AddHttpClient("CanvasAPI", client => client.BaseAddress = new Uri(canvasSettings.ApiBaseUrl))
    .AddHttpMessageHandler<CanvasHttpMessageHandler>();
services.AddScoped<CanvasApiService>();

// Build App

var app = builder.Build();

// Configure Middleware Pipeline

app.UseForwardedHeaders();

if (!environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Exception");
}

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

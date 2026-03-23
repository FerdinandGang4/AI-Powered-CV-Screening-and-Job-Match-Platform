using AI.CVScreening.Api.Services;
using AI.CVScreening.Api.Options;

var builder = WebApplication.CreateBuilder(args);

const string reactClientCorsPolicy = "ReactClient";

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? [];

    options.AddPolicy(reactClientCorsPolicy, policy =>
    {
        if (allowedOrigins.Length == 0)
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
            return;
        }

        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<AppMemoryStore>();
builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection("OpenAI"));
builder.Services.AddSingleton<IAuthService, InMemoryAuthService>();
builder.Services.AddSingleton<IJobPostingService, InMemoryJobPostingService>();
builder.Services.AddSingleton<ICandidateService, InMemoryCandidateService>();
builder.Services.AddSingleton<IScreeningService, InMemoryScreeningService>();
builder.Services.AddHttpClient<IOpenAiRankingService, OpenAiRankingService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(reactClientCorsPolicy);
app.Use(async (context, next) =>
{
    if (HttpMethods.IsOptions(context.Request.Method))
    {
        await next();
        return;
    }

    var path = context.Request.Path.Value ?? string.Empty;
    var isAnonymousPath =
        path.StartsWith("/api/health", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/api/auth", StringComparison.OrdinalIgnoreCase);

    if (isAnonymousPath)
    {
        await next();
        return;
    }

    var authorizationHeader = context.Request.Headers.Authorization.ToString();
    var bearerToken = authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
        ? authorizationHeader["Bearer ".Length..].Trim()
        : string.Empty;
    var recruiterToken = !string.IsNullOrWhiteSpace(bearerToken)
        ? bearerToken
        : context.Request.Headers["X-Recruiter-Token"].ToString();

    var store = context.RequestServices.GetRequiredService<AppMemoryStore>();
    if (string.IsNullOrWhiteSpace(recruiterToken) || !store.RecruiterSessions.ContainsKey(recruiterToken))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new { message = "Authentication required. Please sign up or log in before using the app." });
        return;
    }

    await next();
});
app.UseAuthorization();

app.MapControllers();

app.Run();

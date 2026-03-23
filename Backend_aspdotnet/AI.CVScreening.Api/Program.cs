using AI.CVScreening.Api.Services;

var builder = WebApplication.CreateBuilder(args);

const string reactClientCorsPolicy = "ReactClient";

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
builder.Services.AddSingleton<IJobPostingService, InMemoryJobPostingService>();
builder.Services.AddSingleton<ICandidateService, InMemoryCandidateService>();
builder.Services.AddSingleton<IScreeningService, InMemoryScreeningService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(reactClientCorsPolicy);
app.UseAuthorization();

app.MapControllers();

app.Run();

using Microsoft.EntityFrameworkCore;
using USSDApp.Data;
using USSDApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddMemoryCache(opt => opt.ExpirationScanFrequency = TimeSpan.FromSeconds(30));
builder.Services.AddDbContext<AppDbContext>(dbContextOptions =>
{
    dbContextOptions
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
        .EnableDetailedErrors()
        .EnableSensitiveDataLogging();
});
builder.Services.AddSingleton<SessionService>();
builder.Services.AddTransient<USSDService>();
builder.Services.AddScoped<ResultsService>();
builder.Services.AddScoped<AgentsService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddControllers();
builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();


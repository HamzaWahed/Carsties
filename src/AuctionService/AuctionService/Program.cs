using AuctionService;
using AuctionService.Consumers;
using AuctionService.Data;
using AuctionService.Profiles;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAutoMapper(typeof(AuctionProfile).Assembly);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

    x.AddEntityFrameworkOutbox<AppDbContext>(opts =>
    {
        opts.QueryDelay = TimeSpan.FromSeconds(10);
        opts.UsePostgres();
        opts.UseBusOutbox();
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", h =>
        {
            h.Username(builder.Configuration.GetValue("RabbitMq:Username", "user"));
            h.Password(builder.Configuration.GetValue("RabbitMq:Password", "1234"));
        });

        cfg.ConfigureEndpoints(context);
    });
});

var connString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connString));

// ReSharper disable once InvalidXmlDocComment
/**
 * DbContext is a Scoped service, therefore, you should also make the repository a scoped service as well. Using
 * Singleton turns the scope of DbContext to singleton as well (called captive dependency), which will throw an error.
 * The DbContext is no longer thread-safe, as different requests will attempt to write to the same DbContext instance
 * (concurrent writes).
 *
 * Using Transient for repository is thread-safe (unless DbContext is also transient) as the same DbContext is passed
 * to each repository instance in the same HTTP request scope, but it's not the most memory efficient.
 */
builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["IdentityServiceUrl"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.NameClaimType = "username";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

try
{
    DbInitializer.InitDb(app);
}
catch (Exception e)
{
    Console.WriteLine(e);
}

app.Run();
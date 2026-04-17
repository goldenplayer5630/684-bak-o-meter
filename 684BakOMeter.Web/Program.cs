using _684BakOMeter.Web.Data.Persistence;
using _684BakOMeter.Web.Data.Repositories;
using _684BakOMeter.Web.Hubs;
using _684BakOMeter.Web.Infrastructure.IO;
using _684BakOMeter.Web.Infrastructure.Protocol;
using _684BakOMeter.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- EF Core + PostgreSQL ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Repositories ---
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IChugAttemptRepository, ChugAttemptRepository>();
builder.Services.AddScoped<INfcTagRepository, NfcTagRepository>();

// --- Services ---
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<NfcService>();
builder.Services.AddSingleton<CalibrationService>();
builder.Services.AddSingleton<ChugService>();
builder.Services.AddSingleton<NfcScanBridge>();

// --- SignalR ---
builder.Services.AddSignalR();

// --- Serial transport & protocol ---
builder.Services.AddSingleton<ITransport, SerialPortTransport>();
builder.Services.AddSingleton<ProtocolClient>();
builder.Services.AddHostedService<ProtocolHostedService>();

// --- MVC controllers (for the small API used by the Vue app) ---
builder.Services.AddControllers();

// --- Razor Pages ---
builder.Services.AddRazorPages();

var app = builder.Build();

// --- Create database & seed sample data on startup ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();   // apply any pending migrations
    DbSeeder.Seed(db);       // seed sample data if tables are empty
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();   // API endpoints for the Vue app
app.MapRazorPages();    // Razor Pages
app.MapHub<ChugHub>("/hubs/chug"); // SignalR hub for live chug updates

app.Run();

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MonitorMultiplePeersRTC.WebSocketRTC;
using MonitorMultiplePeersRTC.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout as needed
    options.Cookie.HttpOnly = true; // Ensure session cookies are HttpOnly for security
});
builder.WebHost.UseKestrel(options =>
{
    string certificatePath = Path.Combine("C:\\Users\\Abdullah Ashraf\\source\\repos\\MonitorMultiplePeersRTC\\MonitorMultiplePeersRTC\\lastcert", "monitortc.com.pfx");
    string certificatePassword = "%%%%%$$$$$0y1d1o2o3b11A$$$$$%%%%%";

    options.Listen(System.Net.IPAddress.Any, 443, listenOptions =>
    {
        listenOptions.UseHttps(certificatePath, certificatePassword);
    });
    options.Listen(System.Net.IPAddress.Any, 80, listenOptions =>
    {
        listenOptions.UseHttps(certificatePath, certificatePassword);
    });
});
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseStaticFiles();
app.UseSession();
// Enable WebSockets middleware globally
app.UseWebSockets();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Enable WebSockets middleware globally
app.UseWebSockets();

// Handle WebSocket connections only at /webrtcpeer path
app.Map("/webrtcpeers", builder =>
{
    builder.Run(async context =>
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            Console.WriteLine("WebSocket connection established at /webrtcpeers");

            // Delegate the WebSocket connection acceptance and handling to WebSocketHandler
            await WebSocketHandler.AcceptAndHandleWebSocketAsync(context);
        }
        else
        {
            context.Response.StatusCode = 400; // Bad Request if it's not a WebSocket request
        }
    });
});

// Map the default controller route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

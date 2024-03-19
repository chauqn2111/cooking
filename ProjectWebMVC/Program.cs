
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ProjectLibrary.ObjectBussiness;
using ProjectWebMVC.Areas.User;
using ProjectWebMVC.Areas.User.Services;

var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment;


// Add services to the container.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/Admin/Account/Login"; // ???ng d?n ??n trang ??ng nh?p
    options.ReturnUrlParameter = "returnUrl";
}).AddCookie("Admin", options =>
{
    options.LoginPath = new PathString("/Admin/Account/Login");
}).AddCookie("User", options =>
{
    options.LoginPath = new PathString("/Admin/Account/Login");
});
builder.Services.AddDbContext<CookingWebsiteContext>(options =>
{
    options.UseSqlServer("Data Source=DESKTOP-B0D0J2Q\\CHAU92;Initial Catalog=CookingWebsiteDb3;Persist Security Info=True;User ID=sa;Password=chau840848;TrustServerCertificate=true;Trusted_Connection=SSPI;Encrypt=false;");
});


// Configuring services
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

builder.Services.AddScoped<IVnPayService, VnPayService>();

builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddControllersWithViews();


// Configuring app
var app = builder.Build();

if (!environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Map area routes
app.MapAreaControllerRoute(
    name: "admin",
    areaName: "Admin",
    pattern: "Admin/{controller=HomeAdmin}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "user",
    areaName: "User",
    pattern: "User/{controller=Home}/{action=Index}/{id?}");

// Map default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

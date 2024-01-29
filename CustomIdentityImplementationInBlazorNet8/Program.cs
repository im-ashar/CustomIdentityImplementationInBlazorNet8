using CustomIdentityImplementationInBlazorNet8.Components;
using CustomIdentityImplementationInBlazorNet8.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddIdentity<AppUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
builder.Services.AddAuthentication();

var app = builder.Build();

var scope = app.Services.CreateScope();
await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();

await scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>().CreateAsync(new IdentityRole("Admin"));
await scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>().CreateAsync(new IdentityRole("User"));

//Seed Admin User
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
var adminUser = await userManager.FindByNameAsync("admin@admin.com");
if (adminUser == null)
{
    adminUser = new AppUser
    {
        UserName = "admin@admin.com",
        Email = "admin@admin.com"
    };
    await userManager.CreateAsync(adminUser, "Admin@123");
    await userManager.AddToRoleAsync(adminUser, "Admin");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

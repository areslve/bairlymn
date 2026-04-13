using BairlyMN.Data;
using BairlyMN.Data.Entities;
using BairlyMN.Mvc.Hubs;
using BairlyMN.Services.Implementations;
using BairlyMN.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly("BairlyMN.Data")));

// ── Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, BairlyMN.Mvc.Infrastructure.CustomClaimsFactory>();

// ── Cookie paths — MVC AccountController руу, Identity Area руу биш
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ── MVC
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ── SignalR
builder.Services.AddSignalR();

// ── App services
builder.Services.AddScoped<IListingService, ListingService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

// ── Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(30);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.MapHub<ChatHub>("/hubs/chat");

await SeedAsync(app);
app.Run();

// ─── Seed ────────────────────────────────────────────────────────────────────
static async Task SeedAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    await db.Database.MigrateAsync();

    // Roles
    foreach (var role in new[] { "Admin", "User" })
    {
        if (!await roles.RoleExistsAsync(role))
            await roles.CreateAsync(new IdentityRole(role));
    }

    // Admin user
    const string adminEmail = "admin@bairly.mn";
    if (await users.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Админ",
            LastName = "BairlyMN",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await users.CreateAsync(admin, "Admin@12345");
        if (result.Succeeded)
            await users.AddToRoleAsync(admin, "Admin");
    }

    // Categories
    if (!db.Categories.Any())
    {
        db.Categories.AddRange(
            new Category { Name = "Орон сууц", IconClass = "bi bi-building", SortOrder = 1 },
            new Category { Name = "Байшин", IconClass = "bi bi-house-door", SortOrder = 2 },
            new Category { Name = "Газар", IconClass = "bi bi-map", SortOrder = 3 },
            new Category { Name = "Оффис", IconClass = "bi bi-briefcase", SortOrder = 4 },
            new Category { Name = "Агуулах", IconClass = "bi bi-box-seam", SortOrder = 5 },
            new Category { Name = "Бусад", IconClass = "bi bi-three-dots", SortOrder = 6 }
        );

        await db.SaveChangesAsync();
    }

    // Locations — JSON seed
    if (!db.LocationNodes.Any())
    {
        var jsonPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Data", "Seed", "locations.json");

        await BairlyMN.Data.Seeding.LocationSeeder.SeedAsync(db, jsonPath);
    }
}
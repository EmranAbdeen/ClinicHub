using ClinicHub.Models;
using ClinicHub.Services.Implementations;
using ClinicHub.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// CORS
const string AllowEverythingCorsPolicy = "AllowEverything";

//builder.Services.AddOpenApi();

// Add services to the container.
builder.Services.AddControllersWithViews();

// وظيفتها توفير مكان في الذاكرة لتخزين البيانات مؤقتًا
builder.Services.AddDistributedMemoryCache();

// Session
builder.Services.AddSession();

//Database
builder.Services.AddDbContext<ClinicDbContext>(op =>
{
    op.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionClinic"));
});

builder.Services.AddScoped<IAccountService, AccountService>();


builder.Services.AddCors(op =>
{
    op.AddPolicy(AllowEverythingCorsPolicy, policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors(AllowEverythingCorsPolicy);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/Home/NotFoundPage");

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

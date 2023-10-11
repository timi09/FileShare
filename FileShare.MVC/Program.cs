using FileShare.Core.Services;
using FileShare.Infrastructure.Persistence;
using FileShare.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("FilesDb") ?? throw new InvalidOperationException("Connection string not found.");

builder.Services.AddSingleton<IFileRepository>(new MSSQLDatabase(connectionString));
builder.Services.AddSingleton<ILinkRepository>(new MSSQLDatabase(connectionString));
builder.Services.AddSingleton<ILinkGenerator, FileShare.Infrastructure.LinkGenerator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Download}/{action=Index}");

app.Run();



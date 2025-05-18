using FileShare.Data;
using FileShare.Interfaces;
using FileShare.Models;
using FileShare.Providers;
using FileShare.Services;
using FileShare.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Runtime;
using System.Text;

namespace FileShare;
public class Program
{
    public static void Main(string[] args)
    {
        var secretKey = "0123456789abcdef";

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton<ISecretKeyProvider>(new SecretKeyProvider(Encoding.ASCII.GetBytes(secretKey)));

        ConfigureServices(builder.Services, builder.Configuration);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseRequestLocalization();
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        app.Run();
    }

    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddDefaultIdentity<UserModel>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddScoped<ILinkGenerateService, RandomLinkGenerateService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        
        services.Configure<FileStorageSettings>(configuration.GetSection("FileStorageSettings"));

        services.AddLocalization(options => options.ResourcesPath = "Resources");
        
        services.AddControllersWithViews()
            .AddDataAnnotationsLocalization()
            .AddViewLocalization();

        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[]
            {
                    new CultureInfo("ru"),
                    new CultureInfo("en"),
                };

            options.DefaultRequestCulture = new RequestCulture("ru");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
        });
    }
}

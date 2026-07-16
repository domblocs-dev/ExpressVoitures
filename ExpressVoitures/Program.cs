using ExpressVoitures;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews(options =>
{
    options.ModelMetadataDetailsProviders.Add(new FrenchValidationMetadataProvider());

    var p = options.ModelBindingMessageProvider;
    p.SetValueMustNotBeNullAccessor(_ => "Ce champ est obligatoire.");
    p.SetAttemptedValueIsInvalidAccessor((value, field) => $"La valeur « {value} » n'est pas valide pour « {field} ».");
    p.SetValueMustBeANumberAccessor(field => $"Le champ « {field} » doit être un nombre.");
    p.SetMissingBindRequiredValueAccessor(field => $"La valeur du champ « {field} » est manquante.");
    p.SetUnknownValueIsInvalidAccessor(field => $"La valeur fournie pour « {field} » n'est pas valide.");
    p.SetNonPropertyValueMustBeANumberAccessor(() => "La valeur doit être un nombre.");
    options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
});

var app = builder.Build();

var culturesFr = new[] { new CultureInfo("fr-FR") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("fr-FR"),
    SupportedCultures = culturesFr,
    SupportedUICultures = culturesFr
});

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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

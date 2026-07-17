using Microsoft.AspNetCore.Identity;

namespace ExpressVoitures;

public static class IdentitySeeder
{
    public static async Task SeedGerantAsync(IServiceProvider services, IConfiguration config)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        const string role = Roles.Gerant;

        // 1) Créer le rôle s'il n'existe pas
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }

        // 2) Récupérer les identifiants depuis la configuration
        var email = config["Gerant:Email"];
        var motDePasse = config["Gerant:MotDePasse"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(motDePasse))
        {
            // Le mot de passe est un secret : il doit venir des user-secrets ou d'une variable
            // d'environnement (Gerant__MotDePasse). Sans lui, on ne crée pas le compte gérant.
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(
                "Compte gérant non créé : renseignez 'Gerant:MotDePasse' via user-secrets ou la variable "
                + "d'environnement 'Gerant__MotDePasse'. Voir le README.");
            return;
        }

        // 3) Créer le compte du gérant s'il n'existe pas
        var gerant = await userManager.FindByEmailAsync(email);
        if (gerant is null)
        {
            gerant = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true   // pas de service d'email : on confirme d'office
            };
            await userManager.CreateAsync(gerant, motDePasse);
        }

        // 4) L'ajouter au rôle Gérant s'il n'y est pas déjà
        if (!await userManager.IsInRoleAsync(gerant, role))
        {
            await userManager.AddToRoleAsync(gerant, role);
        }
    }
}
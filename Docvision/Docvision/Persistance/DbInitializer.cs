// Fichier : DbInitializer.cs
using Docvision.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Docvision.Persistance
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            Console.WriteLine("=== DÉBUT DE L'INITIALISATION DE LA BASE DE DONNÉES ===");
            try
            {
                Console.WriteLine("Création du scope...");
                using var scope = serviceProvider.CreateScope();
                
                DocContext context = null;
                RoleManager<IdentityRole> roleManager = null;
                UserManager<ApplicationUser> userManager = null;
                
                try
                {
                    Console.WriteLine("Récupération du contexte...");
                    context = scope.ServiceProvider.GetRequiredService<DocContext>();
                    Console.WriteLine("Contexte récupéré avec succès");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de la récupération du contexte: {ex.Message}");
                    throw;
                }
                
                try
                {
                    Console.WriteLine("Récupération du gestionnaire de rôles...");
                    roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                    Console.WriteLine("Gestionnaire de rôles récupéré avec succès");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de la récupération du gestionnaire de rôles: {ex.Message}");
                    throw;
                }
                
                try
                {
                    Console.WriteLine("Récupération du gestionnaire d'utilisateurs...");
                    userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                    Console.WriteLine("Gestionnaire d'utilisateurs récupéré avec succès");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de la récupération du gestionnaire d'utilisateurs: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Exception interne: {ex.InnerException.Message}");
                    }
                    throw;
                }

                Console.WriteLine("Services récupérés avec succès");

                // 1. Créer les rôles
                Console.WriteLine("Début de la vérification des rôles...");
                string[] roles = { "User", "Admin" };
                foreach (var role in roles)
                {
                    try
                    {
                        Console.WriteLine($"Vérification du rôle: {role}");
                        if (!await roleManager.RoleExistsAsync(role))
                        {
                            Console.WriteLine($"Création du rôle: {role}");
                            var result = await roleManager.CreateAsync(new IdentityRole(role));
                            if (!result.Succeeded)
                            {
                                Console.WriteLine($"Échec de la création du rôle {role}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Le rôle {role} existe déjà");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erreur lors de la vérification/création du rôle {role}: {ex.Message}");
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine($"Exception interne: {ex.InnerException.Message}");
                        }
                    }
                }

                // 2. Vérifier si l'admin existe déjà par email ou nom d'utilisateur
                string email = "adocvision.min@gmail.com";
                string username = "administrateur";
                
                Console.WriteLine($"Recherche de l'admin par email: {email}");
                var adminUserByEmail = await userManager.FindByEmailAsync(email);
                Console.WriteLine($"Admin trouvé par email: {adminUserByEmail != null}");
                
                Console.WriteLine($"Recherche de l'admin par nom d'utilisateur: {username}");
                var adminUserByName = await userManager.FindByNameAsync(username);
                Console.WriteLine($"Admin trouvé par nom d'utilisateur: {adminUserByName != null}");
                
                // Ne créer l'admin que s'il n'existe pas déjà (ni par email, ni par nom d'utilisateur)
                if (adminUserByEmail == null && adminUserByName == null)
                {
                    Console.WriteLine("Création d'un nouvel administrateur...");
                    var newUser = new ApplicationUser
                    {
                        UserName = username,
                        Email = email,
                        PhoneNumber = "+21612345678",
                        EmailConfirmed = true,
                        RefreshToken = "" // Initialiser RefreshToken avec une chaîne vide
                    };

                    Console.WriteLine("Tentative de création de l'utilisateur admin...");
                    var createResult = await userManager.CreateAsync(newUser, "Admin12304");
                    if (createResult.Succeeded)
                    {
                        Console.WriteLine("Utilisateur admin créé avec succès, ajout du rôle Admin...");
                        await userManager.AddToRoleAsync(newUser, "Admin");
                        Console.WriteLine("Administrateur créé avec succès.");
                    }
                    else
                    {
                        Console.WriteLine($"Échec de la création de l'admin : {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    Console.WriteLine("L'administrateur existe déjà, aucune action nécessaire.");
                }

                Console.WriteLine("=== FIN DE L'INITIALISATION DE LA BASE DE DONNÉES ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERREUR CRITIQUE lors de l'initialisation de la base de données: {ex.Message}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Exception interne: {ex.InnerException.Message}");
                }
                
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}

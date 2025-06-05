using Docvision.Persistance;
using CloudinaryDotNet;
using Docvision.Models;
using Docvision.Repositories;
using Docvision.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Docvision.Helpers;
using Back.Controllers;
using System.Text.Json.Serialization;
using doc.Controllers;
using Serilog;
using Docvision.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configurer Serilog
//Log.Logger = new LoggerConfiguration()
// .MinimumLevel.Debug() // Niveau de log minimum
// .WriteTo.Console()   // Afficher les logs dans la console
// .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day) // ?crire dans un fichier avec rotation quotidienne
// .CreateLogger();

//builder.Host.UseSerilog(); // Utiliser Serilog comme logger

// Configuration JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

// Ajouter l'authentification JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };

    // Lire le token ? partir du cookie
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["AuthToken"];
            Console.WriteLine($"Token from cookie: {context.Token}"); // Log pour d?boguer
            return Task.CompletedTask;
        }
    };
});

// Ajouter l'autorisation
builder.Services.AddAuthorization();

// Configuration d'Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<DocContext>()
.AddDefaultTokenProviders();

// Add services to the container.
builder.Services.AddDbContext<DocContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("docsdb")));
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddScoped<HttpClient>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
//builder.Services.AddHttpClient(); // Ajouter IHttpClientFactory
builder.Services.AddHttpClient<DocumentController>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(200);
});
builder.Services.AddHttpClient<ImageController>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(200);
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Configuration d'EmailService
var emailSettings = builder.Configuration.GetSection("EmailSettings");
builder.Services.AddScoped<IEmailService, EmailService>(provider =>
    new EmailService(
        emailSettings["SmtpServer"],
        int.Parse(emailSettings["SmtpPort"]),
        emailSettings["EmailSender"],
        emailSettings["Password"]
    )
);


builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddHttpContextAccessor();
// Ajout de COR
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
              .AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.Services.AddMemoryCache();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var cloudinary = new Cloudinary(new Account(
    builder.Configuration["Cloudinary:CloudName"],
    builder.Configuration["Cloudinary:ApiKey"],
    builder.Configuration["Cloudinary:ApiSecret"]
));
builder.Services.AddSingleton(cloudinary);

var app = builder.Build();

app.MapGet("/api/health", () => Results.Ok("API is running"));

// // Appliquer les migrations AVANT d'appeler CreateRoles
// using (var scope = app.Services.CreateScope())
// {
//     var dbContext = scope.ServiceProvider.GetRequiredService<DocContext>();
//     dbContext.Database.Migrate();
// }
async Task CreateRoles(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roleNames = { "User", "Admin" };

    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}

// Créer les rôles par défaut au démarrage
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<DocContext>();
    var skipAdminCreation = Environment.GetEnvironmentVariable("SKIP_ADMIN_CREATION") == "true";
    
    try 
    {
        // Vérifier si la connexion est possible
        if (dbContext.Database.CanConnect())
        {
            Console.WriteLine("Connexion à la base de données réussie");
            // Appliquer les migrations si nécessaire
            dbContext.Database.Migrate();
        }
        else
        {
            Console.WriteLine("Impossible de se connecter à la base de données, création...");
            dbContext.Database.EnsureCreated();
        }
        
        if (!skipAdminCreation)
        {
            await DbInitializer.SeedAsync(services);
        }
        else
        {
            Console.WriteLine("Création de l'administrateur ignorée (SKIP_ADMIN_CREATION=true)");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erreur lors de la migration ou de l'initialisation: {ex.Message}");
        // Continuer l'exécution même en cas d'erreur
    }
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();





app.Run();




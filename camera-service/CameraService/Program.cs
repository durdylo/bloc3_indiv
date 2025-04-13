using CameraService.Models;
using CameraService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure PostgreSQL database
builder.Services.AddDbContext<CameraDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Enregistrer le service RabbitMQ
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Appliquer les migrations automatiquement au démarrage
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CameraDbContext>();
    // Attendre que la base de données soit disponible
    var retryCount = 0;
    const int maxRetries = 10;
    var delay = TimeSpan.FromSeconds(5);

    while (retryCount < maxRetries)
    {
        try
        {
            dbContext.Database.Migrate();
            
            // Initialiser les données si nécessaire (optionnel)
            if (!dbContext.Cameras.Any())
            {
                var cameras = new List<Camera>
                {
                    new Camera { Nom = "Caméra Entrée Principale", Code = "001", EstAfficher = true },
                    new Camera { Nom = "Caméra Hall d'Accueil", Code = "002", EstAfficher = true },
                    new Camera { Nom = "Caméra Parking Nord", Code = "003", EstAfficher = true },
                    new Camera { Nom = "Caméra Parking Sud", Code = "004", EstAfficher = true },
                    new Camera { Nom = "Caméra Couloir A", Code = "005", EstAfficher = true },
                    new Camera { Nom = "Caméra Couloir B", Code = "006", EstAfficher = true },
                    new Camera { Nom = "Caméra Escalier 1", Code = "007", EstAfficher = true },
                    new Camera { Nom = "Caméra Escalier 2", Code = "008", EstAfficher = true },
                    new Camera { Nom = "Caméra Ascenseur", Code = "009", EstAfficher = true },
                    new Camera { Nom = "Caméra Salle de Conférence", Code = "010", EstAfficher = false },
                    new Camera { Nom = "Caméra Bureau Directeur", Code = "011", EstAfficher = false },
                    new Camera { Nom = "Caméra Cafétéria", Code = "012", EstAfficher = true },
                    new Camera { Nom = "Caméra Zone de Stockage", Code = "013", EstAfficher = true },
                    new Camera { Nom = "Caméra Issue de Secours Est", Code = "014", EstAfficher = true },
                    new Camera { Nom = "Caméra Issue de Secours Ouest", Code = "015", EstAfficher = true },
                    new Camera { Nom = "Caméra Extérieure Nord", Code = "016", EstAfficher = true },
                    new Camera { Nom = "Caméra Extérieure Sud", Code = "017", EstAfficher = true },
                    new Camera { Nom = "Caméra Extérieure Est", Code = "018", EstAfficher = true },
                    new Camera { Nom = "Caméra Extérieure Ouest", Code = "019", EstAfficher = true },
                    new Camera { Nom = "Caméra Serveur Informatique", Code = "020", EstAfficher = false }
                };
                
                dbContext.Cameras.AddRange(cameras);
                dbContext.SaveChanges();
                
                Console.WriteLine("Base de données initialisée avec les données de caméras.");
            }
            
            Console.WriteLine("Migrations appliquées avec succès.");
            break;
        }
        catch (Exception ex)
        {
            retryCount++;
            Console.WriteLine($"Tentative {retryCount}/{maxRetries} - Erreur lors de la connexion à la base de données: {ex.Message}");
            
            if (retryCount < maxRetries)
            {
                Console.WriteLine($"Nouvelle tentative dans {delay.TotalSeconds} secondes...");
                Thread.Sleep(delay);
            }
            else
            {
                Console.WriteLine("Impossible de se connecter à la base de données après plusieurs tentatives.");
                throw;
            }
        }
    }
}

app.UseCors("AllowAll");
app.UseRouting();

app.UseAuthorization();
app.MapControllers();

app.Run();
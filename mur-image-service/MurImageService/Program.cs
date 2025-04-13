using MurImageService.Models;
using MurImageService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure PostgreSQL database
builder.Services.AddDbContext<MurImageDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Enregistrer le service consommateur RabbitMQ comme service d'arrière-plan
builder.Services.AddHostedService<RabbitMQConsumerService>();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Appliquer les migrations automatiquement au démarrage
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MurImageDbContext>();
    // Attendre que la base de données soit disponible
    var retryCount = 0;
    const int maxRetries = 10;
    var delay = TimeSpan.FromSeconds(5);

    while (retryCount < maxRetries)
    {
        try
        {
            dbContext.Database.Migrate();
            
            // Initialiser les données si nécessaire
            if (!dbContext.MurImages.Any())
            {
                // Créer les murs d'images
                var murImage1 = new MurImage { Nom = "Mur de Sécurité Principale" };
                var murImage2 = new MurImage { Nom = "Mur de Surveillance Extérieure" };

                // Ajouter les murs d'images
                dbContext.MurImages.AddRange(new List<MurImage> { murImage1, murImage2 });
                dbContext.SaveChanges();

                // Positions pour le premier mur
                var positionsMur1 = new List<Position>
                {
                    new Position { IdMurImage = murImage1.Id, CodeCamera = "001", EstActif = true },
                    new Position { IdMurImage = murImage1.Id, CodeCamera = "002", EstActif = true },
                    new Position { IdMurImage = murImage1.Id, CodeCamera = "003", EstActif = true },
                    new Position { IdMurImage = murImage1.Id, CodeCamera = "005", EstActif = true },
                    new Position { IdMurImage = murImage1.Id, CodeCamera = "006", EstActif = true },
                    new Position { IdMurImage = murImage1.Id, CodeCamera = "007", EstActif = true },
                    new Position { IdMurImage = murImage1.Id, CodeCamera = "009", EstActif = true },
                    new Position { IdMurImage = murImage1.Id, CodeCamera = "010", EstActif = false },
                    new Position { IdMurImage = murImage1.Id, CodeCamera = null, EstActif = true }
                };

                // Positions pour le deuxième mur
                var positionsMur2 = new List<Position>
                {
                    new Position { IdMurImage = murImage2.Id, CodeCamera = "016", EstActif = true },
                    new Position { IdMurImage = murImage2.Id, CodeCamera = "017", EstActif = true },
                    new Position { IdMurImage = murImage2.Id, CodeCamera = "018", EstActif = true },
                    new Position { IdMurImage = murImage2.Id, CodeCamera = "019", EstActif = true },
                    new Position { IdMurImage = murImage2.Id, CodeCamera = "004", EstActif = true },
                    new Position { IdMurImage = murImage2.Id, CodeCamera = "014", EstActif = true },
                    new Position { IdMurImage = murImage2.Id, CodeCamera = "015", EstActif = true },
                    new Position { IdMurImage = murImage2.Id, CodeCamera = null, EstActif = true },
                    new Position { IdMurImage = murImage2.Id, CodeCamera = null, EstActif = true }
                };

                // Ajouter toutes les positions
                dbContext.Positions.AddRange(positionsMur1);
                dbContext.Positions.AddRange(positionsMur2);
                dbContext.SaveChanges();
                
                Console.WriteLine("Base de données initialisée avec les données de murs d'images et positions.");
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
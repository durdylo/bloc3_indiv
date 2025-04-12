using MurImageService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MurImageService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly MurImageDbContext _context;

        public SeedController(MurImageDbContext context)
        {
            _context = context;
        }

        // POST: api/Seed
        [HttpPost]
        public async Task<IActionResult> SeedData()
        {
            // Supprimer les données existantes
            _context.Positions.RemoveRange(_context.Positions);
            _context.MurImages.RemoveRange(_context.MurImages);
            await _context.SaveChangesAsync();

            // Créer les murs d'images
            var murImage1 = new MurImage { Nom = "Mur de Sécurité Principale" };
            var murImage2 = new MurImage { Nom = "Mur de Surveillance Extérieure" };

            // Ajouter les murs d'images
            await _context.MurImages.AddRangeAsync(new List<MurImage> { murImage1, murImage2 });
            await _context.SaveChangesAsync();

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
            await _context.Positions.AddRangeAsync(positionsMur1);
            await _context.Positions.AddRangeAsync(positionsMur2);
            await _context.SaveChangesAsync();

            return Ok("Données de murs d'images et positions initialisées avec succès!");
        }
    }
}
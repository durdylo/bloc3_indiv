using CameraService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CameraService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly CameraDbContext _context;

        public SeedController(CameraDbContext context)
        {
            _context = context;
        }

        // POST: api/Seed
        [HttpPost]
        public async Task<IActionResult> SeedData()
        {
            // Supprimer les données existantes
            _context.Cameras.RemoveRange(_context.Cameras);
            await _context.SaveChangesAsync();

            // Liste de caméras à créer
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

            // Ajouter les caméras
            await _context.Cameras.AddRangeAsync(cameras);
            await _context.SaveChangesAsync();

            return Ok("Données de caméras initialisées avec succès!");
        }
    }
}
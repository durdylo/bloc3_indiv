using CameraService.Models;
using CameraService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CameraService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CamerasController : ControllerBase
    {
        private readonly CameraDbContext _context;
        private readonly IRabbitMQService _rabbitMQService;
        private readonly ILogger<CamerasController> _logger;

        public CamerasController(
            CameraDbContext context, 
            IRabbitMQService rabbitMQService,
            ILogger<CamerasController> logger)
        {
            _context = context;
            _rabbitMQService = rabbitMQService;
            _logger = logger;
        }

        // GET: api/Cameras
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Camera>>> GetCameras()
        {
            return await _context.Cameras.ToListAsync();
        }

        // GET: api/Cameras/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Camera>> GetCamera(int id)
        {
            var camera = await _context.Cameras.FindAsync(id);

            if (camera == null)
            {
                return NotFound();
            }

            return camera;
        }

        // POST: api/Cameras
        [HttpPost]
        public async Task<ActionResult<Camera>> PostCamera(Camera camera)
        {
            _context.Cameras.Add(camera);
            await _context.SaveChangesAsync();

            // Publier un message pour la nouvelle caméra
            await _rabbitMQService.PublishCameraStatusChangeAsync(camera.Code, camera.EstAfficher);

            return CreatedAtAction(nameof(GetCamera), new { id = camera.Id }, camera);
        }

        // PUT: api/Cameras/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCamera(int id, Camera camera)
        {
            if (id != camera.Id)
            {
                return BadRequest();
            }

            // Récupérer l'ancienne valeur pour comparer
            var oldCamera = await _context.Cameras.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            
            _context.Entry(camera).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                
                // Publier un message si l'état d'affichage a changé
                if (oldCamera != null && oldCamera.EstAfficher != camera.EstAfficher)
                {
                    _logger.LogInformation($"État de la caméra {camera.Code} modifié : {camera.EstAfficher}");
                    await _rabbitMQService.PublishCameraStatusChangeAsync(camera.Code, camera.EstAfficher);
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, $"Erreur de concurrence lors de la mise à jour de la caméra {id}");
                
                if (!CameraExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Cameras/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCamera(int id)
        {
            var camera = await _context.Cameras.FindAsync(id);
            if (camera == null)
            {
                return NotFound();
            }

            _context.Cameras.Remove(camera);
            await _context.SaveChangesAsync();
            
            // Publier un message pour indiquer que la caméra est supprimée
            await _rabbitMQService.PublishCameraStatusChangeAsync(camera.Code, false);

            return NoContent();
        }

        private bool CameraExists(int id)
        {
            return _context.Cameras.Any(e => e.Id == id);
        }
    }
}
using MurImageService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MurImageService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MurImagesController : ControllerBase
    {
        private readonly MurImageDbContext _context;

        public MurImagesController(MurImageDbContext context)
        {
            _context = context;
        }

        // GET: api/MurImages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MurImage>>> GetMurImages()
        {
            return await _context.MurImages.Include(m => m.Positions.OrderBy(p => p.Id)).ToListAsync();
        }

        // GET: api/MurImages/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MurImage>> GetMurImage(int id)
        {
            var murImage = await _context.MurImages
                .Include(m => m.Positions)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (murImage == null)
            {
                return NotFound();
            }

            return murImage;
        }

        // POST: api/MurImages
        [HttpPost]
        public async Task<ActionResult<MurImage>> PostMurImage(MurImage murImage)
        {
            _context.MurImages.Add(murImage);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMurImage), new { id = murImage.Id }, murImage);
        }

        // PUT: api/MurImages/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMurImage(int id, MurImage murImage)
        {
            if (id != murImage.Id)
            {
                return BadRequest();
            }

            _context.Entry(murImage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MurImageExists(id))
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

        // DELETE: api/MurImages/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMurImage(int id)
        {
            var murImage = await _context.MurImages.FindAsync(id);
            if (murImage == null)
            {
                return NotFound();
            }

            _context.MurImages.Remove(murImage);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MurImageExists(int id)
        {
            return _context.MurImages.Any(e => e.Id == id);
        }
    }
}
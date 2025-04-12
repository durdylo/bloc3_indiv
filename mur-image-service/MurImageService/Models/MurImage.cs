using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace MurImageService.Models
{
    public class MurImage
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Nom { get; set; }
        
        public ICollection<Position> Positions { get; set; }
    }
}
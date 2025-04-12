using System.ComponentModel.DataAnnotations;

namespace CameraService.Models
{
    public class Camera
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Nom { get; set; }
        
        [Required]
        [StringLength(10)]
        public string Code { get; set; }
        
        public bool EstAfficher { get; set; }
    }
}
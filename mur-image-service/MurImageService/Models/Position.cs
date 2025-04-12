using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MurImageService.Models
{
    public class Position
    {
        [Key]
        public int Id { get; set; }
        
        [ForeignKey("MurImage")]
        public int IdMurImage { get; set; }
        
        public string CodeCamera { get; set; }
        
        [JsonIgnore]
        public MurImage MurImage { get; set; }
    }
}
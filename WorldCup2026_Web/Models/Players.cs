using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorldCup2026_Web.Models
{
    public class Player
    {
        [Key]
        public int Id { get; set; }

        public int Number { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Pos { get; set; } = string.Empty; // GK, DF, MF, FW

        public string DateOfBirth { get; set; } = string.Empty;

        // Takım İlişkisi (Foreign Key - Her oyuncu bir takıma aittir)
        public int TeamId { get; set; }

        [ForeignKey("TeamId")]
        public Team? Team { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace WorldCup2026_Web.Models
{
    public class Anecdote
    {
        public int Id { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty; // Karttaki kısa özet

        [Required]
        public string LongContent { get; set; } = string.Empty; // Yeni eklenen uzun hikaye alanı

        [Required]
        public string ImageUrl { get; set; } = string.Empty;
    }
}
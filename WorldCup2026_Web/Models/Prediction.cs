using System.ComponentModel.DataAnnotations;

namespace WorldCup2026_Web.Models
{
    public class Prediction
    {
        public int Id { get; set; }

        [Required]
        public int MatchId { get; set; }
        public Match? Match { get; set; }

        // Şimdilik üyelik sistemiyle vakit kaybetmemek için tahminleri 
        // kullanıcı adıyla veya tarayıcı session'ı ile eşleştireceğiz
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public int HomeTeamPrediction { get; set; }

        [Required]
        public int AwayTeamPrediction { get; set; }

        // Maç bittikten sonra bu tahminden kazanılan puan buraya işlenecek
        public int EarnedPoints { get; set; } = 0;

        public bool IsCalculated { get; set; } = false;
    }
}

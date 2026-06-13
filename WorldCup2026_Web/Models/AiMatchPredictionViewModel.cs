namespace WorldCup2026_Web.Models
{
    public class AiMatchPredictionViewModel
    {
        public Match? Match { get; set; }
        public int HomeWinPercentage { get; set; }
        public int AwayWinPercentage { get; set; }
        public string? PredictedWinner { get; set; }
    }
}
namespace WorldCup2026_Web.Models
{
    public class BracketResultViewModel
    {
        // Turnuva Ağacı Öncesi Elenen/Geçen Takımlar
        public List<string> RoundOf32Teams { get; set; } = new();

        // Ağaçta (Bracket) Gösterilecek Aşamalar
        public List<string> RoundOf16Teams { get; set; } = new();
        public List<string> QuarterTeams { get; set; } = new();
        public List<string> SemiTeams { get; set; } = new();
        public List<string> FinalTeams { get; set; } = new();
        public string Champion { get; set; } = string.Empty;
        public bool IsSimulated { get; set; } = false;
    }
}
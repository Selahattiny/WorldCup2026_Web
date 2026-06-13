using System.Collections.Generic;
using WorldCup2026_Web.Models;

namespace WorldCup2026_Web.Models
{
    public class TacticalMatchViewModel
    {
        // Kullanıcının seçebileceği tüm takımların listesi
        public List<Team> AllTeams { get; set; } = new List<Team>();

        // Seçilen takımın ID'si
        public int SelectedTeamId { get; set; }

        // Seçilen rakip takımın ID'si
        public int OpponentTeamId { get; set; }
    }

    // Ajax ile mevkileri doldururken kullanacağımız hafif oyuncu şablonu
    public class PlayerSelectionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Number { get; set; }
        public string Pos { get; set; } = string.Empty;
    }
}
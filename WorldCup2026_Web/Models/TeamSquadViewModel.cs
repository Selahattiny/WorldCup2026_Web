using System.Collections.Generic;

namespace WorldCup2026_Web.Models
{
    public class TeamSquadViewModel
    {
        public string TeamName { get; set; } = string.Empty;
        public string FlagUrl { get; set; } = string.Empty;
        public List<PlayerModel> Goalkeepers { get; set; } = new();
        public List<PlayerModel> Defenders { get; set; } = new();
        public List<PlayerModel> Midfielders { get; set; } = new();
        public List<PlayerModel> Forwards { get; set; } = new();
    }

    public class PlayerModel
    {
        public string Name { get; set; } = string.Empty;
        public int Number { get; set; }
        public string Pos { get; set; } = string.Empty; // GK, DF, MF, FW
        public string DateOfBirth { get; set; } = string.Empty; // JSON'daki date_of_birth için
    }
}
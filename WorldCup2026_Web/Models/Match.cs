using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema; // Eklemeyi unutma

namespace WorldCup2026_Web.Models
{
    public class Match
    {
        [JsonProperty("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Otomatik artırma, ID'yi ben vereceğim diyoruz
        public int Id { get; set; }

        [JsonProperty("home_team_id")]
        public int HomeTeamId { get; set; }
        // ... geri kalan kodlar aynı kalacak
        public Team? HomeTeam { get; set; }

        [JsonProperty("away_team_id")]
        public int AwayTeamId { get; set; }
        public Team? AwayTeam { get; set; }

        [JsonProperty("stadium_id")]
        public int StadiumId { get; set; }
        public Stadium? Stadium { get; set; }

        [JsonProperty("local_date")]
        public DateTime MatchDate { get; set; }

        [JsonProperty("type")]
        public string Stage { get; set; } = string.Empty; // group, knockout vb.

        [JsonProperty("home_score")]
        public int? HomeTeamScore { get; set; }

        [JsonProperty("away_score")]
        public int? AwayTeamScore { get; set; }

        [JsonProperty("time_elapsed")]
        public string Status { get; set; } = "notstarted"; // notstarted, finished, inplay
    }
}
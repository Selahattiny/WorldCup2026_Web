using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema; // Bu using satırını eklemeyi unutma!

namespace WorldCup2026_Web.Models
{
    public class Stadium
    {
        [JsonProperty("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Otomatik artırma, ID'yi ben vereceğim diyoruz
        public int Id { get; set; }

        [JsonProperty("name_tr")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("name_en")]
        public string NameEn { get; set; } = string.Empty;

        [JsonProperty("city_tr")]
        public string City { get; set; } = string.Empty;

        [JsonProperty("city_en")]
        public string CityEn { get; set; } = string.Empty;

        [JsonProperty("country_tr")]
        public string Country { get; set; } = string.Empty;

        [JsonProperty("capacity")]
        public int Capacity { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
    }
}

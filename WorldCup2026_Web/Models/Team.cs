using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema; // Eklemeyi unutma

namespace WorldCup2026_Web.Models
{
    public class Team
    {
        [JsonProperty("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Otomatik artırma, ID'yi ben vereceğim diyoruz
        public int Id { get; set; }

        [JsonProperty("name_tr")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("name_en")]
        public string NameEn { get; set; } = string.Empty; // İngilizce Ülke Adı (Örn: Mexico)

        [JsonProperty("groups")]
        public string GroupName { get; set; } = string.Empty; // Grup Harfi (Örn: A)

        [JsonProperty("flag")]
        public string FlagUrl { get; set; } = string.Empty; // Bayrak URL'i

        [JsonProperty("fifa_code")]
        public string FifaCode { get; set; } = string.Empty; // FIFA Kodu (Örn: MEX)
    }
}
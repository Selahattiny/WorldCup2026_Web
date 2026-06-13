using Newtonsoft.Json;
using WorldCup2026_Web.Models;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace WorldCup2026_Web.Data
{
    public static class SeedDataExtensions
    {
        public static void InitializeData(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<AppDbContext>();
                if (context == null) return;

                // 1. STADYUMLARI YÜKLEME
                if (!context.Stadiums.Any())
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "SeedData", "football.stadiums.json");
                    if (File.Exists(filePath))
                    {
                        var jsonData = File.ReadAllText(filePath);
                        var stadiums = JsonConvert.DeserializeObject<List<Stadium>>(jsonData);
                        if (stadiums != null && stadiums.Count > 0)
                        {
                            context.Stadiums.AddRange(stadiums);
                            context.SaveChanges();
                        }
                    }
                }

                // 2. TAKIMLARI YÜKLEME
                if (!context.Teams.Any())
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "SeedData", "football.teams.json");
                    if (File.Exists(filePath))
                    {
                        var jsonData = File.ReadAllText(filePath);
                        var teams = JsonConvert.DeserializeObject<List<Team>>(jsonData);
                        if (teams != null && teams.Count > 0)
                        {
                            context.Teams.AddRange(teams);
                            context.SaveChanges();
                        }
                    }
                }

                // 3. MAÇLARI GÜVENLİ YÜKLEME
                if (!context.Matches.Any())
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "SeedData", "football.matches.json");
                    if (File.Exists(filePath))
                    {
                        var jsonData = File.ReadAllText(filePath);
                        var allMatches = JsonConvert.DeserializeObject<List<Match>>(jsonData);

                        if (allMatches != null && allMatches.Count > 0)
                        {
                            var validTeamIds = context.Teams.Select(t => t.Id).ToHashSet();
                            var validStadiumIds = context.Stadiums.Select(s => s.Id).ToHashSet();

                            var safeMatches = allMatches.Where(m =>
                                validTeamIds.Contains(m.HomeTeamId) &&
                                validTeamIds.Contains(m.AwayTeamId) &&
                                validStadiumIds.Contains(m.StadiumId)).ToList();

                            if (safeMatches.Count > 0)
                            {
                                context.Matches.AddRange(safeMatches);
                                context.SaveChanges();
                            }
                        }
                    }
                }

                // 4. OYUNCULARI VE KADROLARI GÜVENLİ YÜKLEME
                if (context.Players != null && !context.Players.Any())
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "SeedData", "football.players.json");
                    if (File.Exists(filePath))
                    {
                        var jsonData = File.ReadAllText(filePath);

                        // Null gelme ihtimaline karşı burayı ve yardımcı sınıfları tamamen güvenli hale getiriyoruz
                        var jsonTeams = JsonConvert.DeserializeObject<List<JsonTeamData>>(jsonData);

                        if (jsonTeams != null && jsonTeams.Count > 0)
                        {
                            var dbTeams = context.Teams?.ToList() ?? new List<Team>();

                            // Köprü sözlüğümüz (Eşleşmeyi garantiler)
                            var teamNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "Czech Republic", "Çek Cumhuriyeti" },
                                { "United States", "Amerika Birleşik Devletleri" },
                                { "USA", "Amerika Birleşik Devletleri" },
                                { "Germany", "Almanya" },
                                { "Argentina", "Arjantin" },
                                { "Australia", "Avustralya" },
                                { "Austria", "Avusturya" },
                                { "Belgium", "Belçika" },
                                { "Bosnia and Herzegovina", "Bosna-Hersek" },
                                { "Brazil", "Brezilya" },
                                { "Algeria", "Cezayir" },
                                { "Curaçao", "Curaçao" },
                                { "DR Congo", "Demokratik Kongo Cumhuriyeti" },
                                { "Democratic Republic of the Congo", "Demokratik Kongo Cumhuriyeti" },
                                { "Ecuador", "Ekvador" },
                                { "Morocco", "Fas" },
                                { "Ivory Coast", "Fildişi Sahili" },
                                { "France", "Fransa" },
                                { "South Africa", "Güney Afrika" },
                                { "South Korea", "Güney Kore" },
                                { "Haiti", "Haiti" },
                                { "Netherlands", "Hollanda" },
                                { "England", "İngiltere" },
                                { "Scotland", "İskoçya" },
                                { "Spain", "İspanya" },
                                { "Switzerland", "İsviçre" },
                                { "Italy", "İtalya" },
                                { "Canada", "Kanada" },
                                { "Qatar", "Katar" },
                                { "Colombia", "Kolombiya" },
                                { "Mexico", "Meksika" },
                                { "Paraguay", "Paraguay" },
                                { "Portugal", "Portekiz" },
                                { "Turkey", "Türkiye" },
                                { "Uruguay", "Uruguay" },
                                { "Croatia", "Hırvatistan" }
                            };

                            var playersToAdd = new List<Player>();

                            foreach (var jsonTeam in jsonTeams)
                            {
                                if (jsonTeam == null || string.IsNullOrEmpty(jsonTeam.Name)) continue;

                                if (!teamNameMap.TryGetValue(jsonTeam.Name, out string targetName))
                                {
                                    targetName = jsonTeam.Name;
                                }

                                var targetTeam = dbTeams.FirstOrDefault(t =>
                                    t.Name != null && (
                                    t.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase) ||
                                    t.Name.Contains(targetName))
                                );

                                if (targetTeam != null && jsonTeam.Players != null)
                                {
                                    foreach (var jsonPlayer in jsonTeam.Players)
                                    {
                                        if (jsonPlayer == null) continue;

                                        playersToAdd.Add(new Player
                                        {
                                            Name = jsonPlayer.Name ?? "",
                                            Number = jsonPlayer.Number,
                                            Pos = jsonPlayer.Pos ?? "MF",
                                            DateOfBirth = jsonPlayer.DateOfBirth ?? "",
                                            TeamId = targetTeam.Id
                                        });
                                    }
                                }
                            }

                            if (playersToAdd.Count > 0)
                            {
                                context.Players.AddRange(playersToAdd);
                                context.SaveChanges();
                            }
                        }
                    }
                }
            }
        }
    }

    // Newtonsoft.Json'ın tam uyumlu çalışması için sınıfları dosyanın en dış namespace alanına taşıdık
    public class JsonTeamData
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("players")]
        public List<JsonPlayerData>? Players { get; set; }
    }

    public class JsonPlayerData
    {
        [JsonProperty("number")]
        public int Number { get; set; }

        [JsonProperty("pos")]
        public string Pos { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("date_of_birth")]
        public string DateOfBirth { get; set; } = string.Empty;
    }
}
        
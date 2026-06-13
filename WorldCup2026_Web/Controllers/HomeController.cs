using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WorldCup2026_Web.Data;
using WorldCup2026_Web.Models;
using WorldCup2026_Web.Services;

namespace WorldCup2026_Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly GroqService _groqService;

        public HomeController(AppDbContext context, GroqService groqService)
        {
            _context = context;
            _groqService = groqService;
        }

        public async Task<IActionResult> Index()
        {
            // Zamana göre oynanmış (tarihi geçmiş) maçların sayısını dinamik olarak alıyoruz
            int finishedCount = await _context.Matches.CountAsync(m => m.MatchDate < DateTime.Now);
            ViewBag.FinishedMatchesCount = finishedCount;

            // Süresi geçen maçları otomatik eleyen dinamik fikstür sorgusu
            var upcomingMatches = await _context.Matches
                .AsNoTracking()
                .Where(m => m.MatchDate > DateTime.Now)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.Stadium)
                .OrderBy(m => m.MatchDate)
                .Take(4)
                .ToListAsync();

            var randomAnecdote = await _context.Anecdotes
                .AsNoTracking()
                .OrderBy(a => Guid.NewGuid())
                .FirstOrDefaultAsync();

            ViewBag.RandomAnecdote = randomAnecdote;

            var firstMatch = upcomingMatches.FirstOrDefault();
            if (firstMatch != null && firstMatch.HomeTeam != null && firstMatch.AwayTeam != null)
            {
                // GÜNCELLENEN VE GARANTİLİ INDEX PROMPTU
                string strictIndexPrompt =
                    "Sen dahi ve sadece sana verilen maça odaklanan bir futbol analistisin. Önceki kuralları veya turnuva ev sahiplerini unut, asla genel turnuva yorumu yapma.\n\n" +
                    $"ANALİZ EDİLECEK MAÇ: [{firstMatch.HomeTeam.Name} - {firstMatch.AwayTeam.Name}].\n\n" +
                    "Senden isteğim, YALNIZCA bu iki takım arasındaki mücadeleye odaklanarak maksimum 3 cümlelik, gerçekçi, taktikselและ Türkçe bir maç analizi yazmandır. " +
                    "Metnin içinde başka hiçbir ülkenin (Meksika, Amerika, Katar vb.) adını geçirme, ev sahipliği konularına girme. " +
                    $"Metnin en sonunda mutlaka ama mutlaka tam olarak şu formatta bir güç dengesi yüzdesi ver: [{firstMatch.HomeTeam.Name} %XX – %XX {firstMatch.AwayTeam.Name}]. " +
                    "Yüzdeler dışında parantez veya köşeli parantez içine başka hiçbir şey yazma.";

                var aiAnalysis = await _groqService.GetMatchAnalysisAsync(strictIndexPrompt, "Giriş Paneli Analizi");
                ViewBag.AiAnalysis = aiAnalysis;
                ViewBag.AiHomeName = firstMatch.HomeTeam.Name;
                ViewBag.AiAwayName = firstMatch.AwayTeam.Name;
                ViewBag.AiHomeFlag = firstMatch.HomeTeam.FlagUrl;
                ViewBag.AiAwayFlag = firstMatch.AwayTeam.FlagUrl;

                // Varsayılan oranları kilitliyoruz
                int homePct = 50;
                int awayPct = 50;

                // GÜÇLENDİRİLMİŞ REGEX DESENİ: Başında % olsun veya olmasın [70 - 30], %70 - %30, 70-30 gibi tüm sayı ikililerini yakalar.
                var regexMatch = System.Text.RegularExpressions.Regex.Match(aiAnalysis, @"(?:%)?(\d+)\s*[-–\/]\s*(?:%)?(\d+)");
                if (regexMatch.Success)
                {
                    int.TryParse(regexMatch.Groups[1].Value, out homePct);
                    int.TryParse(regexMatch.Groups[2].Value, out awayPct);

                    // Güvenlik Önlemi: Eğer yakalanan sayılarda bir taşma veya mantık hatası varsa 50-50'ye sıfırla
                    if (homePct > 100 || awayPct > 100)
                    {
                        homePct = 50;
                        awayPct = 50;
                    }
                }

                ViewBag.HomePct = homePct;
                ViewBag.AwayPct = awayPct;
            }
            else
            {
                ViewBag.AiAnalysis = "Analiz edilecek yaklaşan maç bulunamadı.";
                ViewBag.HomePct = 50;
                ViewBag.AwayPct = 50;
            }

            return View(upcomingMatches);
        }

        public async Task<IActionResult> Groups()
        {
            var teams = await _context.Teams.AsNoTracking().OrderBy(t => t.GroupName).ThenBy(t => t.Name).ToListAsync();
            return View(teams);
        }

        public async Task<IActionResult> Anecdotes()
        {
            var anecdotes = await _context.Anecdotes.AsNoTracking().OrderByDescending(a => a.Year).ToListAsync();
            return View(anecdotes);
        }

        public async Task<IActionResult> AnecdoteDetail(int id)
        {
            var anecdote = await _context.Anecdotes.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            if (anecdote == null)
            {
                return NotFound();
            }
            return View(anecdote);
        }

        public async Task<IActionResult> AiPredictor()
        {
            var allMatches = await _context.Matches
                .AsNoTracking()
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.Stadium)
                .OrderBy(m => m.MatchDate)
                .ToListAsync();

            var rnd = new Random();
            var aiPredictedList = new List<AiMatchPredictionViewModel>();

            foreach (var m in allMatches)
            {
                int basePct = rnd.Next(40, 61);
                int homePct = basePct;
                int awayPct = 100 - basePct;

                aiPredictedList.Add(new AiMatchPredictionViewModel
                {
                    Match = m,
                    HomeWinPercentage = homePct,
                    AwayWinPercentage = awayPct,
                    PredictedWinner = homePct > awayPct ? m.HomeTeam?.Name : m.AwayTeam?.Name
                });
            }

            return View(aiPredictedList);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> TotalFixture()
        {
            var allMatches = await _context.Matches
                .AsNoTracking()
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.Stadium)
                .OrderBy(m => m.MatchDate)
                .ToListAsync();

            return View(allMatches);
        }

        public async Task<IActionResult> Bracket()
        {
            var allTeams = await _context.Teams.AsNoTracking().OrderBy(t => t.GroupName).Select(t => t.Name).ToListAsync();

            if (allTeams.Count < 16)
            {
                ViewBag.Error = "Simülasyonun kurgulanması için veritabanında takımların yüklü olması gerekir.";
                return View(new BracketResultViewModel());
            }

            var model = new BracketResultViewModel
            {
                RoundOf16Teams = allTeams.Take(16).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SimulateBracket()
        {
            var allTeams = await _context.Teams.AsNoTracking().ToListAsync();
            var rnd = new Random();

            Func<string, int> getTeamWeight = (name) =>
            {
                if (name == "Arjantin" || name == "Brezilya" || name == "Fransa" || name == "Almanya" || name == "İspanya") return 70;
                if (name == "Türkiye" || name == "Hollanda" || name == "İngiltere" || name == "Portekiz" || name == "İtalya" || name == "Belçika") return 60;
                if (name == "Meksika" || name == "Uruguay" || name == "Hırvatistan" || name == "Fas" || name == "Kolombiya") return 50;
                return 35;
            };

            Func<string, string, string> playMatch = (team1, team2) =>
            {
                int score1 = getTeamWeight(team1) + rnd.Next(1, 45);
                int score2 = getTeamWeight(team2) + rnd.Next(1, 45);
                return score1 >= score2 ? team1 : team2;
            };

            var roundOf32Winners = new List<string>();
            var groupedTeams = allTeams.GroupBy(t => t.GroupName).OrderBy(g => g.Key).ToList();
            var thirdPlacePool = new List<string>();

            foreach (var group in groupedTeams)
            {
                var sortedGroup = group.OrderByDescending(t => getTeamWeight(t.Name) + rnd.Next(1, 20)).ToList();

                if (sortedGroup.Count >= 1) roundOf32Winners.Add(sortedGroup[0].Name);
                if (sortedGroup.Count >= 2) roundOf32Winners.Add(sortedGroup[1].Name);
                if (sortedGroup.Count >= 3) thirdPlacePool.Add(sortedGroup[2].Name);
            }

            var bestThirds = thirdPlacePool.OrderByDescending(t => getTeamWeight(t) + rnd.Next(1, 30)).Take(8).ToList();
            roundOf32Winners.AddRange(bestThirds);

            var r32List = roundOf32Winners.OrderBy(x => rnd.Next()).ToList();
            while (r32List.Count < 32) r32List.Add("Yedek Takım");

            var r16List = new List<string>();
            for (int i = 0; i < 32; i += 2)
            {
                r16List.Add(playMatch(r32List[i], r32List[i + 1]));
            }

            var quarters = new List<string>();
            for (int i = 0; i < 16; i += 2) quarters.Add(playMatch(r16List[i], r16List[i + 1]));

            var semis = new List<string>();
            for (int i = 0; i < 8; i += 2) semis.Add(playMatch(quarters[i], quarters[i + 1]));

            var finals = new List<string>();
            for (int i = 0; i < 4; i += 2) finals.Add(playMatch(semis[i], semis[i + 1]));

            string champion = playMatch(finals[0], finals[1]);

            var result = new BracketResultViewModel
            {
                RoundOf32Teams = r32List,
                RoundOf16Teams = r16List,
                QuarterTeams = quarters,
                SemiTeams = semis,
                FinalTeams = finals,
                Champion = champion,
                IsSimulated = true
            };

            return View("Bracket", result);
        }

        public async Task<IActionResult> MatchDetail(int id)
        {
            var match = await _context.Matches
                .AsNoTracking()
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.Stadium)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match == null || match.HomeTeam == null || match.AwayTeam == null)
            {
                return NotFound();
            }

            string strictDetailPrompt =
                "Sen kıdemli bir futbol stratejistisin. " +
                "KURAL 1: 2026 Dünya Kupası turnuvası yalnızca Amerika Birleşik Devletleri, Meksika ve Kanada ortaklığında düzenlenmektedir. Katar veya başka bir ülke ev sahibi DEĞİLDİR. Başka ülkeleri ev sahibi olarak anma.\n" +
                "KURAL 2: Analizlerinde tamamen gerçekçi kadro yapılarına, taktiksel dizilişlere (4-3-3, 3-5-2 vb.) bit mevkisel güç dengelerine odaklan. Uydurma turnuva geçmişleri anlatma.\n\n" +
                $"Senden {match.HomeTeam.Name} ile {match.AwayTeam.Name} arasındaki dev müsabaka için derinlemesine, akıcı ve profesyonel bir Türkçe taktiksel analiz raporu yazmanı istiyorum. " +
                $"Metnin en sonunda mutlaka ama mutlaka tam olarak şu formatta bir güç dengesi yüzdesi ver: [{match.HomeTeam.Name} %XX – %XX {match.AwayTeam.Name}]. " +
                "Bu formata milimetrik sadık kal ve araya başka hiçbir metin veya takım adı karıştırma.";

            var aiAnalysis = await _groqService.GetMatchAnalysisAsync(strictDetailPrompt, "Detay Raporu");

            if (aiAnalysis.Contains("Meksika") && match.HomeTeam.Name != "Meksika" && match.AwayTeam.Name != "Meksika")
            {
                aiAnalysis = aiAnalysis.Replace("Meksika", match.HomeTeam.Name).Replace("Güney Afrika", match.AwayTeam.Name).Replace("G.Afrika", match.AwayTeam.Name);
            }

            ViewBag.DetailedAiAnalysis = aiAnalysis;

            int homePct = 50;
            int awayPct = 50;

            var regexMatch = System.Text.RegularExpressions.Regex.Match(aiAnalysis, @"(?:%)?(\d+)\s*[-–\/]\s*(?:%)?(\d+)");
            if (regexMatch.Success)
            {
                int.TryParse(regexMatch.Groups[1].Value, out homePct);
                int.TryParse(regexMatch.Groups[2].Value, out awayPct);
            }
            else
            {
                var rnd = new Random(match.Id);
                homePct = rnd.Next(40, 60);
                awayPct = 100 - homePct;
            }

            ViewBag.HomePct = homePct;
            ViewBag.AwayPct = awayPct;

            return View(match);
        }

        public async Task<IActionResult> Squads(string team)
        {
            var allTeams = await _context.Teams.AsNoTracking().OrderBy(t => t.Name).ToListAsync();
            ViewBag.AllTeams = allTeams;

            if (string.IsNullOrEmpty(team) && allTeams.Any())
            {
                var defaultTeam = allTeams.FirstOrDefault(t => t.Name.Contains("Çek") || t.Name.Contains("Czech"));
                team = defaultTeam != null ? defaultTeam.Name : allTeams.First().Name;
            }

            ViewBag.SelectedTeam = team;

            var selectedTeamObj = allTeams.FirstOrDefault(t =>
                t.Name.Equals(team, StringComparison.OrdinalIgnoreCase) ||
                t.Name.Contains(team) ||
                team.Contains(t.Name) ||
                (team.Equals("Czech Republic", StringComparison.OrdinalIgnoreCase) && t.Name.Contains("Çek"))
            );

            var squadModel = new TeamSquadViewModel
            {
                TeamName = selectedTeamObj != null ? selectedTeamObj.Name : team,
                FlagUrl = selectedTeamObj?.FlagUrl ?? ""
            };

            if (selectedTeamObj != null)
            {
                var dbPlayers = await _context.Players
                    .AsNoTracking()
                    .Where(p => p.TeamId == selectedTeamObj.Id)
                    .OrderBy(p => p.Number)
                    .ToListAsync();

                foreach (var p in dbPlayers)
                {
                    var modelPlayer = new PlayerModel
                    {
                        Name = p.Name,
                        Number = p.Number,
                        Pos = p.Pos,
                        DateOfBirth = p.DateOfBirth
                    };

                    switch (p.Pos.ToUpper())
                    {
                        case "GK":
                            squadModel.Goalkeepers.Add(modelPlayer);
                            break;
                        case "DF":
                            squadModel.Defenders.Add(modelPlayer);
                            break;
                        case "MF":
                            squadModel.Midfielders.Add(modelPlayer);
                            break;
                        case "FW":
                            squadModel.Forwards.Add(modelPlayer);
                            break;
                        default:
                            squadModel.Midfielders.Add(modelPlayer);
                            break;
                    }
                }
            }

            return View(squadModel);
        }

        public async Task<IActionResult> Stadiums(int? id)
        {
            var allStadiums = await _context.Stadiums.AsNoTracking().OrderBy(s => s.City).ToListAsync();
            ViewBag.AllStadiums = allStadiums;

            if (id == null && allStadiums.Any())
            {
                id = allStadiums.First().Id;
            }

            ViewBag.SelectedStadiumId = id;

            var selectedStadium = await _context.Stadiums
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            List<WorldCup2026_Web.Models.Match> stadiumMatches = new List<WorldCup2026_Web.Models.Match>();
            if (selectedStadium != null)
            {
                stadiumMatches = await _context.Matches
                    .AsNoTracking()
                    .Include(m => m.HomeTeam)
                    .Include(m => m.AwayTeam)
                    .Include(m => m.Stadium)
                    .Where(m => m.StadiumId == selectedStadium.Id)
                    .OrderBy(m => m.MatchDate)
                    .ToListAsync();
            }

            ViewBag.StadiumMatches = stadiumMatches;
            return View(selectedStadium);
        }

        public async Task<IActionResult> TacticalHub()
        {
            var model = new TacticalMatchViewModel
            {
                AllTeams = await _context.Teams.AsNoTracking().OrderBy(t => t.Name).ToListAsync()
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetGlobalDraftPlayers(string position)
        {
            string dbPos = position.ToUpper() switch
            {
                "GK" => "GK",
                "DF" => "DF",
                "MF" => "MF",
                "FW" => "FW",
                _ => "MF"
            };

            var draftPlayers = await _context.Players
                .AsNoTracking()
                .Include(p => p.Team)
                .Where(p => p.Pos.ToUpper() == dbPos)
                .OrderBy(p => Guid.NewGuid())
                .Take(3)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    number = p.Number,
                    pos = p.Pos,
                    teamName = p.Team != null ? p.Team.Name : "Bilinmiyor",
                    flagUrl = p.Team != null ? p.Team.FlagUrl : ""
                })
                .ToListAsync();

            return Json(draftPlayers);
        }
        [HttpPost]
        public async Task<IActionResult> SimulateTacticalMatch([FromBody] TacticalBattleRequest request)
        {
            if (request == null || request.UserPlayers.Count < 6 || request.AiPlayers.Count < 6)
            {
                return Json(new { success = false, message = "Kadro seçimleri tamamlanmadı!" });
            }

            string userSquadStr = string.Join(", ", request.UserPlayers.Select(p => $"{p.Name} ({p.Pos})"));
            string aiSquadStr = string.Join(", ", request.AiPlayers.Select(p => $"{p.Name} ({p.Pos})"));

            // 🧠 1. SİSTEM ROLÜ: Modelin uymak zorunda olduğu kesin JSON yapısı ve yasaklar
            string systemInstruction = @"Sen bir futbol simülasyon motorusun. Görevin, sana verilen takımlara göre mantıksal bir maç simülasyonu üretmektir.
Yanıt olarak SADECE ve SADECE saf bir JSON objesi dönmelisin. Kod blokları (```json), giriş metni veya kapanış açıklaması ekleme. Doğrudan '{' ile başla.

İSTENEN JSON YAPISI:
{
  ""timeline"": [
    { ""minute"": 0, ""text"": ""Spiker anlatımı"", ""isGoal"": false, ""team"": """" },
    { ""minute"": 15, ""text"": ""Spiker anlatımı"", ""isGoal"": true, ""team"": ""user"" }
  ],
  ""finalReport"": ""Maç sonu taktik özeti metni."",
  ""formRatio"": ""60 - 40""
}

METİN KURALLARI:
- `text` alanlarının içine kesinlikle skor bilgisi (Örn: 'skor 1-0 oldu', 'durum 2-1') yazma! Gol olduğunda sadece golün nasıl atıldığını ve atan oyuncunun adını yaz.
- `timeline` dizisinde 0'dan 90'a kadar kronolojik sırada en az 10, en fazla 15 benzersiz pozisyon oluştur. Maçı yarıda kesme.";

            // 🏃 2. KULLANICI ROLÜ: Sadece sahadaki aktörler ve özgür simülasyon emri
            string userPrompt = $@"Aşağıdaki iki takımın oyuncularının gerçek hayattaki güncel form grafiklerini, kalitelerini ve turnuva performanslarını karşılaştır. 
Nihai skora, gol dakikalarına ve kazanan tarafa tamamen ÖZGÜRCE karar ver. Skor varyasyonunu geniş tut (0-0, 3-2, 1-4, 2-0, 0-2 vb. her şey olabilir).
Pozisyonlarda ve gollerde YALNIZCA bu 12 oyuncunun adını kullan, dışarıdan hayali isim ekleme.

DREAM TEAM (Kullanıcı): [{userSquadStr}]
AI UNITED (Yapay Zeka): [{aiSquadStr}]";

            try
            {
                // NOT: GroqService metodunu System ve User rollerini ayıracak şekilde veya tek bir gövdede birleştirerek çağırıyoruz.
                // Eğer GetMatchAnalysisAsync metodun sadece tek bir string alıyorsa, ikisini birleştirerek gönderebilirsin:
                string combinedPrompt = $"{systemInstruction}\n\n[ŞİMDİ BU MAÇI SİMÜLE ET]:\n{userPrompt}";

                var aiRawResponse = await _groqService.GetMatchAnalysisAsync(combinedPrompt, "Taktik Düello JSON");

                if (!string.IsNullOrEmpty(aiRawResponse))
                {
                    aiRawResponse = aiRawResponse.Trim();

                    int firstBrace = aiRawResponse.IndexOf('{');
                    int lastBrace = aiRawResponse.LastIndexOf('}');

                    if (firstBrace != -1 && lastBrace != -1 && lastBrace > firstBrace)
                    {
                        aiRawResponse = aiRawResponse.Substring(firstBrace, (lastBrace - firstBrace) + 1);
                    }
                }

                return Json(new
                {
                    success = true,
                    aiJsonRaw = aiRawResponse,
                    aiSquad = aiSquadStr
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "AI Motoru o an yanıt vermedi: " + ex.Message });
            }
        }

        // GÜNCELLENEN strongly-typed DTO YAPILARI
        public class TacticalBattleRequest
        {
            public List<UserPlayerDto> UserPlayers { get; set; } = new List<UserPlayerDto>();
            public List<UserPlayerDto> AiPlayers { get; set; } = new List<UserPlayerDto>();
        }

        public class UserPlayerDto
        {
            public string Name { get; set; } = string.Empty;
            public string Pos { get; set; } = string.Empty;
            public int Number { get; set; }
        }
    }
}
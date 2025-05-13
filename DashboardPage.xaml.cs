using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TeamManagerContext;

namespace LoL_eSport_Team_Mangager
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        private readonly string _username;

        public DashboardPage(string username)
        {
            InitializeComponent();
            _username = username;
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            using (var context = new cnTeamManager.TeamManagerContext())
            {
                // === Edzői adatok kártya ===
                var coach = context.Users
                    .FirstOrDefault(u => u.Username.ToLower() == _username.ToLower());

                if (coach == null)
                {
                    SetUnknownCoach();
                    return;
                }

                var team = context.Teams.FirstOrDefault(t => t.CoachId == coach.UserId);
                if (team == null)
                {
                    SetUnknownCoach();
                    return;
                }

                CoachNameText.Text = $"Név: {coach.Username}";
                TeamNameText.Text = $"Csapat: {team.Name}";

                var matchCount = context.Matches.Count(m => m.TeamId == team.Id);
                MatchCountText.Text = $"Lejátszott meccsek: {matchCount}";

                var playerIds = context.Players
                    .Where(p => p.TeamId == team.Id)
                    .Select(p => p.Id)
                    .ToList();

                var allStats = context.PlayerStats.ToList();

                var averageKDA = allStats
                    .Where(ps => playerIds.Contains(ps.PlayerId))
                    .Select(ps => ps.KDA ?? 0)
                    .DefaultIfEmpty(0)
                    .Average();

                AverageKdaText.Text = $"Csapat átlagos KDA: {Math.Round(averageKDA, 2)}";

                // === Következő meccs kártya ===
                var upcomingMatch = context.Matches
                .Where(m => m.TeamId == team.Id && m.Date > DateTime.Now)
                .OrderBy(m => m.Date)
                .Select(m => new
                {
                    m.Id,
                    m.Date,
                    m.OpponentId
                })
                .FirstOrDefault();

                if (upcomingMatch != null)
                {
                    var opponent = context.Teams.FirstOrDefault(t => t.Id == upcomingMatch.OpponentId);
                    NextMatchOpponentText.Text = $"Ellenfél: {opponent?.Name ?? "ismeretlen"}";
                    NextMatchDateText.Text = $"Időpont: {upcomingMatch.Date:yyyy. MM. dd. HH:mm}";

                    var playerIdsForTeam = context.Players
                        .Where(p => p.TeamId == team.Id)
                        .Select(p => p.Id)
                        .ToList();

                    var statsForNextMatch = context.PlayerStats
                        .Where(ps => ps.MatchId == upcomingMatch.Id && playerIdsForTeam.Contains(ps.PlayerId))
                        .ToList();
                }
                else
                {
                    NextMatchOpponentText.Text = "Ellenfél: ismeretlen";
                    NextMatchDateText.Text = "Időpont: ismeretlen";
                    NextMatchHighlightPlayerText.Text = "Kiemelt játékos: ismeretlen";
                }

                // === Játékosok átlagos KDA-ja kártya ===
                KdaListPanel.Children.Clear();

                var playersInTeam = context.Players
                    .Where(p => p.TeamId == team.Id && p.IsPlayerActiveInThisTeam == true)
                    .Select(p => new { p.Id, p.Name })
                    .ToList();

                foreach (var player in playersInTeam)
                {
                    var kdaValues = context.PlayerStats
                        .Where(ps => ps.PlayerId == player.Id && ps.KDA.HasValue)
                        .Select(ps => ps.KDA.Value)
                        .ToList();

                    string kdaText;

                    if (kdaValues.Count > 0)
                    {
                        var avgKda = kdaValues.Average();
                        kdaText = $"KDA: {Math.Round(avgKda, 2)}";
                    }
                    else
                    {
                        kdaText = "KDA: ismeretlen";
                    }

                    var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 0) };
                    row.Children.Add(new TextBlock
                    {
                        Text = player.Name,
                        Width = 150,
                        FontSize = 10,
                        Foreground = (Brush)FindResource("LoLTextBrush")
                    });
                    row.Children.Add(new TextBlock
                    {
                        Text = kdaText,
                        Foreground = Brushes.LimeGreen,
                        FontSize = 10

                    });

                    KdaListPanel.Children.Add(row);
                }

                // === Legutóbbi meccs statisztika kártya ===
                LastMatchInfoText.Text = "Meccs: ismeretlen";
                LastMatchStats1.Text = "";
                LastMatchStats2.Text = "";
                LastMatchStats3.Text = "";

                var lastMatch = context.Matches
                    .Where(m => m.TeamId == team.Id && m.Date <= DateTime.Now)
                    .OrderByDescending(m => m.Date)
                    .FirstOrDefault();

                if (lastMatch != null)
                {
                    var opponent = context.Teams.FirstOrDefault(t => t.Id == lastMatch.OpponentId);
                    LastMatchInfoText.Text = $"Meccs #{lastMatch.Id} - {team.Name} vs {opponent?.Name ?? "ismeretlen"}";

                    var playerStats = context.PlayerStats
                        .Where(ps => ps.MatchId == lastMatch.Id)
                        .Join(context.Players.Where(p => p.TeamId == team.Id),
                              ps => ps.PlayerId,
                              p => p.Id,
                              (ps, p) => new { p.Name, ps.Score, ps.Form })
                        .ToList();

                    if (playerStats.Count >= 1)
                        LastMatchStats1.Text = $"{playerStats[0].Name}: Score: {playerStats[0].Score}, Form: {playerStats[0].Form}";
                    if (playerStats.Count >= 2)
                        LastMatchStats2.Text = $"{playerStats[1].Name}: Score: {playerStats[1].Score}, Form: {playerStats[1].Form}";
                    if (playerStats.Count >= 3)
                        LastMatchStats3.Text = $"{playerStats[2].Name}: Score: {playerStats[2].Score}, Form: {playerStats[2].Form}";
                }
                else
                {
                    LastMatchInfoText.Text = "Nincs lejátszott meccs";
                }

                // === Legjobb játékos kártya ===
                BestPlayerNameText.Text = "ismeretlen";
                BestPlayerStatsText.Text = "Statisztika: ismeretlen";
                BestPlayerNoteText.Text = "Megjegyzés: -";

                if (lastMatch != null)
                {
                    var bestPlayer = context.PlayerStats
                        .Where(ps => ps.MatchId == lastMatch.Id)
                        .Join(context.Players.Where(p => p.TeamId == team.Id),
                              ps => ps.PlayerId,
                              p => p.Id,
                              (ps, p) => new { p.Name, ps.Score, ps.Form })
                        .OrderByDescending(p => p.Score)
                        .FirstOrDefault();

                    if (bestPlayer != null)
                    {
                        BestPlayerNameText.Text = bestPlayer.Name;
                        BestPlayerStatsText.Text = $"Statisztika: Score: {bestPlayer.Score}, Form: {bestPlayer.Form}";
                        BestPlayerNoteText.Text = "Megjegyzés: MVP teljesítmény!";
                    }
                }

                // === Csapat statisztikák kártya ===
                var rawMatches = context.Matches
                    .Where(m => m.TeamId == team.Id && m.Result != null)
                    .ToList();

                var winCount = rawMatches.Count(r => r.Result == "Win");
                var totalPlayed = rawMatches.Count;
                var winRate = totalPlayed > 0 ? (double)winCount / totalPlayed * 100 : 0;

                WinRateText.Text = $"Átlagos győzelmi arány: {Math.Round(winRate, 2)}%";
            }
        }

        private void SetUnknownCoach()
        {
            CoachNameText.Text = "Név: ismeretlen";
            TeamNameText.Text = "Csapat: ismeretlen";
            MatchCountText.Text = "Lejátszott meccsek: 0";
            AverageKdaText.Text = "Csapat átlagos KDA: 0";

            NextMatchOpponentText.Text = "Ellenfél: ismeretlen";
            NextMatchDateText.Text = "Időpont: ismeretlen";
            NextMatchHighlightPlayerText.Text = "Kiemelt játékos: ismeretlen";
        }
    }
}

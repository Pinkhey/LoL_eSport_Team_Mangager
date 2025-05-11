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

                // === Következő meccs ===
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
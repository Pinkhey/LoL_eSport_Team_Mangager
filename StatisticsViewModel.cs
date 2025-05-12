using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LoL_eSport_Team_Manager;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.Collections.ObjectModel;
using cnTeamManager;
using LiveChartsCore.SkiaSharpView.WPF;
using LiveChartsCore.SkiaSharpView.SKCharts;

namespace LoL_eSport_Team_Mangager
{
    public class PlayerStatDisplay
    {
        public string Name { get; set; }
        public string Role { get; set; }
        public double? KDA { get; set; }
        public int? Score { get; set; }
        public int? Form { get; set; }
    }

    public class StatisticsViewModel
    {
        public List<PlayerStatDisplay> PlayerStatsList { get; set; }
        public IEnumerable<ISeries> KdaColumnSeries { get; set; }
        public Axis[] KdaXAxis { get; set; }
        public IEnumerable<ISeries> FormSeries { get; set; }
        public IEnumerable<ISeries> WinLossSeries { get; set; }

        public StatisticsViewModel(cnTeamManager.TeamManagerContext context, int teamId)
        {
            var players = context.Players
                .Where(p => teamId == 0 || p.TeamId == teamId)  // Admin = minden csapat
                .ToList();

            // Táblázatos adatok
            PlayerStatsList = players.Select(p =>
            {
                var stats = context.PlayerStats.Where(s => s.PlayerId == p.Id).ToList();
                return new PlayerStatDisplay
                {
                    Name = p.Name,
                    Role = p.Role,
                    KDA = stats.Count > 0 ? stats.Average(s => s.KDA) : 0,
                    Score = stats.Count > 0 ? (int?)stats.Sum(s => s.Score) : 0,
                    Form = stats.Count > 0 ? stats.Last().Form : 0
                };
            }).ToList();

            // Oszlopdiagram: Átlagos KDA
            KdaColumnSeries = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = PlayerStatsList.Select(p => p.KDA ?? 0).ToList()
                }
            };

            KdaXAxis = new[]
            {
                new Axis
                {
                    Labels = PlayerStatsList.Select(p => p.Name).ToArray(),
                    LabelsRotation = 15
                }
            };

            // Vonaldiagram: Forma változás játékosonként
            FormSeries = players.Select(p =>
            {
                IReadOnlyCollection<double> formData = context.PlayerStats
                    .Where(s => s.PlayerId == p.Id)
                    .OrderBy(s => s.MatchId)
                    .Select(s => (double?)s.Form)
                    .Where(f => f.HasValue)
                    .Cast<double>()
                    .ToList();

                return new LineSeries<double>
                {
                    Name = p.Name,
                    Values = formData,
                    GeometrySize = 8
                };
            }).ToList();

            // Kördiagram: Win/Loss arány
            var matchResults = context.Matches
                .Where(m => teamId == 0 || m.TeamId == teamId)
                .Select(m => m.Result.ToLower())
                .ToList();

            var winCount = matchResults.Count(r => r == "win");
            var lossCount = matchResults.Count(r => r == "loss");

            WinLossSeries = new ISeries[]
            {
                new PieSeries<double> { Values = new[] { (double)winCount }, Name = "Win" },
                new PieSeries<double> { Values = new[] { (double)lossCount }, Name = "Loss" }
            };
        }
    }
}

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
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace LoL_eSport_Team_Mangager
{
    public partial class StatisticsPage : Page
    {
        public ObservableCollection<ISeries> KdaColumnSeries { get; set; }
        public Axis[] KdaXAxis { get; set; }
        public Axis[] KdaYAxis { get; set; }

        public ObservableCollection<ISeries> FormSeries { get; set; }
        public Axis[] FormXAxis { get; set; }
        public Axis[] FormYAxis { get; set; }

        public ObservableCollection<ISeries> WinLossSeries { get; set; }

        public int? TeamId { get; set; }
        public bool IsAdmin { get; set; }

        public StatisticsPage(int? teamId, bool isAdmin = false)
        {
            InitializeComponent();
            TeamId = teamId;
            IsAdmin = isAdmin;

            Logger.Log($"StatisticsPage megnyitva - Admin: {IsAdmin}, TeamId: {TeamId}", "INFO", "StatisticsPage");

            if (IsAdmin)
            {
                TeamSelector.Visibility = Visibility.Visible;
                LoadTeams();
            }

            LoadStatistics();
        }

        private void LoadTeams()
        {
            try
            {
                using var context = new cnTeamManager.TeamManagerContext();
                var teams = context.Teams
                                   .Select(t => new { t.Id, t.Name })
                                   .ToList();
                TeamSelector.ItemsSource = teams;
                TeamSelector.DisplayMemberPath = "Name";
                TeamSelector.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba a csapatok betöltésekor: {ex.Message}");
            }
        }

        private void TeamSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TeamSelector.SelectedValue is int selectedId)
            {
                TeamId = selectedId;
                LoadStatistics();
            }
        }

        private void LoadStatistics()
        {
            if (TeamId == null) return;

            try
            {
                using var context = new cnTeamManager.TeamManagerContext();

                // Játékos statisztikák betöltése
                var stats = (from player in context.Players
                             where player.TeamId == TeamId && player.IsPlayerActiveInThisTeam == true
                             join stat in context.PlayerStats on player.Id equals stat.PlayerId
                             join match in context.Matches on stat.MatchId equals match.Id
                             select new
                             {
                                 Name = player.Name,
                                 Role = player.Role,
                                 KDA = stat.KDA,
                                 Score = stat.Score,
                                 Form = stat.Form,
                                 MatchId = match.Id
                             })
                             .ToList();

                StatsDataGrid.ItemsSource = stats;

                // Átlagos KDA oszlopdiagram
                var avgKdas = stats
                    .GroupBy(s => s.Name)
                    .Select(g => new { Name = g.Key, AvgKDA = g.Average(s => s.KDA) ?? 0 })
                    .ToList();

                KdaColumnSeries = new ObservableCollection<ISeries>
        {
            new ColumnSeries<double>
            {
                Values = avgKdas.Select(x => x.AvgKDA).ToArray(),
                Name = "KDA",
                Fill = new SolidColorPaint(SKColor.Parse("#C89B3C"))

            }



        };

                KdaXAxis = new[]
                {
            new Axis
            {
                Labels = avgKdas.Select(x => x.Name).ToArray(),
                LabelsPaint = new SolidColorPaint(SKColor.Parse("#C89B3C")),
                // Betűszín nem lett meghatározva, az alapértelmezett lesz
            }
        };

                KdaYAxis = new[]
                {
            new Axis
            {
                // Betűszín nem lett meghatározva, az alapértelmezett lesz
            }
        };

                // Forma változás diagram
                FormSeries = new ObservableCollection<ISeries>();
                var groupedForm = stats.GroupBy(s => s.Name);
                foreach (var group in groupedForm)
                {
                    FormSeries.Add(new LineSeries<double>
                    {
                        Name = group.Key,
                        Values = group.Select(s => (double)s.Form).ToArray()
                    });
                }

                FormXAxis = new[]
                {
            new Axis
            {
               LabelsPaint = new SolidColorPaint(SKColor.Parse("#C89B3C")),
            }
        };

                FormYAxis = new[]
                {
            new Axis
            {
                LabelsPaint = new SolidColorPaint(SKColor.Parse("#C89B3C")),
            }
        };

                // Win/Loss diagram
                int wins = context.Matches.Count(m => m.TeamId == TeamId && m.Result == "Win");
                int losses = context.Matches.Count(m => m.TeamId == TeamId && m.Result == "Lose");

                WinLossSeries = new ObservableCollection<ISeries>
        {
            new PieSeries<double> { Values = new[] { (double)wins }, Name = "Win" },
            new PieSeries<double> { Values = new[] { (double)losses }, Name = "Lose" }
        };

                // DataContext frissítése
                DataContext = null;
                DataContext = this;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba a statisztikák betöltésekor: {ex.Message}");
            }
        }
    }
}
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TeamManagerContext;

namespace LoL_eSport_Team_Mangager
{
    public partial class MatchesPage : Page, INotifyPropertyChanged
    {
        public int? UserId { get; set; }
        public bool IsAdmin { get; set; }

        private List<Teams> allTeams;

        public ObservableCollection<MatchResult> Matches { get; set; } = new();

        private List<Matches> upcomingMatchesToDelete = new();

        public MatchesPage(int? userId, bool isAdmin = false)
        {
            InitializeComponent();
            UserId = userId;
            IsAdmin = isAdmin;
            DataContext = this;

            LoadMatchesFromDatabase();
        }

        private void LoadMatchesFromDatabase()
        {
            using (var context = new cnTeamManager.TeamManagerContext())
            {
                var teams = context.Teams
                    .Where(t => t.CoachId == UserId)
                    .ToList();

                if (!teams.Any())
                {
                    MessageBox.Show("Nem található csapat ehhez a felhasználóhoz.");
                    return;
                }

                var ownTeam = context.Teams.FirstOrDefault(t => t.CoachId == UserId);
                if (ownTeam == null)
                {
                    MessageBox.Show("Nem található a bejelentkezett edző csapata.");
                    return;
                }

                allTeams = context.Teams
                    .Where(t => t.Id != ownTeam.Id && t.League == ownTeam.League)
                    .ToList();
                OpponentComboBox.ItemsSource = allTeams;

                var teamIds = teams.Select(t => t.Id).ToList();

                var matches = context.Matches
                    .Where(m => teamIds.Contains(m.TeamId))
                    .Include(m => m.Teams1)
                    .OrderByDescending(m => m.Date)
                    .Select(m => new MatchResult
                    {
                        MatchId = m.Id,
                        MatchDate = m.Date,
                        TeamName = m.Teams1.Name,
                        LogoUrl = m.Teams1.LogoURL ?? "",
                        Result = m.Result
                    })
                    .ToList();

                Matches = new ObservableCollection<MatchResult>(matches);
                OnPropertyChanged(nameof(Matches));

                // Előkészítjük a törlés listát
                upcomingMatchesToDelete = context.Matches
                    .Where(m => m.TeamId == ownTeam.Id && m.Date > DateTime.Now)
                    .Include(m => m.Teams1)
                    .ToList();

                MatchesListBox.ItemsSource = upcomingMatchesToDelete
                    .Select(m => new
                    {
                        Match = m,
                        DisplayText = $"{m.Date:yyyy.MM.dd} {m.Teams1.Name}"
                    })
                    .ToList();
                MatchesListBox.DisplayMemberPath = "DisplayText";
                MatchesListBox.SelectedValuePath = "Match.Id";
            }
        }

        private void ShowAddMatchCard_Click(object sender, RoutedEventArgs e)
        {
            AddMatchCard.Visibility = Visibility.Visible;
            DeleteMatchCard.Visibility = Visibility.Collapsed;
        }

        private void ShowDeleteMatchCard_Click(object sender, RoutedEventArgs e)
        {
            DeleteMatchCard.Visibility = Visibility.Visible;
            AddMatchCard.Visibility = Visibility.Collapsed;
        }

        private void AddMatchButton_Click(object sender, RoutedEventArgs e)
        {
            if (!DateTime.TryParse(MatchDateTextBox.Text, out DateTime matchDateTime))
            {
                MessageBox.Show("Hibás dátum formátum. Használj ilyen formát: 2025-07-01");
                return;
            }

            if (matchDateTime <= DateTime.Now)
            {
                MessageBox.Show("A meccs dátuma csak jövőbeli lehet.");
                return;
            }

            if (OpponentComboBox.SelectedItem is not Teams selectedOpponent)
            {
                MessageBox.Show("Kérlek, válassz ki egy ellenfelet.");
                return;
            }

            using (var context = new cnTeamManager.TeamManagerContext())
            {
                var ownTeam = context.Teams.FirstOrDefault(t => t.CoachId == UserId);
                if (ownTeam == null)
                {
                    MessageBox.Show("Nem található a bejelentkezett edző csapata.");
                    return;
                }

                if (ownTeam.League != selectedOpponent.League)
                {
                    MessageBox.Show("Csak azonos ligában lévő ellenfelet választhatsz ki.");
                    return;
                }

                var newMatch = new Matches
                {
                    Date = matchDateTime,
                    TeamId = ownTeam.Id,
                    OpponentId = selectedOpponent.Id,
                    Result = "-"
                };

                context.Matches.Add(newMatch);
                context.SaveChanges();
            }

            MessageBox.Show("Meccs sikeresen hozzáadva!");
            MatchDateTextBox.Text = "";
            OpponentComboBox.SelectedItem = null;

            LoadMatchesFromDatabase();
        }

        private void DeleteMatchButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = MatchesListBox.SelectedItem;
            if (selectedItem == null)
            {
                MessageBox.Show("Kérlek, válassz ki egy törlendő meccset.");
                return;
            }

            var match = (Matches)selectedItem.GetType().GetProperty("Match").GetValue(selectedItem);

            var result = MessageBox.Show($"Biztosan törölni szeretnéd ezt a meccset?\n{match.Date:yyyy.MM.dd} - {match.Teams1?.Name}", "Megerősítés", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes)
                return;

            using (var context = new cnTeamManager.TeamManagerContext())
            {
                var matchToDelete = context.Matches.FirstOrDefault(m => m.Id == match.Id);
                if (matchToDelete != null && matchToDelete.Date > DateTime.Now)
                {
                    context.Matches.Remove(matchToDelete);
                    context.SaveChanges();
                    MessageBox.Show("Meccs törölve.");
                }
                else
                {
                    MessageBox.Show("Csak jövőbeli meccset lehet törölni.");
                }
            }

            LoadMatchesFromDatabase();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class MatchResult
    {
        public int MatchId { get; set; }
        public DateTime MatchDate { get; set; }
        public string LogoUrl { get; set; }
        public string TeamName { get; set; }
        public string Result { get; set; }

        public string ResultText => Result switch
        {
            "Win" => "Win",
            "Lose" => "Lose",
            _ => "-"
        };

        public Brush ResultColor => Result switch
        {
            "Win" => Brushes.Green,
            "Lose" => Brushes.Red,
            _ => Brushes.Gray
        };

        public Brush CardBackground => Result switch
        {
            "Win" => Brushes.LightGreen,
            "Lose" => Brushes.LightCoral,
            _ => Brushes.LightGray
        };
    }
}

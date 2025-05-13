using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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
using TeamManagerContext;
using static System.Collections.Specialized.BitVector32;

namespace LoL_eSport_Team_Mangager
{
    public partial class MatchesPage : Page, INotifyPropertyChanged
    {
        public int? UserId { get; set; } // Ez most a Users.Id, azaz az edző azonosítója
        public bool IsAdmin { get; set; }

        private List<Teams> allTeams;

        public ObservableCollection<MatchResult> Matches { get; set; }

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
                // Lekérjük a felhasználóhoz (coach-hoz) tartozó csapat(ok)at
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

                //ComboBoxba megjelenítjük az összes ellenfél csapatot
                allTeams = context.Teams
                      .Where(t => t.Id != ownTeam.Id && t.League == ownTeam.League)
                      .ToList();
                OpponentComboBox.ItemsSource = allTeams;

                // A csapatok ID-ját kigyűjtjük
                var teamIds = teams.Select(t => t.Id).ToList();

                // Olyan meccsek, ahol a felhasználó csapata TeamId-ként szerepel
                var matches = context.Matches
                    .Where(m => teamIds.Contains(m.TeamId))
                    .Include(m => m.Teams1) // Teams1 az ellenfél
                    .OrderByDescending(m => m.Date)
                    .Select(m => new MatchResult
                    {
                        MatchDate = m.Date,
                        TeamName = m.Teams1.Name,
                        LogoUrl = m.Teams1.LogoURL ?? "",
                        Result = m.Result
                    })
                    .ToList();

                Matches = new ObservableCollection<MatchResult>(matches);
                OnPropertyChanged(nameof(Matches));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private void AddMatchButton_Click(object sender, RoutedEventArgs e)
        {
            // Dátumellenőrzés
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

                // Ellenőrizzük, hogy az ellenfél ugyanabban a ligában van-e
                if (ownTeam.League != selectedOpponent.League)
                {
                    MessageBox.Show("Csak azonos ligában lévő ellenfelet választhatsz ki.");
                    return;
                }

                // Hozzáadjuk az új meccset
                var newMatch = new Matches
                {
                    Date = matchDateTime,
                    TeamId = ownTeam.Id,
                    OpponentId = selectedOpponent.Id,
                    Result = "-" // Alapértelmezett érték, még nincs eredmény
                };

                context.Matches.Add(newMatch);
                context.SaveChanges();
            }

            MessageBox.Show("Meccs sikeresen hozzáadva!");

            // Meccsek újratöltése
            LoadMatchesFromDatabase();

            // Tisztítás
            MatchDateTextBox.Text = "";
            OpponentComboBox.SelectedItem = null;
        }
    }

    public class MatchResult
    {
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

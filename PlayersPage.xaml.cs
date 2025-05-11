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
using TeamManagerContext;
using System.IO;

namespace LoL_eSport_Team_Mangager
{
    public partial class PlayersPage : Page
    {
        public int? TeamId { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsPlayerActiveInThisTeam { get; set; }

        public PlayersPage(int? teamId, bool isAdmin = false, bool isPlayerActiveInThisTeam = true)
        {
            InitializeComponent();
            TeamId = teamId;
            IsAdmin = isAdmin;
            IsPlayerActiveInThisTeam = isPlayerActiveInThisTeam;

            LogToFile($"PlayersPage megnyitva - Admin: {IsAdmin}, TeamId: {TeamId}");

            if (IsAdmin)
            {
                AdminTeamSelectorPanel.Visibility = Visibility.Visible;
                LoadTeamsForAdmin();
            }

            LoadPlayersForSelectedTeam();
        }

        private void LoadTeamsForAdmin()
        {
            try
            {
                using (var context = new cnTeamManager.TeamManagerContext())
                {
                    var teams = context.Teams
                        .Select(t => new { t.Id, t.Name })
                        .ToList();

                    TeamSelectorComboBox.ItemsSource = teams;
                    TeamSelectorComboBox.DisplayMemberPath = "Name";
                    TeamSelectorComboBox.SelectedValuePath = "Id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba a csapatok betöltésekor: {ex.Message}");
            }
        }

        private void TeamSelectorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TeamSelectorComboBox.SelectedValue is int selectedId)
            {
                TeamId = selectedId;
                LoadPlayersForSelectedTeam();
            }
        }

        private void LoadPlayersForSelectedTeam()
        {
            if (TeamId == null) return;

            try
            {
                using (var context = new cnTeamManager.TeamManagerContext())
                {
                    var players = context.Players
                        .Where(p => p.TeamId == TeamId && p.IsPlayerActiveInThisTeam == true)
                        .Select(p => new { p.Id, p.Name })
                        .ToList();

                    PlayerListComboBox.ItemsSource = players;
                    PlayerListComboBox.DisplayMemberPath = "Name";
                    PlayerListComboBox.SelectedValuePath = "Id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba a játékosok betöltésekor: {ex.Message}");
            }
        }

        private void AddPlayer_Click(object sender, RoutedEventArgs e)
        {
            PlayerForm.Visibility = Visibility.Visible;
        }

        private void SavePlayer_Click(object sender, RoutedEventArgs e)
        {
            if (TeamId == null)
            {
                MessageBox.Show("Nem vagy jogosult játékosokat hozzáadni ehhez a csapathoz.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string name = PlayerNameTextBox.Text?.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                string error = "Hibás adatbevitel: játékos neve üres.";
                LogToFile(error);
                MessageBox.Show("Kérlek, add meg a játékos nevét!");
                return;
            }

            if (PlayerPositionComboBox.SelectedItem == null)
            {
                string error = "Hibás adatbevitel: nincs pozíció kiválasztva.";
                LogToFile(error);
                MessageBox.Show("Kérlek, válassz pozíciót!");
                return;
            }

            string position = (PlayerPositionComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

            if (string.IsNullOrEmpty(position))
            {
                MessageBox.Show("Érvénytelen pozíció kiválasztás.");
                return;
            }

            try
            {
                using (var context = new cnTeamManager.TeamManagerContext())
                {
                    var newPlayer = new Players
                    {
                        Name = name,
                        Role = position,
                        TeamId = TeamId.Value,
                        IsPlayerActiveInThisTeam = true
                    };

                    context.Players.Add(newPlayer);
                    context.SaveChanges();

                    MessageBox.Show($"Játékos sikeresen mentve:\nNév: {name}\nPozíció: {position}");

                    LogToFile($"Új játékos hozzáadva: {name}, Pozíció: {position}, TeamId: {TeamId}");

                }

                // Clear form
                PlayerNameTextBox.Text = "";
                PlayerPositionComboBox.SelectedIndex = -1;
                PlayerForm.Visibility = Visibility.Collapsed;

                // Refresh player list
                LoadPlayersForSelectedTeam();
            }
            catch (Exception ex)
            {
                LogToFile($"Hiba játékos mentése közben: {ex.Message}");
                MessageBox.Show($"Hiba mentés közben: {ex.Message}");
            }
        }

        private void DeletePlayer_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerListComboBox.SelectedValue is int playerId)
            {
                try
                {
                    using (var context = new cnTeamManager.TeamManagerContext())
                    {
                        var player = context.Players
                            .FirstOrDefault(p => p.Id == playerId && p.TeamId == TeamId && p.IsPlayerActiveInThisTeam == true);

                        if (player != null)
                        {
                            player.IsPlayerActiveInThisTeam = false;
                            context.SaveChanges();

                            string playerName = player.Name ?? "(ismeretlen név)";
                            MessageBox.Show($"A(z) {playerName} játékos inaktiválva lett.");

                            LogToFile($"Játékos inaktiválva: {playerName}, PlayerId: {player.Id}, TeamId: {TeamId}");

                            LoadPlayersForSelectedTeam();
                            PlayerListComboBox.SelectedIndex = -1;
                        }
                        else
                        {
                            MessageBox.Show("A kiválasztott játékos nem található.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogToFile($"Hiba játékos törlésekor: {ex.Message}");
                    MessageBox.Show($"Hiba a játékos törlésekor: {ex.Message}");
                }
            }
            else
            {
                string error = "Hibás művelet: nem választottak ki játékost törléshez.";
                LogToFile(error);
                MessageBox.Show("Kérlek, válassz ki egy játékost a törléshez.");
            }
        }

        private void LogToFile(string message)
        {
            string logFilePath = "playersPage_log.txt";
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
            File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
        }
    }
}
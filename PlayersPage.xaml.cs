using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MaterialDesignThemes.Wpf;
using LoL_eSport_Team_Manager;
using TeamManagerContext;

namespace LoL_eSport_Team_Mangager
{
    public partial class PlayersPage : Page
    {
        public int? TeamId { get; set; }
        public bool IsAdmin { get; set; }

        public PlayersPage(int? teamId, bool isAdmin = false)
        {
            InitializeComponent();
            TeamId = teamId;
            IsAdmin = isAdmin;
            if (IsAdmin)
            {
                AdminTeamSelectorPanel.Visibility = Visibility.Visible;
                LoadTeamsForAdmin();
            }

            Logger.Log("PlayersPage megnyitva - Admin: " + IsAdmin + ", TeamId: " + TeamId, "INFO", "PlayersPage");
            LoadPlayers();
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
                Logger.Log("Hiba csapatok betöltésekor");
                MessageBox.Show($"Hiba a csapatok betöltésekor: {ex.Message}");
            }
        }



        private void LoadPlayers()
        {
            if (TeamId == null) return;

            PlayersCardPanel.Children.Clear();

            try
            {
                using (var context = new cnTeamManager.TeamManagerContext())
                {
                    var players = context.Players
                        .Where(p => p.TeamId == TeamId && p.IsPlayerActiveInThisTeam == true)
                        .ToList();

                    PlayerListComboBox.ItemsSource = players;
                    PlayerListComboBox.DisplayMemberPath = "Name";
                    PlayerListComboBox.SelectedValuePath = "Id";

                    foreach (var player in players)
                    {
                        var card = new Card
                        {
                            Margin = new Thickness(10),
                            Width = 200,
                            Background = (Brush)FindResource("LoLSecondaryBrush")
                        };

                        var stack = new StackPanel { Margin = new Thickness(10) };

                        stack.Children.Add(new TextBlock
                        {
                            Text = player.Name,
                            FontWeight = FontWeights.Bold,
                            Foreground = (Brush)FindResource("LoLTextBrush"),
                            FontSize = 16,
                            Margin = new Thickness(0, 0, 0, 5)
                        });

                        stack.Children.Add(new TextBlock
                        {
                            Text = $"Pozíció: {player.Role}",
                            Foreground = (Brush)FindResource("LoLTextBrush")
                        });

                        if (!string.IsNullOrEmpty(player.PhotoURL))
                        {
                            try
                            {
                                var image = new Image
                                {
                                    Height = 100,
                                    Margin = new Thickness(0, 10, 0, 0),
                                    Source = new BitmapImage(new Uri(player.PhotoURL))
                                };
                                stack.Children.Add(image);
                            }
                            catch
                            {
                                // URL hibák figyelmen kívül hagyva
                            }
                        }

                        card.Content = stack;
                        PlayersCardPanel.Children.Add(card);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Hiba a játékosok betöltésekor: " + ex.Message);
                MessageBox.Show("Hiba történt a játékosok betöltésekor.");
            }
        }

        private void AddPlayer_Click(object sender, RoutedEventArgs e)
        {
            PlayerForm.Visibility = Visibility.Visible;
            DeleteForm.Visibility = Visibility.Collapsed;
        }

        private void ToggleDeletePlayerForm(object sender, RoutedEventArgs e)
        {
            DeleteForm.Visibility = DeleteForm.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            PlayerForm.Visibility = Visibility.Collapsed;
        }

        private void SavePlayer_Click(object sender, RoutedEventArgs e)
        {
            if (TeamId == null) return;

            string name = PlayerNameTextBox.Text?.Trim();
            string position = (PlayerPositionComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(position))
            {
                Logger.Log("Kitöltetlen mező hiba");
                MessageBox.Show("Töltsd ki a mezőket.");
                return;
            }

            try
            {
                using (var context = new cnTeamManager.TeamManagerContext())
                {
                    var exists = context.Players.Any(p => p.TeamId == TeamId && p.Name == name && p.IsPlayerActiveInThisTeam == true);
                    if (exists)
                    {
                        MessageBox.Show("Ilyen nevű aktív játékos már létezik ebben a csapatban.");
                        Logger.Log("duplikált játékos felvételi probálkozás");
                        return;
                    }

                    var newPlayer = new Players
                    {
                        Name = name,
                        Role = position,
                        TeamId = TeamId.Value,
                        IsPlayerActiveInThisTeam = true
                    };
                    Logger.Log($"Új játékos hozzáadva: {name}, Pozíció: {position}, TeamId: {TeamId.Value}", "INFO", "PlayersPage");
                    context.Players.Add(newPlayer);
                    context.SaveChanges();
                }

                PlayerForm.Visibility = Visibility.Collapsed;
                PlayerNameTextBox.Text = "";
                PlayerPositionComboBox.SelectedIndex = -1;

                LoadPlayers();
            }
            catch (Exception ex)
            {
                Logger.Log("Hiba mentéskor: " + ex.Message);
                MessageBox.Show("Mentés nem sikerült.");
            }
        }

        private void DeletePlayer_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PlayerListComboBox.Text))
            {
                Logger.Log("Kitöltetlen mező hiba");
                MessageBox.Show("Töltsd ki a mezőket.");
                return;
            }
            if (PlayerListComboBox.SelectedValue is int playerId)
            {
                try
                {
                    using (var context = new cnTeamManager.TeamManagerContext())
                    {
                        var player = context.Players.FirstOrDefault(p => p.Id == playerId);
                      
                        if (player != null)
                        {
                            player.IsPlayerActiveInThisTeam = false;
                            context.SaveChanges();
                            Logger.Log($"Játékos inaktiválva: {player.Name}, PlayerId: {player.Id}, TeamId: {TeamId}", "INFO", "PlayersPage");

                        }
                    }
                    DeleteForm.Visibility = Visibility.Collapsed;
                    LoadPlayers();
                }
                catch (Exception ex)
                {
                    Logger.Log("Hiba törléskor: " + ex.Message);
                    MessageBox.Show("Törlés nem sikerült.");
                }
            }
        }
    }
}

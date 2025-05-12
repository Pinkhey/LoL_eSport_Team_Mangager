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
using cnTeamManager;
using System.Windows.Media.Imaging;
using TeamManagerContext;
using static System.Net.WebRequestMethods;
using LoL_eSport_Team_Manager;


namespace LoL_eSport_Team_Mangager
{
    public partial class RegisterPage : Page
    {
        private cnTeamManager.TeamManagerContext context; // itt már helyesen használjuk a TeamManagerContext típust



        private Dictionary<string, List<string>> regionLeagues = new Dictionary<string, List<string>>
        {
            { "EU", new List<string> { "LEC" } },
            { "KR", new List<string> { "LCK" } },
            { "NA", new List<string> { "LCS" } },
            { "CH", new List<string> { "LPL" } },
            { "LAN", new List<string> { "LLA" } }
        };

        private List<string> logoUrls = new List<string>
        {
            "https://static.wikia.nocookie.net/lolesports_gamepedia_en/images/7/79/PlacementIcon1.png/revision/latest/scale-to-width-down/1000?cb=20201021171046",
            "https://static.wikia.nocookie.net/lolesports_gamepedia_en/images/3/34/Skin_Loading_Screen_Oktoberfest_Gragas.jpg/revision/latest?cb=20191214204052",
            "https://static.wikia.nocookie.net/lolesports_gamepedia_en/images/a/a0/Rune_Conqueror.png/revision/latest?cb=20180320191055",
            "https://static.wikia.nocookie.net/lolesports_gamepedia_en/images/d/d9/Logo_square.png/revision/latest?cb=20191217012447"
        };

        public RegisterPage(string username)
        {
            InitializeComponent();
            context = new cnTeamManager.TeamManagerContext(); // Az új példányosítás itt helyes
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LogoSelectorListBox.ItemsSource = logoUrls;
            UserDeleteComboBox.ItemsSource = context.Users.Select(u => u.Username).ToList();
        }

        private void RegionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RegionComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedRegion = selectedItem.Content.ToString();
                if (regionLeagues.TryGetValue(selectedRegion, out List<string> leagues))
                {
                    LeagueComboBox.ItemsSource = leagues;
                    LeagueComboBox.SelectedIndex = 0;
                }
            }
        }

        private void LogoSelectorListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LogoSelectorListBox.SelectedItem is string selectedUrl)
            {
                LogoUrlTextBox.Text = selectedUrl;
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string username = UsernameTextBox.Text.Trim();
                string rawPassword = PasswordBox.Password;
                string hashedPassword = PasswordHelper.HashPassword(rawPassword);
                bool isAdmin = IsAdminCheckBox.IsChecked == true;
                string teamName = TeamNameTextBox.Text.Trim();
                string region = (RegionComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                string league = LeagueComboBox.SelectedItem as string;
                string logoUrl = LogoUrlTextBox.Text.Trim();

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(rawPassword) ||
                    string.IsNullOrEmpty(teamName) || string.IsNullOrEmpty(region) ||
                    string.IsNullOrEmpty(league) || string.IsNullOrEmpty(logoUrl))
                {
                    MessageBox.Show("Minden mezőt ki kell tölteni.");
                    return;
                }

                if (context.Users.Any(u => u.Username == username))
                {
                    Logger.Log($"Sikertelen regisztráció (duplikált név): {username}", "WARNING", "RegisterPage");
                    MessageBox.Show("Ez a felhasználónév már létezik.");
                    return;
                }

                var newUser = new Users
                {
                    Username = username,
                    PasswordHash = hashedPassword,
                    IsAdmin = isAdmin
                };

                context.Users.Add(newUser);
                context.SaveChanges(); // előbb mentjük, hogy legyen UserId

                var newTeam = new Teams
                {
                    Name = teamName,
                    Region = region,
                    League = league,
                    LogoURL = logoUrl,
                    CoachId = newUser.UserId // Kapcsolat beállítása
                };

                context.Teams.Add(newTeam);
                context.SaveChanges();

                Logger.Log($"Új felhasználó regisztrálva: {username}", "INFO", "RegisterPage");
                MessageBox.Show("Sikeres regisztráció!");

                UsernameTextBox.Text = "";
                PasswordBox.Password = "";
                IsAdminCheckBox.IsChecked = false;
                TeamNameTextBox.Text = "";
                RegionComboBox.SelectedIndex = -1;
                LeagueComboBox.ItemsSource = null;
                LogoUrlTextBox.Text = "";
                LogoSelectorListBox.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba történt: " + ex.Message);
            }
        }

        private void DeleteUserFromDatabase_Click(object sender, RoutedEventArgs e)
        {
            if (UserDeleteComboBox.SelectedItem is string username)
            {
                try
                {
                    var userToDelete = context.Users.FirstOrDefault(u => u.Username == username);
                    if (userToDelete != null)
                    {
                        var teams = context.Teams.Where(t => t.Users.Username == username).ToList();
                        context.Teams.RemoveRange(teams);

                        context.Users.Remove(userToDelete);
                        context.SaveChanges();

                        Logger.Log($"Felhasználó törölve: {username}", "INFO", "RegisterPage");
                        MessageBox.Show("Felhasználó törölve.");

                        UserDeleteComboBox.ItemsSource = context.Users.Select(u => u.Username).ToList();
                        UserDeleteComboBox.SelectedIndex = -1;
                    }
                    else
                    {
                        Logger.Log($"Törlés sikertelen: felhasználó nem található ({username})", "WARNING", "RegisterPage");
                        MessageBox.Show("A felhasználó nem található.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"Hiba a felhasználó törlésekor ({username}): {ex.Message}", "ERROR", "RegisterPage");
                    MessageBox.Show("Hiba történt a törlés során: " + ex.Message);
                }
            }
        }
    }
}
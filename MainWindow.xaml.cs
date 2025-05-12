using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Linq;
using cnTeamManager;
using LoL_eSport_Team_Mangager;
using System.IO;
using System;

namespace LoL_eSport_Team_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly cnTeamManager.TeamManagerContext _context = new cnTeamManager.TeamManagerContext();
        public string? LoggedInUsername { get; set; }
        public bool IsUserAdmin { get; set; }
        public int LoggedInUserId { get; set; }
        public int? LoggedInCoachTeamId { get; set; }  // Nullable

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DashboardPage(LoggedInUsername));
        }

        private void Players_Click(object sender, RoutedEventArgs e)
        {
            if (LoggedInCoachTeamId == null && !IsUserAdmin)
            {
                MessageBox.Show("Nem tartozik csapat ehhez a felhasználóhoz.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (IsUserAdmin)
            {
                var playersPage = new PlayersPage(null, isAdmin: true);
                MainFrame.Navigate(playersPage);
            }
            else
            {
                var playersPage = new PlayersPage(LoggedInCoachTeamId);
                MainFrame.Navigate(playersPage);
            }
        }

        private void Matches_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new MatchesPage(LoggedInCoachTeamId, IsUserAdmin));
        }

        private void Statistics_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new StatisticsPage(LoggedInCoachTeamId, IsUserAdmin));
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log($"Felhasználó kijelentkezett: {LoggedInUsername}", "INFO", "MainWindow");

            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void AddCoachButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new RegisterPage(LoggedInUsername));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string coachName = UsernameDisplay.Text;
            MainFrame.Navigate(new Pages.WelcomePage(LoggedInUsername));

            // Hide AddCoachButton for non-admin users
            if (!IsUserAdmin)
            {
                AddCoachButton.Visibility = Visibility.Collapsed;
            }

            // Töltsük be a csapat logóját, ha van hozzá tartozó TeamId
            try
            {
                if (LoggedInCoachTeamId.HasValue)
                {
                    using (var context = new cnTeamManager.TeamManagerContext())
                    {
                        var team = context.Teams.FirstOrDefault(t => t.Id == LoggedInCoachTeamId.Value);
                        if (team != null && !string.IsNullOrWhiteSpace(team.LogoURL))
                        {
                            TeamLogoImage.Source = new BitmapImage(new Uri(team.LogoURL));
                            TeamLogoImage.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Hiba a csapat logó betöltésekor: " + ex.Message);
            }
        }

        private void BackgroundVideo_Loaded(object sender, RoutedEventArgs e)
        {
            BackgroundVideo.Play();
        }

        private void BackgroundVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            BackgroundVideo.Position = TimeSpan.Zero;
            BackgroundVideo.Play();
        }
    }
}

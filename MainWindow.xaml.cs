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

namespace LoL_eSport_Team_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
                var playersPage = new PlayersPage(LoggedInCoachTeamId /*?? 0*/); // 0 if admin (just for default, can be improved)
                MainFrame.Navigate(playersPage);
            }

            
        }

        private void Matches_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new MatchesPage());
        }

        private void Statistics_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new StatisticsPage());
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            LogToFile($"Felhasználó {LoggedInUsername} kijelentkezett.");

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
            string coachName = UsernameDisplay.Text; // vagy vmi változóból, ha ott van a név
            MainFrame.Navigate(new Pages.WelcomePage(LoggedInUsername));


            // Hide AddCoachButton for non-admin users
            if (!IsUserAdmin)
            {
                AddCoachButton.Visibility = Visibility.Collapsed;
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

        private void LogToFile(string message)
        {
            string logFilePath = "login_log.txt"; // this is the same log file used before
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
            System.IO.File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
        }
    }
}
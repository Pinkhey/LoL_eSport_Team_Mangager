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

namespace LoL_eSport_Team_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string LoggedInUsername { get; set; }
        public bool IsUserAdmin { get; set; }

        public MainWindow()
        {
            InitializeComponent();


            using (var context = new cnTeamManager.TeamManagerContext())
            {
                var teams = context.Teams.ToList();
                MessageBox.Show($"Csapatok száma: {teams.Count}");
            }
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DashboardPage());
        }

        private void Players_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PlayersPage());
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
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void AddPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new RegisterPage());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Hide AddPlayerButton for non-admin users
            if (!IsUserAdmin)
            {
                AddPlayerButton.Visibility = Visibility.Collapsed;
            }
        }
    }
}
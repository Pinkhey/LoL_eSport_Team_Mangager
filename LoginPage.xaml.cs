using LoL_eSport_Team_Manager;
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

namespace LoL_eSport_Team_Mangager
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }



        private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
           // UsernamePlaceholder.Visibility = string.IsNullOrEmpty(UsernameTextBox.Text)
             //   ? Visibility.Visible
               // : Visibility.Collapsed;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            //PasswordPlaceholder.Visibility = string.IsNullOrEmpty(PasswordBox.Password)
              //  ? Visibility.Visible
                //: Visibility.Collapsed;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            using (var context = new cnTeamManager.TeamManagerContext())
            {
                var user = context.Users
                                  .FirstOrDefault(u => u.Username == username && u.PasswordHash == password);

                
                if (user != null)
                {

                    bool isAdmin = user.IsAdmin;

                    MainWindow mainWindow = new MainWindow
                    {
                        LoggedInUsername = username,
                        IsUserAdmin = isAdmin,
                    };

                    mainWindow.Show();
                    mainWindow.UsernameDisplay.Text = $"Felhasználónév: {username}";
                    mainWindow.IsUserAdminDisplay.Text = isAdmin ? "admin" : "user";
                    mainWindow.LogoutButton.Visibility = Visibility.Visible;

                    Window.GetWindow(this)?.Close();
                }
                else
                {
                    MessageBox.Show("Hibás felhasználónév vagy jelszó!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BackgroundVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            BackgroundVideo.Position = TimeSpan.Zero;
            BackgroundVideo.Play();
        }
        private void BackgroundVideo_Loaded(object sender, RoutedEventArgs e)
        {
            BackgroundVideo.Play();  // Indítjuk manuálisan!
        }

    }
}


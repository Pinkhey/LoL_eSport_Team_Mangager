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
using TeamManagerContext;

namespace LoL_eSport_Team_Mangager
{
    /// <summary>
    /// Interaction logic for RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;
            bool isAdmin = IsAdminCheckBox.IsChecked == true;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Kérjük, töltsön ki minden mezőt!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new cnTeamManager.TeamManagerContext())
            {
                // Check if user already exists
                if (context.Users.Any(u => u.Username == username))
                {
                    MessageBox.Show("Ez a felhasználónév már létezik!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var newUser = new Users
                {
                    Username = username,
                    PasswordHash = password,
                    IsAdmin = isAdmin
                };

                context.Users.Add(newUser);
                context.SaveChanges();

                MessageBox.Show("Sikeres regisztráció!", "Kész", MessageBoxButton.OK, MessageBoxImage.Information);

                // Optional: navigate back to dashboard or clear form
            }
        }
    }
}

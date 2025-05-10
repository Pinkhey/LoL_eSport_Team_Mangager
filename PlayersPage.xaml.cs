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

namespace LoL_eSport_Team_Mangager
{
    /// <summary>
    /// Interaction logic for PlayersPage.xaml
    /// </summary>
    public partial class PlayersPage : Page
    {
        public int TeamId { get; set; }

        public PlayersPage(int teamId)
        {
            InitializeComponent();
            TeamId = teamId;
        }

        private void AddPlayer_Click(object sender, RoutedEventArgs e)
        {
            PlayerForm.Visibility = Visibility.Visible;
        }

        private void SavePlayer_Click(object sender, RoutedEventArgs e) 
        {

            string name = PlayerNameTextBox.Text;

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Kérlek, add meg a játékos nevét!");
                return;
            }

            if (PlayerPositionComboBox.SelectedItem is ComboBoxItem selectedPositionItem)
            {
                string position = selectedPositionItem.Content.ToString();

                using (var context = new cnTeamManager.TeamManagerContext()) 
                {
                    var newPlayer = new Players
                    {
                        Name = name,
                        Role = position,
                        TeamId = TeamId,
                    };

                    context.Players.Add(newPlayer);
                    context.SaveChanges();

                    MessageBox.Show($"Játékos sikeresen mentve:\nNév: {name}\nPozíció: {position} az adatbázisba!");
                }

                   

                // Clear form
                PlayerNameTextBox.Text = "";
                PlayerPositionComboBox.SelectedIndex = -1;
                PlayerForm.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("Kérlek, válassz pozíciót!");
            }
        }

        private void DeletePlayer_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Játékos törölve");
        }
    }
}

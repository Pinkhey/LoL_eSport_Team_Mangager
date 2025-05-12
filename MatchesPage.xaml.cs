using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using static System.Collections.Specialized.BitVector32;

namespace LoL_eSport_Team_Mangager
{
    /// <summary>
    /// Interaction logic for MatchesPage.xaml
    /// </summary>
    public partial class MatchesPage : Page, INotifyPropertyChanged
    {

        public int? TeamId { get; set; }
        public bool IsAdmin { get; set; }
        public ObservableCollection<MatchResult> Matches { get; set; }
        public MatchesPage(int? teamId, bool isAdmin = false)
        {
            InitializeComponent();
            TeamId = teamId;
            IsAdmin = isAdmin;
            DataContext = this;

            LoadMatchesFromDatabase();
        }

        private void LoadMatchesFromDatabase()
        {
            using (var context = new cnTeamManager.TeamManagerContext())
            {
                // saját csapat meghatározása az edző alapján
                var team = context.Teams
                            .FirstOrDefault(t => t.CoachId == TeamId);

                if (team == null)
                {
                    MessageBox.Show("Nem található csapat ehhez az edzőhöz.");
                    return;
                }

                // saját csapat meccsei, ellenfél adataival
                var matches = context.Matches
                    .Where(m => m.TeamId == team.Id)
                    .Include(m => m.Teams1)
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
    }

    //
    public class MatchResult
    {
        public DateTime MatchDate { get; set; }
        public string LogoUrl { get; set; }
        public string TeamName { get; set; }
        public string Result { get; set; } // "Win", "Lose", vagy "-"

        public string ResultText => Result switch
        {
            "Win" => "W",
            "Lose" => "L",
            "-" => "-"
        };

        public Brush ResultColor => Result switch
        {
            "Win" => Brushes.Green,
            "Lose" => Brushes.Red,
            "-" => Brushes.Gray
        };
    }
}

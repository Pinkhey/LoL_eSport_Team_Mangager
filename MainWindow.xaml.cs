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

namespace LoL_eSport_Team_Mangager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //ha 3 a csapatok száma akkor jó
            using (var context = new cnTeamManager.TeamManagerContext())
            {
                var teams = context.Teams.ToList();
                MessageBox.Show($"Csapatok száma: {teams.Count}");
            }
        }
    }
}
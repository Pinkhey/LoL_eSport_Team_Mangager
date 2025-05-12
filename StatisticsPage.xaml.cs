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
using LoL_eSport_Team_Manager;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.Collections.ObjectModel;
using cnTeamManager;
using LiveChartsCore.SkiaSharpView.WPF;


namespace LoL_eSport_Team_Mangager
{
    public partial class StatisticsPage : Page
    {
        private readonly cnTeamManager.TeamManagerContext _context = new cnTeamManager.TeamManagerContext();
        private readonly int _teamId;

        public StatisticsPage(cnTeamManager.TeamManagerContext context, int teamId)
        {
            InitializeComponent();
            _context = context;
            _teamId = teamId;

            DataContext = new StatisticsViewModel(_context, _teamId);
        }

        private void TeamSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Későbbi admin funkcióhoz – most üres
        }
    }
}

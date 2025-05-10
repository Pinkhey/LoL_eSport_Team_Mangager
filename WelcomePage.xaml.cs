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

namespace LoL_eSport_Team_Manager.Pages
{
    public partial class WelcomePage : Page
    {
        public WelcomePage(string username)
        {
            InitializeComponent();
            CoachNameText.Text = username;

            // LoL témájú idézetek listája
            var quotes = new[]
            {
                "„A győzelem nem a reflexeken múlik, hanem a döntéseken.”",
                "„Az igazi edző nem parancsol, hanem tanít – és hallgat, amikor kell.”",
                "„Egy jól időzített ping többet ér, mint ezer szó.”",
                "„Draftolj tudatosan, rotálj okosan, győzz méltósággal.”",
                "„A látás többet ér, mint a sebzés.”",
                "„Az ösvényeken játékosok, a háttérben mesterek döntik el a meccseket.”",
                "„Nem az számít, hány killt szereztél – hanem hogy mit tanultál a hibáidból.”",
                "„A legjobb csapat nem a legerősebb, hanem a legösszehangoltabb.”"
            };

            // Véletlenszerű kiválasztás
            var random = new Random();
            int index = random.Next(quotes.Length);
            QuoteText.Text = quotes[index];
        }
    }
}


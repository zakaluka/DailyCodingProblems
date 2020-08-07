using Windows.Foundation;
using Windows.UI.ViewManagement;

namespace aoc2019_4.xamarin.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            LoadApplication(new aoc2019_4.xamarin.App());
        }
    }
}

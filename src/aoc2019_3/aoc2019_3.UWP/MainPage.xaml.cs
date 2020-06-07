using Windows.Foundation;
using Windows.UI.ViewManagement;

namespace aoc2019_3.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            LoadApplication(new aoc2019_3.App());
        }
    }
}

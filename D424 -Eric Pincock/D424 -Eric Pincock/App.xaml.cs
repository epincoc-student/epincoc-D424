using D424__Eric_Pincock.Pages;

namespace D424__Eric_Pincock
{
    public partial class App : Application
    {
        public App(LoginPage loginPage)
        {
            InitializeComponent();

            MainPage = new LoginPage();
        }
    }
}
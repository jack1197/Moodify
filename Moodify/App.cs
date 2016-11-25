using Microsoft.WindowsAzure.MobileServices;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Moodify
{

    public interface IAuthenticate
    {
        Task<bool> Authenticate();
    }


    public partial class App : Application
	{
		public static NavigationPage NavigationPage { get; private set; }
		public static RootPage RootPage;

		public static bool MenuIsPresented
		{
			get
			{
				return RootPage.IsPresented;
			}
			set
			{
				RootPage.IsPresented = value;
			}
		}

		public App()
		{
			var menuPage = new MenuPage();
			NavigationPage = new NavigationPage(new HomePage());
			RootPage = new RootPage();
			RootPage.Master = menuPage;
			RootPage.Detail = NavigationPage;
			MainPage = RootPage;
		}

        public static IAuthenticate Authenticator { get; private set; }

        public static void Init(IAuthenticate authenticator)
        {
            Authenticator = authenticator;
        }

        protected override void OnStart()
		{
			// Handle when your app starts
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}
	}
}

using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace Moodify
{
	public class MenuPageViewModel
	{
		public ICommand GoHomeCommand { get; set; }
        public ICommand GoSecondCommand { get; set; }
        public ICommand GoThirdCommand { get; set; }

        public MenuPageViewModel()
		{
			GoHomeCommand = new Command(GoHome);
            GoSecondCommand = new Command(GoSecond);
            GoThirdCommand = new Command(GoThird);
        }

		void GoHome(object obj)
		{
            App.RootPage.Detail = new NavigationPage(new HomePage());
			App.MenuIsPresented = false;
		}

        void GoSecond(object obj)
        {
            App.RootPage.Detail = new NavigationPage(new SecondPage());
            App.MenuIsPresented = false;
        }

        void GoThird(object obj)
        {
            App.RootPage.Detail = new NavigationPage(new ThirdPage());
            App.MenuIsPresented = false;
        }
    }
}

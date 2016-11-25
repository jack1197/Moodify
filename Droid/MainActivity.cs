using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.Permissions;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;

namespace Moodify.Droid
{
	[Activity(Label = "Moodify", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IAuthenticate
	{
		protected override void OnCreate(Bundle bundle)
		{
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;

			base.OnCreate(bundle);

			global::Xamarin.Forms.Forms.Init(this, bundle);

            App.Init((IAuthenticate)this);
            LoadApplication(new App());
		}

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        // Define a authenticated user.
        private MobileServiceUser user;

        public async Task<bool> Authenticate()
        {
            var success = false;
            var message = string.Empty;
            try
            {
                user = await AzureManager.AzureManagerInstance.AzureClient.LoginAsync(this, MobileServiceAuthenticationProvider.Facebook);
                if (user != null)
                {
                    message = string.Format("You are now signed in as {0}", user.UserId);
                    
                    success = true;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            //Display message
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetMessage(message);
            builder.SetTitle("Sign-in result");
            builder.SetNegativeButton("OK", (s, e) => {});
            builder.Create().Show();

            return success;
        }

    }
}

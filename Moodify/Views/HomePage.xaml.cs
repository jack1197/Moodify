using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;
using System.Threading.Tasks;
using System;
using Plugin.Media;
using Microsoft.ProjectOxford.Emotion;

namespace Moodify
{
    public partial class HomePage : ContentPage
	{
		public HomePage()
		{
			InitializeComponent();
        }

        private async void TakePicture_Clicked(object sender, EventArgs args)
        {
            var cameraStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
            var storageStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);

            if (cameraStatus != PermissionStatus.Granted || storageStatus != PermissionStatus.Granted)
            {
                var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Camera, Permission.Storage });
                cameraStatus = results[Permission.Camera];
                storageStatus = results[Permission.Storage];
            }

            if (cameraStatus == PermissionStatus.Granted && storageStatus == PermissionStatus.Granted)
            {
                var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    DefaultCamera = Plugin.Media.Abstractions.CameraDevice.Front,
                    Directory = "Moodify",
                    Name = $"{DateTime.UtcNow}.jpg",
                    CompressionQuality = 92
                });

                if (file == null)
                    return;

                image.Source = ImageSource.FromStream(() => file.GetStream());

                UploadingIndicator.IsVisible = true;
                UploadingIndicator.IsRunning = true;
                errorLabel.IsVisible = false;
                EmotionView.ItemsSource = null;

                try
                {
                    string emotionKey = "266df984c83c4953942c2aa0ec78848c";

                    EmotionServiceClient emotionClient = new EmotionServiceClient(emotionKey);

                    var emotionResults = await emotionClient.RecognizeAsync(file.GetStream());

                    UploadingIndicator.IsRunning = false;
                    UploadingIndicator.IsVisible = false;

                    var temp = emotionResults[0].Scores;

                    errorLabel.IsVisible = true;
                    errorLabel.Text = Newtonsoft.Json.JsonConvert.SerializeObject(temp);

                    EmotionView.ItemsSource = temp.ToRankedList();

                }
                catch (Exception ex)
                {
                    errorLabel.IsVisible = true;
                    if (ex is IndexOutOfRangeException)
                    {
                        errorLabel.Text = "No face detected, please try again :)";
                    }
                    else
                    {
                        errorLabel.Text = ex.Message;
                    }
                }

                file.Dispose();
            }
            else
            {
                await DisplayAlert("Permissions Denied", "Unable to take photos.", "OK");
                //On iOS you may want to send your user to the settings screen.
                //CrossPermissions.Current.OpenAppSettings();
            }


        }

	}
}

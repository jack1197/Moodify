using Microsoft.ProjectOxford.Emotion;
using Moodify.DataModels;
using Plugin.Media;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Moodify
{
    public partial class HomePage : ContentPage
    {

        bool authenticated = false;

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (authenticated == true)
            {
                // Hide the Sign-in button.
                this.loginButton.IsVisible = false;
            }
        }

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
                EmotionGrid.IsVisible = false;
                ResultsContainer.IsVisible = true;

                    string emotionKey = "266df984c83c4953942c2aa0ec78848c";

                    EmotionServiceClient emotionClient = new EmotionServiceClient(emotionKey);

                    var emotionResults = await emotionClient.RecognizeAsync(file.GetStream());

                    var temp = emotionResults[0].Scores;

                    //errorLabel.IsVisible = true;
                    //errorLabel.Text = Newtonsoft.Json.JsonConvert.SerializeObject(temp);

                    PopulateEmotions((List<KeyValuePair<string, float>>)temp.ToRankedList());

                    Timeline emotion = new Timeline
                    {
                        Anger = temp.Anger,
                        Contempt = temp.Contempt,
                        Fear = temp.Fear,
                        Happiness = temp.Happiness,
                        Neutral = temp.Neutral,
                        Sadness = temp.Sadness,
                        Surprise = temp.Surprise,
                        Disgust = temp.Disgust,
                        Date = DateTime.Now
                    };
                    
                    await AzureManager.AzureManagerInstance.AddTimeline(emotion);
                try
                {


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
                        errorLabel.Text = ex.Message + ex.StackTrace;
                    }
                }

                UploadingIndicator.IsRunning = false;
                UploadingIndicator.IsVisible = false;
                file.Dispose();
            }
            else
            {
                await DisplayAlert("Permissions Denied", "Unable to take photos.", "OK");
                //On iOS you may want to send your user to the settings screen.
                //CrossPermissions.Current.OpenAppSettings();
            }


        }

        private void PopulateEmotions(List<KeyValuePair<string, float>> emotions)
        {
            EmotionGrid.IsVisible = true;
            EmotionGrid.Children.Clear();
            EmotionGrid.RowDefinitions.Clear();
            for (int i = 0; i < emotions.Count; i++)
            {
                EmotionGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                EmotionGrid.Children.Add(new Label { Text = emotions[i].Key }, 0, i);
                EmotionGrid.Children.Add(new Label { Text = String.Format("{0:p2}", emotions[i].Value) }, 1, i);
            }
        }


        private async void ViewTimeline_Clicked(object sender, EventArgs args)
        {
            TimelineContainer.IsVisible = true;
            DownloadingIndicator.IsVisible = true;
            DownloadingIndicator.IsRunning = true;
            timelineErrorLabel.IsVisible = false;

            try
            {
                List<Timeline> timelines = await AzureManager.AzureManagerInstance.GetTimelines();

                DownloadingIndicator.IsRunning = false;
                DownloadingIndicator.IsVisible = false;

                PopulateTimeline(timelines);
            }
            catch (Exception ex)
            {
                timelineErrorLabel.Text = ex.Message;
                timelineErrorLabel.IsVisible = true;
                DownloadingIndicator.IsVisible = false;
            }
        }


        private void PopulateTimeline(List<Timeline> timeline)
        {
            TimelineGrid.Children.Clear();
            TimelineGrid.RowDefinitions.Clear();

            if (timeline.Count == 0)
                { return; }

            TimelineGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            TimelineGrid.Children.Add(new Label { Text = "Date/Time" }, 0, 0);
            TimelineGrid.Children.Add(new Label { Text = "Happiness" }, 1, 0);
            TimelineGrid.Children.Add(new Label { Text = "Anger" }, 2, 0);


            for (int i = 0; i < timeline.Count; i++)
            {
                TimelineGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                TimelineGrid.Children.Add(new Label { Text = timeline[i].Date.ToString("dd/MMM/yy h:mm tt"), VerticalOptions = LayoutOptions.Center }, 0, i+1);
                TimelineGrid.Children.Add(new Label { Text = String.Format("{0:p2}", timeline[i].Happiness), VerticalOptions = LayoutOptions.Center }, 1, i+1);
                TimelineGrid.Children.Add(new Label { Text = String.Format("{0:p2}", timeline[i].Anger), VerticalOptions = LayoutOptions.Center }, 2, i + 1);

                Button button = new Button()
                {
                    TextColor = Color.White,
                    BackgroundColor = Color.Red,
                    Text = "X",
                    VerticalOptions = LayoutOptions.Fill
                };

                int scopedIndex = i;
                button.Clicked += async (s, e) =>
                {
                    ((Button)s).IsEnabled = false;
                    await AzureManager.AzureManagerInstance.DeleteTimeline(timeline[scopedIndex]);
                    ViewTimeline_Clicked(s, e);
                };

                TimelineGrid.Children.Add(button, 3, i + 1);

            }
            TimelineGrid.IsVisible = true;
        }

        async void loginButton_Clicked(Object sender, EventArgs args)
        {

            if (App.Authenticator != null)
                authenticated = await App.Authenticator.Authenticate();

            if (authenticated == true)
                this.loginButton.IsVisible = false;
        }
    }
}

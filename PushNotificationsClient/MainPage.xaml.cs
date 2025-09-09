using Plugin.Firebase.CloudMessaging;

namespace PushNotificationsClient
{
    public partial class MainPage : ContentPage
    {
        public DateTime MaxDate { get; } = DateTime.Today.AddMonths(3);
        public TimeSpan SelectedTime { get; set; } = new TimeSpan(12, 0, 0);
        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private async void GetFCMToken(object sender, EventArgs e)
        {
            try
            {
                await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
                var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
                Console.WriteLine(token);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnScheduleAppointmentClicked(object sender, EventArgs e)
        {
            await CrossFirebaseCloudMessaging.Current.SubscribeToTopicAsync("Appointments");
        }
    }
}

using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using Windows.UI.Xaml.Navigation;
using System;
using System.Diagnostics;

namespace SDKTemplate
{
    public sealed partial class Settings : Page
    {
        MainPage rootPage;
        ApplicationDataContainer roamingSettings = null;

        public Settings()
        {
            this.InitializeComponent();               
            roamingSettings = ApplicationData.Current.RoamingSettings;

        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            rootPage = MainPage.Current;
            Object value;


            value = roamingSettings.Values["location"];
            if (value == null) value = "True";
            if (value.ToString() == "True")
                checkBox_location.IsChecked = true;
            else
                checkBox_location.IsChecked = false;
            
            value = roamingSettings.Values["time"];
            if (value == null) value = "True";
            if (value.ToString() == "True")
                checkBox_time.IsChecked = true;
            if (value.ToString() == "False")
                checkBox_time.IsChecked = false;

            value = roamingSettings.Values["fluoride"];
            if (value == null) value = "True"; 
            if (value.ToString() == "True")
                fluoride.IsChecked = true;
            if (value.ToString() == "False")
                fluoride.IsChecked = false;
        }

        private void location_Checked(object sender, RoutedEventArgs e)
        {
            roamingSettings.Values["location"] = "True";
        }

        private void time_Checked(object sender, RoutedEventArgs e)
        {
            roamingSettings.Values["time"] = "True";
        }

        private void fluoride_Checked(object sender, RoutedEventArgs e)
        {
            roamingSettings.Values["fluoride"] = "True";
        }

        private void location_Unchecked(object sender, RoutedEventArgs e)
        {
            roamingSettings.Values["location"] = "False";
        }

        private void time_Unchecked(object sender, RoutedEventArgs e)
        {
            roamingSettings.Values["time"] = "False";
        }
    }
}



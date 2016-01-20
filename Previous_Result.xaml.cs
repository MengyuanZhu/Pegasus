using System.ComponentModel;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System;
using System.Text;
using Windows.UI.Xaml.Navigation;
using Windows.Data.Xml.Dom;
using Windows.Storage;
namespace SDKTemplate
{
    public sealed partial class Previous_Result : Page
    {
        private MainPage rootPage;
        string file;
        XmlDocument doc;

        public Previous_Result()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            rootPage = MainPage.Current;
            file = "result.xml";
            StorageFile storageFile = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(file);
            XmlLoadSettings loadSettings = new XmlLoadSettings();
            loadSettings.ProhibitDtd = false;
            loadSettings.ResolveExternals = false;
            doc = await XmlDocument.LoadFromFileAsync(storageFile, loadSettings);
            var results = doc.SelectNodes("descendant::result");
            int num_results = 1;
            foreach (var result in results)
            {
                listBox.Items.Add(num_results.ToString());
                num_results += 1;
            }       
        }
        
        private void sample_list_tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            int item = Int32.Parse(listBox.SelectedValue.ToString())-1;

            var results = doc.SelectNodes("descendant::result");
            String[] date = results[item].SelectSingleNode("descendant::date").FirstChild.NodeValue.ToString().Split();
            String location = results[item].SelectSingleNode("descendant::location").FirstChild.NodeValue.ToString();
            String concentration = results[item].SelectSingleNode("descendant::concentration").FirstChild.NodeValue.ToString();
            String model = results[item].SelectSingleNode("descendant::model").FirstChild.NodeValue.ToString();
            
            listView.Items.Clear();
            listView.Items.Add("Results:");
            listView.Items.Add("Date: "+ date[0]);
            listView.Items.Add("Time: "+date[1]);            
            listView.Items.Add("Location: "+location);
            listView.Items.Add("Model: "+model);
            listView.Items.Add("Concentration: " + concentration);
        }
        
    }
}

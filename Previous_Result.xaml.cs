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
using Windows.UI.Popups;

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
            listBox.Items.Add("Index");
            StorageFile storageFile;
            file = "history.xml";
           
            
                storageFile= await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(file);
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

                if (num_results > 1) //show the first history result
                {
                    listBox.SelectedIndex = 1;
                    select_item(0);
                }
            
          


            //rootPage.NotifyUser(doc.InnerText.ToString(), NotifyType.StatusMessage);

        }
        
        private void select_item(int item)  //show result of history item
        {
            var results = doc.SelectNodes("descendant::result");
            String[] date = results[item].SelectSingleNode("descendant::date").FirstChild.NodeValue.ToString().Split();
            String location = results[item].SelectSingleNode("descendant::location").FirstChild.NodeValue.ToString();
            String concentration = results[item].SelectSingleNode("descendant::concentration").FirstChild.NodeValue.ToString();
            String model = results[item].SelectSingleNode("descendant::model").FirstChild.NodeValue.ToString();

            listView.Items.Clear();
            listView.Items.Add("Results:");
            listView.Items.Add("Date: " + date[0]);
            listView.Items.Add("Time: " + date[1]);
            listView.Items.Add("Location: " + location);
            listView.Items.Add("Model: " + model);
            listView.Items.Add("Concentration: " + concentration);

        }

        private void sample_list_tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e) //tap item list 
        {
            if (listBox.SelectedValue.ToString() == "Index")
            {
                return ;
            }

            int item = Int32.Parse(listBox.SelectedValue.ToString())-1;
            select_item(item);
            
        }

        private async void btnDelete_tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e) ///delete result
        {
            var dialog = new MessageDialog("Sure?");
            dialog.Commands.Add(new UICommand("Yes") { Id = 0 });
            dialog.Commands.Add(new UICommand("No") { Id = 1 });
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;
            var result = await dialog.ShowAsync();

            
            if ((int)result.Id==0)
            {
                listBox.Items.RemoveAt(listBox.SelectedIndex);
                listView.Items.Clear();                   
            }
        }
    }
}

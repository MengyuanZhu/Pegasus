using System.ComponentModel;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using System;

namespace SDKTemplate
{
    public sealed partial class Pending_Result : Page
    {
        private MainPage rootPage;
        string file;
        XmlDocument doc;

        public Pending_Result()
        {
            this.InitializeComponent();
            
            titleBar = ApplicationView.GetForCurrentView().TitleBar;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            rootPage = MainPage.Current;
            file = "status.xml";
            StorageFile storageFile = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(file);
            XmlLoadSettings loadSettings = new XmlLoadSettings();
            loadSettings.ProhibitDtd = false;
            loadSettings.ResolveExternals = false;
            doc = await XmlDocument.LoadFromFileAsync(storageFile, loadSettings);
            try
            {
                var results = doc.SelectNodes("descendant::result");
                
                int num_results = 1;
                

                foreach (var result in results)
                {
                    String id = results[num_results - 1].SelectSingleNode("descendant::id").FirstChild.NodeValue.ToString();
                    String status = results[num_results-1].SelectSingleNode("descendant::status").FirstChild.NodeValue.ToString();
                    listBox.Items.Add("ID: "+id+" "+ status);
                    num_results += 1;
                }
                if (num_results==1)
                    listBox.Items.Add("You don't have pending jobs.");
            }
            
            catch(Exception)
            { 
            listBox.Items.Add("You don't have pending jobs.");
            }

        }

        #region Data binding


        private ApplicationViewTitleBar titleBar;

        public ApplicationViewTitleBar TitleBar
        {
            get { return titleBar; }
        }
        #endregion
    }
}

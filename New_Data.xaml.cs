using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Capture;      //For MediaCapture  
using Windows.Media.MediaProperties;  //For Encoding Image in JPEG format  
using Windows.Storage;         //For storing Capture Image in App storage or in Picture Library  
using System;
using System.Threading;
using Windows.Networking.Sockets;
using Windows.Networking;
using Windows.Storage.Streams;
using System.IO;
using Windows.Devices.Geolocation;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.UI.Xaml.Media;
using Windows.Services.Maps;   //city name
using Windows.Data.Xml.Dom;

namespace SDKTemplate
{
    public sealed partial class New_Data : Page
    {

        ApplicationDataContainer roamingSettings = null;
        private CancellationTokenSource _cts = null;
        private MainPage rootPage = MainPage.Current;
        MediaCapture captureManager;
        double latitude = 0;
        double longitude = 0;
        string time = "null";
        string cityname="null";
        string concentration="null";


        public New_Data()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            roamingSettings = ApplicationData.Current.RoamingSettings;
        }

        private async void ReverseGeocode(double latitude, double longtitude)//to get the city name
        {
            // Location to reverse geocode.
            BasicGeoposition location = new BasicGeoposition();
            location.Latitude = latitude;
            location.Longitude = longtitude;
            Geopoint pointToReverseGeocode = new Geopoint(location);

            // Reverse geocode the specified geographic location.
            MapLocationFinderResult result =
                await MapLocationFinder.FindLocationsAtAsync(pointToReverseGeocode);

            // If the query returns results, display the name of the town
            // contained in the address of the first result.
            if (result.Status == MapLocationFinderStatus.Success)
            {
               cityname=  result.Locations[0].Address.Town;
            }
        }



        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            bt_capture.IsEnabled = false;
            captureManager = new MediaCapture();    //Define MediaCapture object  
            try
            {
                await captureManager.InitializeAsync();   //Initialize MediaCapture and       
            }  
            catch(Exception)
            {
                rootPage.NotifyUser("Could not initialize camera", NotifyType.ErrorMessage );
                return;
            }         
                   
            PreviewControl.Source = captureManager;   //Start preiving on CaptureElement  
            PreviewControl.Stretch = Stretch.Fill;
                        
            await captureManager.StartPreviewAsync();  //Start camera capturing 
            Object value = roamingSettings.Values["location"];
            bt_capture.IsEnabled = true;

            if (value.ToString() == "True")   //If location is set, get geolocation values.
            {
                if (_cts != null)
                {
                    _cts.Cancel();
                    _cts = null;
                }                               
                try
                {
                    // Request permission to access location
                    var accessStatus = await Geolocator.RequestAccessAsync();

                    switch (accessStatus)
                    {
                        case GeolocationAccessStatus.Allowed:
                            MapService.ServiceToken = "FLnByFC6agPsR0ooiEZp~wwi1VZZh24BXrHipd94OTg~Ag_LVCFXZEuLwjb8rVVO_rJaeYeLh_IzhTCrPBHUjge9--oezSW2jC_2XX_kdDMf";

                            _cts = new CancellationTokenSource();
                            CancellationToken token = _cts.Token;
                            // Get cancellation token

                            rootPage.NotifyUser("Getting location, please wait...", NotifyType.StatusMessage);
                            // If DesiredAccuracy or DesiredAccuracyInMeters are not set (or value is 0), DesiredAccuracy.Default is used.
                            Geolocator geolocator = new Geolocator();
                            geolocator.DesiredAccuracy = Windows.Devices.Geolocation.PositionAccuracy.Default;

                            // Carry out the operation
                            Geoposition pos = await geolocator.GetGeopositionAsync().AsTask(token);
                            rootPage.NotifyUser("Location updated.", NotifyType.StatusMessage);
                            latitude=pos.Coordinate.Point.Position.Latitude;
                            longitude = pos.Coordinate.Point.Position.Longitude;
                            ReverseGeocode(latitude, longitude);

                            break;

                        case GeolocationAccessStatus.Denied:
                            rootPage.NotifyUser("Access to location is denied.", NotifyType.ErrorMessage);
                            break;

                        case GeolocationAccessStatus.Unspecified:
                            rootPage.NotifyUser("Unspecified error.", NotifyType.ErrorMessage);
                            break;
                    }
                }
                catch (TaskCanceledException)
                {
                    rootPage.NotifyUser("Canceled.", NotifyType.StatusMessage);
                }
                catch (Exception ex)
                {
                    rootPage.NotifyUser(ex.ToString(), NotifyType.ErrorMessage);
                }
                finally
                {
                    _cts = null;
                }

            }

            value = roamingSettings.Values["time"];
            if (value.ToString() == "True")
            {
                DateTime localDate = DateTime.Now;
                time = localDate.ToString();  
            }
        }
        
        async private void capture_Click(object sender, RoutedEventArgs e)
        {
            XmlDocument doc; //doc is history doc
            string history_file_xml;
            ImageEncodingProperties imgFormat = ImageEncodingProperties.CreateJpeg();
            StorageFile file = await KnownFolders.PicturesLibrary.CreateFileAsync("Photo.jpg", CreationCollisionOption.ReplaceExisting);
            await captureManager.CapturePhotoToStorageFileAsync(imgFormat, file);
            bt_capture.IsEnabled = false;
            
            rootPage.NotifyUser("Done saving images, connecting...", NotifyType.StatusMessage);
            StreamSocket socket = new StreamSocket();
            
            HostName hostName= new HostName("rs1008linux01.cloudapp.net");

            try {                
                await socket.ConnectAsync(hostName, "12346");                
            }
            catch(Exception)
            {
                rootPage.NotifyUser("Error: Host server not available.", NotifyType.ErrorMessage);
                return;
            }
            rootPage.NotifyUser("Connected", NotifyType.StatusMessage);
            DataWriter writer= new DataWriter(socket.OutputStream);            
            rootPage.NotifyUser("Uploading...", NotifyType.StatusMessage);
            byte[] data=  new byte[1024];
            Stream x= await file.OpenStreamForReadAsync();
            long file_length = x.Length;
            long tail = file_length % 1024;
            double offset = 0;
            string progress;
            try
            {
                while (file_length > offset)
                {

                    await x.ReadAsync(data, 0, 1024);
                    offset = offset + 1024;
                    if(file_length<offset)
                    {
                        byte[] temp = new byte[tail];
                        Array.Copy(data, temp, (int)tail);
                        writer.WriteBytes(temp);

                    }
                    else
                        writer.WriteBytes(data);                   
                    
                    progress = "Progress: " + (Math.Round(offset / x.Length * 100, 0)).ToString() + "%";
                   
                    rootPage.NotifyUser(progress, NotifyType.StatusMessage);
                    await writer.StoreAsync();
                    await writer.FlushAsync();
                }
            }

            catch(Exception ex)
            {
                rootPage.NotifyUser(ex.Message, NotifyType.StatusMessage);
                return;
            }         
            
            writer.DetachStream();
            x.Dispose();

            rootPage.NotifyUser("Uploading finished.", NotifyType.StatusMessage);

            byte[] buffer = new byte[128];

            Stream results=socket.InputStream.AsStreamForRead();
            var memoryStream = new MemoryStream();
            results.CopyTo(memoryStream);
            buffer = memoryStream.ToArray();
            //await socket.InputStream.ReadAsync(buffer.AsBuffer(), 128,InputStreamOptions.ReadAhead);

            socket.Dispose();
            

            bt_capture.IsEnabled = true;
            concentration = Encoding.UTF8.GetString(buffer)+" nm"; //get concentration result
            geolocation.Text = concentration;
            rootPage.NotifyUser("Data generated.", NotifyType.StatusMessage);


            history_file_xml = "history.xml";

            StorageFile storageFile = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(history_file_xml);
     

            XmlLoadSettings loadSettings = new XmlLoadSettings();
            loadSettings.ProhibitDtd = false;
            loadSettings.ResolveExternals = false;            
            doc = await XmlDocument.LoadFromFileAsync(storageFile, loadSettings);

            var rootnode = doc.SelectSingleNode("results");
            IXmlNode child = doc.CreateElement("result");                    
            rootnode.AppendChild(child);

            IXmlNode xml_id = doc.CreateElement("id");
            xml_id.InnerText = "5";
            child.AppendChild(xml_id);

            IXmlNode xml_date = doc.CreateElement("date");
            xml_date.InnerText = "2016-2-19 4:12:12";
            child.AppendChild(xml_date);

            IXmlNode xml_location = doc.CreateElement("location");
            xml_location.InnerText = cityname;
            child.AppendChild(xml_location);

            IXmlNode xml_concentration = doc.CreateElement("concentration");            
            xml_concentration.InnerText = geolocation.Text;            
            child.AppendChild(xml_concentration);

            IXmlNode xml_model= doc.CreateElement("model");
            xml_model.InnerText = "Fluoride Ion";
            child.AppendChild(xml_model);

            //rootPage.NotifyUser(doc.GetXml(), NotifyType.StatusMessage);
            
            await doc.SaveToFileAsync(storageFile);
            
        }
    }
}

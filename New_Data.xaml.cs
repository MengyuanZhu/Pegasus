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
namespace SDKTemplate
{
    public sealed partial class New_Data : Page
    {
        ApplicationDataContainer roamingSettings = null;
        private CancellationTokenSource _cts = null;
        private MainPage rootPage = MainPage.Current;
        MediaCapture captureManager;
        string latitude = "null";
        string longitude = "null";
        string time = "null";

        public New_Data()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            roamingSettings = ApplicationData.Current.RoamingSettings;
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
                            latitude=pos.Coordinate.Point.Position.Latitude.ToString();
                            longitude = pos.Coordinate.Point.Position.Longitude.ToString();
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
            await socket.InputStream.ReadAsync(buffer.AsBuffer(), 128,InputStreamOptions.ReadAhead);
            socket.Dispose();
            bt_capture.IsEnabled = true;
            rootPage.NotifyUser(Encoding.UTF8.GetString(buffer), NotifyType.StatusMessage);
            

        }
    }
}

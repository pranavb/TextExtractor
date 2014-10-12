using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.ApplicationModel;
using Windows.Graphics.Imaging;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;

using Windows.UI.Xaml.Controls;
using WindowsPreview.Media.Ocr;

using Microsoft.Live;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;



// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace PicToTextRT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, IFileOpenPickerContinuable
    {
        OcrEngine ocrEngine;
        UInt32 width;
        UInt32 height;
        string recognizedText = "";

        // Define the permission scopes
        private static readonly string[] scopes = new string[] { "wl.signin", "wl.basic", "Office.OneNote_Create" };

        // Set up the Live variables
        private LiveAuthClient authClient;
        private LiveConnectClient liveClient;

        // URI for the OneNote API
        private static readonly Uri PagesEndPoint = new Uri("https://www.onenote.com/api/v1.0/pages");


        public MainPage()
        {
            this.InitializeComponent();

            tbResponse.Text = string.Empty;
            
            ocrEngine = new OcrEngine(OcrLanguage.English);
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.

        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            openPicker.PickSingleFileAndContinue();
        }

        public async void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
        {
            if (args.Files.Count > 0)
            {
                //args.Files[0].Path;
                var img = args.Files[0];
                using (var stream = await img.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    // Create image decoder.
                    var decoder = await BitmapDecoder.CreateAsync(stream);

                    width = decoder.PixelWidth;
                    height = decoder.PixelHeight;

                    // Get pixels in BGRA format.
                    var pixels = await decoder.GetPixelDataAsync(
                        BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Straight,
                        new BitmapTransform(),
                        ExifOrientationMode.RespectExifOrientation,
                        ColorManagementMode.ColorManageToSRgb);

                    // Extract text from image.
                    OcrResult result = await ocrEngine.RecognizeAsync(height, width, pixels.DetachPixelData());

                    //// Check whether text is detected.
                    //if (result.Lines != null)
                    //{
                    //    // Collect recognized text.
                        
                    //    foreach (var line in result.Lines)
                    //    {
                    //        foreach (var word in line.Words)
                    //        {
                    //            recognizedText += word.Text + " ";
                    //        }
                    //        recognizedText += Environment.NewLine;
                    //    }

                    //    // Display recognized text.
                    //    outputPara.Text = recognizedText;
                    //    //saveBtn.IsEnabled = true;
                    //}
                }

            }
            else
            {
                outputPara.Text = "";
            }

            try
            {
                authClient = new LiveAuthClient();
                LiveLoginResult loginResult = await authClient.InitializeAsync(scopes);

                if (loginResult.Status == LiveConnectSessionStatus.Connected)
                {
                    liveClient = new LiveConnectClient(loginResult.Session);
                }
            }

            //Use the text box to display any exceptions.
            catch (LiveAuthException authExp)
            {
                tbResponse.Text = authExp.ToString();
            }
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LiveLoginResult loginResult = await authClient.LoginAsync(scopes);

                // Use the text box to indicate whether the user is logged in.
                if (loginResult.Status == LiveConnectSessionStatus.Connected)
                {

                    liveClient = new LiveConnectClient(loginResult.Session);
                    tbResponse.Text = "logged in";
                }

            }
            // Use the text box to display exceptions.
            catch (LiveAuthException authExp)
            {
                tbResponse.Text = authExp.ToString();
            }
        }

        private async void btnCreate_Page(object sender, RoutedEventArgs e)
        {
            await CreatePage();
        }

        private async Task CreatePage()
        {
            try
            {
                var client = new HttpClient();

                // Note: API only supports JSON return type.
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                // This allows you to see what happens when an unauthenticated call is made.
                if (IsAuthenticated)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authClient.Session.AccessToken);
                }


                string date = GetDate();

                var createMessage = new HttpRequestMessage(HttpMethod.Post, PagesEndPoint)
                {
                    Content = new StringContent("It worked! =D", System.Text.Encoding.UTF8, "text/plain")
                };

                HttpResponseMessage response = await client.SendAsync(createMessage);

                tbResponse.Text = response.ToString();
            }

            catch (Exception e)
            {
                tbResponse.Text = e.ToString();
            }

        }

        private static string GetDate()
        {
            return DateTime.Now.ToString("o");
        }

        public bool IsAuthenticated
        {
            get { return authClient.Session != null && !string.IsNullOrEmpty(authClient.Session.AccessToken); }
        }

    }
}

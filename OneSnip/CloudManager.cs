using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OneDrive.Sdk;
using Microsoft.OneDrive.Sdk.WindowsForms;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;

namespace OneSnip
{
    class CloudManager
    {
        private IOneDriveClient oneDriveClient;
        private AppConfig config;
        private static string returnUrl = "https://login.live.com/oauth20_desktop.srf";
        private string imgurClient;
        private string imgurSecret;

        private string filePath;
        private byte[] buffer;

        public CloudManager()
        {
            string keystring = System.Text.Encoding.UTF8.GetString(Properties.Resources.MyAPIKeys);

            JObject keys = JObject.Parse(keystring);

            config = new AppConfig();
            config.MicrosoftAccountAppId = (String)keys["AppIDs"]["OneDrive"];
            config.MicrosoftAccountClientSecret = (String)keys["Secrets"]["OneDrive"];
            config.MicrosoftAccountScopes = new string[] { "wl.offline_access", "onedrive.readwrite" };

            imgurClient = (String)keys["AppIDs"]["Imgur"];
            imgurSecret = (String)keys["Secrets"]["Imgur"];
        }

        #region DPIHelpers
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

        [DllImport("User32.dll")]
        private static extern IntPtr MonitorFromPoint([In]System.Drawing.Point pt, [In]uint dwFlags);

        [DllImport("Shcore.dll")]
        private static extern IntPtr GetDpiForMonitor([In]IntPtr hmonitor, [In]Int32 dpiType, [Out]out uint dpiX, [Out]out uint dpiY);
        #endregion

        public byte[] getBuffer()
        {
            return buffer;
        }

        public string getFilePath()
        {
            return filePath;
        }

        public async Task<ImageResult> handleImage(Bitmap image, Screen screenUsed, bool forceUpload = false)
        {
            string fileName = DateTime.Now.ToString("MMddyyyyhhmmss") + ".jpg";
            filePath = "OneSnip/" + fileName;
            string link;

            string cloudName = (Properties.Settings.Default.cloudTarget == "OneDrive") ? "OneDrive" : "Imgur";

            MemoryStream memStream = new MemoryStream();
            image.Save(memStream, ImageFormat.Jpeg);

            buffer = memStream.ToArray();

            string copyMode = Properties.Settings.Default.copyMode;
            if (forceUpload || (copyMode == "Text" || copyMode == "ImageText"))
            {
                try
                {
                    link = await uploadImageAndGetLink(buffer, filePath, screenUsed);
                }
                catch (Exception ex)
                {
                    System.GC.Collect();
                    link = null;
                }
            } else
            {
                link = null;
            }

            System.GC.Collect();

            return new ImageResult(cloudName, image, link);
        }

        public async Task<string> uploadImageAndGetLink(byte[] imageBuffer, string filePath, Screen screenUsed)
        {
            string link;

            imageBuffer = GetDpiScaledImage(imageBuffer, screenUsed);

            if (Properties.Settings.Default.cloudTarget == "OneDrive")
            {
                var uploadedItem = await oneDriveClient.Drive.Root.ItemWithPath(filePath).Content.Request().PutAsync<Item>(new MemoryStream(imageBuffer));
                var item = await oneDriveClient.Drive.Root.ItemWithPath(filePath).CreateLink("view").Request().PostAsync();

                link = item.Link.WebUrl;
            }
            else
            {
                link = await imgurUpload(imageBuffer);
            }

            return link;
        }

        private byte[] GetDpiScaledImage(byte[] imageBuffer, Screen screenUsed)
        {
            Bitmap original = (Bitmap)Bitmap.FromStream(new MemoryStream(imageBuffer));

            float scale = getScalingFactor(screenUsed);

            if (scale <= 1)
            {
                return imageBuffer;
            }

            float newWidth = (original.Width / scale);
            float newHeight = (original.Height / scale);

            Bitmap image = new Bitmap((int)newWidth, (int)newHeight);
            Graphics g = Graphics.FromImage(image);

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            g.DrawImage(original, 0, 0, newWidth, newHeight);

            MemoryStream memStream = new MemoryStream();
            image.Save(memStream, ImageFormat.Jpeg);

            return memStream.ToArray();
        }

        private float getScalingFactor(Screen screenUsed)
        {
            uint dpiX;
            uint dpiY;

            var pnt = new System.Drawing.Point(screenUsed.Bounds.Left + 1, screenUsed.Bounds.Top + 1);
            var mon = MonitorFromPoint(pnt, 2/*MONITOR_DEFAULTTONEAREST*/);
            
            GetDpiForMonitor(mon, 0 /*DpiType Effective*/, out dpiX, out dpiY);

            return dpiX / (float)96; // 1.25 = 125%
        }

        private async Task<string> imgurUpload(byte[] imageBuffer)
        {
            string link;
            using (var w = new System.Net.WebClient())
            {
                w.Headers.Add("Authorization", "Client-ID " + imgurClient);
                var values = new System.Collections.Specialized.NameValueCollection
                        {
                            { "image", Convert.ToBase64String(imageBuffer) }
                        };

                byte[] response = w.UploadValues("https://api.imgur.com/3/upload.xml", values);

                System.Xml.Linq.XDocument xmlResponse = System.Xml.Linq.XDocument.Load(new MemoryStream(response));
                string status = xmlResponse.Root.LastAttribute.Value;

                if (status == "200")
                {
                    link = xmlResponse.Root.Elements("link").First().Value;
                }
                else
                {
                    throw new Exception();
                }
            }

            return link;
        }

        public async void AuthMSA(bool zeroUserAction = false)
        {

            string refresh_token = Properties.Settings.Default.refreshToken;

            if (refresh_token != null && refresh_token != "")
            {
                //attempt to silently sign in

                oneDriveClient = await OneDriveClient.GetSilentlyAuthenticatedMicrosoftAccountClient(
                            config.MicrosoftAccountAppId,
                            returnUrl,
                            config.MicrosoftAccountScopes,
                            refresh_token);

                var auth = await oneDriveClient.AuthenticateAsync();

                if (oneDriveClient.IsAuthenticated)
                {
                    Properties.Settings.Default.refreshToken = auth.RefreshToken;
                    Properties.Settings.Default.accessToken = auth.AccessToken;
                    Properties.Settings.Default.tokenType = auth.AccessTokenType;
                    Properties.Settings.Default.Save();

                    return;
                }

            }

            if (zeroUserAction)
            {
                return; //don't ask to login if the user hasn't done anything yet
            }

            //if we're here then either there was no refresh token, or the refresh token did not provide auth

            oneDriveClient = OneDriveClient.GetMicrosoftAccountClient(
                         config.MicrosoftAccountAppId,
                         returnUrl,
                         config.MicrosoftAccountScopes,
                         webAuthenticationUi: new FormsWebAuthenticationUi());

            try
            {

                var auth = await oneDriveClient.AuthenticateAsync();
                if (oneDriveClient.IsAuthenticated)
                {
                    Properties.Settings.Default.refreshToken = auth.RefreshToken;
                    Properties.Settings.Default.accessToken = auth.AccessToken;
                    Properties.Settings.Default.tokenType = auth.AccessTokenType;
                    Properties.Settings.Default.Save();

                    return;
                }
            }
            catch (OneDriveException ex)
            {
                ex.ToString();
                int num = 1 + 1;
            }
        }

        public void SignOutMSA()
        {
            Properties.Settings.Default.refreshToken = "";
            Properties.Settings.Default.accessToken = "";
            Properties.Settings.Default.tokenType = "";
            Properties.Settings.Default.Save();
            oneDriveClient.SignOutAsync();
        }

        public bool OneDriveIsAuthenticated()
        {
            return (oneDriveClient != null && oneDriveClient.IsAuthenticated);
        }


    }
}

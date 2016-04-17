using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.OneDrive.Sdk;
using Microsoft.OneDrive.Sdk.WindowsForms;
using System.IO;
using System.Drawing.Imaging;
using Microsoft.Win32;
using System.Xml.Linq;
using System.Linq;
using ESCommon.Rtf;

namespace OneSnip
{
    class OneSnipTray : ApplicationContext
    {
        private static NotifyIcon notifyIcon;
        private static Editor editorForm;
        private static List<Snipper> snippers = new List<Snipper>();
        private static CloudManager cloudManager;

        public OneSnipTray()
        {
            InitializeContext();
        }

        private void InitializeContext()
        {
            setupTrayIcon();
            cloudManager = new CloudManager();
            setDefaultCopyMode();

            cloudManager.AuthMSA(true); //true makes it so that we attempt silent-login only

            if (!hasAutoStartEverBeenEnabled())
            {
                setStartup();
            }
        }

        private void setupTrayIcon()
        {
            var components = new System.ComponentModel.Container();
            notifyIcon = new NotifyIcon(components)
            {
                ContextMenuStrip = new ContextMenuStrip(),
                Icon = OneSnip.Properties.Resources.onesnip64x64,
                Text = "OneSnip - Click to take a screenshot and share a link",
                Visible = true
            };
            notifyIcon.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
            notifyIcon.MouseUp += notifyIcon_MouseUp;
            notifyIcon.BalloonTipClicked += EditImage;
        }

        private void setDefaultCopyMode()
        {
            if (Properties.Settings.Default.copyMode == "")
            {
                Properties.Settings.Default.copyMode = "Text";
                Properties.Settings.Default.Save();
            }
        }

        public static CloudManager getCloudManager()
        {
            return cloudManager;
        }

        private static void notifyIcon_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (needToAuthMSA())
                {
                    cloudManager.AuthMSA();
                }
                else
                {
                    if (snippers.Count == 0)
                    {
                        Screen[] screens = Screen.AllScreens;
                        Snip snip = new Snip();
                        snip.SnipCreated += Snip_SnipCreated;

                        foreach (Screen screen in screens)
                        {
                            Snipper snipperForm = new Snipper(screen, snip);
                            snipperForm.Closed += snipperForm_Closed;
                            snipperForm.Show();

                            snippers.Add(snipperForm);
                        }

                    }
                }
            }
        }

        private static bool needToAuthMSA()
        {
            return (!cloudManager.OneDriveIsAuthenticated() && Properties.Settings.Default.cloudTarget == "OneDrive") ? true : false;
        }

        private static void snipperForm_Closed(object sender, EventArgs e)
        {
            foreach (Snipper snipper in snippers)
            {
                snipper.Hide();
                snipper.Dispose();
            }
            snippers.Clear();
        }

        private async static void Snip_SnipCreated(object sender, SnipEventArgs e)
        {

            ImageResult result = await cloudManager.handleImage(e.image);
            ToastMetadata toastData = new ToastMetadata();

            DoClipboardAndSetupToast(result, toastData);

            toastData.ShowToast(notifyIcon);
        }

        public static void AddToClipboard(ImageResult result)
        {
            DoClipboardAndSetupToast(result, new ToastMetadata(), true);
        }

        private static void DoClipboardAndSetupToast(ImageResult result, ToastMetadata toastData, bool forceText = false)
        {
            string copyMode = Properties.Settings.Default.copyMode;

            if (copyMode == "Text")
            {
                Clipboard.SetText(result.link);
                toastData.setupToast("OneSnip", result.cloudName + " link copied to clipboard. Click to edit your snip.");
            }

            else if (forceText || copyMode == "ImageText")
            {

                string combined = getCombinedImageText(result);

                DataObject data = new DataObject();
                data.SetData(DataFormats.Text, true, result.link); //we explicitly put in the link because some clients like browsers and IM windows won't know how to render combiend (RTF) data
                data.SetData(DataFormats.Rtf, true, combined);

                Clipboard.SetDataObject(data, true);

                toastData.setupToast("OneSnip", result.cloudName + " link and image copied to clipboard. Click to edit your snip.");
            }

            else //they asked for image-only
            {
                Clipboard.SetImage(result.image);
                toastData.setupToast("OneSnip", "Image copied to clipboard. Click to edit your snip.");
            }
        }

        private static string getCombinedImageText(ImageResult result)
        {
            RtfDocument rtf = new RtfDocument();
            RtfParagraph rtfTextBlock = new RtfParagraph();

            RtfImage rtfImage = new RtfImage((System.Drawing.Bitmap)result.image, RtfImageFormat.Jpeg);
            rtfTextBlock.AppendParagraph(rtfImage);

            RtfFormattedText rtfLink = new RtfFormattedText(result.link);
            rtfTextBlock.AppendParagraph(rtfLink);

            rtf.Contents.Add(rtfTextBlock);
            RtfWriter writer = new RtfWriter();

            return writer.GetContent(rtf);
        }

        private static void EditImage(object sender, EventArgs e)
        {
            editorForm = new Editor(cloudManager.getBuffer(), cloudManager.getFilePath());
            editorForm.Closed += editorForm_Closed;
            editorForm.Show();
        }

        public static void balloonForNewLink(string link)
        {
            ToastMetadata toastData = new ToastMetadata();
            toastData.setupToast("OneSnip", "Clipboard has link to your edited image.");
            toastData.ShowToast(notifyIcon);
        }

        private static void editorForm_Closed(object sender, EventArgs e)
        {
            editorForm.Hide();
            editorForm.Dispose();
            editorForm = null;

            System.GC.Collect();
        }

        private static void ContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            e.Cancel = false;

            ToolStripMenuItem ClipboardBehavior = new ToolStripMenuItem();
            ToolStripMenuItem ClipboardBehavior_ImageOnly = new ToolStripMenuItem();
            ToolStripMenuItem ClipboardBehavior_TextOnly = new ToolStripMenuItem();
            ToolStripMenuItem ClipboardBehavior_ImageText = new ToolStripMenuItem();

            ClipboardBehavior.Text = "Copy mode";
            ClipboardBehavior_ImageOnly.Text = "Image";
            ClipboardBehavior_ImageOnly.Click += ClipboardBehavior_ImageOnly_Click;

            ClipboardBehavior_TextOnly.Text = "Link";
            ClipboardBehavior_TextOnly.Click += ClipboardBehavior_TextOnly_Click;


            ClipboardBehavior_ImageText.Text = "Image + Link";
            ClipboardBehavior_ImageText.Click += ClipboardBehavior_ImageText_Click;

            if (Properties.Settings.Default.copyMode == "Image")
            {
                ClipboardBehavior_ImageOnly.Checked = true;
            } else if (Properties.Settings.Default.copyMode == "ImageText")
            {
                ClipboardBehavior_ImageText.Checked = true;
            } else
            {
                ClipboardBehavior_TextOnly.Checked = true;
            }


            ClipboardBehavior.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            ClipboardBehavior_ImageOnly, ClipboardBehavior_TextOnly, ClipboardBehavior_ImageText});

            ToolStripMenuItem CloudTarget = new ToolStripMenuItem();
            ToolStripMenuItem CloudTarget_ODC = new ToolStripMenuItem();
            ToolStripMenuItem CloudTarget_Imgur = new ToolStripMenuItem();

            CloudTarget.Text = "Cloud target";
            CloudTarget_ODC.Text = "OneDrive";
            CloudTarget_ODC.Click += CloudTarget_ODC_Click;
            CloudTarget_Imgur.Text = "Imgur";
            CloudTarget_Imgur.Click += CloudTarget_Imgur_Click;

            if (Properties.Settings.Default.cloudTarget == "OneDrive")
            {
                CloudTarget_ODC.Checked = true;
            } else
            {
                CloudTarget_Imgur.Checked = true;
            }

            CloudTarget.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            CloudTarget_ODC, CloudTarget_Imgur});

            ToolStripMenuItem autoStart = new ToolStripMenuItem();
            autoStart.Text = "Run when logging on to Windows";
            autoStart.Click += AutoStart_Click;

            if (isAutoStartEnabled())
            {
                autoStart.Checked = true;
            }


            ToolStripMenuItem SignInSignOut = new ToolStripMenuItem();
            if (!cloudManager.OneDriveIsAuthenticated())
            {
                SignInSignOut.Text = "Sign in to OneDrive";
                SignInSignOut.Click += MSASignIn_Click;
            } else
            {
                SignInSignOut.Text = "Sign out of OneDrive";
                SignInSignOut.Click += MSASignOut_Click;
            }

            ToolStripMenuItem Quit = new ToolStripMenuItem();
            Quit.Text = "Exit";
            Quit.Click += Quit_Click;

            ToolStripMenuItem BuildInfo = new ToolStripMenuItem();
            BuildInfo.Text = getBuildInfoString();
            BuildInfo.Enabled = false;


            notifyIcon.ContextMenuStrip.Items.Clear();
            notifyIcon.ContextMenuStrip.Items.Add(ClipboardBehavior);
            notifyIcon.ContextMenuStrip.Items.Add(CloudTarget);
            notifyIcon.ContextMenuStrip.Items.Add(autoStart);
            notifyIcon.ContextMenuStrip.Items.Add(SignInSignOut);
            notifyIcon.ContextMenuStrip.Items.Add(BuildInfo);
            notifyIcon.ContextMenuStrip.Items.Add(Quit);

        }

        private static string getBuildInfoString()
        {
            string date = Properties.Resources.BuildDate.ToString().Trim();
            string head = Properties.Resources.HEAD.ToString();

            string branch = head.Split('/')[2].Trim();

            DateTime dt = new DateTime();
            DateTime.TryParse(date, out dt);

            date = dt.ToString("MM/dd/yy h:mm tt");

            return "Built from " + branch + " (" + date + ")";
        }

        private static void AutoStart_Click(object sender, EventArgs e)
        {
            if (isAutoStartEnabled())
            {
                removeStartup();
            } else
            {
                setStartup();
            }

        }

        private static bool hasAutoStartEverBeenEnabled()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            string startPath = (string)rk.GetValue("OneSnip");

            return !(startPath == null);
        }

        private static bool isAutoStartEnabled()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            string startPath = (string)rk.GetValue("OneSnip");

            return (startPath != null && startPath == Application.ExecutablePath.ToString());
        }

        private static void setStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rk.SetValue("OneSnip", Application.ExecutablePath.ToString());
        }

        private static void removeStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rk.SetValue("OneSnip", "");
        }

        private static void CloudTarget_Imgur_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.cloudTarget = "Imgur";
            Properties.Settings.Default.Save();
        }

        private static void CloudTarget_ODC_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.cloudTarget = "OneDrive";
            Properties.Settings.Default.Save();
            if(needToAuthMSA())
            {
                cloudManager.AuthMSA();
            }
        }

        private static void ClipboardBehavior_ImageText_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.copyMode = "ImageText";
            Properties.Settings.Default.Save();
        }

        private static void ClipboardBehavior_TextOnly_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.copyMode = "Text";
            Properties.Settings.Default.Save();
        }

        private static void ClipboardBehavior_ImageOnly_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.copyMode = "Image";
            Properties.Settings.Default.Save();
        }

        private static void Quit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private static void MSASignOut_Click(object sender, EventArgs e)
        {
            cloudManager.SignOutMSA();
        }

        private static async void MSASignIn_Click(object sender, EventArgs e)
        {
            cloudManager.AuthMSA();
        }

    }
}

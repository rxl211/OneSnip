using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OneSnip
{
    public partial class Snipper : Form
    {
        //These variables control the mouse position
        int selectX;
        int selectY;
        int selectWidth;
        int selectHeight;
        public Pen selectPen;

        private Screen curScreen;
        private Snip snip;

        private string windowsVersion;


        //This variable control when you start the right click
        bool start = false;
        public Snipper(Screen _screen, Snip _snip, string _windowsVersion)
        {
            InitializeComponent();
            curScreen = _screen;
            snip = _snip;
            windowsVersion = _windowsVersion;

            this.Location = curScreen.WorkingArea.Location;
            this.ShowInTaskbar = false;
        }

        private void Snipper_Load(object sender, EventArgs e)
        {
            //Hide the Form
            this.Hide();

            pictureBox1.Size = new System.Drawing.Size(this.Width, this.Height);

            /*
            Windows 10 RS2 has a bug that impacts window hit targets with see-through windows like what we have here with picturebox1.
            We'll get around it by screenshotting the desktops before snip is taken and letting the user snip that screenshot. So it is a slightly worse experience but at least it works for most use cases. 
            */
            if (windowsVersion == "1703")
            {
                
                Bitmap printscreen = new Bitmap(curScreen.Bounds.Width, curScreen.Bounds.Height);
                Graphics graphics = Graphics.FromImage(printscreen as Image);
                graphics.CopyFromScreen(curScreen.Bounds.X, curScreen.Bounds.Y, 0, 0, printscreen.Size);

                prePrintscreen.Size = new System.Drawing.Size(this.Width, this.Height);
                prePrintscreen.Image = printscreen;
                prePrintscreen.Refresh();
                this.Controls.Add(this.prePrintscreen);
                prePrintscreen.BringToFront();
            }

            

            //Show Form
            this.Show();
            //Cross Cursor
            Cursor = Cursors.Cross;
        }

        private void handleMouseMove(PictureBox pb, MouseEventArgs e)
        {
            //validate if right-click was trigger
            if (start)
            {
                //refresh picture box
                pb.Refresh();

                selectPen = new Pen(Color.Red, 3);
                selectPen.DashStyle = DashStyle.Dash;
                //set corner square to mouse coordinates
                selectWidth = e.X - selectX;
                selectHeight = e.Y - selectY;
                int newSelectX;
                int newWidth;
                int newSelectY;
                int newHeight;

                if (selectWidth < 0)
                {
                    newSelectX = selectX - Math.Abs(selectWidth);
                    newWidth = Math.Abs(selectWidth);
                }
                else
                {
                    newSelectX = selectX;
                    newWidth = selectWidth;
                }

                if (selectHeight < 0)
                {
                    newSelectY = selectY - Math.Abs(selectHeight);
                    newHeight = Math.Abs(selectHeight);
                }
                else
                {
                    newSelectY = selectY;
                    newHeight = selectHeight;
                }

                //draw dotted rectangle
                pb.CreateGraphics().DrawRectangle(selectPen, newSelectX, newSelectY, newWidth, newHeight);
            }
        }

        private void handleMouseDown(PictureBox pb, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                //starts coordinates for rectangle
                selectX = e.X;
                selectY = e.Y;
                start = true;
            }
            //refresh picture box
            pb.Refresh();
        }

        private void handleMouseUp(PictureBox pb, MouseEventArgs e)
        {
            //same functionality when mouse is over
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                pb.Refresh();
                selectWidth = e.X - selectX;
                selectHeight = e.Y - selectY;

                if (selectWidth < 0)
                {
                    selectX = selectX - Math.Abs(selectWidth);
                    selectWidth = Math.Abs(selectWidth);
                }

                if (selectHeight < 0)
                {
                    selectY = selectY - Math.Abs(selectHeight);
                    selectHeight = Math.Abs(selectHeight);
                }

                //pictureBox1.CreateGraphics().DrawRectangle(selectPen, selectX, selectY, selectWidth, selectHeight);

            }

            //function save image to clipboard
            SaveToClipboard();
            start = false;
        }

        private void prePrintscreen_MouseMove(object sender, MouseEventArgs e)
        {
            handleMouseMove(prePrintscreen, e);
        }

        private void prePrintscreen_MouseUp(object sender, MouseEventArgs e)
        {
            handleMouseUp(prePrintscreen, e);
        }

        private void prePrintscreen_MouseDown(object sender, MouseEventArgs e)
        {
            handleMouseDown(prePrintscreen, e);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            handleMouseMove(pictureBox1, e);
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            handleMouseUp(pictureBox1, e);
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            handleMouseDown(pictureBox1, e);
        }

        private void SaveToClipboard()
        {
            //validate if something selected
            if (selectWidth > 0)
            {
                Bitmap printscreen = new Bitmap(curScreen.Bounds.Width, curScreen.Bounds.Height);
                Graphics graphics = Graphics.FromImage(printscreen as Image);
                graphics.CopyFromScreen(curScreen.Bounds.X, curScreen.Bounds.Y, 0, 0, printscreen.Size);

                Rectangle rect = new Rectangle(selectX, selectY, selectWidth, selectHeight);
                Bitmap _img = new Bitmap(selectWidth, selectHeight);
                Graphics g = Graphics.FromImage(_img);

                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawImage(printscreen, 0, 0, rect, GraphicsUnit.Pixel);

                this.Close();
                snip.setImage(_img, curScreen);

                //Clipboard.SetImage(_img);
                //OneSnipTray.confirmImageInClipboard();
            }

            //close overlay
            this.Close();
        }

        private void Snipper_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}

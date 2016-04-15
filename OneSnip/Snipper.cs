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



        //This variable control when you start the right click
        bool start = false;
        public Snipper(Screen _screen, Snip _snip)
        {
            InitializeComponent();
            curScreen = _screen;
            snip = _snip;
            this.Location = curScreen.WorkingArea.Location;
            this.ShowInTaskbar = false;
        }

        private void Snipper_Load(object sender, EventArgs e)
        {
            //Hide the Form
            this.Hide();

            pictureBox1.Size = new System.Drawing.Size(this.Width, this.Height);

            //Show Form
            this.Show();
            //Cross Cursor
            Cursor = Cursors.Cross;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            //validate if there is an image
            if (pictureBox1.Image == null)
                //return;
            //validate if right-click was trigger
            if(start)
            {
                //refresh picture box
                pictureBox1.Refresh();

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
                } else
                {
                    newSelectX = selectX;
                    newWidth = selectWidth;
                }

                if (selectHeight < 0)
                {
                    newSelectY = selectY - Math.Abs(selectHeight);
                    newHeight = Math.Abs(selectHeight);
                } else
                {
                    newSelectY = selectY;
                    newHeight = selectHeight;
                }

                    //draw dotted rectangle
                    pictureBox1.CreateGraphics().DrawRectangle(selectPen, newSelectX, newSelectY, newWidth, newHeight);
            }
            
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            //validate if there is image
            if (pictureBox1.Image == null)
                //return;
            //same functionality when mouse is over
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                pictureBox1.Refresh();
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

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                //starts coordinates for rectangle
                selectX = e.X;
                selectY = e.Y;
                start = true;
            }
            //refresh picture box
            pictureBox1.Refresh();
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
                g.DrawImage(printscreen, 0, 0, rect, GraphicsUnit.Pixel);

                this.Close();
                snip.setImage(_img);

                //Clipboard.SetImage(_img);
                //OneSnipTray.confirmImageInClipboard();
            }

            //close overlay
            this.Close();
        }
    }
}

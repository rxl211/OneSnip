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
    public partial class Editor : Form
    {
        private bool drawing = false;
        private GraphicsPath drawPath = new GraphicsPath();
        private Bitmap image;
        private Graphics imageGraphics;
        private Screen originalScreen; //what screen was used to snip the image we're editing?
        private Stack<Bitmap> undoStack = new Stack<Bitmap>();

        private string filePath;

        //These variables control the mouse position
        int lastX;
        int lastY;

        public Editor(byte[] imageBuffer, string _filePath, Screen _originalScreen)
        {
            InitializeComponent();
            filePath = _filePath;
            originalScreen = _originalScreen;

            Image img;
            using (var ms = new MemoryStream(imageBuffer))
            {
                img = Image.FromStream(ms);
            }

            image = (Bitmap)img;

            pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBox1.Anchor = AnchorStyles.None;

            splitContainer1.MinimumSize = new Size(image.Width + 20, image.Height + splitContainer1.Panel1.Height);

            this.SetClientSizeCore(image.Width + 20, image.Height + splitContainer1.Panel1.Height + 20);

            pictureBox1.Image = image;
            pictureBox1.Location = new Point((pictureBox1.Parent.ClientSize.Width / 2) - (pictureBox1.Width / 2),
                                        (pictureBox1.Parent.ClientSize.Height / 2) - (pictureBox1.Height / 2));
            pictureBox1.Refresh();

            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            pictureBox1.MouseUp += PictureBox1_MouseUp;

            button_Undo.Enabled = false;

        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                //starts coordinates for rectangle
                lastX = e.X;
                lastY = e.Y;
                drawing = true;
                pictureBox1.Refresh();

                //add whatever the image currently is to the undo stack so we can revert later if needed
                Bitmap imageCopy = new Bitmap(image);
                undoStack.Push(imageCopy);

                //image = (Bitmap)pictureBox1.Image;
                imageGraphics = Graphics.FromImage(image);
            }
            //refresh picture box
            pictureBox1.Refresh();
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if(drawing)
            {
                Pen pen = new Pen(Color.Red, 4);

                GraphicsPath test = new GraphicsPath();

                drawPath.AddLine(lastX, lastY, e.X, e.Y);

                lastX = e.X;
                lastY = e.Y;

                imageGraphics.DrawPath(pen, drawPath);
                imageGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                imageGraphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                imageGraphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                imageGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                pictureBox1.Image = image;
            }
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            //allPaths.Add(drawPath);
            
            drawing = false;
            drawPath = new GraphicsPath();
            button_Undo.Enabled = true;
            //pictureBox1.Image = image;
        }

        private void button_Draw_Paint(object sender, PaintEventArgs e)
        {
            return;
        }

        private void button_Copy_Click(object sender, EventArgs e)
        {
            Clipboard.SetImage(image);
        }

        private async void button_UploadAndClose_Click(object sender, EventArgs e)
        {
            button_UploadAndClose.Enabled = false;
            button_UploadAndClose.Text = "Uploading...";
            button_UploadAndClose.Refresh();

            CloudManager cloudManager = OneSnipTray.getCloudManager();

            Graphics g = Graphics.FromImage(image);

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            //g.DrawImage(printscreen, 0, 0, rect, GraphicsUnit.Pixel);

            ImageResult imageResult = await cloudManager.handleImage(image, originalScreen, true);


            OneSnipTray.AddToClipboard(imageResult);
            OneSnipTray.balloonForNewLink(imageResult.link);

            this.Close();
        }

        private void button_Undo_Click(object sender, EventArgs e)
        {
            image = undoStack.Pop();
            pictureBox1.Image = image;
            pictureBox1.Refresh();

            if (undoStack.Count == 0)
            {
                button_Undo.Enabled = false;
            }
        }
    }
}

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

            pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize; //this makes sure the picturebox can fit the whole image (but it has nothing to do with the size of the window)
            pictureBox1.Anchor = AnchorStyles.None;
            pictureBox1.Dock = DockStyle.Fill;
            //bottomPanel.Margin = new Padding(20);

            //tableLayoutPanel1.MinimumSize = new Size(image.Width + 20, image.Height + topPanel.Height); //this doesn't seem to be doing anything?

            topPanel.AutoSize = true;
            topPanel.Width = image.Width;

            bottomPanel.AutoSize = true;
            bottomPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            //tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, image.Height));
            tableLayoutPanel1.Controls.Add(this.bottomPanel, 0, 1);

            tableLayoutPanel1.Size = new Size(image.Width, bottomPanel.Height + topPanel.Height);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            //this.SetClientSizeCore(image.Width, image.Height + topPanel.Height - 900); //this is setting the size of the window
            //this.ClientSize = new Size(image.Width, image.Height + topPanel.Height);


            //flowLayoutPanel1.Dock = DockStyle.Fill;

            for (int i = 0; i < flowLayoutPanel1.Controls.Count; i++)
            {
                Control b = flowLayoutPanel1.Controls[i];

                if (i < flowLayoutPanel1.Controls.Count - 1)
                {
                    b.Margin = new Padding(0, 0, 20, 0);
                } else
                {
                    b.Margin = new Padding(0, 0, 0, 0); //we don't want to add right-margin to the last element
                }
            }

            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Anchor = AnchorStyles.None;
            //flowLayoutPanel1.BackColor = Color.Green;

            
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.Dock = DockStyle.Top;

            tableLayoutPanel2.Controls.Add(flowLayoutPanel1);
            //tableLayoutPanel2.BackColor = Color.Pink;
            tableLayoutPanel2.Height = flowLayoutPanel1.Height;

            

            pictureBox1.Image = image;
            pictureBox1.Anchor = AnchorStyles.None;
            pictureBox1.Location = new Point((bottomPanel.Width / 2) - (pictureBox1.Width / 2),
                                         (bottomPanel.Height / 2) - (pictureBox1.Height / 2));

            //pictureBox1.Location = new Point(0, 0);
            pictureBox1.Refresh();

            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            pictureBox1.MouseUp += PictureBox1_MouseUp;

            Color mainFrame = Color.FromArgb(38, 38, 38);
            Color toolbar = Color.FromArgb(13, 13, 13);

            tableLayoutPanel2.BackColor = toolbar;
            bottomPanel.BackColor = mainFrame;
            tableLayoutPanel1.BackColor = mainFrame;


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
            if (drawing)
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
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                drawing = false;
                drawPath = new GraphicsPath();
                button_Undo.Enabled = true;
            }
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

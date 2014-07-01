using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace CebTest
{
    public partial class Form1 : Form
    {
        //private int origW; 
        //private int origH;

        public Size OrigSize { get; set; }
        public Size TarSize { get; set; }


        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void showButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openImage = new OpenFileDialog();
            DialogResult isGood = openImage.ShowDialog();

            if(isGood ==DialogResult.OK)
            {
                pictureBox1.Load(openImage.FileName);
                pictureBox1.SizeMode = PictureBoxSizeMode.Normal;

                int imgW = pictureBox1.Image.Width;
                int imgH = pictureBox1.Image.Height;

                OrigSize = new Size(imgW, imgH);
            }            
        }

        Image sbPerc(Image imgPh, int perc) 
        {
            double nPerc = ((double)perc / 100);

            int srcW = imgPh.Width; //origW;
            int srcH = imgPh.Height; //origH

            int srcX = 0;
            int srcY = 0;

            int destX = 0;
            int destY = 0;

            int destW = (int)(OrigSize.Width * nPerc);
            int destH = (int)(OrigSize.Height * nPerc);

            Bitmap bmPhoto = new Bitmap(destW, destH, PixelFormat.Format24bppRgb);

            bmPhoto.SetResolution(imgPh.HorizontalResolution, imgPh.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);

            grPhoto.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPh, new Rectangle(destX, destY, destW, destH), new Rectangle(srcX, srcY, srcW, srcH), GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }


        private void clearButton_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
        }


        private void closeButton_Click(object sender, EventArgs e)
        {
            zoomBar.Value = 100;
            this.Close();

        }

        //private void checkBox1_CheckedChanged(object sender, EventArgs e)
        //{

        //    if (checkBox1.Checked)
        //        pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
        //    else
        //        pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;

        //}

        private void zoomBar_Scroll(object sender, EventArgs e)
        {
            //bool isSynced = (zoomUpDown.Value.Equals(zoomBar.Value) ) ? true : false;
            
            //if (!isSynced)
            //{
                zoomUpDown.Value = zoomBar.Value;
                reSizePic((Int32)zoomBar.Value);
            //}
             
            
        }

        
        private void reSizePic(int inVal) 
        {
            int val = inVal;
            Image scaledImg = sbPerc(pictureBox1.Image, val);
            pictureBox1.Image = scaledImg;
                       
        }


        private void zoomUpDown_ValueChanged(object sender, EventArgs e)
        {
            ////bool isSynced = (zoomUpDown.Value.Equals(zoomBar.Value)) ? true : false;

            ////if (!isSynced)
            ////{
            //zoomBar.Value = (int)zoomUpDown.Value;
            //reSizePic((Int32)zoomUpDown.Value);
            ////}
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {

            bool dragging;
            MouseDown start = new MouseButtons();
            if (e.Button == MouseButtons.Left)
            {
                dragging = true;
                start = e.Location;
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Debug.WriteLine("mousemove X: " + e.X + " Y: " + e.Y);

                pictureBox1.Location = new Point(pictureBox1.Left + e.Location.X - start.X,
                    pictureBox1.Top + e.Location.Y - start.Y);
            }
        }


     
    }

    //public partial class SPBox : UserControl 
    //{
    //    private Image image;
    //    private bool center;
        
    //}
}

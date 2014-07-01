using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Windows.Forms.VisualStyles;
using System.IO;
//using System.Windows.Media;




namespace CebTest
{
    public partial class Form1 : Form
    {
        public Size OrigSize { get; set; }
        public Size TarSize { get; set; }
        private bool dragging = false;
        private Point start;
        private string imgFLoc;

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

                string tFn = openImage.FileName;

                if(Directory.Exists(tFn))
                {
                    imgFLoc = Path.GetDirectoryName(tFn);
                }
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
        }//scale by percentage

        private void clearButton_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            zoomBar.Value = 100;
            this.Close();

        }

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
                //Debug.WriteLine("mousemove X: " + e.X + " Y: " + e.Y);

                pictureBox1.Location = new Point(pictureBox1.Left + e.Location.X - start.X,
                    pictureBox1.Top + e.Location.Y - start.Y);
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void processImageButton_Click(object sender, EventArgs e)
        {
            Image tempImg = pictureBox1.Image;
            Bitmap sImg = null;
            int thresh = default(int);
            string inputTVal;
            try
            {
                inputTVal = Microsoft.VisualBasic.Interaction.InputBox("Input a threshhold value:", "Gimmie input!", "15");
                thresh = Convert.ToInt32(inputTVal);

                procImage(thresh, tempImg);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            
            if (tempImg != null)
            {
                try{sImg = procImage(thresh, tempImg); sImg.Save(@"E:\Users\Steven\Downloads\Processed_img001.png"); }
                catch (Exception ex){ MessageBox.Show(ex.Message); }                
            }
            else
            {
                MessageBox.Show("No image was selected!\nPlease select an image", "Cannot process!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }            
        }

        public struct boardPixel
        {
            private int wLoc, hLoc;
            public boardPixel(int widthlocation, int heightlocation) {wLoc = widthlocation; hLoc = heightlocation;}             
            public int WLOC{get{return wLoc;} set{wLoc = value;}}
            public int HLOC{get{return hLoc;} set{hLoc = value;}}        
        }

        public static Bitmap convr2GrayScale(Bitmap OrigImg) 
        {
            Bitmap newTBitmap = new Bitmap(OrigImg.Width, OrigImg.Height);

            Graphics newGraphic = Graphics.FromImage(newTBitmap);

            ColorMatrix cM = new ColorMatrix(new float[][] {
                new float[] {.3f, .3f, .3f, 0, 0}, 
                new float[] {.59f, .59f, .59f, 0, 0},
                new float[] {.11f, .11f, .11f, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {0, 0, 0, 0, 1}});

            ImageAttributes atts = new ImageAttributes();

            atts.SetColorMatrix(cM);

            newGraphic.DrawImage(OrigImg, new Rectangle(0, 0, OrigImg.Width, OrigImg.Height), 0, 0, OrigImg.Width, OrigImg.Height, GraphicsUnit.Pixel, atts);

            newGraphic.Dispose();

            return newTBitmap;        
        }

        public Bitmap procImage(int threshHoldDiff, Image inImage) 
        {
            Bitmap retImag = new Bitmap(convr2GrayScale(new Bitmap(inImage)));
            Bitmap retImag2 = new Bitmap(convr2GrayScale(new Bitmap(inImage)));

            //int maxWidth = retImag.Width;
            //int maxHeight = retImag.Height;
            
            List<boardPixel> pBorders = new List<boardPixel>();

            for (int w = 0; w < OrigSize.Width; w++)
            {
                for (int h = 0; h < OrigSize.Height; h++)
                {
                    Color[] evalArray = getEvalArray(w, h, retImag);

                    //Color threshPix = retImag.GetPixel(w, h); 

                    if (evalMe(retImag.GetPixel(w, h), evalArray, threshHoldDiff))
                    {
                        retImag2.SetPixel(w, h, Color.Red);
                        pBorders.Add(new boardPixel(w, h));
                    }
                }
            }
            printToFile(pBorders);
            return retImag2;
        }

        public Color[] getEvalArray(int currW, int currH, Bitmap srcPic)
        {

            Color[] retArray;

            //retArray = new Color[9];
            //retArray[0] = srcPic.GetPixel(3, 4);
            try
            {
                if (((currW >= 1) && (currH >= 1)) && (currW < OrigSize.Width - 1) && (currH < OrigSize.Height - 1)) // normal case(no sides/corners)
                {
                    int rASize = 9;
                    retArray = new Color[rASize];
                    for (int w = currW - 1; w <= currW + 1; w++)
                    {
                        for (int h = currH - 1; h <= currH + 1; h++)
                        {
                            retArray[--rASize] = srcPic.GetPixel(w, h);
                        }
                    }
                    return retArray;
                }
                else if (currW == 0 && currH == 0)                     //top left corner (starting corner)
                {
                    int rASize = 3;
                    retArray = new Color[rASize];
                    retArray[--rASize] = srcPic.GetPixel(1, 0);
                    retArray[--rASize] = srcPic.GetPixel(1, 1);
                    retArray[--rASize] = srcPic.GetPixel(0, 1);

                    return retArray;
                }
                else if (currW == 0 && currH == OrigSize.Height - 1)     // bot left corner
                {
                    int rASize = 3;
                    retArray = new Color[rASize];
                    retArray[--rASize] = srcPic.GetPixel(0, OrigSize.Height - 2);
                    retArray[--rASize] = srcPic.GetPixel(1, OrigSize.Height - 2);
                    retArray[--rASize] = srcPic.GetPixel(1, OrigSize.Height - 1);

                    return retArray;
                }
                else if (currW == OrigSize.Width - 1 && currH == 0)       //top right corner
                {
                    int rASize = 3;
                    retArray = new Color[rASize];
                    retArray[--rASize] = srcPic.GetPixel(OrigSize.Width - 2, 0);
                    retArray[--rASize] = srcPic.GetPixel(OrigSize.Width - 2, 1);
                    retArray[--rASize] = srcPic.GetPixel(OrigSize.Width - 1, 1);

                    return retArray;
                }
                else if (currW == OrigSize.Width - 1 && currH == OrigSize.Height - 1)       //bot right corner
                {
                    int rASize = 3;
                    retArray = new Color[rASize];
                    retArray[--rASize] = srcPic.GetPixel(OrigSize.Width - 2, OrigSize.Height - 2);
                    retArray[--rASize] = srcPic.GetPixel(OrigSize.Width - 1, OrigSize.Height - 2);
                    retArray[--rASize] = srcPic.GetPixel(OrigSize.Width - 2, OrigSize.Height - 1);

                    return retArray;
                }
                else if (currW == OrigSize.Width - 1 && ((currH < OrigSize.Height - 1) && (currH > 0))) //right side
                {
                    int rASize = 5;
                    retArray = new Color[rASize];

                    retArray[--rASize] = srcPic.GetPixel(OrigSize.Width - 1, currH - 1);
                    retArray[--rASize] = srcPic.GetPixel(OrigSize.Width - 1, currH + 1);
                    retArray[--rASize] = srcPic.GetPixel(OrigSize.Width - 2, currH - 1);
                    retArray[--rASize] = srcPic.GetPixel(OrigSize.Width - 2, currH);
                    retArray[--rASize] = srcPic.GetPixel(OrigSize.Width - 2, currH + 1);

                    return retArray;
                }
                else if ((currW == 0) && ((currH < OrigSize.Height - 1) && (currH > 0))) //left side
                {
                    int rASize = 5;
                    retArray = new Color[rASize];

                    retArray[--rASize] = srcPic.GetPixel(0, currH - 1);
                    retArray[--rASize] = srcPic.GetPixel(0, currH + 1);
                    retArray[--rASize] = srcPic.GetPixel(1, currH - 1);
                    retArray[--rASize] = srcPic.GetPixel(1, currH);
                    retArray[--rASize] = srcPic.GetPixel(1, currH + 1);

                    return retArray;
                }
                else if (((currW > 0) && (currW < OrigSize.Width - 1)) && (currH == 0)) //top side
                {
                    int rASize = 5;
                    retArray = new Color[rASize];

                    retArray[--rASize] = srcPic.GetPixel(currW - 1, 0);
                    retArray[--rASize] = srcPic.GetPixel(currW - 1, 1);
                    retArray[--rASize] = srcPic.GetPixel(currW, 1);
                    retArray[--rASize] = srcPic.GetPixel(currW + 1, 0);
                    retArray[--rASize] = srcPic.GetPixel(currW + 1, 1);

                    return retArray;
                }

                else if (((currW > 0) && (currW < OrigSize.Width - 1)) && (currH == OrigSize.Height - 1))
                {
                    int rASize = 5;
                    retArray = new Color[rASize];

                    retArray[--rASize] = srcPic.GetPixel(currW - 1, OrigSize.Height - 2);
                    retArray[--rASize] = srcPic.GetPixel(currW - 1, OrigSize.Height - 1);
                    retArray[--rASize] = srcPic.GetPixel(currW, OrigSize.Height - 2);
                    retArray[--rASize] = srcPic.GetPixel(currW + 1, OrigSize.Height - 2);
                    retArray[--rASize] = srcPic.GetPixel(currW + 1, OrigSize.Height - 1);

                    return retArray;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "There was an error in the Color[] getEvalArray(int,int,BitMap) method!!");
                //return retArray = default(Color []);
            }
            return retArray = default(Color[]);
        }

        public Color[] getEvalArrayRightDown(int currW, int currH, Bitmap srcPic) 
        {
            Color[] retArray;
            
            //retArray = new Color[9];
            //retArray[0] = srcPic.GetPixel(3, 4);
            try
            {
                if (((currW >= 1) && (currH >= 1)) && (currW < OrigSize.Width - 1) && (currH < OrigSize.Height - 1)) // normal case(no sides/corners)
                {
                    int rASize = 2;
                    retArray = new Color[rASize];
                   
                    retArray[--rASize] = srcPic.GetPixel(currW, currH+1);
                    retArray[--rASize] = srcPic.GetPixel(currW + 1, currH);
                     
                    return retArray;
                }
                else if (currW == 0 && currH == 0)                     //top left corner (starting corner)
                {
                    int rASize = 2;
                    retArray = new Color[rASize];

                    retArray[--rASize] = srcPic.GetPixel(currW, currH + 1);
                    retArray[--rASize] = srcPic.GetPixel(currW + 1, currH);

                    return retArray;
                }
                else if (currW == 0 && currH == OrigSize.Height - 1)     // bot left corner
                {
                    int rASize = 1;
                    retArray = new Color[rASize];
                                        
                    retArray[--rASize] = srcPic.GetPixel(currW + 1, currH);

                    return retArray;
                }
                else if (currW == OrigSize.Width - 1 && currH == 0)       //top right corner
                {
                    int rASize = 1;
                    retArray = new Color[rASize];

                    retArray[--rASize] = srcPic.GetPixel(currW, currH + 1);
                    
                    return retArray;
                }
                else if (currW == OrigSize.Width - 1 && currH == OrigSize.Height - 1)       //bot right corner
                {
                    int rASize = 1;
                    retArray = new Color[rASize];
                   
                    retArray[--rASize] = srcPic.GetPixel(currW, currH);
                   
                    return retArray;
                }
                else if (currW == OrigSize.Width - 1 && ((currH < OrigSize.Height - 1) && (currH > 0))) //right side
                {
                    int rASize = 1;
                    retArray = new Color[rASize];
                   
                    retArray[--rASize] = srcPic.GetPixel(currW, currH+1);
                    

                    return retArray;
                }
                else if ((currW == 0) && ((currH < OrigSize.Height - 1) && (currH > 0))) //left side
                {
                    int rASize = 2;
                    retArray = new Color[rASize];
                   
                    retArray[--rASize] = srcPic.GetPixel(currW, currH+1);
                    retArray[--rASize] = srcPic.GetPixel(currW + 1, currH);

                    return retArray;
                }
                else if (((currW > 0) && (currW < OrigSize.Width - 1)) && (currH == 0)) //top side
                {
                    int rASize = 2;
                    retArray = new Color[rASize];
                   
                    retArray[--rASize] = srcPic.GetPixel(currW, currH+1);
                    retArray[--rASize] = srcPic.GetPixel(currW + 1, currH);

                    return retArray;
                }

                else if (((currW > 0) && (currW < OrigSize.Width - 1)) && (currH == OrigSize.Height - 1)) //bot
                {
                    int rASize = 1;
                    retArray = new Color[rASize];
                                       
                    retArray[--rASize] = srcPic.GetPixel(currW + 1, currH);

                    return retArray;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "There was an error in the Color[] getEvalArray(int,int,BitMap) method!!");
                //return retArray = default(Color []);
            }
            return retArray = default(Color[]);           
        }

        public bool evalMe(Color srcPix, Color [] threshPixels, int threshHoldDiff) 
        {
            bool isAboveThresh = false;
            int thresh = threshHoldDiff;

            for (int i = 0; !isAboveThresh && i < threshPixels.Length; i++)
            {
                if (thresh < Math.Abs(( srcPix.R - threshPixels[i].R) ))
                {
                    isAboveThresh = true;
                    return isAboveThresh ;
                }
            }
            return isAboveThresh;
        }



        //public int ToMyArgb(System.Drawing.Color color) 
        //{
        //    byte[] bytes = new byte[] { color.A, color.R, color.G, color.B };
        //    return BitConverter.ToInt32(bytes, 0);
        //}

        //public void printToFile(List<boardPixel> inList) 
        //{            
        //    //string reportFileLoc = 
        //    using (System.IO.StreamWriter file = new System.IO.StreamWriter(imgFLoc + "bMAP_Shape_locations.txt"))
        //    {
        //        foreach (boardPixel elem in inList)
        //        {
        //            file.WriteLine(elem);
        //        }
        //    }
        //}

        

       
    }


}


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
        public boardPixel[,] markedReds;

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

                markedReds = new boardPixel[imgW, imgH];

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
            Bitmap preProcImg = null;
            Bitmap postProcImg = null;
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
                try{
                    preProcImg = procImage(thresh, tempImg); preProcImg.Save(@"E:\Users\Steven\Downloads\Pre-Process_Img01.png");
                    postProcImg = getEvalArray2(3, 3, preProcImg); postProcImg.Save(@"E:\Users\Steven\Downloads\Post-Process_Img01.png");

                    MessageBox.Show("Success!! Image has been processed and stored in folder!", "Alright there!");
                    
                }
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
            private bool isFlagged;
            public boardPixel(int widthlocation, int heightlocation, bool marked) { wLoc = widthlocation; hLoc = heightlocation; isFlagged = marked; }
            public boardPixel(int widthlocation, int heightlocation) { wLoc = widthlocation; hLoc = heightlocation; isFlagged = false; }            
            public bool MARK { get { return isFlagged; } set { isFlagged = value;} }
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
            //printToFile(pBorders);
            return retImag2;
        }

        public Color[] getEvalArray(int currW, int currH, Bitmap srcPic)
        {

            Color[] retArray;

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

            }
            return retArray = default(Color[]);
        }

        public Bitmap getEvalArray2(int currW, int currH, Bitmap srcPic) 
        {
            Bitmap shapeLocMap = new Bitmap(srcPic);
           
                for (int w = currW; w < srcPic.Width; w++)
                {
                    for (int h = currH; h < srcPic.Height; h++)
                    {             
                        
                        Color currPixel = srcPic.GetPixel(w, h);

                        if (markedReds[w, h].MARK == false && currPixel.R >= 240)
                        {                            
                            boardPixel potenShape = new boardPixel(w, h, true);
                            
                            boardPixel[] sizeArray = new boardPixel[4];

                            sizeArray = shapeOutlineLocation(potenShape, srcPic);                            

                            shapeTarRectDrawer(shapeLocMap, sizeArray);

                            break;
                        }

                        
                        //if (sizeArray[0].MARK && sizeArray[1].MARK && sizeArray[2].MARK && sizeArray[3].MARK)
                        //{

                        //}
                    }
                }
            

            return shapeLocMap;
        } 

        public Bitmap shapeTarRectDrawer(Bitmap inMap, boardPixel[] minMaxs) 
        {            
            Image tempImg = new Bitmap(inMap);

            int topLW = minMaxs[3].WLOC;
            int topLH = minMaxs[1].HLOC;
            int recWidth = minMaxs[2].WLOC - minMaxs[3].WLOC;
            int recHeight = minMaxs[0].HLOC - minMaxs[1].HLOC;

            //System.Drawing.Graphics graphicsObj;

            //graphicsObj = this.CreateGraphics();
            
            Pen drawer = new Pen(System.Drawing.Color.Red, 5);
            Rectangle shapeRectLoc = new Rectangle(topLW, topLH, recWidth, recHeight);

            using (Graphics g = Graphics.FromImage(tempImg)) 
            {
                g.DrawRectangle(drawer, shapeRectLoc);
            }


            return new Bitmap(tempImg);
        }

        public Color[] getEvalArrayRightDown(int currW, int currH, Bitmap srcPic) 
        {
            Color[] retArray;       

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

        public boardPixel[] shapeOutlineLocation(boardPixel firstRedFound, Bitmap srcPic) 
        {
            boardPixel[] minMaxLocArray = new boardPixel[4];
            Queue<boardPixel> nextReds = new Queue<boardPixel>();
            Bitmap shapelocation = new Bitmap(srcPic);

            //element 0: max height 
            //element 1: min height
            //element 2: max width
            //element 3: min width
            for (int i=0; i < 4; i++) //setting initial min/max values based on first red found
            {
                minMaxLocArray[i].HLOC = firstRedFound.HLOC;
                minMaxLocArray[i].WLOC = firstRedFound.WLOC;
            }
            firstRedFound.MARK= true;

            nextReds.Enqueue(firstRedFound);

            foreach ( boardPixel spot in nextReds)
            {
                int startW = spot.WLOC;
                int startH = spot.HLOC;

                for(int w = startW; w < startW +2; w++)
                {
                    for(int h =startH -1; h < startH +2; h ++)
                    {
                        Color evalC = srcPic.GetPixel(w, h);
                        if (evalC.Equals(Color.Red)) 
                        {

                            if (!nextReds.Contains(new boardPixel(w, h, true)))      //checking to see if value is not already in queue. 
                            {
                                nextReds.Enqueue(new boardPixel(w, h, true));       //adding to queue. 
                                if (h > minMaxLocArray[0].HLOC)
                                    minMaxLocArray[0].HLOC = h; minMaxLocArray[0].WLOC = w;
                                if( h < minMaxLocArray[0].HLOC)
                                    minMaxLocArray[1].HLOC= h; minMaxLocArray[1].WLOC = w;
                                if (w > minMaxLocArray[2].WLOC)
                                    minMaxLocArray[2].HLOC = h; minMaxLocArray[2].WLOC = w;
                                if (w < minMaxLocArray[3].WLOC)
                                    minMaxLocArray[3].HLOC = h; minMaxLocArray[3].WLOC = w;


                                markedReds[w, h].MARK = true;

                            }

                        }

                    }
                }

                nextReds.Dequeue();
            }            

            return minMaxLocArray;
        } 

        public void printToFile(List<boardPixel> inList) 
        {            
            //string reportFileLoc = 
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(imgFLoc + "bMAP_Shape_locations.txt"))
            {
                foreach (boardPixel elem in inList)
                {
                    file.WriteLine(elem);
                }
            }
        }

        

       
    }


}


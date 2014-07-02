/*
 *  Program:    CebTest(Cerberus Test)
 *  Client:     Visionetic
 *  @Author:    Steven Etienne
 *  Purpose:    
 *              An imaging processing program. Will identify shapes in an image based on difference in RGB pixel channels
 *              Outlines the image and boxes them in. Outputs results of 1st and 2nd processed image as well as shape co-ordinates
 *              to file. 
 *              Allows user to select image of choice.
 *              Allows user to Zoom/Pan image.
 *              Allows user to clear existing image.
 *              
 * @version: 0.8 7/2/2014
 */

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


namespace CebTest
{
    public partial class Form1 : Form
    {
        public Size OrigSize { get; set; }
        public Size TarSize { get; set; }
        private bool dragging = false;
        private Point start = Point.Empty;
        private Point target = Point.Empty;
        private string imgFLoc;
        private string destFLoc;
        public boardPixel[,] markedPossibleShaps;
        List<ShapeLoc> shapeXYminMaxLocs;

        //class for holding shape location
        public class ShapeLoc
        {
            public int minW, minH, maxW, maxH;
            public boardPixel minPW, minPH, maxPW, maxPH;
        }

        //class for recording pixel locations and first time and second time processing information
        public class boardPixel
        {
            private int wLoc, hLoc;
            private bool isFlaggedRed, isFlaggedBlue;    
            public bool MARKRED { get { return isFlaggedRed; } set { isFlaggedRed = value; } }
            public bool MARKEDBLUE { get { return isFlaggedBlue; } set { isFlaggedBlue = value; } }
            public int WLOC { get { return wLoc; } set { wLoc = value; } }
            public int HLOC { get { return hLoc; } set { hLoc = value; } }
        }

        public Form1(){ InitializeComponent();}

        private void Form1_Load(object sender, EventArgs e){}
                
        private void showButton_Click(object sender, EventArgs e)
        {            
            OpenFileDialog openImage = new OpenFileDialog();
            openImage.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif|All Images|*.BMP;*.DIB;*.RLE;*.JPG;*.JPEG;*.JPE;*.JFIF;*.GIF;*.TIF;*.TIFF;*.PNG";
            
            openImage.FilterIndex = 5;
            DialogResult isGood = openImage.ShowDialog();

            if(isGood ==DialogResult.OK)
            {
                pictureBox1.Load(openImage.FileName);
                pictureBox1.SizeMode = PictureBoxSizeMode.Normal;
                
                int imgW = pictureBox1.Image.Width;
                int imgH = pictureBox1.Image.Height;

                //initializing 2-d array representative of known marked pixels
                markedPossibleShaps = new boardPixel[imgW, imgH];
                for (int w = 0; w < imgW; w++) 
                {
                    for (int h = 0; h < imgH; h++) 
                    {
                        markedPossibleShaps[w, h] = new boardPixel { 
                            WLOC = w, HLOC = h,
                            MARKEDBLUE = false, MARKRED = false
                        };
                    }
                }

                OrigSize = new Size(imgW, imgH);
                imgFLoc = Path.GetDirectoryName(openImage.FileName);                
            }
    
            shapeXYminMaxLocs = new List<ShapeLoc>();
            zoomBar.Value = 100;
            zoomUpDown.Value = 100;
        }

        //scale image by percentage (zoom feature)
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
            OrigSize = new Size(0, 0);
            TarSize = new Size(0,0);
            dragging = false;
            imgFLoc = "";
            destFLoc = "";
            markedPossibleShaps= new boardPixel[0,0];
            if(shapeXYminMaxLocs.Any()){shapeXYminMaxLocs.Clear();}
        }

        private void closeButton_Click(object sender, EventArgs e)
        {this.Close(); }

        private void zoomBar_Scroll(object sender, EventArgs e)
        {           
                zoomUpDown.Value = zoomBar.Value;
                reSizePic((Int32)zoomBar.Value);
        }
        
        private void reSizePic(int inVal) 
        {
            int val = inVal;
            Image scaledImg = sbPerc(pictureBox1.Image, val);
            pictureBox1.Image = scaledImg;                       
        }

        private void zoomUpDown_ValueChanged(object sender, EventArgs e){}       

        public bool allChanEval() { return (MessageBox.Show("Evaluate by all channels? If no is selected,\nI will evaluate by just the red channel.", "Select channel evaluation:", MessageBoxButtons.YesNo) == DialogResult.Yes); }

        public void preEvalChooses(bool allChn)
        {
            Image tempImg = pictureBox1.Image;
            Bitmap preProcImg = null;
            Bitmap postProcImg = null;
            int thresh = default(int);
            string inputTVal = Microsoft.VisualBasic.Interaction.InputBox("Input a threshhold value:", "Gimmie input!", "1");

            if (int.TryParse(inputTVal, out thresh) && thresh >= 1 && thresh <= 255)
            {
                procImage(thresh, tempImg, allChn);

                if (tempImg != null)
                {
                    createNewFolder();
                    preProcImg = procImage(thresh, tempImg, allChn); preProcImg.Save(@"" + destFLoc + @"\Pre-Process_Img01.png");
                    postProcImg = findShapes(preProcImg); postProcImg.Save(destFLoc + @"\Post-Process_Img01.png");

                    MessageBox.Show("Success!! Image has been processed and stored in folder!", "Alright there!");
                }
                else { MessageBox.Show("No image was selected!\nPlease select an image", "Cannot process!", MessageBoxButtons.OK, MessageBoxIcon.Error);}
            }
            else{MessageBox.Show("Cannot use this thresh-hold value!", "Invalid input!");}
        }

        private void processImageButton_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("You haven't selected an image yet...", "Hold on there!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else 
            {
                bool allChan = allChanEval();
                if (allChan)
                {
                    preEvalChooses(allChan);
                }
                else
                {
                    preEvalChooses(allChan);
                }
            }
        }       

        //not currently using this function for anything.. but is here just in case 
        //public static Bitmap convr2GrayScale(Bitmap OrigImg) 
        //{
        //    Bitmap newTBitmap = new Bitmap(OrigImg.Width, OrigImg.Height);

        //    Graphics newGraphic = Graphics.FromImage(newTBitmap);

        //    ColorMatrix cM = new ColorMatrix(new float[][] {
        //        new float[] {.3f, .3f, .3f, 0, 0}, 
        //        new float[] {.59f, .59f, .59f, 0, 0},
        //        new float[] {.11f, .11f, .11f, 0, 0},
        //        new float[] {0, 0, 0, 1, 0},
        //        new float[] {0, 0, 0, 0, 1}});

        //    ImageAttributes atts = new ImageAttributes();

        //    atts.SetColorMatrix(cM);

        //    newGraphic.DrawImage(OrigImg, new Rectangle(0, 0, OrigImg.Width, OrigImg.Height), 0, 0, OrigImg.Width, OrigImg.Height, GraphicsUnit.Pixel, atts);

        //    newGraphic.Dispose();

        //    return newTBitmap;        
        //}

        public Bitmap procImage(int threshHoldDiff, Image inImage, bool allChan) 
        {
            Bitmap retImag = new Bitmap(inImage);
            Bitmap retImag2 = new Bitmap(inImage);

            for (int w = 0; w < OrigSize.Width; w++)
            {
                for (int h = 0; h < OrigSize.Height; h++)
                {
                    Color[] evalArray = getEvalArray(w, h, retImag);

                    if (evalMe(retImag.GetPixel(w, h), evalArray, threshHoldDiff, allChan))
                    {
                        markedPossibleShaps[w, h].MARKRED = true;
                        retImag2.SetPixel(w, h, Color.Red);              
                    }
                }
            }            
            return retImag2;
        }

        //get array of all pixels arround pixel under evaluation
        public Color[] getEvalArray(int currW, int currH, Bitmap srcPic)
        {
            Color[] retArray;

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

            return retArray = default(Color[]);
        }    
        
        //get array of pixel beneath (height + 1) and pixel to the right(width +1)
        public Color[] getEvalArrayRightDown(int currW, int currH, Bitmap srcPic) 
        {
            Color[] retArray;       


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
         

            return retArray = default(Color[]);           
        }

        //Evaluate me. Determine if any pixels in array are above the thresh hold.
        public bool evalMe(Color srcPix, Color [] threshPixels, int threshHoldDiff, bool allChan) 
        {
            bool isAboveThresh = false;
            int thresh = threshHoldDiff;

            if (allChan)
            {
                for (int i = 0; !isAboveThresh && i < threshPixels.Length; i++)
                {
                    if (thresh < Math.Abs((srcPix.R - threshPixels[i].R)) || thresh < Math.Abs((srcPix.B - threshPixels[i].B)) || thresh < Math.Abs((srcPix.G - threshPixels[i].G)))
                    {
                        isAboveThresh = true;
                        return isAboveThresh;
                    }
                }
            }
            else
            {
                for (int i = 0; !isAboveThresh && i < threshPixels.Length; i++)
                {
                    if (thresh < Math.Abs((srcPix.R - threshPixels[i].R)))
                    {
                        isAboveThresh = true;
                        return isAboveThresh;
                    }
                }
            }
            return isAboveThresh;
        }        

        //Find the shapes.
        public Bitmap findShapes(Bitmap processedImg)
        {            
            Bitmap pImageTemp = processedImg;
          
            //the for loops are set to avoid the edges of the image
            //will most likely be an issue for shapes that are only
            //paritally in frame. 
            for (int pIW = 2; pIW < OrigSize.Width - 3; pIW++)
            {
                for (int pIH = 3; pIH < OrigSize.Height - 3; pIH++)
                {
                    if (markedPossibleShaps[pIW, pIH].MARKRED && !markedPossibleShaps[pIW, pIH].MARKEDBLUE)
                    {
                        Queue<boardPixel> someQueue = new Queue<boardPixel>();
                        markedPossibleShaps[pIW, pIH].MARKEDBLUE = true;
                        someQueue.Enqueue(markedPossibleShaps[pIW, pIH]);

                        //creating new shape bounds based on first red
                        ShapeLoc newShape = new ShapeLoc
                        {
                            minW = pIW,
                            minH = pIH,
                            maxW = pIW,
                            maxH = pIH,
                            maxPH = markedPossibleShaps[pIW, pIH],
                            minPH = markedPossibleShaps[pIW, pIH],
                            maxPW = markedPossibleShaps[pIW, pIH],
                            minPW = markedPossibleShaps[pIW, pIH]
                        };

                        while (someQueue.Any())
                        {
                            //getting pixel from somequeue holding values
                            boardPixel currPixel = someQueue.Dequeue();
                            //setting min/max based if pixel in shape has smaller/larger value
                            if (currPixel.WLOC > newShape.maxW) { newShape.maxW = currPixel.WLOC; newShape.maxPW = currPixel; }
                            if (currPixel.WLOC < newShape.minW) { newShape.minW = currPixel.WLOC; newShape.minPW = currPixel; }
                            if (currPixel.HLOC > newShape.maxH) { newShape.maxH = currPixel.HLOC; newShape.maxPH = currPixel; }
                            if (currPixel.HLOC < newShape.minH) { newShape.minH = currPixel.HLOC; newShape.minPH = currPixel; }

                            findShapeMinMaxClockwise(currPixel.WLOC, currPixel.HLOC, someQueue);
                        }
                        shapeXYminMaxLocs.Add(newShape);
                    }
                }
            }

            printSLocsToTextFile();

            int shapeNumber = 1;
            while (shapeXYminMaxLocs.Any()) 
            {
                shapeTarRectDrawer(pImageTemp, shapeNumber ++);
                shapeXYminMaxLocs.RemoveAt(0);
            }

            return pImageTemp;
        }

        //check if location passed is inside bounds
        public bool checkPixel(int w, int h) 
        {
                return w > 1 &&
                    w < OrigSize.Width - 2 &&
                   h > 1 &&
                   h < OrigSize.Height - 2 &&
                    markedPossibleShaps[w, h].MARKRED &&
                    !markedPossibleShaps[w, h].MARKEDBLUE;
         
        }        

        //searching clockwise direction for other marked red pixels
        //if found marking blue
        public void findShapeMinMaxClockwise(int inW, int inH,  Queue<boardPixel> sQueue)
        {

                if (checkPixel(inW + 1, inH))
                {
                    markedPossibleShaps[inW + 1, inH].MARKEDBLUE = true;
                    sQueue.Enqueue(markedPossibleShaps[inW + 1, inH]);
                }

                if (checkPixel(inW + 1, inH + 1))
                {
                    markedPossibleShaps[inW + 1, inH + 1].MARKEDBLUE = true;
                    sQueue.Enqueue(markedPossibleShaps[inW + 1, inH + 1]);
                }

                if (checkPixel(inW, inH + 1))
                {
                    markedPossibleShaps[inW, inH + 1].MARKEDBLUE = true;
                    sQueue.Enqueue(markedPossibleShaps[inW, inH + 1]);
                }

                if (checkPixel(inW - 1, inH + 1))
                {
                    markedPossibleShaps[inW - 1, inH + 1].MARKEDBLUE = true;
                    sQueue.Enqueue(markedPossibleShaps[inW - 1, inH + 1]);
                }

                if (checkPixel(inW - 1, inH))
                {
                    markedPossibleShaps[inW - 1, inH ].MARKEDBLUE = true;
                    sQueue.Enqueue(markedPossibleShaps[inW - 1, inH]);
                }

                if (checkPixel(inW - 1, inH - 1))
                {
                    markedPossibleShaps[inW - 1, inH - 1].MARKEDBLUE = true;
                    sQueue.Enqueue(markedPossibleShaps[inW - 1, inH - 1]);
                }

                if (checkPixel(inW, inH - 1))
                {
                    markedPossibleShaps[inW, inH - 1].MARKEDBLUE = true;
                    sQueue.Enqueue(markedPossibleShaps[inW, inH - 1]);
                }

                if (checkPixel(inW + 1, inH - 1))
                {
                    markedPossibleShaps[inW + 1, inH - 1].MARKEDBLUE = true;
                    sQueue.Enqueue(markedPossibleShaps[inW + 1, inH - 1]);
                }
        }

       //retangle drawer
        public void shapeTarRectDrawer(Bitmap inMap, int sNum)
        {
            ShapeLoc currShape = shapeXYminMaxLocs.First();

            Pen drawer = new Pen(System.Drawing.Color.Blue, 1);            

            Rectangle shapeRectLoc = new Rectangle(currShape.minW, currShape.minH, currShape.maxW - currShape.minW , currShape.maxH - currShape.minH);

            using (Graphics g = Graphics.FromImage(inMap))
            {
                g.DrawRectangle(drawer, shapeRectLoc);
                g.DrawString("" + sNum, new Font("Arial", 9), Brushes.Blue, new Point(currShape.minW + 3, currShape.minH + 3));
            }            
        }

        //List<ShapeLoc> shapeXYminMaxLocs;
        public void printSLocsToTextFile() 
        {            
 
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(destFLoc + @"\bMAP_Shape_locations.txt"))
            {
                int num = 1;
                foreach (ShapeLoc shape in shapeXYminMaxLocs)
                {
                    file.WriteLine("Shape " + num + ":");
                    file.WriteLine("\nMin Width Location: " + shape.minW);
                    file.WriteLine("\nMax Width Location: " + shape.maxW);
                    file.WriteLine("\nMin Height Location: " + shape.minH);
                    file.WriteLine("\nMax Height Location: " + shape.minH);
                    file.WriteLine("\n(=============================)\n\n");
                    num++;
                }
            }
        }

        public void createNewFolder() 
        {
            destFLoc = imgFLoc + @"\Processed_Results";

            DirectoryInfo f = Directory.CreateDirectory(destFLoc);

        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            //if (e.Button == MouseButtons.Left)
            start = new Point(e.Location.X - target.X, e.Location.Y - target.Y); 
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                target = new Point(e.Location.X - start.X, e.Location.Y - start.Y);
                pictureBox1.Invalidate();
            }
           
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        { dragging = false; }

        private void pictureBox1_Paint(object sender, PaintEventArgs e) 
        {
            if (pictureBox1.Image != null)
            {
                e.Graphics.Clear(Color.Transparent);
                e.Graphics.DrawImage(pictureBox1.Image, target);
            }
        }


    }


}


using System;
using System.Collections.Generic;

using System.Drawing;
using System.Windows.Forms;

using APNGLib;

namespace APNGViewer
{
    public class APNGBox : PictureBox
    {
        public int CurrentFrameNumber { get; private set; }
        public IList<Bitmap> Images { get; private set; }

        private APNG APNGFile;
        private Timer timer;

        private uint playthroughs;

        public APNGBox(APNG png)
        {
            APNGFile = png;
            CurrentFrameNumber = 0;
            Images = new List<Bitmap>();
            InitImages();
            Image = Images[0];
            Size = Image.Size;
            playthroughs = 0;
            if (APNGFile.IsAnimated)
            {
                timer = new Timer();
                timer.Interval = APNGFile.GetFrame(0).Milliseconds;
                timer.Tick += new EventHandler(timer_Tick);
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (APNGFile.MaxPlays == 0 || playthroughs < APNGFile.MaxPlays)
            {
                NextImage();
                Frame f = APNGFile.GetFrame(CurrentFrameNumber);
                timer.Interval = f.Milliseconds;
            }
        }

        private void InitImages()
        {
            if (APNGFile.IsAnimated)
            {
                Bitmap current = new Bitmap((int)APNGFile.Width, (int)APNGFile.Height);
                Bitmap previous = null;

                ImageRender.RenderNextFrame(current, Point.Empty, APNGFile.ToBitmap(0), Frame.BlendOperation.SOURCE);
                Images.Add(new Bitmap(current));

                for (int i = 1; i < APNGFile.FrameCount; i++)
                {
                    APNGLib.Frame oldFrame = APNGFile.GetFrame(i - 1);
                    Bitmap prev = previous == null ? null : new Bitmap(previous);
                    if (oldFrame.DisposeOp != APNGLib.Frame.DisposeOperation.PREVIOUS)
                    {
                        previous = new Bitmap(current);
                    }
                    ImageRender.DisposeBuffer(current, new Rectangle((int)oldFrame.XOffset, (int)oldFrame.YOffset, (int)oldFrame.Width, (int)oldFrame.Height), oldFrame.DisposeOp, prev);
                    APNGLib.Frame currFrame = APNGFile.GetFrame(i);
                    ImageRender.RenderNextFrame(current, new Point((int)currFrame.XOffset, (int)currFrame.YOffset), APNGFile.ToBitmap(i), currFrame.BlendOp);
                    Images.Add(new Bitmap(current));
                }
            }
            else
            {
                Images.Add(APNGFile.ToBitmap());
            }
        }

        public void NextImage()
        {
            CurrentFrameNumber++;
            if (CurrentFrameNumber >= APNGFile.FrameCount)
            {
                playthroughs++;
                CurrentFrameNumber = 0;
            }
            Image = Images[CurrentFrameNumber];
            
        }

        public void ToImage(int index)
        {
            Image = Images[index];
            CurrentFrameNumber = index;
        }

        public void Start()
        {
            if (timer != null)
            {
                timer.Start();
            }
        }

        public void Stop()
        {
            if (timer != null)
            {
                timer.Stop();
            }
        }
    }
}

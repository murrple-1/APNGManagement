using System;
using System.Collections.Generic;
using System.Text;

using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Input;

using APNGLib;


namespace APNGViewer_OpenGL
{
    class Program : GameWindow
    {
        public int CurrentFrameNumber { get; private set; }
        public IList<int> Textures { get; private set; }

        private APNG APNGFile;

        private int numPlays = 0;
        private double elapsedTime = 0.0;

        public Program(APNG png)
            : base(400, 300, GraphicsMode.Default, "APNG Demo")
        {
            APNGFile = png;
            CurrentFrameNumber = 0;
            Textures = new List<int>();
        }

        private void InitImages()
        {
            if (APNGFile.IsAnimated)
            {
                Bitmap current = new Bitmap((int)APNGFile.Width, (int)APNGFile.Height);
                Bitmap previous = null;

                ImageRender.RenderNextFrame(current, Point.Empty, APNGFile.ToBitmap(0), Frame.BlendOperation.SOURCE);
                Textures.Add(GenerateTexture(current));

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
                    Textures.Add(GenerateTexture(current));
                    if (prev != null)
                    {
                        prev.Dispose();
                    }
                }
                current.Dispose();
                if (previous != null)
                {
                    previous.Dispose();
                }
            }
            else
            {
                Bitmap bm = APNGFile.ToBitmap();
                Textures.Add(GenerateTexture(bm));
                bm.Dispose();
            }
        }

        private int GenerateTexture(Bitmap bm)
        {
            int texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);

            System.Drawing.Imaging.BitmapData data = bm.LockBits(
                new System.Drawing.Rectangle(0, 0, bm.Width, bm.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, 
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bm.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            return texture;
        }

        public void NextImage()
        {
            if (APNGFile.MaxPlays == 0 || numPlays < APNGFile.MaxPlays)
            {
                CurrentFrameNumber++;
                if (CurrentFrameNumber >= APNGFile.FrameCount)
                {
                    CurrentFrameNumber = 0;
                    numPlays++;
                }
            }
        }

        public void ToImage(int index)
        {
            CurrentFrameNumber = index;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            InitImages();
        }
        
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Width, Height);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            if (APNGFile.IsAnimated)
            {
                elapsedTime += e.Time;
                if (elapsedTime >= APNGFile.GetFrame(CurrentFrameNumber).Seconds)
                {
                    NextImage();
                    elapsedTime = 0.0;
                }
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.ClearColor(Color.White);
            GL.BindTexture(TextureTarget.Texture2D, Textures[CurrentFrameNumber]);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
            
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(-1.0f, -1.0f);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(1.0f, -1.0f);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(1.0f, 1.0f);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(-1.0f, 1.0f);
            GL.End();
            SwapBuffers();
        }

        [STAThread]
        static void Main()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        APNGLib.APNG png = new APNGLib.APNG();
                        using (Stream s = File.OpenRead(ofd.FileName))
                        {
                            png.Load(s);
                        }
                        using (Program prog = new Program(png))
                        {
                            prog.Run();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }
                    
                }
            }
        }
    }
}

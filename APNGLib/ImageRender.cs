using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace APNGLib
{
    public static class ImageRender
    {
        public static void DisposeBuffer(Bitmap buffer, Rectangle region, APNGLib.Frame.DisposeOperation dispose, Bitmap prevBuffer)
        {
            using (Graphics g = Graphics.FromImage(buffer))
            {
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;

                Brush b = new SolidBrush(Color.Transparent);
                switch (dispose)
                {
                    case APNGLib.Frame.DisposeOperation.NONE:
                        break;
                    case APNGLib.Frame.DisposeOperation.BACKGROUND:
                        g.FillRectangle(b, region);
                        break;
                    case APNGLib.Frame.DisposeOperation.PREVIOUS:
                        if (prevBuffer != null)
                        {
                            g.FillRectangle(b, region);
                            g.DrawImage(prevBuffer, region, region, GraphicsUnit.Pixel);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public static void RenderNextFrame(Bitmap buffer, Point point, Bitmap nextFrame, APNGLib.Frame.BlendOperation blend)
        {
            using (Graphics g = Graphics.FromImage(buffer))
            {
                switch (blend)
                {
                    case APNGLib.Frame.BlendOperation.OVER:
                        g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                        break;
                    case APNGLib.Frame.BlendOperation.SOURCE:
                        g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                        break;
                    default:
                        break;
                }
                g.DrawImage(nextFrame, point);
            }
        }

        public static void ClearFrame(Bitmap buffer)
        {
            using (Graphics g = Graphics.FromImage(buffer))
            {
                g.Clear(Color.Transparent);
            }
        }
    }
}

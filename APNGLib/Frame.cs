using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APNGLib
{
    public class Frame
    {
        public enum DisposeOperation
        {
            NONE, BACKGROUND, PREVIOUS
        }

        public enum BlendOperation
        {
            SOURCE, OVER
        }

        public fcTLChunk fctl { get; private set; }
        private IList<IDATChunk> idats;
        private IList<fdATChunk> fdats;
        public bool IFrame { get; set; }

        public IEnumerable<IDATChunk> IDATs
        {
            get
            {
                return (IEnumerable<IDATChunk>) idats;
            }
        }

        public IEnumerable<fdATChunk> fdATs
        {
            get
            {
                return (IEnumerable<fdATChunk>) fdats;
            }
        }

        public void AddChunk(IDATChunk i)
        {
            if (IFrame)
            {
                idats.Add(i);
            }
            else
            {
                throw new ApplicationException("Cannot add IDAT chunk to fdAT frame");
            }
        }

        public void AddChunk(fdATChunk f)
        {
            if (IFrame)
            {
                throw new ApplicationException("Cannot add fdAT chunk to IDAT frame");
            }
            else
            {
                fdats.Add(f);
            }
        }

        public uint Width
        {
            get
            {
                return fctl.Width;
            }

            set
            {
                fctl.Width = value;
            }
        }

        public uint Height
        {
            get
            {
                return fctl.Height;
            }
            set
            {
                fctl.Height = value;
            }
        }

        public uint XOffset
        {
            get
            {
                return fctl.XOffset;
            }
            set
            {
                fctl.XOffset = value;
            }
        }

        public uint YOffset
        {
            get
            {
                return fctl.YOffset;
            }
            set
            {
                fctl.YOffset = value;
            }
        }

        public ushort DelayNumerator
        {
            get
            {
                return fctl.DelayNumerator;
            }
            set
            {
                fctl.DelayNumerator = value;
                milliFlag = false;
                secFlag = false;
            }
        }

        public ushort DelayDenominator
        {
            get
            {
                return fctl.DelayDenominator;
            }
            set
            {
                fctl.DelayDenominator = value;
                milliFlag = false;
                secFlag = false;
            }
        }

        private bool milliFlag = false;
        private int milli;
        public int Milliseconds
        {
            get
            {
                if (!milliFlag)
                {
                    const int MillisecondsPerSecond = 1000;
                    milli = (int)(Seconds * MillisecondsPerSecond);
                    milliFlag = true;
                }
                return milli;
            }
        }

        private bool secFlag = false;
        private float sec;
        public float Seconds
        {
            get
            {
                if (!secFlag)
                {
                    sec = (float)DelayNumerator / (float)DelayDenominator;
                    secFlag = true;
                }
                return sec;
            }
        }

        public DisposeOperation DisposeOp
        {
            get
            {
                switch(fctl.DisposeOperation)
                {
                    case 0:
                        return DisposeOperation.NONE;
                    case 1:
                        return DisposeOperation.BACKGROUND;
                    case 2:
                        return DisposeOperation.PREVIOUS;
                    default:
                        throw new ApplicationException("Invalid Dispose Op");
                }
            }
            set
            {
                fctl.DisposeOperation = (byte)value;
            }
        }

        public BlendOperation BlendOp
        {
            get
            {
                switch (fctl.BlendOperation)
                {
                    case 0:
                        return BlendOperation.SOURCE;
                    case 1:
                        return BlendOperation.OVER;
                    default:
                        throw new ApplicationException("Invalid Blend Op");
                }
            }
            set
            {
                fctl.BlendOperation = (byte)value;
            }
        }

        public Frame(bool first, fcTLChunk fChunk)
        {
            IFrame = first;
            if (IFrame)
            {
                idats = new List<IDATChunk>();
            }
            else
            {
                fdats = new List<fdATChunk>();
            }
            fctl = fChunk;
        }

        public IList<byte> ImageData
        {
            get
            {
                IList<byte> value = new List<byte>();
                if (IFrame)
                {
                    foreach (IDATChunk id in idats)
                    {
                        foreach (byte b in id.ImageData)
                        {
                            value.Add(b);
                        }
                    }
                }
                else
                {
                    foreach (fdATChunk fd in fdats)
                    {
                        foreach (byte b in fd.FrameData)
                        {
                            value.Add(b);
                        }
                    }
                }
                return value;
            }
        }
    }
}

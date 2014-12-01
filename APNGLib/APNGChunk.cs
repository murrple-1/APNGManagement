using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APNGLib
{
    public class fcTLChunk : PNGChunk
    {
        public const String NAME = "fcTL";

        private ushort[] AcceptedDisposeOps = { 0, 1, 2 };
        private ushort[] AcceptedBlendOps = { 0, 1 };

        public override byte[] ChunkData
        {
            get
            {
                byte[] SeqNumA = PNGUtils.GetBytes(SequenceNumber);
                byte[] WidthA = PNGUtils.GetBytes(Width);
                byte[] HeightA = PNGUtils.GetBytes(Height);
                byte[] XOffsetA = PNGUtils.GetBytes(XOffset);
                byte[] YOffsetA = PNGUtils.GetBytes(YOffset);
                byte[] DelayNumA = PNGUtils.GetBytes(DelayNumerator);
                byte[] DelayDenA = PNGUtils.GetBytes(DelayDenominator);
                byte[] DisOpA = PNGUtils.GetBytes(DisposeOperation);
                byte[] BlendOpA = PNGUtils.GetBytes(BlendOperation);
                return PNGUtils.Combine(SeqNumA, WidthA, HeightA, XOffsetA, YOffsetA, DelayNumA, DelayDenA, DisOpA, BlendOpA);
            }
            set
            {
                int offset = 0;
                SequenceNumber = PNGUtils.ParseUint(value, ref offset);
                Width = PNGUtils.ParseUint(value, ref offset);
                Height = PNGUtils.ParseUint(value, ref offset);
                XOffset = PNGUtils.ParseUint(value, ref offset);
                YOffset = PNGUtils.ParseUint(value, ref offset);
                DelayNumerator = PNGUtils.ParseUshort(value, ref offset);
                DelayDenominator = PNGUtils.ParseUshort(value, ref offset);
                DisposeOperation = PNGUtils.ParseByte(value, ref offset);
                BlendOperation = PNGUtils.ParseByte(value, ref offset);

                if (XOffset < 0 || YOffset < 0 || Width <= 0 || Height <= 0)
                {
                    throw new ApplicationException("Frame size cannot be understood");
                }
                if (!AcceptedDisposeOps.Contains(DisposeOperation))
                {
                    throw new ApplicationException("Dispose Operation not supported");
                }
                if (!AcceptedBlendOps.Contains(BlendOperation))
                {
                    throw new ApplicationException("Blend Operation not supported");
                }
            }
        }

        public uint SequenceNumber { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }
        public uint XOffset { get; set; }
        public uint YOffset { get; set; }
        public ushort DelayNumerator { get; set; }
        public ushort DelayDenominator { get; set; }
        public byte DisposeOperation { get; set; }
        public byte BlendOperation { get; set; }

        public fcTLChunk() :
            base(NAME)
        {
        }
    }

    public class fdATChunk : PNGChunk
    {
        public const String NAME = "fdAT";

        public override byte[] ChunkData
        {
            get
            {
                byte[] SeqNumA = PNGUtils.GetBytes(SequenceNumber);
                return PNGUtils.Combine(SeqNumA, FrameData);
            }
            set
            {
                int offset = 0;
                SequenceNumber = PNGUtils.ParseUint(value, ref offset);
                FrameData = new byte[value.Length - offset];
                Array.Copy(value, offset, FrameData, 0, value.Length - offset);
            }
        }

        public uint SequenceNumber { get; set; }
        public byte[] FrameData { get; set; }

        public fdATChunk() :
            base(NAME)
        {
        }
    }

    public class acTLChunk : PNGChunk
    {
        public const String NAME = "acTL";

        public override byte[] ChunkData
        {
            get
            {
                byte[] numFramesA = PNGUtils.GetBytes(NumFrames);
                byte[] numPlaysA = PNGUtils.GetBytes(NumPlays);
                return PNGUtils.Combine(numFramesA, numPlaysA);
            }
            set
            {
                int offset = 0;
                NumFrames = PNGUtils.ParseUint(value, ref offset);
                NumPlays = PNGUtils.ParseUint(value, ref offset);
            }
        }

        public uint NumFrames { get; set; }
        public uint NumPlays { get; set; }

        public acTLChunk() :
            base(NAME)
        {
        }
    }
}

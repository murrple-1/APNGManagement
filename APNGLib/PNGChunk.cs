using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APNGLib
{
    public class PNGChunk
    {
        public uint ChunkLength
        {
            get
            {
                return (uint) ChunkData.Length;
            }
        }
        public string ChunkType { get; set; }
        private byte[] data;
        public virtual byte[] ChunkData { 
            get
            {
                return data;
            }
            set
            {
                data = value;
            }
        }
        public uint ChunkCRC
        {
            get
            {
                return CalculateCRC();
            }
        }

        public PNGChunk()
        {
        }

        public PNGChunk(string Type)
        {
            ChunkType = Type;
        }

        public byte[] Chunk
        {
            get
            {
                int offset = 0;
                byte[] dataBits = ChunkData;
                byte[] lengthBits = PNGUtils.GetBytes((uint)dataBits.Length);
                byte[] typeBits = System.Text.Encoding.UTF8.GetBytes(ChunkType);
                byte[] crcBits = PNGUtils.GetBytes(CalculateCRC());

                int length = lengthBits.Length + typeBits.Length + dataBits.Length + crcBits.Length;
                byte[] value = new byte[length];

                Array.Copy(lengthBits, 0, value, offset, lengthBits.Length);
                offset += lengthBits.Length;
                Array.Copy(typeBits, 0, value, offset, typeBits.Length);
                offset += typeBits.Length;
                Array.Copy(dataBits, 0, value, offset, dataBits.Length);
                offset += dataBits.Length;
                Array.Copy(crcBits, 0, value, offset, crcBits.Length);
                return value;
            }
        }

        public uint CalculateCRC()
        {
            uint calculatedCRC = CRC.INITIAL_CRC;

            calculatedCRC = CRC.UpdateCRC(calculatedCRC, Encoding.UTF8.GetBytes(ChunkType));
            calculatedCRC = CRC.UpdateCRC(calculatedCRC, ChunkData);

            calculatedCRC = ~calculatedCRC;
            return calculatedCRC;
        }

        protected static bool IsPrintable(char c)
        {
            return ((c >= 32 && c <= 126) || (c >= 161 && c <= 255));
        }

        protected static readonly byte[] NullSeparator = new byte[] { 0 };
    }

    public class IHDRChunk : PNGChunk
    {
        public const String NAME = "IHDR";

        private static readonly byte[] AllowedColorTypes = { 0, 2, 3, 4, 6 };

        public override byte[] ChunkData
        {
            get
            {
                byte[] WidthA = PNGUtils.GetBytes(Width);
                byte[] HeightA = PNGUtils.GetBytes(Height);
                byte[] theRest = new byte[] { BitDepth, ColorType, CompressionMethod, FilterMethod, InterlaceMethod };
                return PNGUtils.Combine(WidthA, HeightA, theRest);
            }
            set
            {
                int offset = 0;
                Width = PNGUtils.ParseUint(value, ref offset);
                Height = PNGUtils.ParseUint(value, ref offset);
                BitDepth = PNGUtils.ParseByte(value, ref offset);
                ColorType = PNGUtils.ParseByte(value, ref offset);
                if (!AllowedColorTypes.Contains(ColorType))
                {
                    throw new ApplicationException("Colour type is not supported");
                }
                CompressionMethod = PNGUtils.ParseByte(value, ref offset);
                FilterMethod = PNGUtils.ParseByte(value, ref offset);
                InterlaceMethod = PNGUtils.ParseByte(value, ref offset);
            }
        }

        public uint Width { get; set; }
        public uint Height { get; set; }
        public byte BitDepth { get; set; }
        public byte ColorType { get; set; }
        public byte CompressionMethod { get; set; }
        public byte FilterMethod { get; set; }
        public byte InterlaceMethod { get; set; }

        public IHDRChunk() :
            base(NAME)
        { }
    }

    public class PLTEChunk : PNGChunk
    {
        public const String NAME = "PLTE";

        public override byte[] ChunkData
        {
            get
            {
                byte[] value = new byte[0];
                foreach (Entry e in PaletteEntries)
                {
                    byte[] add = new byte[] { e.Red, e.Green, e.Blue };
                    value = PNGUtils.Combine(value, add);
                }
                return value;
            }
            set
            {
                if ((value.Length % 3) != 0)
                {
                    throw new ApplicationException("PLTE chunk length not divisible by 3");
                }
                int offset = 0;
                while (offset < value.Length)
                {
                    Entry e = new Entry();
                    e.Red = PNGUtils.ParseByte(value, ref offset);
                    e.Green = PNGUtils.ParseByte(value, ref offset);
                    e.Blue = PNGUtils.ParseByte(value, ref offset);
                    PaletteEntries.Add(e);
                }
                if (PaletteEntries.Count > 256)
                {
                    throw new ApplicationException("Too many entries on PLTE chunk");
                }
            }
        }

        public class Entry
        {
            public byte Red;
            public byte Green;
            public byte Blue;
        }

        public IList<Entry> PaletteEntries { get; set; }

        public PLTEChunk() :
            base(NAME)
        {
            PaletteEntries = new List<Entry>();
        }
    }

    public class IDATChunk : PNGChunk
    {
        public const String NAME = "IDAT";

        public override byte[] ChunkData
        {
            get
            {
                return ImageData;
            }
            set
            {
                ImageData = value;
            }
        }

        public byte[] ImageData { get; set; }

        public IDATChunk() :
            base(NAME)
        { }
    }

    public class IENDChunk : PNGChunk
    {
        public const String NAME = "IEND";

        public override byte[] ChunkData
        {
            get
            {
                return new byte[] { };
            }
            set
            {
                // do nothing
            }
        }

        public IENDChunk() :
            base(NAME)
        { }
    }

    public abstract class tRNSChunk : PNGChunk
    {
        public const String NAME = "tRNS";

        public tRNSChunk() :
            base(NAME)
        { }
    }

    public class tRNSChunkType0 : tRNSChunk
    {
        public override byte[] ChunkData
        {
            get
            {
                byte[] GreyA = PNGUtils.GetBytes(GreySample);
                return GreyA;
            }
            set
            {
                GreySample = PNGUtils.ParseUshort(value);
            }
        }

        public ushort GreySample { get; set; }

        public tRNSChunkType0() :
            base()
        { }
    }

    public class tRNSChunkType2 : tRNSChunk
    {
        public override byte[] ChunkData
        {
            get
            {
                byte[] RedA = PNGUtils.GetBytes(RedSample);
                byte[] BlueA = PNGUtils.GetBytes(BlueSample);
                byte[] GreenA = PNGUtils.GetBytes(GreenSample);
                return PNGUtils.Combine(RedA, BlueA, GreenA);
            }
            set
            {
                int offset = 0;
                RedSample = PNGUtils.ParseUshort(value, ref offset);
                BlueSample = PNGUtils.ParseUshort(value, ref offset);
                GreenSample = PNGUtils.ParseUshort(value, ref offset);
            }
        }

        public ushort RedSample { get; set; }
        public ushort BlueSample { get; set; }
        public ushort GreenSample { get; set; }

        public tRNSChunkType2() :
            base()
        { }
    }

    public class tRNSChunkType3 : tRNSChunk
    {
        public override byte[] ChunkData
        {
            get
            {
                return AlphaSettings;
            }
            set
            {
                AlphaSettings = value;
            }
        }

        public byte[] AlphaSettings { get; set; }

        public tRNSChunkType3() :
            base()
        { }
    }

    public class cHRMChunk : PNGChunk
    {
        public const String NAME = "cHRM";

        public override byte[] ChunkData
        {
            get
            {
                byte[] WhiteXA = PNGUtils.GetBytes(WhitePointX);
                byte[] WhiteYA = PNGUtils.GetBytes(WhitePointY);
                byte[] RedXA = PNGUtils.GetBytes(RedX);
                byte[] RedYA = PNGUtils.GetBytes(RedY);
                byte[] GreenXA = PNGUtils.GetBytes(GreenX);
                byte[] GreenYA = PNGUtils.GetBytes(GreenY);
                byte[] BlueXA = PNGUtils.GetBytes(BlueX);
                byte[] BlueYA = PNGUtils.GetBytes(BlueY);
                return PNGUtils.Combine(WhiteXA, WhiteYA, RedXA, RedYA, GreenXA, GreenYA, BlueXA, BlueYA);
            }
            set
            {
                int offset = 0;
                WhitePointX = PNGUtils.ParseUint(value, ref offset);
                WhitePointY = PNGUtils.ParseUint(value, ref offset);
                RedX = PNGUtils.ParseUint(value, ref offset);
                RedY = PNGUtils.ParseUint(value, ref offset);
                GreenX = PNGUtils.ParseUint(value, ref offset);
                GreenY = PNGUtils.ParseUint(value, ref offset);
                BlueX = PNGUtils.ParseUint(value, ref offset);
                BlueY = PNGUtils.ParseUint(value, ref offset);
            }
        }

        public uint WhitePointX { get; set; }
        public uint WhitePointY { get; set; }
        public uint RedX { get; set; }
        public uint RedY { get; set; }
        public uint GreenX { get; set; }
        public uint GreenY { get; set; }
        public uint BlueX { get; set; }
        public uint BlueY { get; set; }

        public cHRMChunk() :
            base(NAME)
        { }
    }

    public class gAMAChunk : PNGChunk
    {
        public const String NAME = "gAMA";

        public override byte[] ChunkData
        {
            get
            {
                byte[] GammaA = PNGUtils.GetBytes(Gamma);
                return GammaA;
            }
            set
            {
                Gamma = PNGUtils.ParseUint(value);
            }
        }

        public uint Gamma { get; set; }

        public gAMAChunk() :
            base(NAME)
        { }
    }

    public class iCCPChunk : PNGChunk
    {
        public const String NAME = "iCCP";

        public override byte[] ChunkData
        {
            get
            {
                byte[] NameA = Encoding.UTF8.GetBytes(Name);
                return PNGUtils.Combine(NameA, NullSeparator, new byte[] { CompressionMethod }, CompressionProfile);
            }
            set
            {
                int offset = 0;
                Name = PNGUtils.ParseString(value, ref offset);
                foreach (char c in Name)
                {
                    if (!IsPrintable(c))
                    {
                        throw new ApplicationException("Non-printable character in iCCP chunk name");
                    }
                }
                CompressionMethod = PNGUtils.ParseByte(value, ref offset);
                CompressionProfile = new byte[value.Length - offset];
                Array.Copy(value, offset, CompressionProfile, 0, CompressionProfile.Length - offset);
            }
        }

        public string Name { get; set; }
        public byte CompressionMethod { get; set; }
        public byte[] CompressionProfile { get; set; }

        public iCCPChunk() :
            base(NAME)
        { }
    }

    public abstract class sBITChunk : PNGChunk
    {
        public const String NAME = "sBIT";

        public sBITChunk() :
            base(NAME)
        { }
    }

    public class sBITChunkType0 : sBITChunk
    {
        public override byte[] ChunkData
        {
            get
            {
                return new byte[] { SignificantGreyscaleBits };
            }
            set
            {
                SignificantGreyscaleBits = PNGUtils.ParseByte(value);
            }
        }

        public byte SignificantGreyscaleBits { get; set; }

        public sBITChunkType0() :
            base()
        { }
    }

    public class sBITChunkType2 : sBITChunk
    {
        public override byte[] ChunkData
        {
            get
            {
                return new byte[] { SignificantRedBits, SignificantGreenBits, SignificantBlueBits };
            }
            set
            {
                int offset = 0;
                SignificantRedBits = PNGUtils.ParseByte(value, ref offset);
                SignificantGreenBits = PNGUtils.ParseByte(value, ref offset);
                SignificantBlueBits = PNGUtils.ParseByte(value, ref offset);
            }
        }

        public byte SignificantRedBits { get; set; }
        public byte SignificantGreenBits { get; set; }
        public byte SignificantBlueBits { get; set; }

        public sBITChunkType2() :
            base()
        { }
    }

    public class sBITChunkType3 : sBITChunk
    {
        public override byte[] ChunkData
        {
            get
            {
                return new byte[] { SignificantRedBits, SignificantGreenBits, SignificantBlueBits };
            }
            set
            {
                int offset = 0;
                SignificantRedBits = PNGUtils.ParseByte(value, ref offset);
                SignificantGreenBits = PNGUtils.ParseByte(value, ref offset);
                SignificantBlueBits = PNGUtils.ParseByte(value, ref offset);
            }
        }

        public byte SignificantRedBits { get; set; }
        public byte SignificantGreenBits { get; set; }
        public byte SignificantBlueBits { get; set; }

        public sBITChunkType3() :
            base()
        { }
    }

    public class sBITChunkType4 : sBITChunk
    {
        public override byte[] ChunkData
        {
            get
            {
                return new byte[] { SignificantGreyscaleBits, SignificantAlphaBits };
            }
            set
            {
                int offset = 0;
                SignificantGreyscaleBits = PNGUtils.ParseByte(value, ref offset);
                SignificantAlphaBits = PNGUtils.ParseByte(value, ref offset);
            }
        }

        public byte SignificantGreyscaleBits { get; set; }
        public byte SignificantAlphaBits { get; set; }

        public sBITChunkType4() :
            base()
        { }
    }

    public class sBITChunkType6 : sBITChunk
    {
        public override byte[] ChunkData
        {
            get
            {
                return new byte[] { SignificantRedBits, SignificantGreenBits, SignificantBlueBits, SignificantAlphaBits };
            }
            set
            {
                int offset = 0;
                SignificantRedBits = PNGUtils.ParseByte(value, ref offset);
                SignificantGreenBits = PNGUtils.ParseByte(value, ref offset);
                SignificantBlueBits = PNGUtils.ParseByte(value, ref offset);
                SignificantAlphaBits = PNGUtils.ParseByte(value, ref offset);
            }
        }

        public byte SignificantRedBits { get; set; }
        public byte SignificantGreenBits { get; set; }
        public byte SignificantBlueBits { get; set; }
        public byte SignificantAlphaBits { get; set; }

        public sBITChunkType6() :
            base()
        { }
    }

    public class sRGBChunk : PNGChunk
    {
        public const String NAME = "sRGB";

        public override byte[] ChunkData
        {
            get
            {
                byte[] IntentA = new byte[] { RenderingIntent };
                return IntentA;
            }
            set
            {
                RenderingIntent = PNGUtils.ParseByte(value);
            }
        }

        public byte RenderingIntent { get; set; }

        public sRGBChunk() :
            base(NAME)
        { }
    }

    public class tEXtChunk : PNGChunk
    {
        public const String NAME = "tEXt";

        public override byte[] ChunkData
        {
            get
            {
                byte[] KeywordA = Encoding.UTF8.GetBytes(Keyword);
                byte[] TextA = Encoding.UTF8.GetBytes(Text);
                return PNGUtils.Combine(KeywordA, NullSeparator, TextA);
            }
            set
            {
                int offset = 0;
                Keyword = PNGUtils.ParseString(value, ref offset);
                foreach (char c in Keyword)
                {
                    if (!IsPrintable(c))
                    {
                        throw new ApplicationException("Non-printable character in tEXT chunk keyword");
                    }
                }
                Text = PNGUtils.ParseString(value, ref offset);
            }
        }

        public string Keyword { get; set; }
        public string Text { get; set; }

        public tEXtChunk() :
            base(NAME)
        { }
    }

    public class zTXtChunk : PNGChunk
    {
        public const String NAME = "zTXt";

        public override byte[] ChunkData
        {
            get
            {
                byte[] KeywordA = Encoding.UTF8.GetBytes(Keyword);
                return PNGUtils.Combine(KeywordA, NullSeparator, new byte[] { CompressionMethod }, TextDatastream);
            }
            set
            {
                int offset = 0;
                Keyword = PNGUtils.ParseString(value, ref offset);
                foreach (char c in Keyword)
                {
                    if (!IsPrintable(c))
                    {
                        throw new ApplicationException("Non-printable character in zTXt chunk keyword");
                    }
                }
                CompressionMethod = PNGUtils.ParseByte(value, ref offset);

                TextDatastream = new byte[value.Length - offset];
                Array.Copy(value, offset, TextDatastream, 0, value.Length - offset);
            }
        }

        public string Keyword { get; set; }
        public byte CompressionMethod { get; set; }
        public byte[] TextDatastream { get; set; }

        public zTXtChunk() :
            base(NAME)
        { }
    }

    public class iTXtChunk : PNGChunk
    {
        public const String NAME = "iTXt";

        public override byte[] ChunkData
        {
            get
            {
                byte[] KeywordA = Encoding.UTF8.GetBytes(Keyword);
                byte[] TagA = Encoding.UTF8.GetBytes(LanguageTag);
                byte[] TransKeyA = Encoding.UTF8.GetBytes(TranslatedKeyword);
                byte[] TextA = Encoding.UTF8.GetBytes(Text);
                return PNGUtils.Combine(KeywordA, NullSeparator, new byte[] { CompressionFlag, CompressionMethod }, TagA, NullSeparator, TransKeyA, NullSeparator, TextA);
            }
            set
            {
                int offset = 0;
                Keyword = PNGUtils.ParseString(value, ref offset);
                foreach (char c in Keyword)
                {
                    if (!IsPrintable(c))
                    {
                        throw new ApplicationException("Non-printable character in iTXt chunk keyword");
                    }
                }
                CompressionFlag = PNGUtils.ParseByte(value, ref offset);
                CompressionMethod = PNGUtils.ParseByte(value, ref offset);
                LanguageTag = PNGUtils.ParseString(value, ref offset);
                TranslatedKeyword = PNGUtils.ParseString(value, ref offset);
                Text = PNGUtils.ParseString(value, ref offset);
            }
        }

        public string Keyword { get; set; }
        public byte CompressionFlag { get; set; }
        public byte CompressionMethod { get; set; }
        public string LanguageTag { get; set; }
        public string TranslatedKeyword { get; set; }
        public string Text { get; set; }

        public iTXtChunk() :
            base(NAME)
        { }
    }

    public abstract class bKGDChunk : PNGChunk
    {
        public const String NAME = "bKGD";

        public bKGDChunk() :
            base(NAME)
        { }
    }

    public class bKGDChunkType0 : bKGDChunk
    {
        public override byte[] ChunkData
        {
            get
            {
                byte[] greyA = PNGUtils.GetBytes(Greyscale);
                return greyA;
            }
            set
            {
                Greyscale = PNGUtils.ParseUshort(value);
            }
        }

        public ushort Greyscale { get; set; }

        public bKGDChunkType0() :
            base()
        { }
    }

    public class bKGDChunkType2 : bKGDChunk
    {
        public override byte[] ChunkData
        {
            get
            {
                byte[] RedA = PNGUtils.GetBytes(Red);
                byte[] GreenA = PNGUtils.GetBytes(Green);
                byte[] BlueA = PNGUtils.GetBytes(Blue);
                return PNGUtils.Combine(RedA, GreenA, BlueA);
            }
            set
            {
                int offset = 0;
                Red = PNGUtils.ParseUshort(value, ref offset);
                Green = PNGUtils.ParseUshort(value, ref offset);
                Blue = PNGUtils.ParseUshort(value, ref offset);
            }
        }

        public ushort Red { get; set; }
        public ushort Green { get; set; }
        public ushort Blue { get; set; }

        public bKGDChunkType2() :
            base()
        { }
    }

    public class bKGDChunkType3 : bKGDChunk
    {
        public override byte[] ChunkData
        {
            get
            {
                byte[] pIndexA = PNGUtils.GetBytes(PaletteIndex);
                return pIndexA;
            }
            set
            {
                PaletteIndex = PNGUtils.ParseByte(value);
            }
        }

        public byte PaletteIndex { get; set; }

        public bKGDChunkType3() :
            base()
        { }
    }

    public class bKGDChunkType4 : bKGDChunk
    {
        public override byte[] ChunkData
        {
            get
            {
                byte[] greyA = PNGUtils.GetBytes(Greyscale);
                return greyA;
            }
            set
            {
                Greyscale = PNGUtils.ParseUshort(value);
            }
        }

        public ushort Greyscale { get; set; }

        public bKGDChunkType4() :
            base()
        { }
    }

    public class bKGDChunkType6 : bKGDChunk
    {
        public override byte[] ChunkData
        {
            get
            {
                byte[] RedA = PNGUtils.GetBytes(Red);
                byte[] GreenA = PNGUtils.GetBytes(Green);
                byte[] BlueA = PNGUtils.GetBytes(Blue);
                return PNGUtils.Combine(RedA, GreenA, BlueA);
            }
            set
            {
                int offset = 0;
                Red = PNGUtils.ParseUshort(value, ref offset);
                Green = PNGUtils.ParseUshort(value, ref offset);
                Blue = PNGUtils.ParseUshort(value, ref offset);
            }
        }

        public ushort Red { get; set; }
        public ushort Green { get; set; }
        public ushort Blue { get; set; }

        public bKGDChunkType6() :
            base()
        { }
    }

    public class hISTChunk : PNGChunk
    {
        public const String NAME = "hIST";

        public override byte[] ChunkData
        {
            get
            {
                byte[] value = new byte[0];
                foreach (ushort f in Frequency)
                {
                    byte[] freqA = PNGUtils.GetBytes(f);
                    value = PNGUtils.Combine(value, freqA);
                }
                return value;
            }
            set
            {
                int size = value.Length / sizeof(ushort);
                Frequency = new ushort[size];
                int offset = 0;
                for (int i = 0; i < size; i++)
                {
                    Frequency[i] = PNGUtils.ParseUshort(value, ref offset);
                }
            }
        }

        public ushort[] Frequency { get; set; }

        public hISTChunk() :
            base(NAME)
        { }
    }

    public class pHYsChunk : PNGChunk
    {
        public const String NAME = "pHYs";

        public override byte[] ChunkData
        {
            get
            {
                byte[] ppuXA = PNGUtils.GetBytes(PixelsPerUnitXAxis);
                byte[] ppuYA = PNGUtils.GetBytes(PixelsPerUnitYAxis);
                byte[] UnitA = new byte[] { Unit };
                return PNGUtils.Combine(ppuXA, ppuYA, UnitA);
            }
            set
            {
                int offset = 0;
                PixelsPerUnitXAxis = PNGUtils.ParseUint(value, ref offset);
                PixelsPerUnitYAxis = PNGUtils.ParseUint(value, ref offset);
                Unit = PNGUtils.ParseByte(value, ref offset);
            }
        }

        public uint PixelsPerUnitXAxis { get; set; }
        public uint PixelsPerUnitYAxis { get; set; }
        public byte Unit { get; set; }

        public pHYsChunk() :
            base(NAME)
        { }
    }

    public class sPLTChunk : PNGChunk
    {
        public const String NAME = "sPLT";

        public override byte[] ChunkData
        {
            get
            {
                byte[] NameA = Encoding.UTF8.GetBytes(Name);
                byte[] DepthA = new byte[] { SampleDepth };
                byte[] value = PNGUtils.Combine(NameA, DepthA);
                foreach (SuggestedPalette sp in palettes)
                {
                    if (SampleDepth == 8)
                    {
                        value = PNGUtils.Combine(value, new byte[] { (byte)sp.Red, (byte)sp.Green, (byte)sp.Blue, (byte)sp.Alpha });
                    }
                    else if (SampleDepth == 16)
                    {
                        byte[] RedA = PNGUtils.GetBytes(sp.Red);
                        byte[] GreenA = PNGUtils.GetBytes(sp.Green);
                        byte[] BlueA = PNGUtils.GetBytes(sp.Blue);
                        byte[] AlphaA = PNGUtils.GetBytes(sp.Alpha);
                        value = PNGUtils.Combine(value, RedA, GreenA, BlueA, AlphaA);
                    }
                    byte[] freqA = PNGUtils.GetBytes(sp.Frequency);
                    value = PNGUtils.Combine(value, freqA);
                }
                return value;
            }
            set
            {
                int offset = 0;
                Name = PNGUtils.ParseString(value, ref offset);
                foreach (char c in Name)
                {
                    if (!IsPrintable(c))
                    {
                        throw new ApplicationException("Non-printable characters in sPLT chunk name");
                    }
                }
                SampleDepth = PNGUtils.ParseByte(value, ref offset);
                while (offset < value.Length)
                {
                    SuggestedPalette sp = new SuggestedPalette();
                    if (SampleDepth == 8)
                    {
                        sp.Red = PNGUtils.ParseByte(value, ref offset);
                        sp.Green = PNGUtils.ParseByte(value, ref offset);
                        sp.Blue = PNGUtils.ParseByte(value, ref offset);
                        sp.Alpha = PNGUtils.ParseByte(value, ref offset);
                    }
                    else if (SampleDepth == 16)
                    {
                        sp.Red = PNGUtils.ParseUshort(value, ref offset);
                        sp.Green = PNGUtils.ParseUshort(value, ref offset);
                        sp.Blue = PNGUtils.ParseUshort(value, ref offset);
                        sp.Alpha = PNGUtils.ParseUshort(value, ref offset);
                    }
                    else
                    {
                        throw new ApplicationException("Suggest Palette Sample Depth not 8 or 16");
                    }
                    sp.Frequency = PNGUtils.ParseUshort(value, ref offset);
                    palettes.Add(sp);
                }
            }
        }

        public string Name { get; set; }
        public byte SampleDepth { get; set; }

        public struct SuggestedPalette
        {
            public ushort Red { get; set; }
            public ushort Green { get; set; }
            public ushort Blue { get; set; }
            public ushort Alpha { get; set; }
            public ushort Frequency { get; set; }
        }

        public ICollection<SuggestedPalette> palettes;

        public sPLTChunk() :
            base(NAME)
        {
            palettes = new List<SuggestedPalette>();
        }
    }

    public class tIMEChunk : PNGChunk
    {
        public const String NAME = "tIME";

        public override byte[] ChunkData
        {
            get
            {
                byte[] YearA = PNGUtils.GetBytes(Year);
                byte[] TimeA = new byte[] { Month, Day, Hour, Minute, Second };
                return PNGUtils.Combine(YearA, TimeA);
            }
            set
            {
                int offset = 0;
                Year = PNGUtils.ParseUshort(value, ref offset);
                Month = PNGUtils.ParseByte(value, ref offset);
                Day = PNGUtils.ParseByte(value, ref offset);
                Hour = PNGUtils.ParseByte(value, ref offset);
                Minute = PNGUtils.ParseByte(value, ref offset);
                Second = PNGUtils.ParseByte(value, ref offset);
            }
        }

        public ushort Year { get; set; }
        public byte Month { get; set; }
        public byte Day { get; set; }
        public byte Hour { get; set; }
        public byte Minute { get; set; }
        public byte Second { get; set; }

        public tIMEChunk() :
            base(NAME)
        { }
    }
}

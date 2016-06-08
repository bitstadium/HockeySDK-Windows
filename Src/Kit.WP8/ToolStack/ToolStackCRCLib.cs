/*
 * ToolStack.com C# CRC library by Greg Ross
 * 
 * Homepage: http://ToolStack.com/CRCLib
 * 
 * This library is based upon the examples hosted at the PNG and zlib
 * home pages (www.libpng.org/pub/png/ & zlib.net), ported to C#.
 * 
 * This is public domain software, use and abuse as you see fit.
 * 
 * Version 1.0 - Released Feburary 22, 2012
 *         2.0 - Fixed adler() and addToAdler() to increment the index after the loop instead of before
 *             - Fixed addToAdler() to work with offsets
 *             - Fixed resetAdler() to reset the A & B values correctly
 */

using System;

namespace ToolStackCRCLib
{
    /// <summary>
    /// Computes the CRC32 value for a given data set
    /// </summary>
    public class CRC32
    {
        private UInt32[] crcTable = {
				    0, 1996959894, 3993919788, 2567524794, 124634137,
				    1886057615, 3915621685, 2657392035, 249268274, 2044508324,
				    3772115230, 2547177864, 162941995, 2125561021, 3887607047,
				    2428444049, 498536548, 1789927666, 4089016648, 2227061214,
				    450548861, 1843258603, 4107580753, 2211677639, 325883990,
				    1684777152, 4251122042, 2321926636, 335633487, 1661365465,
				    4195302755, 2366115317, 997073096, 1281953886, 3579855332,
				    2724688242, 1006888145, 1258607687, 3524101629, 2768942443,
				    901097722, 1119000684, 3686517206, 2898065728, 853044451,
				    1172266101, 3705015759, 2882616665, 651767980, 1373503546,
				    3369554304, 3218104598, 565507253, 1454621731, 3485111705,
				    3099436303, 671266974, 1594198024, 3322730930, 2970347812,
				    795835527, 1483230225, 3244367275, 3060149565, 1994146192,
				    31158534, 2563907772, 4023717930, 1907459465, 112637215,
				    2680153253, 3904427059, 2013776290, 251722036, 2517215374,
				    3775830040, 2137656763, 141376813, 2439277719, 3865271297,
				    1802195444, 476864866, 2238001368, 4066508878, 1812370925,
				    453092731, 2181625025, 4111451223, 1706088902, 314042704,
				    2344532202, 4240017532, 1658658271, 366619977, 2362670323,
				    4224994405, 1303535960, 984961486, 2747007092, 3569037538,
				    1256170817, 1037604311, 2765210733, 3554079995, 1131014506,
				    879679996, 2909243462, 3663771856, 1141124467, 855842277,
				    2852801631, 3708648649, 1342533948, 654459306, 3188396048,
				    3373015174, 1466479909, 544179635, 3110523913, 3462522015,
				    1591671054, 702138776, 2966460450, 3352799412, 1504918807,
				    783551873, 3082640443, 3233442989, 3988292384, 2596254646,
				    62317068, 1957810842, 3939845945, 2647816111, 81470997,
				    1943803523, 3814918930, 2489596804, 225274430, 2053790376,
				    3826175755, 2466906013, 167816743, 2097651377, 4027552580,
				    2265490386, 503444072, 1762050814, 4150417245, 2154129355,
				    426522225, 1852507879, 4275313526, 2312317920, 282753626,
				    1742555852, 4189708143, 2394877945, 397917763, 1622183637,
				    3604390888, 2714866558, 953729732, 1340076626, 3518719985,
				    2797360999, 1068828381, 1219638859, 3624741850, 2936675148,
				    906185462, 1090812512, 3747672003, 2825379669, 829329135,
				    1181335161, 3412177804, 3160834842, 628085408, 1382605366,
				    3423369109, 3138078467, 570562233, 1426400815, 3317316542,
				    2998733608, 733239954, 1555261956, 3268935591, 3050360625,
				    752459403, 1541320221, 2607071920, 3965973030, 1969922972,
				    40735498, 2617837225, 3943577151, 1913087877, 83908371,
				    2512341634, 3803740692, 2075208622, 213261112, 2463272603,
				    3855990285, 2094854071, 198958881, 2262029012, 4057260610,
				    1759359992, 534414190, 2176718541, 4139329115, 1873836001,
				    414664567, 2282248934, 4279200368, 1711684554, 285281116,
				    2405801727, 4167216745, 1634467795, 376229701, 2685067896,
				    3608007406, 1308918612, 956543938, 2808555105, 3495958263,
				    1231636301, 1047427035, 2932959818, 3654703836, 1088359270,
				    936918000, 2847714899, 3736837829, 1202900863, 817233897,
				    3183342108, 3401237130, 1404277552, 615818150, 3134207493,
				    3453421203, 1423857449, 601450431, 3009837614, 3294710456,
				    1567103746, 711928724, 3020668471, 3272380065, 1510334235,
				    755167117
			    };

        private UInt32 crcValue = (UInt32)0xffffffffL;

        /* 
         * Make the table for a fast CRC. 
         * This function is included here only for completness, a static arrary
         * has already been defined for these values.         
         */

        /// <summary>
        /// Populates the CRC32 checksum values for every byte.  This should only be called
        /// if you are using this library on a non-standard platform that uses some kind of
        /// weird binary encoding of bytes.
        /// </summary>
        public void makeCRCTable()
        {
            UInt32 c;
            UInt32 n, k;

            for (n = 0; n < 256; n++)
            {
                c = n;
                for (k = 0; k < 8; k++)
                {
                    if ((c & (UInt32)1) == 1)
                        c = (UInt32)0xedb88320L ^ (c >> 1);
                    else
                        c = c >> 1;
                }

                crcTable[n] = c;
            }
        }

        /// <summary>
        /// Returns the current running CRC32 result for a PNG file.
        /// </summary>
        /// <returns>An unsigned 32bit integer representing the current CRC32.</returns>
        public UInt32 crc()
        {
            return crcValue ^ (UInt32)0xffffffffL;
        }

        /// <summary>
        /// Returns the current running CRC32 result for a PNG file.
        /// </summary>
        /// <param name="buf">A byte[] to process.</param>
        /// <returns>An unsigned 32bit integer representing the CRC32.</returns>
        public UInt32 crc(byte[] buf)
        {
            return crc(buf, buf.Length, 0);
        }

        /// <summary>
        /// Returns the current running CRC32 result for a PNG file.
        /// </summary>
        /// <param name="buf">A byte[] to process.</param>
        /// <param name="len">The length of the byte[].</param>
        /// <returns>An unsigned 32bit integer representing the CRC32.</returns>
        public UInt32 crc(byte[] buf, int len)
        {
            return crc(buf, len, 0);
        }

        /// <summary>
        /// Returns the current running CRC32 result for a PNG file.
        /// </summary>
        /// <param name="buf">A byte[] to process.</param>
        /// <param name="len">The length of the byte[].</param>
        /// <param name="offset">The offset to start processing byte[] at.</param>
        /// <returns>An unsigned 32bit integer representing the CRC32.</returns>
        public UInt32 crc(byte[] buf, int len, UInt32 offset)
        {
            UInt32 c = (UInt32)0xffffffffL;
            UInt32 n;

            for (n = offset; n < offset + len; n++)
            {
                c = crcTable[(c ^ buf[n]) & 0xff] ^ (c >> 8);
            }

            return c ^ (UInt32)0xffffffffL;
        }

        /// <summary>
        /// Adds to the current running CRC32 the bytes from buf[].
        /// </summary>
        /// <param name="buf">A byte[] to process.</param>
        public void addToCRC(byte[] buf)
        {
            addToCRC(buf, buf.Length, 0);
        }

        /// <summary>
        /// Adds to the current running CRC32 the bytes from buf[].
        /// </summary>
        /// <param name="buf">A byte[] to process.</param>
        /// <param name="len">The length of the byte[].</param>
        public void addToCRC(byte[] buf, int len)
        {
            addToCRC(buf, len, 0);
        }

        /// <summary>
        /// Adds to the current running CRC32 the bytes from buf[].
        /// </summary>
        /// <param name="buf">A byte[] to process.</param>
        /// <param name="len">The length of the byte[].</param>
        /// <param name="offset">The offset to start processing byte[] at.</param>
        public void addToCRC(byte[] buf, int len, UInt32 offset)
        {
            UInt32 n;
            UInt32 c = crcValue;

            for (n = offset; n < offset + len; n++)
            {
                c = crcTable[(c ^ buf[n]) & (UInt32)0xff] ^ (c >> 8);
            }

            crcValue = c;
        }

        /// <summary>
        /// Resets the running CRC32.
        /// </summary>
        public void resetCRC()
        {
            crcValue = (UInt32)0xffffffffL;
        }
    }

    /// <summary>
    /// Computes the Adler32 CRC value for a given data set
    /// </summary>
    public class Adler32
    {
        private const int MOD_ADLER = 65521;
        private UInt32 AdlerA = 1;
        private UInt32 AdlerB = 0;

        /*
         * The Adler32 checksum code for use in zlib compression
        */

        /// <summary>
        /// Returns the current running Adler32 result.
        /// </summary>
        /// <returns>An unsigned 32bit integer representing the current Adler32.</returns>
        public UInt32 adler()
        {
            return (AdlerB << 16) | AdlerA;
        }

        /// <summary>
        /// Returns the current running Adler32 result.
        /// </summary>
        /// <param name="data">A byte[] to process.</param>
        /// <returns>An unsigned 32bit integer representing the current Adler32.</returns>
        public UInt32 adler(byte[] data)
        {
            return adler(data, data.Length, 0);
        }

        /// <summary>
        /// Returns the current running Adler32 result.
        /// </summary>
        /// <param name="data">A byte[] to process.</param>
        /// <param name="len">The length of the byte[].</param>
        /// <returns>An unsigned 32bit integer representing the current Adler32.</returns>
        public UInt32 adler(byte[] data, int len)
        {
            return adler( data, len, 0 );
        }
            /// <summary>
        /// Returns the current running Adler32 result.
        /// </summary>
        /// <param name="data">A byte[] to process.</param>
        /// <param name="len">The length of the byte[].</param>
        /// <param name="offset">The offset to start processing byte[] at.</param>
        /// <returns>An unsigned 32bit integer representing the current Adler32.</returns>
        public UInt32 adler(byte[] data, int len, UInt32 offset)
        {
            UInt32 a = 1, b = 0;
            UInt32 index;

            /* Process each byte of the data in order */
            for (index = offset; index < offset + len; index++)
            {
                a = (a + data[index]) % MOD_ADLER;
                b = (b + a) % MOD_ADLER;
            }

            return (b << 16) | a;
        }

        /// <summary>
        /// Adds to the current running Adler32 the bytes from buf[].
        /// </summary>
        /// <param name="data">A byte[] to process.</param>
        public void addToAdler(byte[] data)
        {
            addToAdler(data, data.Length, 0);
        }

        /// <summary>
        /// Adds to the current running Adler32 the bytes from buf[].
        /// </summary>
        /// <param name="data">A byte[] to process.</param>
        /// <param name="len">The length of the byte[].</param>
        public void addToAdler(byte[] data, int len)
        {
            addToAdler(data, len, 0);
        }

        /// <summary>
        /// Adds to the current running Adler32 the bytes from buf[].
        /// </summary>
        /// <param name="data">A byte[] to process.</param>
        /// <param name="len">The length of the byte[].</param>
        /// <param name="offset">The offset to start processing byte[] at.</param>
        public void addToAdler(byte[] data, int len, UInt32 offset)
        {
            UInt32 index;

            /* Process each byte of the data in order */
            for (index = offset; index < offset + len; index++)
            {
                AdlerA = (AdlerA + data[index]) % MOD_ADLER;
                AdlerB = (AdlerB + AdlerA) % MOD_ADLER;
            }
        }

        /// <summary>
        /// Resets the running Adler32.
        /// </summary>
        public void resetAdler()
        {
            AdlerA = 1;
            AdlerB = 0;
        }
    }
}
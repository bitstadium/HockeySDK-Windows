/*
 * ToolStack.com C# PNG Writer library by Greg Ross
 * 
 * Homepage: http://ToolStack.com/PNGWriter
 * 
 * This library is inspired by the examples hosted at the forums on WriteableBitmapEx
 * project at the codeplex site (http://writeablebitmapex.codeplex.com/discussions/274445), however
 * there's not really any of that code left, just some constants.
 * 
 * Compression is currently not supported but I am looking at adding it in.
 * 
 * This is public domain software, use and abuse as you see fit.
 * 
 * Version 1.0 - Released Feburary 22, 2012
 *         2.0 - Rewrote WriteDataChunksUncompressed() pretty much from the ground up to reduce the 3
 *               copies of the image in memory down to just a single copy.  This also reduced the 
 *               number of loops performed to manipulate the data from 2 to 1.
 *         2.1 - De-multiplied alpha channel, thanks to Jan for supplying the fix!
 */

using System;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ToolStackCRCLib;

namespace ToolStackPNGWriterLib
{
    static class PngChunkTypes
    {
        /// <summary>
        /// The first chunk in a png file. Can only exists once. Contains 
        /// common information like the width and the height of the image or
        /// the used compression method.
        /// </summary>
        public const string Header = "IHDR";
        /// <summary>
        /// The PLTE chunk contains from 1 to 256 palette entries, each a three byte
        /// series in the RGB format.
        /// </summary>
        public const string Palette = "PLTE";
        /// <summary>
        /// The IDAT chunk contains the actual image data. The image can contains more
        /// than one chunk of this type. All chunks together are the whole image.
        /// </summary>
        public const string Data = "IDAT";
        /// <summary>
        /// This chunk must appear last. It marks the end of the PNG datastream. 
        /// The chunk's data field is empty. 
        /// </summary>
        public const string End = "IEND";
        /// <summary>
        /// This chunk specifies that the image uses simple transparency: 
        /// either alpha values associated with palette entries (for indexed-color images) 
        /// or a single transparent color (for grayscale and truecolor images). 
        /// </summary>
        public const string PaletteAlpha = "tRNS";
        /// <summary>
        /// Textual information that the encoder wishes to record with the image can be stored in 
        /// tEXt chunks. Each tEXt chunk contains a keyword and a text string.
        /// </summary>
        public const string Text = "tEXt";
        /// <summary>
        /// This chunk specifies the relationship between the image samples and the desired 
        /// display output intensity.
        /// </summary>
        public const string Gamma = "gAMA";
        /// <summary>
        /// The pHYs chunk specifies the intended pixel size or aspect ratio for display of the image. 
        /// </summary>
        public const string Physical = "pHYs";
    }

    sealed class PngHeader
    {
        /// <summary>
        /// The dimension in x-direction of the image in pixels.
        /// </summary>
        public int Width;
        /// <summary>
        /// The dimension in y-direction of the image in pixels.
        /// </summary>
        public int Height;
        /// <summary>
        /// Bit depth is a single-byte integer giving the number of bits per sample 
        /// or per palette index (not per pixel). Valid values are 1, 2, 4, 8, and 16, 
        /// although not all values are allowed for all color types. 
        /// </summary>
        public byte BitDepth;
        /// <summary>
        /// Color type is a integer that describes the interpretation of the 
        /// image data. Color type codes represent sums of the following values: 
        /// 1 (palette used), 2 (color used), and 4 (alpha channel used).
        /// </summary>
        public byte ColorType;
        /// <summary>
        /// Indicates the method  used to compress the image data. At present, 
        /// only compression method 0 (deflate/inflate compression with a sliding 
        /// window of at most 32768 bytes) is defined.
        /// </summary>
        public byte CompressionMethod;
        /// <summary>
        /// Indicates the preprocessing method applied to the image 
        /// data before compression. At present, only filter method 0 
        /// (adaptive filtering with five basic filter types) is defined.
        /// </summary>
        public byte FilterMethod;
        /// <summary>
        /// Indicates the transmission order of the image data. 
        /// Two values are currently defined: 0 (no interlace) or 1 (Adam7 interlace).
        /// </summary>
        public byte InterlaceMethod;
    }


    /// <summary>
    /// WriteableBitmap Extensions for PNG Writing
    /// </summary>
    public static class PNGWriter
    {
        private const int MaxBlockSize = 0xFFFF;
        
        private static Stream _stream;
        private static WriteableBitmap _image;

        private const double DefaultDensityX = 75;
        private const double DefaultDensityY = 75;
        
        private static bool IsWritingGamma = true;
        private static double Gamma = 2.2f;

        /* Data in a PNG is in RGBA format but source from a writeablebitmap is in BGRA format
                BGRA=2,1,0,3

                Red = 0;
                Green = 1;
                Blue = 2;
                Alpha = 3;
        */
        private static int[] WBByteOrder = { 2, 1, 0, 3 };
        private static Boolean WBBODetectionRun = false;

        /// <summary>
        /// Detects the color order of a stored byte array.  Byte order may change between platforms, you should call this once before writting a PNG or if you have any issues with colors changing.
        /// </summary>
        public static void DetectWBByteOrder()
        {
            // We should only ever run the detection once (assuming it succeeded at least).
            if (WBBODetectionRun == true)
            {
                return;
            }

            // Create a 3x1 WriteableBitmap to write RGB colors to.
            WriteableBitmap TestWB = new WriteableBitmap(3,1);

            // Create the red 1 pixel rectangle.
            var redRectangle = new Rectangle();
            redRectangle.Width = 1;
            redRectangle.Height = 1;
            redRectangle.Fill = new SolidColorBrush( Colors.Red );

            // Create the green 1 pixel rectangle.
            var greenRectangle = new Rectangle();
            greenRectangle.Width = 1;
            greenRectangle.Height = 1;
            greenRectangle.Fill = new SolidColorBrush( Colors.Green );

            // Create the blue 1 pixel rectangle.
            var blueRectangle = new Rectangle();
            blueRectangle.Width = 1;
            blueRectangle.Height = 1;
            blueRectangle.Fill = new SolidColorBrush( Colors.Blue );

            // Render the three 1 px rectangles.
            TestWB.Render(redRectangle, new TranslateTransform() { X = 0, Y = 0 } );
            TestWB.Render(greenRectangle, new TranslateTransform() { X = 1, Y = 0 } );
            TestWB.Render(blueRectangle, new TranslateTransform() { X = 2, Y = 0 } );

            // Invalidate the bitmap to make it actually render.
            TestWB.Invalidate();

            // Go get the 4 byte arrays of each red/green/blue pixels that we just rendered.
            byte[] redBytes = BitConverter.GetBytes( TestWB.Pixels[0] );
            byte[] greenBytes = BitConverter.GetBytes(TestWB.Pixels[1]);
            byte[] blueBytes = BitConverter.GetBytes(TestWB.Pixels[2]);

            // Just in case something goes terrible wrong, set all the color values to invalid settings.
            int trans = 4;
            int red = 4;
            int green = 4;
            int blue = 4;

            // Find the alpha channel, this wil be the only byte that is the same in all three pixels.
            if (redBytes[0] == greenBytes[0] && blueBytes[0] == greenBytes[0]) { trans = 0; }
            if (redBytes[1] == greenBytes[1] && blueBytes[1] == greenBytes[1]) { trans = 1; }
            if (redBytes[2] == greenBytes[2] && blueBytes[2] == greenBytes[2]) { trans = 2; }
            if (redBytes[3] == greenBytes[3] && blueBytes[3] == greenBytes[3]) { trans = 3; }

            // if we didn't detect the alpha channel, just give up :(
            if (trans != 4)
            {
                // now set all the alpha channel's to zero to get them out of the way.
                redBytes[trans] = 0;
                greenBytes[trans] = 0;
                blueBytes[trans] = 0;

                // Find the red channel.
                if (redBytes[0] == 255) { red = 0; }
                else if (redBytes[1] == 255) { red = 1; }
                else if (redBytes[2] == 255) { red = 2; }
                else if (redBytes[3] == 255) { red = 3; }


                // Find the green channel, note that Colors.Green is not dark green, but light green so use 128 instead of 255 to detect it.
                if (greenBytes[0] == 128) { green = 0; }
                else if (greenBytes[1] == 128) { green = 1; }
                else if (greenBytes[2] == 128) { green = 2; }
                else if (greenBytes[3] == 128) { green = 3; }


                // Find the blue channel.
                if (blueBytes[0] == 255) { blue = 0; }
                else if (blueBytes[1] == 255) { blue = 1; }
                else if (blueBytes[2] == 255) { blue = 2; }
                else if (blueBytes[3] == 255) { blue = 3; }
            }

            // Now set the byte order, if any of the values are still set to 4, something went wrong and return the default values.
            if( red == 4 || green == 4 || blue == 4 || trans == 4 )
            {
                WBByteOrder = new int[] { 2, 1, 0, 3};
            }
            else
            {
                WBBODetectionRun = true;
                WBByteOrder = new int[] { red, green, blue, trans };
            }
        }

        /// <summary>
        /// Write and PNG file out to a file stream.  Currently compression is not supported.
        /// </summary>
        /// <param name="image">The WriteableBitmap to work on.</param>
        /// <param name="stream">The destination file stream.</param>
        public static void WritePNG(WriteableBitmap image, System.IO.Stream stream)
        {
            WritePNG(image, stream, -1);
        }

        /// <summary>
        /// Write and PNG file out to a file stream.  Currently compression is not supported.
        /// </summary>
        /// <param name="image">The WriteableBitmap to work on.</param>
        /// <param name="stream">The destination file stream.</param>
        /// <param name="compression">Level of compression to use (-1=auto, 0=none, 1-100 is percentage).</param>
        public static void WritePNG(WriteableBitmap image, System.IO.Stream stream, int compression)
        {
            // Set the global class variables for the image and stream.
            _image = image;
            _stream = stream;

            // Write the png header.
            stream.Write(
                new byte[] 
                { 
                    0x89, 0x50, 0x4E, 0x47, 
                    0x0D, 0x0A, 0x1A, 0x0A 
                }, 0, 8);

            // Set the PNG header values for this image.
            PngHeader header = new PngHeader();
            header.Width = image.PixelWidth;
            header.Height = image.PixelHeight;
            header.ColorType = 6;
            header.BitDepth = 8;
            header.FilterMethod = 0;
            header.CompressionMethod = 0;
            header.InterlaceMethod = 0;

            // Write out the header.
            WriteHeaderChunk(header);
            // Write out the rest of the mandatory fields to the PNG.
            WritePhysicsChunk();
            WriteGammaChunk();

            // Currently only uncompressed PNG's are supported, so this if statement really doesn't do anything ;).
            if (compression == -1)
            {
                // Autodetect compression setting
                WriteDataChunksUncompressed();
            }
            else if (compression == 0)
            {
                // Write PNG without any compression
                WriteDataChunksUncompressed();
            }
            else
            {
                // Write the PNG with a desired compression level
                WriteDataChunks( compression );
            }

            // Write out the end of the PNG.
            WriteEndChunk();

            // Flush the stream to make sure it's all written.
            stream.Flush();
        }


        private static void WritePhysicsChunk()
        {
            int dpmX = (int)Math.Round(DefaultDensityX * 39.3700787d);
            int dpmY = (int)Math.Round(DefaultDensityY * 39.3700787d);

            byte[] chunkData = new byte[9];

            WriteInteger(chunkData, 0, dpmX);
            WriteInteger(chunkData, 4, dpmY);

            chunkData[8] = 1;

            WriteChunk(PngChunkTypes.Physical, chunkData);
        }

        private static void WriteGammaChunk()
        {
            if (IsWritingGamma)
            {
                int gammeValue = (int)(Gamma * 100000f);

                byte[] fourByteData = new byte[4];

                byte[] size = BitConverter.GetBytes(gammeValue);
                fourByteData[0] = size[3]; fourByteData[1] = size[2]; fourByteData[2] = size[1]; fourByteData[3] = size[0];

                WriteChunk(PngChunkTypes.Gamma, fourByteData);
            }
        }

        // Currently only uncompressed PNG are supported, so just call the uncompressed method.
        private static void WriteDataChunks( int compression )
        {
            WriteDataChunksUncompressed();
        }

        private static void WriteDataChunksUncompressed()
        {
            // First setup some variables we're going to use later on so we can calculate how big of byte[] we need 
            // to store the entire PNG file in so we only keep a single copy of the data in memory.

            // Figure out how much image data we're going to have:
            //      H * W * (number of bytes in an ARGB value) + H to account for the filter byte in PNG files
            int dataLength = _image.PixelWidth * _image.PixelHeight * 4 + _image.PixelHeight;

            // Variables for the number of PNG blocks and how big the last block is going to be.
            int blockCount;
            int lastBlockSize;

            // We could have an exactly even count of blocks (ie MaxBlockSize * x), but that seems unlikely.
            // If we don't, then add one for the remainder of the data and figure out how much data will be
            // left.
            if ((dataLength % MaxBlockSize) == 0)
            {
                blockCount = dataLength / MaxBlockSize;
                lastBlockSize = MaxBlockSize;
            }
            else
            {
                blockCount = (dataLength / MaxBlockSize) + 1;
                lastBlockSize = dataLength - ( MaxBlockSize * ( blockCount - 1 ) );
            }

            // The size of the PNG file will be:
            //      2 header bytes +
            //      ( blockCount - 1 ) * 
            //      (
            //          1 last block byte +
            //          2 block size bytes +
            //          2 block size one's complement bytes +
            //          maxBlockSize ) +
            //      (
            //          1 last block byte +
            //          2 block size bytes +
            //          2 block size one's complement bytes +
            //          lastBlockSize ) +
            //      4 Adler32 bytes +
            //      
            //  = 2 + ((blockCount-1)*(5+MaxBlockSize)) + (5+lastBlockSize) + 4
            //  = 11 + ((blockCount-1)*(5+MaxBlockSize)) + lastBlockSize
            //
            int pngLength;
            pngLength = 11 + ((blockCount - 1) * (5 + MaxBlockSize)) + lastBlockSize;

            // Make a buffer to store the PNG in.
            byte[] data = new byte[pngLength];

            // Write zlib headers.
            data[0] = 0x78;
            data[1] = 0xDA;

            //  zlib compression uses Adler32 CRCs instead of CRC32s, so setup on up to calculate.
            Adler32 crcCode = new Adler32();
            crcCode.resetAdler();

            // Setup some variables to use in the loop.
            var blockRemainder = 0;                         // How much of the current block we have left, 0 to start so we write the block header out on the first block.
            var currentBlock = 0;                           // The current block we're working on, start with 0 as we increment in the first pass thorugh.
            var dataPointer = 2;                            // A pointer to where we are in the data array, start at 2 as we 'wrote' two bytes a few lines ago.
            var pixelSource = 0;                            // The current pixel we're working on from the image.
            byte[] pixel = new byte[4];                     // Temporary storage to store the current pixel in as a byte array.

            // This is the main logic loop, we're going to be doing a lot of work so stick with me...
            //      The loop has three parts to it:
            //          1. looping through each row (y)
            //          2. looping through each pixel in the row (x)
            //          3. looping through each byte of the pixel (z)

            // Loop thorough each row in the image.
            for (int y = 0; y < _image.PixelHeight; y++)
            {
                // This code appears twice, once here and once in the pixel byte loop (loop 3).
                // It checks to see if we're at the boundry for the PNG block and if so writes
                // out a new block header.  It get executed on the first time through to setup
                // the first block but is unlikly to get executed again as it would mean the 
                // block boundry is at a row boundry, which seems unlikly.
                if (blockRemainder == 0)
                {
                    // Setup a temporary byte array to store the block size in.
                    byte[] tempBytes = new byte[2];

                    // Increment the current block count.
                    currentBlock++;

                    // Figure out the current block size and if we're at the last block, write
                    // out and 1 to let the zlib decompressor know.  By default, use the MaxBlockSize.
                    int length = MaxBlockSize;

                    if (currentBlock == blockCount)
                    {
                        length = lastBlockSize;
                        data[dataPointer] = 0x01;
                    }
                    else
                    {
                        data[dataPointer] = 0x00;
                    }

                    // Each and every time we write something to the data array, increment the pointer.
                    dataPointer++;

                    // Write the block length out.
                    tempBytes = BitConverter.GetBytes(length);
                    data[dataPointer + 0] = tempBytes[0];
                    data[dataPointer + 1] = tempBytes[1];
                    dataPointer += 2;

                    // Write one's compliment of length for error checking.
                    tempBytes = BitConverter.GetBytes((ushort)~length);
                    data[dataPointer + 0] = tempBytes[0];
                    data[dataPointer + 1] = tempBytes[1];
                    dataPointer += 2;

                    // Reset the remaining block size to the next block's length.
                    blockRemainder = length;
                }
                
                // Set the filter byte to 0, not really required as C# initalizes the byte array to 0 by default, but here for clarity.
                data[dataPointer] = 0;

                // Add the current byte to the running Adler32 value, note we ONLY add the filter byte and the pixel bytes to the
                // Adler32 CRC, all other header and block header bytes are execluded from the CRC.
                crcCode.addToAdler(data, 1, (UInt32)dataPointer);

                // Increment the data pointer and decrement the remain block value.
                dataPointer++;
                blockRemainder--;

                // Loop thorough each pixel in the row, you have to do this as the source format and destination format may be different.
                for (int x = 0; x < _image.PixelWidth; x++ )
                {
                    // Data is in RGBA format but source may not be
                    pixel = BitConverter.GetBytes(_image.Pixels[pixelSource]);

                    // Loop through the 4 bytes of the pixel and 'write' them to the data array.
                    for (int z = 0; z < 4; z++)
                    {
                        // This is the second appearance of this code code.
                        // It checks to see if we're at the boundry for the PNG block and if so writes
                        // out a new block header.  
                        if (blockRemainder == 0)
                        {
                            // Setup a temporary byte array to store the block size in.
                            byte[] tempBytes = new byte[2];

                            // Increment the current block count.
                            currentBlock++;

                            // Figure out the current block size and if we're at the last block, write
                            // out and 1 to let the zlib decompressor know.  By default, use the MaxBlockSize.
                            int length = MaxBlockSize;

                            if (currentBlock == blockCount)
                            {
                                length = lastBlockSize;
                                data[dataPointer] = 0x01;
                            }
                            else
                            {
                                data[dataPointer] = 0x00;
                            }

                            // Each and every time we write something to the data array, increment the pointer.
                            dataPointer++;

                            // Write the block length out.
                            tempBytes = BitConverter.GetBytes(length);
                            data[dataPointer + 0] = tempBytes[0];
                            data[dataPointer + 1] = tempBytes[1];
                            dataPointer += 2;

                            // Write one's compliment of length for error checking.
                            tempBytes = BitConverter.GetBytes((ushort)~length);
                            data[dataPointer + 0] = tempBytes[0];
                            data[dataPointer + 1] = tempBytes[1];
                            dataPointer += 2;

                            // Reset the remaining block size to the next block's length.
                            blockRemainder = length;
                        }

                        // Store the pixel's byte in to the data array. We use the WBByteOrder array to ensure 
                        // we have the write order of bytes to store in the PNG file.
                        if( z != 3 && pixel[WBByteOrder[3]] != 0 && pixel[WBByteOrder[3]] != 255 )
                            {
                            // Calculate unmultiplied pixel value from premultiplied value (Windows Phone always uses premultiplied ARGB32)  
                            data[dataPointer] = (byte)( ( 255 * pixel[WBByteOrder[z]] ) / pixel[WBByteOrder[3]] ); 
                            } 
                        else 
                            {  
                            // Alpha channel or no need to unpremultiply  
                            data[dataPointer] = pixel[WBByteOrder[z]]; 
                            }

                        // Add the current byte to the running Adler32 value, note we ONLY add the filter byte and the pixel bytes to the
                        // Adler32 CRC, all other header and block header bytes are execluded from the CRC.
                        crcCode.addToAdler(data, 1, (UInt32)dataPointer);

                        // Increment the data pointer and decrement the remain block value.
                        dataPointer++;
                        blockRemainder--;
                    }

                    // Increment where we start writting the next pixel and where we get the next pixel from.
                    pixelSource++;
                }
            }

            // Whew, wipe that brow, we're done all the complex bits now!

            // Write the Adler32 CRC out, but reverse the order of the bytes to match the zlib spec.
            pixel = BitConverter.GetBytes(crcCode.adler());
            data[dataPointer + 0] = pixel[3];
            data[dataPointer + 1] = pixel[2];
            data[dataPointer + 2] = pixel[1];
            data[dataPointer + 3] = pixel[0];

            // Yes, yes, I know I said "Each and every time we write something to the data array, increment the pointer."
            // but we're done with it now so I'm not going to bother ;)

            // Write the entire PNG data chunk out to the file stream.
            WriteChunk(PngChunkTypes.Data, data, 0, pngLength);
        }

        private static void WriteEndChunk()
        {
            WriteChunk(PngChunkTypes.End, null);
        }

        private static void WriteHeaderChunk(PngHeader header)
        {
            byte[] chunkData = new byte[13];

            WriteInteger(chunkData, 0, header.Width);
            WriteInteger(chunkData, 4, header.Height);

            chunkData[8] = header.BitDepth;
            chunkData[9] = header.ColorType;
            chunkData[10] = header.CompressionMethod;
            chunkData[11] = header.FilterMethod;
            chunkData[12] = header.InterlaceMethod;

            WriteChunk(PngChunkTypes.Header, chunkData);
        }

        private static void WriteChunk(string type, byte[] data)
        {
            WriteChunk(type, data, 0, data != null ? data.Length : 0);
        }

        private static void WriteChunk(string type, byte[] data, int offset, int length)
        {
            // Write out the length to the PNG.
            WriteInteger(_stream, length);

            // Write the chunck type out to the PNG.
            byte[] typeArray = new byte[4];
            typeArray[0] = (byte)type[0];
            typeArray[1] = (byte)type[1];
            typeArray[2] = (byte)type[2];
            typeArray[3] = (byte)type[3];

            _stream.Write(typeArray, 0, 4);

            // If we have some data to write out (some chunk types don't), the do so.
            if (data != null)
            {
                _stream.Write(data, offset, length);
            }

            // All chunk types need to have a CRC32 value at their end to make sure they haven't been currupted.
            CRC32 crcCode = new CRC32();
            crcCode.addToCRC(typeArray, 4);

            if (data != null)
            {
                crcCode.addToCRC(data, length, (UInt32)offset);
            }

            WriteInteger(_stream, (uint)crcCode.crc());
        }

        private static void WriteInteger(byte[] data, int offset, int value)
        {
            byte[] buffer = BitConverter.GetBytes(value);

            Array.Reverse(buffer);
            Array.Copy(buffer, 0, data, offset, 4);
        }

        private static void WriteInteger(Stream stream, int value)
        {
            byte[] buffer = BitConverter.GetBytes(value);

            Array.Reverse(buffer);

            stream.Write(buffer, 0, 4);
        }

        private static void WriteInteger(Stream stream, uint value)
        {
            byte[] buffer = BitConverter.GetBytes(value);

            Array.Reverse(buffer);

            stream.Write(buffer, 0, 4);
        }
    }
}

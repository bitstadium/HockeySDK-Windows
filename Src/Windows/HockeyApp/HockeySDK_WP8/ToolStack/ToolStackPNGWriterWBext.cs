/*
 * ToolStack.com C# WriteableBitmap extension for PNG Writer library by Greg Ross
 * 
 * Homepage: http://ToolStack.com/PNGWriter
 * 
 * This library is based upon the examples hosted at the forums on WriteableBitmapEx
 * project at the codeplex site (http://writeablebitmapex.codeplex.com/discussions/274445).
 * 
 * This is public domain software, use and abuse as you see fit.
 * 
 * Version 1.0 - Released Feburary 22, 2012
*/

using System;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Shapes;
using System.Windows.Media;
using ToolStackCRCLib;
using ToolStackPNGWriterLib;

namespace System.Windows.Media.Imaging
{
    /// <summary>
    /// WriteableBitmap Extensions for PNG Writing
    /// </summary>
    public static partial class WriteableBitmapExtensions
    {
        /// <summary>
        /// Write and PNG file out to a file stream.  Currently compression is not supported.
        /// </summary>
        /// <param name="image">The WriteableBitmap to work on.</param>
        /// <param name="stream">The destination file stream.</param>
        public static void WritePNG(this WriteableBitmap image, System.IO.Stream stream)
        {
            WritePNG(image, stream, -1);
        }

        /// <summary>
        /// Write and PNG file out to a file stream.  Currently compression is not supported.
        /// </summary>
        /// <param name="image">The WriteableBitmap to work on.</param>
        /// <param name="stream">The destination file stream.</param>
        /// <param name="compression">Level of compression to use (-1=auto, 0=none, 1-100 is percentage).</param>
        public static void WritePNG(this WriteableBitmap image, System.IO.Stream stream, int compression)
        {
            PNGWriter.DetectWBByteOrder();
            PNGWriter.WritePNG(image, stream, compression);
        }
    }
}

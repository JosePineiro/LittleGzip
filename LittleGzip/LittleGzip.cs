/////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// LittleGZip C#. (MIT) Jose M. Piñeiro
/// Version: 1.0.0.1 (Dec 30, 2017)
/// 
/// This file is Test for libdeflate wrapper
/// LittleGzip can:
/// - Compress several files in a very little GZIP
/// - Compress in very little time
/// - Use very little code
/// - Very little learning for use
/// - Create multimember GZIP. Store the original filename for complete extraction.
/// 
/// LittleGzip can not:
/// - Create a large GZIP ( > 2.147.483.647 bytes)
/// - Store a large file ( > 2.147.483.647 bytes)
/// - Use little memory ( need two times the compresed file )
/// - Decompress one GZIP file.
///
/// Use library from: https://github.com/ebiggers/libdeflate
///////////////////////////////////////////////////////////////////////////////////////////////////////////// 
/// Compress Functions:
/// LittleGZip(string filename)
/// - Create a new GZIP file.
/// 
/// LittleGZip Create(Stream stream)
/// - Create a new GZIP stream.
/// 
/// AddFile(string pathFilename, string filenameInGzip, int compressionLevel = 6, string comment = "")
/// - Add full contents of a file into the GZIP storage. Optionally you can put a file comment.
/// 
/// AddBuffer(byte[] inBuffer, string filenameInGzip, DateTime modifyTime, int compressionLevel = 6, string comment = "")
/// - Add full contents of a array into the GZIP storage. Optionally you can put a file comment.
/// 
/// Close()
/// - Close the GZIP storage.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;

#if NET45
using System.Threading.Tasks;
#endif

namespace System.IO.Compression
{
#if NETSTANDARD
    /// <summary>
    /// Extension method for covering missing Close() method in .Net Standard
    /// </summary>
    public static class StreamExtension
    {
        public static void Close(this Stream stream)
        {
            stream.Dispose(); 
            GC.SuppressFinalize(stream);
        }
    }
#endif

    /// <summary>
    /// Unique class for compression/decompression file. Represents a Zip file.
    /// </summary>
    public class LittleGzip : IDisposable
    {
        #region Private structs
        /// <summary>
        /// Represents an member in GZip file
        /// </summary>
        private struct GzipMember
        {
            /// <summary>Flags of this member</summary>
            public byte Flags;
            /// <summary>
            /// the most recent modification time of the original file being compressed.  The time is in Unix format, i.e., seconds since 00:00:00 GMT, Jan.  1, 1970.
            /// </summary>
            public UInt32 ModificationTime;
            /// <summary>
            /// 2 = compressor used maximum compression, slowest algorithm
            /// 4 = compressor used fastest algorithm
            /// </summary>
            public byte ExtraFLags;
            /// <summary>
            /// 0 - FAT filesystem (MS-DOS, OS/2, NT/Win32)
            /// 1 - Amiga
            /// 2 - VMS (or OpenVMS)
            /// 3 - Unix
            /// 4 - VM/CMS
            /// 5 - Atari TOS
            /// 6 - HPFS filesystem (OS/2, NT)
            /// 7 - Macintosh
            /// 8 - Z-System
            /// 9 - CP/M
            /// 10 - TOPS-20
            /// 11 - NTFS filesystem (NT)
            /// 12 - QDOS
            /// 13 - Acorn RISCOS
            /// 255 - unknown
            /// </summary>
            public byte OperatingSystem;
            /// <summary>
            /// Name in ISO 8859-1 (LATIN-1) characters. Forced to lower case if gzip is generated on a file system with case insensitive names.
            /// </summary>
            public byte[] FileName;
            /// <summary>
            /// Coment in ISO 8859-1 (LATIN-1) characters.
            /// </summary>
            public byte[] FileComment;
            /// <summary>
            /// Cyclic Redundancy Check value of the uncompressed data computed according to CRC-32 algorithm used in the ISO 3309 standard.
            /// </summary>
            public UInt32 Crc32;
            /// <summary>
            /// This contains the size of the original (uncompressed) input data modulo 2^32.
            /// </summary>
            public UInt32 InputSize;
        }

        [Flags]
        enum MemberFlags : byte
        {
            /// <summary>The file is probably ASCII text</summary>
            FTEXT = 1,
            /// <summary>CRC16 for the gzip header is present immediately before the compressed data</summary>
            FHCRC = 2,
            /// <summary>Optional extra fields are present</summary>
            FEXTRA = 4,
            /// <summary>Original file name is present, terminated by a zero byte. </summary>
            FNAME = 8,
            /// <summary>Zero-terminated file comment is present.</summary>
            FCOMMENT = 16,
        }
        #endregion

        #region Private fields
        // Filename of storage file
        private string gzipFileName;
        // Stream object of storage file
        private Stream gzipFileStream;
        // Inform of archive blocking the zip file. Null if not blocked.
        private string blocked = null;
        #endregion

        #region Public methods
        /// <summary>
        /// Create a new GZIP file
        /// </summary>
        /// <param name="pathFilename">Full path of GZip file to create</param>
        /// <param name="zipComment">General comment for GZip file</param>
        /// <returns>LittleGZip object</returns>
        public LittleGzip(string pathFilename)
        {
            Stream gzipStream;

            try
            {
                if (File.Exists(pathFilename))
                    gzipStream = new FileStream(pathFilename, FileMode.Open, FileAccess.ReadWrite);
                else
                    gzipStream = (Stream)new FileStream(pathFilename, FileMode.Create, FileAccess.ReadWrite);

                this.gzipFileName = pathFilename;
                this.gzipFileStream = gzipStream;
            }
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn LittleGzip"); }
        }

        /// <summary>
        /// Create a new Gzip storage in a stream
        /// </summary>
        /// <param name="gzipStream">Stream Zip to create</param>
        /// <param name="zipComment">General comment for Zip file</param>
        /// <returns>LittleGZip object</returns>
        public LittleGzip(Stream gzipStream)
        {
            try
            {
                if (!gzipStream.CanSeek)
                    throw new InvalidOperationException("Stream cannot seek");

                this.gzipFileStream = gzipStream;
            }
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn LittleGzip"); }
        }

        /// <summary>
        /// Add full contents of a file into the Zip storage
        /// </summary>
        /// <param name="_pathname">Full path of file to add to GZIP storage</param>
        /// <param name="filenameInGzip">Filename and path  for stored file</param>>
        /// <param name="compressionLevel">Level os compression. 0 = store, 6 = medium/default, 13 = high</param>
        /// <param name="fileComment">Comment for stored file</param>   
        public void AddFile(string pathFilename, string filenameInGzip, int compressionLevel = 6, string fileComment = "")
        {
            byte[] inBuffer = null;
            try
            {
                //Check the maximun file size
                if (pathFilename.Length > Int32.MaxValue - 56)
                    throw new Exception("File is too large to be processed by this program. Maximum size " + (Int32.MaxValue - 56));

                //Read the imput file
                if (new System.IO.FileInfo(pathFilename).Length > 0)
                    inBuffer = File.ReadAllBytes(pathFilename);
                else
                    inBuffer = new byte[0];

                DateTime modifyTime = File.GetLastWriteTime(pathFilename);

                //Add inBuffer to Zip
                AddBuffer(inBuffer, filenameInGzip, modifyTime, compressionLevel, fileComment);
                inBuffer = null;
            }
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn LittleGzip.AddFile"); }
        }

        /// <summary>
        /// Add full contents of a array into the Gzip storage
        /// </summary>
        /// <param name="inBuffer">Data to store in Gzip</param>
        /// <param name="filenameInGzip">Filename and path  for stored file</param>>
        /// <param name="modifyTime">Modify time for stored file</param>
        /// <param name="compressionLevel">Level os compression. 0 = store, 6 = medium/default, 13 = high</param>
        /// <param name="fileComment">Comment for stored file</param>   
        public void AddBuffer(byte[] inBuffer, string filenameInGzip, DateTime modifyTime, int compressionLevel = 6, string fileComment = "")
        {
            try
            {
                byte[] outBuffer = null;
                uint compressedSize;

                // Prepare the GzipMember
                GzipMember member = new GzipMember();
                member.OperatingSystem = 11;
                member.FileName = NormalizedFilename(filenameInGzip);
                member.Flags = (byte)MemberFlags.FNAME;
                member.ModificationTime = DateTimeToUnixTimestamp(modifyTime);
                member.InputSize = (uint)inBuffer.Length;
                if (compressionLevel > 6)
                    member.ExtraFLags = 2;
                else
                    member.ExtraFLags = 4;

                if (fileComment != String.Empty)
                {
                    member.Flags = (byte)(MemberFlags.FNAME | MemberFlags.FCOMMENT);
                    member.FileComment = ConvertLatin1(fileComment);
                }
                // Deflate the Source and get ZipFileEntry data
                UnsafeNativeMethods.Libdeflate(inBuffer, compressionLevel, true, out outBuffer, out compressedSize, out member.Crc32);

                //Wait for idle GzipFile stream
                while (blocked != filenameInGzip)
                {
                    if (blocked == null)
                        blocked = filenameInGzip;
                    else
                    {
                        Thread.Sleep(5);
                        Application.DoEvents();
                    }
                }

                // Write member to stream
                WriteMember(ref member, ref outBuffer, compressedSize);

                //unblock zip file
                blocked = null;
            }
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn LittleGzip.AddBuffer"); }
        }

        /// <summary>
        /// Close file and streams
        /// </summary>
        /// <remarks>This is a required step, unless automatic dispose is used</remarks>
        public void Close()
        {
            try
            {
                if (this.gzipFileStream != null)
                {
                    this.gzipFileStream.Flush();
                    this.gzipFileStream.Dispose();
                    this.gzipFileStream = null;
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn LittleGzip.Close"); }
        }
        #endregion

        #region Private methods

        /// <summary>
        /// Append one member to GZIP stream
        /// </summary>
        /// <param name="member">Member metadata</param>
        /// <param name="deflatedData">Deflated file data</param>
        /// <param name="compressedSize">Compressed size</param>
        private void WriteMember(ref GzipMember member, ref byte[] deflatedData, uint compressedSize)
        {
            try
            {
                this.gzipFileStream.Write(new byte[] { 0x1f, 0x8b, 0x08 }, 0, 3);                           // ID1 (IDentification 1) + ID2 (IDentification 2) + CM (Compression Method)
                this.gzipFileStream.Write(BitConverter.GetBytes((ushort)(member.Flags)), 0, 1);             // FLG (FLaGs)
                this.gzipFileStream.Write(BitConverter.GetBytes(member.ModificationTime), 0, 4);            // MTIME (Modification TIME)
                this.gzipFileStream.Write(BitConverter.GetBytes((ushort)(member.ExtraFLags)), 0, 1);        // XFL (eXtra FLags)
                this.gzipFileStream.Write(BitConverter.GetBytes((ushort)(member.OperatingSystem)), 0, 1);   // OS (Operating System)
                this.gzipFileStream.Write(member.FileName, 0, member.FileName.Length);                      //Filename
                if (member.FileComment != null)
                    this.gzipFileStream.Write(member.FileComment, 0, member.FileComment.Length);            //Comment of file
                this.gzipFileStream.Write(deflatedData, 0, (int)compressedSize);
                this.gzipFileStream.Write(BitConverter.GetBytes(member.Crc32), 0, 4);                       // CRC32
                this.gzipFileStream.Write(BitConverter.GetBytes(member.InputSize), 0, 4);                   // Uncompressed size
            }
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn LittleGzip.WriteMember"); }
        }

        /// <summary>
        /// Convert windows time to unix time
        /// </summary>
        /// <param name="dateTime">Time in windows format</param>
        /// <returns>Time in unix format</returns>
        private UInt32 DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (UInt32)(TimeZoneInfo.ConvertTimeToUtc(dateTime) - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        }

        /// <summary>
        /// Convert string to LATIN1, null terminate.
        /// </summary>
        /// <param name="text">Text to convert</param>
        /// <returns>text in Latin1, null temrinated</returns>
        private byte[] ConvertLatin1(string text)
        {
            byte[] latin1 = Encoding.GetEncoding("iso-8859-1").GetBytes(text + "0");
            latin1[latin1.Length - 1] = 0;
            return latin1;
        }

        /// <summary>
        /// Replaces backslashes with slashes to store in zip header. Remove unit letter. Convert to lowcase LATIN1 and null terminated
        /// </summary>
        /// <param name="filename">Filename to convert</param>
        /// <returns>Filename in array null terminated</returns>
        private byte[] NormalizedFilename(string filename)
        {
            try
            {
                filename = filename.Replace('\\', '/');

                int pos = filename.IndexOf(':');
                if (pos >= 0)
                    filename = filename.Remove(0, pos + 1);

                filename = filename.Trim('/').ToLower();
                return ConvertLatin1(filename);
            }
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn LittleZip.NormalizedFilename"); }
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Closes the Zip file stream
        /// </summary>
        public void Dispose()
        {
            this.Close();
        }
        #endregion
    }

    [SuppressUnmanagedCodeSecurityAttribute]
    internal sealed partial class UnsafeNativeMethods
    {
        /// <summary>
        /// Make CRC-32 checksum
        /// </summary>
        /// <param name="buffer">Data to checksum</param>
        /// <returns>The updated checksum</returns>
        public static uint GetCrc32(byte[] buffer)
        {
            IntPtr ptrBuffer = IntPtr.Zero;
            GCHandle pinnedBuffer;

            try
            {
                //Get ptrInBuffer
                pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                ptrBuffer = pinnedBuffer.AddrOfPinnedObject();

                uint crc = UnsafeNativeMethods.LibdeflateCrc32(0, ptrBuffer, buffer.Length);

                pinnedBuffer.Free();

                return crc;
            }
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn Libdeflate.Deflate"); }
        }


        /// <summary>
        /// Deflate array 
        /// </summary>
        /// <param name="inBuffer">Data to deflate</param>
        /// <param name="compressionLevel">The compression level on a zlib-like scale but with a higher maximum value (1 = fastest, 6 = medium/default, 9 = slow, 13 = slowest)</param>
        /// <param name="outBuffer">Data deflated</param>
        /// <param name="deflatedSize">Size of deflated data</param>
        /// <param name="crc32">CRC of deflated data</param>
        public static void Libdeflate(byte[] inBuffer, int compressionLevel, bool force, out byte[] outBuffer, out uint deflatedSize, out uint crc32)
        {
            IntPtr ptrInBuffer = IntPtr.Zero;
            IntPtr ptrOutBuffer = IntPtr.Zero;
            GCHandle pinnedInArray;
            GCHandle pinnedOutArray;
            int maxCompresedSize;
            IntPtr compressor = IntPtr.Zero;
            try
            {
                //Get ptrInBuffer
                pinnedInArray = GCHandle.Alloc(inBuffer, GCHandleType.Pinned);
                ptrInBuffer = pinnedInArray.AddrOfPinnedObject();

                //Allocate compressor
                compressor = UnsafeNativeMethods.LibdeflateAllocCompressor(compressionLevel);
                if (compressor == null)
                    throw new Exception("Out of memory");

                //Get CRC32
                crc32 = UnsafeNativeMethods.LibdeflateCrc32(0, ptrInBuffer, inBuffer.Length);

                //Allocate output buffer
                if (force)
                    maxCompresedSize = UnsafeNativeMethods.LibdeflateDeflateCompressBound(compressor, inBuffer.Length);
                else
                    maxCompresedSize = inBuffer.Length - 1;

                outBuffer = new byte[maxCompresedSize];
                pinnedOutArray = GCHandle.Alloc(outBuffer, GCHandleType.Pinned);
                ptrOutBuffer = pinnedOutArray.AddrOfPinnedObject();

                //compress
                deflatedSize = (uint)UnsafeNativeMethods.LibdeflateDeflateCompress(compressor, ptrInBuffer, inBuffer.Length, ptrOutBuffer, maxCompresedSize);

                //Free resources
                UnsafeNativeMethods.LibdeflateFreeCompressor(compressor);
                pinnedInArray.Free();
                pinnedOutArray.Free();
            }
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn Libdeflate.Deflate"); }
        }

        /* ========================================================================== */
        /*                             Compression                                    */
        /* ========================================================================== */

        /// <summary>
        /// Allocates a new compressor that supports DEFLATE, zlib, and gzip compression.
        ///
        /// Note: for compression, the sliding window size is defined at compilation time
        /// to 32768, the largest size permissible in the DEFLATE format. It cannot be
        /// changed at runtime.
        /// 
        /// A single compressor is not safe to use by multiple threads concurrently.
        /// However, different threads may use different compressors concurrently.
        /// </summary>
        /// <param name="compression_level">The compression level on a zlib-like scale but with a higher maximum value (1 = fastest, 6 = medium/default, 9 = slow, 13 = slowest)</param>
        /// <returns>Pointer to the new compressor, or NULL if out of memory.</returns>
        public static IntPtr LibdeflateAllocCompressor(int compression_level)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return libdeflate_alloc_compressor_x86(compression_level);
                case 8:
                    return libdeflate_alloc_compressor_x64(compression_level);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libdeflate_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libdeflate_alloc_compressor")]
        private static extern IntPtr libdeflate_alloc_compressor_x86(int compression_level);
        [DllImport("libdeflate_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libdeflate_alloc_compressor")]
        private static extern IntPtr libdeflate_alloc_compressor_x64(int compression_level);

        /// <summary>Performs raw DEFLATE in the ZLIB format compression on a buffer of data.</summary>
        /// <param name="compressor">Pointer to the compressor</param>
        /// <param name="inData">Data to compress</param>
        /// <param name="in_nbytes">Length of data to compress</param>
        /// <param name="outBuffer">Data compresed</param>
        /// <param name="out_nbytes_avail">Leght of buffer for data compresed</param>
        /// <returns>Compressed size in bytes, or 0 if the data could not be compressed to 'out_nbytes_avail' bytes or fewer.</returns>
        public static long LibdeflateZlibCompress(IntPtr compressor, IntPtr inBuffer, int in_nbytes, IntPtr outBuffer, int out_nbytes_avail)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return (long)(libdeflate_zlib_compress_x86(compressor, inBuffer, (UIntPtr)in_nbytes, outBuffer, (UIntPtr)out_nbytes_avail));
                case 8:
                    return (long)(libdeflate_zlib_compress_x64(compressor, inBuffer, (UIntPtr)in_nbytes, outBuffer, (UIntPtr)out_nbytes_avail));
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libdeflate_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libdeflate_zlib_compress")]
        private static extern UIntPtr libdeflate_zlib_compress_x86(IntPtr compressor, IntPtr inBuffer, UIntPtr in_nbytes, IntPtr outBuffer, UIntPtr out_nbytes_avail);
        [DllImport("libdeflate_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libdeflate_zlib_compress")]
        private static extern UIntPtr libdeflate_zlib_compress_x64(IntPtr compressor, IntPtr inBuffer, UIntPtr in_nbytes, IntPtr outBuffer, UIntPtr out_nbytes_avail);

        /// <summary>Performs raw DEFLATE compression on a buffer of data.</summary>
        /// <param name="compressor">Pointer to the compressor</param>
        /// <param name="inData">Data to compress</param>
        /// <param name="in_nbytes">Length of data to compress</param>
        /// <param name="outData">Data compresed</param>
        /// <param name="out_nbytes_avail">Leght of buffer for data compresed</param>
        /// <returns>Compressed size in bytes, or 0 if the data could not be compressed to 'out_nbytes_avail' bytes or fewer.</returns>
        public static long LibdeflateDeflateCompress(IntPtr compressor, IntPtr inBuffer, int in_nbytes, IntPtr outBuffer, int out_nbytes_avail)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return (long)(libdeflate_deflate_compress_x86(compressor, inBuffer, (UIntPtr)in_nbytes, outBuffer, (UIntPtr)out_nbytes_avail));
                case 8:
                    return (long)(libdeflate_deflate_compress_x64(compressor, inBuffer, (UIntPtr)in_nbytes, outBuffer, (UIntPtr)out_nbytes_avail));
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libdeflate_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libdeflate_deflate_compress")]
        private static extern UIntPtr libdeflate_deflate_compress_x86(IntPtr compressor, IntPtr inBuffer, UIntPtr in_nbytes, IntPtr outBuffer, UIntPtr out_nbytes_avail);
        [DllImport("libdeflate_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libdeflate_deflate_compress")]
        private static extern UIntPtr libdeflate_deflate_compress_x64(IntPtr compressor, IntPtr inBuffer, UIntPtr in_nbytes, IntPtr outBuffer, UIntPtr out_nbytes_avail);

        /// <summary>Performs raw GZIP compression on a buffer of data.</summary>
        /// <param name="compressor">Pointer to the compressor</param>
        /// <param name="inData">Data to compress</param>
        /// <param name="in_nbytes">Length of data to compress</param>
        /// <param name="outData">Data compresed</param>
        /// <param name="out_nbytes_avail">Leght of buffer for data compresed</param>
        /// <returns>Compressed size in bytes, or 0 if the data could not be compressed to 'out_nbytes_avail' bytes or fewer.</returns>
        public static long libdeflateGzipCompress(IntPtr compressor, IntPtr inBuffer, int in_nbytes, IntPtr outBuffer, int out_nbytes_avail)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return (long)(libdeflate_gzip_compress_x86(compressor, inBuffer, (UIntPtr)in_nbytes, outBuffer, (UIntPtr)out_nbytes_avail));
                case 8:
                    return (long)(libdeflate_gzip_compress_x64(compressor, inBuffer, (UIntPtr)in_nbytes, outBuffer, (UIntPtr)out_nbytes_avail));
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libdeflate_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libdeflate_gzip_compress")]
        private static extern UIntPtr libdeflate_gzip_compress_x86(IntPtr compressor, IntPtr inBuffer, UIntPtr in_nbytes, IntPtr outBuffer, UIntPtr out_nbytes_avail);
        [DllImport("libdeflate_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libdeflate_gzip_compress")]
        private static extern UIntPtr libdeflate_gzip_compress_x64(IntPtr compressor, IntPtr inBuffer, UIntPtr in_nbytes, IntPtr outBuffer, UIntPtr out_nbytes_avail);

        /// <summary>Get the worst-case upper bound on the number of bytes of compressed data that may be produced
        /// by compressing any buffer of length less than or equal to 'in_nbytes'.
        /// Mathematically, this bound will necessarily be a number greater than or equal to 'in_nbytes'.
        /// It may be an overestimate of the true upper bound.  
        /// As a special case, 'compressor' may be NULL.  This causes the bound to be taken across *any*
        /// libdeflate_compressor that could ever be allocated with this build of the library, with any options.
        /// 
        /// With block-based compression, it is usually preferable to separately store the uncompressed size of each
        /// block and to store any blocks that did not compress to less than their original size uncompressed.  In that
        /// scenario, there is no need to know the worst-case compressed size, since the maximum number of bytes of
        /// compressed data that may be used would always be one less than the input length.  You can just pass a
        /// buffer of that size to libdeflate_deflate_compress() and store the data uncompressed if libdeflate_deflate_compress()
        /// returns 0, indicating that the compressed data did not fit into the provided output buffer.</summary>
        /// <param name="compressor">Pointer to the compressor</param>
        /// <param name="in_nbytes">Length of data to compress</param>
        /// <returns>Worst-case upper bound on the number of bytes of compressed data that may be produced by compressing any buffer of length less than or equal to 'in_nbytes'.</returns>
        public static int LibdeflateZlibCompressBound(IntPtr compressor, int in_nbytes)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return (int)libdeflate_zlib_compress_bound_x86(compressor, (UIntPtr)in_nbytes);
                case 8:
                    return (int)libdeflate_zlib_compress_bound_x64(compressor, (UIntPtr)in_nbytes);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libdeflate_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libdeflate_zlib_compress_bound")]
        private static extern UIntPtr libdeflate_zlib_compress_bound_x86(IntPtr compressor, UIntPtr in_nbytes);
        [DllImport("libdeflate_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libdeflate_zlib_compress_bound")]
        private static extern UIntPtr libdeflate_zlib_compress_bound_x64(IntPtr compressor, UIntPtr in_nbytes);

        /// <summary>Returns a worst-case upper bound on the number of bytes of compressed data that may be produced
        /// by compressing any buffer of length less than or equal to 'in_nbytes'.
        /// Mathematically, this bound will necessarily be a number greater than or equal to 'in_nbytes'.
        /// It may be an overestimate of the true upper bound.  
        /// As a special case, 'compressor' may be NULL.  This causes the bound to be taken across *any*
        /// libdeflate_compressor that could ever be allocated with this build of the library, with any options.
        /// </summary>
        /// <param name="compressor">Pointer to the compressor</param>
        /// <param name="in_nbytes">Length of data to compress</param>
        /// <returns>Worst-case upper bound on the number of bytes of compressed data that may be produced by compressing any buffer of length less than or equal to 'in_nbytes'.</returns>
        public static int LibdeflateDeflateCompressBound(IntPtr compressor, int in_nbytes)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return (int)libdeflate_deflate_compress_bound_x86(compressor, (UIntPtr)in_nbytes);
                case 8:
                    return (int)libdeflate_deflate_compress_bound_x64(compressor, (UIntPtr)in_nbytes);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libdeflate_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libdeflate_deflate_compress_bound")]
        private static extern UIntPtr libdeflate_deflate_compress_bound_x86(IntPtr compressor, UIntPtr in_nbytes);
        [DllImport("libdeflate_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libdeflate_deflate_compress_bound")]
        private static extern UIntPtr libdeflate_deflate_compress_bound_x64(IntPtr compressor, UIntPtr in_nbytes);

        /// <summary>Returns a worst-case upper bound on the number of bytes of compressed data that may be produced
        /// by compressing any buffer of length less than or equal to 'in_nbytes'.
        /// Mathematically, this bound will necessarily be a number greater than or equal to 'in_nbytes'.
        /// It may be an overestimate of the true upper bound.  
        /// As a special case, 'compressor' may be NULL.  This causes the bound to be taken across *any*
        /// libdeflate_compressor that could ever be allocated with this build of the library, with any options.
        /// 
        /// With block-based compression, it is usually preferable to separately store the uncompressed size of each
        /// block and to store any blocks that did not compress to less than their original size uncompressed.  In that
        /// scenario, there is no need to know the worst-case compressed size, since the maximum number of bytes of
        /// compressed data that may be used would always be one less than the input length.  You can just pass a
        /// buffer of that size to libdeflate_deflate_compress() and store the data uncompressed if libdeflate_deflate_compress()
        /// returns 0, indicating that the compressed data did not fit into the provided output buffer.</summary>
        /// <param name="compressor">Pointer to the compressor</param>
        /// <param name="in_nbytes">Length of data to compress</param>
        /// <returns>Worst-case upper bound on the number of bytes of compressed data that may be produced by compressing any buffer of length less than or equal to 'in_nbytes'.</returns>
        public static int LibdeflateGzipCompressBound(IntPtr compressor, int in_nbytes)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return (int)libdeflate_gzip_compress_bound_x86(compressor, (UIntPtr)in_nbytes);
                case 8:
                    return (int)libdeflate_gzip_compress_bound_x64(compressor, (UIntPtr)in_nbytes);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libdeflate_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libdeflate_gzip_compress_bound")]
        private static extern UIntPtr libdeflate_gzip_compress_bound_x86(IntPtr compressor, UIntPtr in_nbytes);
        [DllImport("libdeflate_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libdeflate_gzip_compress_bound")]
        private static extern UIntPtr libdeflate_gzip_compress_bound_x64(IntPtr compressor, UIntPtr in_nbytes);

        /// <summary>Frees a compressor that was allocated with libdeflate_alloc_compressor()</summary>
        /// <param name="compressor">Pointer to the compressor</param>
        public static void LibdeflateFreeCompressor(IntPtr compressor)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    libdeflate_free_compressor_x86(compressor);
                    break;
                case 8:
                    libdeflate_free_compressor_x64(compressor);
                    break;
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libdeflate_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libdeflate_free_compressor")]
        private static extern void libdeflate_free_compressor_x86(IntPtr compressor);
        [DllImport("libdeflate_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libdeflate_free_compressor")]
        private static extern void libdeflate_free_compressor_x64(IntPtr compressor);


        /* ========================================================================== */
        /*                                Checksums                                   */
        /* ========================================================================== */

        /// <summary>Updates a running CRC-32 checksum</summary>
        /// <param name="crc">Inial value of checksum. When starting a new checksum will be 0</param>
        /// <param name="buffer">Data to checksum</param>
        /// <param name="len">Length of data</param>
        /// <returns>The updated checksum</returns>
        public static UInt32 LibdeflateCrc32(UInt32 crc, IntPtr inBuffer, int len)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return libdeflate_crc32_x86(crc, inBuffer, (UIntPtr)len);
                case 8:
                    return libdeflate_crc32_x64(crc, inBuffer, (UIntPtr)len);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libdeflate_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libdeflate_crc32")]
        private static extern UInt32 libdeflate_crc32_x86(UInt32 crc, IntPtr inBuffer, UIntPtr len);
        [DllImport("libdeflate_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libdeflate_crc32")]
        private static extern UInt32 libdeflate_crc32_x64(UInt32 crc, IntPtr inBuffer, UIntPtr len);

        /// <summary>Updates a running Adler-32 checksum</summary>
        /// <param name="crc">Inial value of checksum. When starting a new checksum will be 1</param>
        /// <param name="buffer">Data to checksum</param>
        /// <param name="len">Length of data</param>
        /// <returns>The updated checksum</returns>
        public static UInt32 LibdeflateAdler32(UInt32 crc, IntPtr inBuffer, int len)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return libdeflate_adler32_x86(crc, inBuffer, (UIntPtr)len);
                case 8:
                    return libdeflate_adler32_x64(crc, inBuffer, (UIntPtr)len);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libdeflate_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libdeflate_adler32")]
        private static extern UInt32 libdeflate_adler32_x86(UInt32 crc, IntPtr inBuffer, UIntPtr len);
        [DllImport("libdeflate_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libdeflate_adler32")]
        private static extern UInt32 libdeflate_adler32_x64(UInt32 crc, IntPtr inBuffer, UIntPtr len);
    }
}
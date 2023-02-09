using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Protean.Tools.IFilter
{
    [Flags()]
    public enum IFILTER_INIT : uint
    {
        NONE = 0U,
        CANON_PARAGRAPHS = 1U,
        HARD_LINE_BREAKS = 2U,
        CANON_HYPHENS = 4U,
        CANON_SPACES = 8U,
        APPLY_INDEX_ATTRIBUTES = 16U,
        APPLY_CRAWL_ATTRIBUTES = 256U,
        APPLY_OTHER_ATTRIBUTES = 32U,
        INDEXING_ONLY = 64U,
        SEARCH_LINKS = 128U,
        FILTER_OWNED_VALUE_OK = 512U
    }

    public enum CHUNK_BREAKTYPE
    {
        CHUNK_NO_BREAK = 0,
        CHUNK_EOW = 1,
        CHUNK_EOS = 2,
        CHUNK_EOP = 3,
        CHUNK_EOC = 4
    }

    [Flags()]
    public enum CHUNKSTATE
    {
        CHUNK_TEXT = 0x1,
        CHUNK_VALUE = 0x2,
        CHUNK_FILTER_OWNED_VALUE = 0x4
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PROPSPEC
    {
        public uint ulKind;
        public uint propid;
        public IntPtr lpwstr;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FULLPROPSPEC
    {
        public Guid guidPropSet;
        public PROPSPEC psProperty;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STAT_CHUNK
    {
        public uint idChunk;
        [MarshalAs(UnmanagedType.U4)]
        public CHUNK_BREAKTYPE breakType;
        [MarshalAs(UnmanagedType.U4)]
        public CHUNKSTATE flags;
        public uint locale;
        [MarshalAs(UnmanagedType.Struct)]
        public FULLPROPSPEC attribute;
        public uint idChunkSource;
        public uint cwcStartSource;
        public uint cwcLenSource;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FILTERREGION
    {
        public uint idChunk;
        public uint cwcStart;
        public uint cwcExtent;
    }

    [ComImport()]
    [Guid("89BCB740-6119-101A-BCB7-00DD010655AF")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IFilter
    {
        [PreserveSig()]
        int Init([MarshalAs(UnmanagedType.U4)] IFILTER_INIT grfFlags, uint cAttributes, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] FULLPROPSPEC[] aAttributes, ref uint pdwFlags);
        [PreserveSig()]
        int GetChunk(STAT_CHUNK pStat);
        [PreserveSig()]
        int GetText(ref uint pcwcBuffer, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder buffer);
        void GetValue(ref UIntPtr ppPropValue);
        void BindRegion([MarshalAs(UnmanagedType.Struct)] FILTERREGION origPos, ref Guid riid, ref UIntPtr ppunk);
    }

    [ComImport()]
    [Guid("f07f3920-7b8c-11cf-9be8-00aa004b9986")]
    public class CFilter
    {
    }

    public class IFilterConstants
    {
        public const uint PID_STG_DIRECTORY = 0x2U;
        public const uint PID_STG_CLASSID = 0x3U;
        public const uint PID_STG_STORAGETYPE = 0x4U;
        public const uint PID_STG_VOLUME_ID = 0x5U;
        public const uint PID_STG_PARENT_WORKID = 0x6U;
        public const uint PID_STG_SECONDARYSTORE = 0x7U;
        public const uint PID_STG_FILEINDEX = 0x8U;
        public const uint PID_STG_LASTCHANGEUSN = 0x9U;
        public const uint PID_STG_NAME = 0xAU;
        public const uint PID_STG_PATH = 0xBU;
        public const uint PID_STG_SIZE = 0xCU;
        public const uint PID_STG_ATTRIBUTES = 0xDU;
        public const uint PID_STG_WRITETIME = 0xEU;
        public const uint PID_STG_CREATETIME = 0xFU;
        public const uint PID_STG_ACCESSTIME = 0x10U;
        public const uint PID_STG_CHANGETIME = 0x11U;
        public const uint PID_STG_CONTENTS = 0x13U;
        public const uint PID_STG_SHORTNAME = 0x14U;
        public const long FILTER_E_END_OF_CHUNKS = 2147751680L;
        public const long FILTER_E_NO_MORE_TEXT = 2147751681L;
        public const long FILTER_E_NO_MORE_VALUES = 2147751682L;
        public const long FILTER_E_NO_TEXT = 2147751685L;
        public const long FILTER_E_NO_VALUES = 2147751686L;
        public const int FILTER_S_LAST_TEXT = 268041;
    }

    /// 
    /// IFilter return codes
    public enum IFilterReturnCodes : uint
    {
        /// 
        /// Success
        S_OK = 0U,
        /// 
        /// The function was denied access to the filter file.
        E_ACCESSDENIED = int.MinValue + 0x00070005,
        /// 
        /// The function encountered an invalid handle, probably due to a low-memory situation.
        E_HANDLE = int.MinValue + 0x00070006,
        /// 
        /// The function received an invalid parameter.
        E_INVALIDARG = int.MinValue + 0x00070057,
        /// 
        /// Out of memory
        E_OUTOFMEMORY = int.MinValue + 0x0007000E,
        /// 
        /// Not implemented
        E_NOTIMPL = int.MinValue + 0x00004001,
        /// 
        /// Unknown error
        E_FAIL = int.MinValue + 0x00000008,
        /// 
        /// File not filtered due to password protection
        FILTER_E_PASSWORD = int.MinValue + 0x0004170B,
        /// 
        /// The document format is not recognised by the filter
        FILTER_E_UNKNOWNFORMAT = int.MinValue + 0x0004170C,
        /// 
        /// No text in current chunk
        FILTER_E_NO_TEXT = int.MinValue + 0x00041705,
        /// 
        /// No more chunks of text available in object
        FILTER_E_END_OF_CHUNKS = int.MinValue + 0x00041700,
        /// 
        /// No more text available in chunk
        FILTER_E_NO_MORE_TEXT = int.MinValue + 0x00041701,
        /// 
        /// No more property values available in chunk
        FILTER_E_NO_MORE_VALUES = int.MinValue + 0x00041702,
        /// 
        /// Unable to access object
        FILTER_E_ACCESS = int.MinValue + 0x00041703,
        /// 
        /// Moniker doesn't cover entire region
        FILTER_W_MONIKER_CLIPPED = 0x41704U,
        /// 
        /// Unable to bind IFilter for embedded object
        FILTER_E_EMBEDDING_UNAVAILABLE = int.MinValue + 0x00041707,
        /// 
        /// Unable to bind IFilter for linked object
        FILTER_E_LINK_UNAVAILABLE = int.MinValue + 0x00041708,
        /// 
        /// This is the last text in the current chunk
        FILTER_S_LAST_TEXT = 0x41709U,
        /// 
        /// This is the last value in the current chunk
        FILTER_S_LAST_VALUES = 0x4170AU
    }

    /// 
    /// Convenience class which provides static methods to extract text from files using installed IFilters
    public class DefaultParser
    {
        public DefaultParser()
        {
        }

        [DllImport("query.dll", CharSet = CharSet.Unicode)]
        private static extern int LoadIFilter(string pwcsPath, [MarshalAs(UnmanagedType.IUnknown)] object pUnkOuter, ref IFilter ppIUnk);

        private static IFilter loadIFilter(string filename)
        {
            object outer = null;
            IFilter filter = null;

            // Try to load the corresponding IFilter
            int resultLoad = LoadIFilter(filename, outer, ref filter);
            if (resultLoad != (int)IFilterReturnCodes.S_OK)
            {
                return null;
            }

            return filter;
        }

        public static bool IsParseable(string filename)
        {
            return loadIFilter(filename) is object;
        }

        public static string Extract(string path)
        {
            var sb = new StringBuilder();
            IFilter filter = null;
            try
            {
                filter = loadIFilter(path);
                if (filter is null)
                {
                    return string.Empty;
                }

                uint i = 0U;
                var ps = new STAT_CHUNK();
                var iflags = IFILTER_INIT.CANON_HYPHENS | IFILTER_INIT.CANON_PARAGRAPHS | IFILTER_INIT.CANON_SPACES | IFILTER_INIT.APPLY_CRAWL_ATTRIBUTES | IFILTER_INIT.APPLY_INDEX_ATTRIBUTES | IFILTER_INIT.APPLY_OTHER_ATTRIBUTES | IFILTER_INIT.HARD_LINE_BREAKS | IFILTER_INIT.SEARCH_LINKS | IFILTER_INIT.FILTER_OWNED_VALUE_OK;
                if (filter.Init(iflags, 0U, null, ref i) != (int)IFilterReturnCodes.S_OK)
                {
                    throw new Exception("Problem initializing an IFilter for:" + Constants.vbLf + path + " " + Constants.vbLf + Constants.vbLf);
                }

                while (filter.GetChunk(ps) == (int)IFilterReturnCodes.S_OK)
                {
                    if (ps.flags == CHUNKSTATE.CHUNK_TEXT)
                    {
                        IFilterReturnCodes scode = 0;
                        while (scode == IFilterReturnCodes.S_OK || scode == IFilterReturnCodes.FILTER_S_LAST_TEXT)
                        {
                            uint pcwcBuffer = 65536U;
                            var sbBuffer = new StringBuilder((int)pcwcBuffer);

                            // ts changed directCast to Ctype when adding from C#
                            scode = (IFilterReturnCodes)Conversions.ToUInteger(filter.GetText(ref pcwcBuffer, sbBuffer));
                            if (pcwcBuffer > 0L && sbBuffer.Length > 0)
                            {
                                if (sbBuffer.Length < pcwcBuffer)
                                {
                                    // Should never happen, but it happens !
                                    pcwcBuffer = (uint)sbBuffer.Length;
                                }

                                sb.Append(sbBuffer.ToString(0, (int)pcwcBuffer));
                                // "\r\n"
                                sb.Append(" ");
                            }
                        }
                    }
                }
            }
            finally
            {
                if (filter is object)
                {
                    Marshal.ReleaseComObject(filter);
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }

            return sb.ToString();
        }
    }
}
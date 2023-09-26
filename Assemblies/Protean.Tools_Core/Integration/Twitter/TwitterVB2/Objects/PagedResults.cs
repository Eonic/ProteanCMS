using System.Collections.Generic;
namespace Protean.Tools.Integration.Twitter.TwitterVB2
{
    public partial class PagedResults<T> : List<T>
    {

        public PagedResults()
        {
            LCursor = -1;
        }

        public PagedResults(long Cursor)
        {
            LCursor = Cursor;
        }

        private long LCursor;
        public long Cursor
        {
            get
            {
                return LCursor;
            }
            set
            {
                LCursor = value;
            }
        }

        public bool HasMore
        {
            get
            {
                return Cursor != 0L;
            }
        }

    }
}

namespace Spolty.Framework.Parameters.Paging
{
    internal class PagingParameter : IParameterMarker
    {
        private int _pageNumber;
        private int _pageSize;
        private int _count;
        private int _skipRecords;

        public int PageNumber
        {
            get { return _pageNumber; }
            set
            {
                if (_pageNumber != value)
                {
                    _pageNumber = value;
                    RecalculateSkipRecords();
                }
            }
        }

        public int PageSize
        {
            get { return _pageSize; }
            set
            {
                if (_pageSize != value)
                {
                    _pageSize = value;
                    RecalculateSkipRecords();
                }
            }
        }

        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }

        public int SkipRecords
        {
            get { return _skipRecords;  }
        }

        public PagingParameter(int pageNumber, int pageSize)
        {
            _pageNumber = pageNumber;
            _pageSize = pageSize;
            RecalculateSkipRecords();
        }

        private void RecalculateSkipRecords()
        {
            _skipRecords = _pageSize * _pageNumber;
        }
    }
}
namespace ProductInventoryAPI.Shared.Common
{
    /// <summary>
    /// Generic paged result for API responses
    /// </summary>
    /// <typeparam name="T">Type of items in the paged result</typeparam>
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public PagedResult()
        {
        }

        public PagedResult(IEnumerable<T> items, int pageNumber, int pageSize, int totalCount)
        {
            Items = items;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalCount = totalCount;
        }

        public static PagedResult<T> Create(IEnumerable<T> items, int pageNumber, int pageSize, int totalCount)
        {
            return new PagedResult<T>(items, pageNumber, pageSize, totalCount);
        }
    }
}
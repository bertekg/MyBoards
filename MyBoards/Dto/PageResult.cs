namespace MyBoards.Dto;

public class PageResult<T>
{
    public int TotalPages { get; set; }
    public int ItemsFrom { get; set; }
    public int ItemsTo { get; set; }
    public int TotalItemsCount { get; set; }
    public List<T> Items { get; set; }

    public PageResult(List<T> items, int totalItemsCount, int pageSize, int pageNumber) 
    {
        Items = items;
        TotalItemsCount = totalItemsCount;
        ItemsFrom = (pageNumber - 1) * pageSize + 1;
        ItemsTo = ItemsFrom + Items.Count - 1;
        TotalPages = (int)Math.Ceiling(totalItemsCount / (double)pageSize);
    }
}

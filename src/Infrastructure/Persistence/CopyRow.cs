namespace Infrastructure.Persistence;

public sealed class CopyRow
{
    public int Id { get; set; }
    public string BookId { get; set; } = "";
    public string InventoryNumber { get; set; } = "";
    public string Condition { get; set; } = "Good";
    public bool IsIssued { get; set; }
    public BookRow Book { get; set; } = null!;
}
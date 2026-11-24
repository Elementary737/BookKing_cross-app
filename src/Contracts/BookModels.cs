using System.ComponentModel.DataAnnotations;

namespace Contracts;

public sealed record BookDto(string Id, string Isbn, string Title, string Author, int Year, int Copies);

public sealed record SaveBookRequest(string Isbn, string Title, string Author, int Year, int Copies);

public sealed record PagedResult<T>(List<T> Items, int Total, int Page, int PageSize);

public sealed class BookModel
{
    public string Id { get; set; } = "";

    [Required(ErrorMessage = "ISBN обов'язковий")]
    [StringLength(32, ErrorMessage = "Максимальна довжина ISBN — 32 символи")]
    public string Isbn { get; set; } = "";

    [Required(ErrorMessage = "Назва видання обов'язкова")]
    [StringLength(200, ErrorMessage = "Максимальна довжина назви — 200 символів")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Автор обов'язковий")]
    [StringLength(150, ErrorMessage = "Максимальна довжина імені автора — 150 символів")]
    public string Author { get; set; } = "";

    [Range(1000, 2100, ErrorMessage = "Вкажіть коректний рік видання")]
    public int Year { get; set; } = DateTime.Now.Year;

    [Range(0, int.MaxValue, ErrorMessage = "Кількість примірників не може бути від'ємною")]
    public int Copies { get; set; }
}

public sealed record ApiResult(bool Ok, string? Message = null, Dictionary<string, string[]>? Errors = null);

public sealed record ValidationProblemResponse(
    string? Title,
    Dictionary<string, string[]>? Errors = null,
    string? Detail = null);

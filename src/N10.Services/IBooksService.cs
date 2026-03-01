using N10.Data.Entities;
using N10.Services.Models;
using N10.Services.Results;

namespace N10.Services;

public interface IBooksService
{
    Task<Result<List<BookModel>>> GetBooksAsync();
    IQueryable<Book> GetBooksQuery(string? include = null);
}

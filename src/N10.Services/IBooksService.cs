using N10.Data.Entities;
using N10.Services.Models;

namespace N10.Services;

public interface IBooksService
{
    Task<IEnumerable<BookModel>> GetBooksAsync();
    IQueryable<Book> GetBooksQuery(string? include = null);
}

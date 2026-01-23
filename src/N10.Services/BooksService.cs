using Microsoft.Extensions.Caching.Hybrid;
using N10.Data.Entities;
using N10.Data.Repositories;
using N10.Services.Mappings;
using N10.Services.Models;

namespace N10.Services;

public class BooksService(IBookRepository repository, HybridCache cache) : IBooksService
{
    public async Task<IEnumerable<BookModel>> GetBooksAsync()
    {
        return await cache.GetOrCreateAsync(
            "books-all",
            async token =>
            {
                var books = await repository.GetAllAsync();
                return books.Select(b => b.ToModel()).ToList();
            });
    }

    public IQueryable<Book> GetBooksQuery(string? include = null) => repository.Query(include);
}

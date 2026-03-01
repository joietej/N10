using Microsoft.Extensions.Caching.Hybrid;
using N10.Data.Entities;
using N10.Data.Repositories;
using N10.Services.Mappings;
using N10.Services.Models;
using N10.Services.Results;

namespace N10.Services;

public class BooksService(IBookRepository repository, HybridCache cache) : IBooksService
{
    public async Task<Result<List<BookModel>>> GetBooksAsync()
    {
        try
        {
            var books = await cache.GetOrCreateAsync(
                "books-all",
                async token =>
                {
                    var books = await repository.GetAllAsync();
                    return books.Select(b => b.ToModel()).ToList();
                });

            return books;
        }
        catch (Exception ex)
        {
            return Error.Unexpected("Books.GetAll.Failed", ex.Message);
        }
    }

    public IQueryable<Book> GetBooksQuery(string? include = null) => repository.Query(include);
}

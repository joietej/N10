using Microsoft.AspNetCore.Http.HttpResults;
using N10.Services;
using N10.Services.Models;

namespace N10.WebApi.Services;

public class BooksApiService(IBooksService booksService, ILogger<BooksApiService> logger) : IBooksApiService
{
    public async Task<Results<Ok<IEnumerable<BookModel>>, InternalServerError>> GetBooksAsync()
    {
        try
        {
            var books = await booksService.GetBooksAsync();
            return TypedResults.Ok(books);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
        }

        return TypedResults.InternalServerError();
    }
}

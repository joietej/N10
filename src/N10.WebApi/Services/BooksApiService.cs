using Microsoft.AspNetCore.Http.HttpResults;
using N10.Services;
using N10.Services.Models;

namespace N10.WebApi.Services;

public class BooksApiService(IBooksService booksService, ILogger<BooksApiService> logger) : IBooksApiService
{
    public async Task<Results<Ok<List<BookModel>>, InternalServerError>> GetBooksAsync()
    {
        var result = await booksService.GetBooksAsync();

        return result.Match<Results<Ok<List<BookModel>>, InternalServerError>>(
            onSuccess: books => TypedResults.Ok(books),
            onFailure: errors =>
            {
                logger.LogError("Failed to get books: {Errors}", string.Join("; ", errors));
                return TypedResults.InternalServerError();
            });
    }
}

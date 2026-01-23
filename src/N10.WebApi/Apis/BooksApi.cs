using N10.Services;
using N10.WebApi.Services;

namespace N10.WebApi.Apis;

public static class BooksApi
{
    public static RouteGroupBuilder MapBooksApi(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/books");

        group.MapGet("/", async (IBooksApiService booksService) => await booksService.GetBooksAsync())
            .WithName("GetBooks")
            .WithDescription("Get all books");

        return group;
    }
}

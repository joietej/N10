using N10.Services;
using N10.Services.Mappings;
using N10.Services.Models;

namespace N10.WebApi.GraphQL.Queries;

public class BookQuery
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<BookModel> GetBooks([Service] IBooksService booksService) =>
        booksService
            .GetBooksQuery("Author")
            .Select(x => x.ToModel());
}

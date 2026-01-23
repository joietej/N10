using N10.Data.Entities;
using N10.Services.Models;

namespace N10.Services.Mappings;

public static class BookMappings
{
    public static BookModel ToModel(this Book book)
    {
        return new BookModel
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author?.ToModel()
        };
    }
}

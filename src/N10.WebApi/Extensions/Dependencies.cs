using N10.Data.Init;
using N10.Data.Repositories;
using N10.Services;
using N10.WebApi.Services;

namespace N10.WebApi.Extensions;

public static class Dependencies
{
    public static WebApplicationBuilder AddConfig(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IDbInitializer, DbInitializer>();
        builder.Services.AddScoped<IBookRepository, BookRepository>();
        builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();

        builder.Services.AddScoped<IBooksService, BooksService>();

        builder.Services.AddScoped<IBooksApiService, BooksApiService>();

        return builder;
    }
}

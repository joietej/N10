using Microsoft.AspNetCore.Http.HttpResults;
using N10.Services.Models;

namespace N10.WebApi.Services;

public interface IBooksApiService
{
    Task<Results<Ok<List<BookModel>>, InternalServerError>> GetBooksAsync();
}

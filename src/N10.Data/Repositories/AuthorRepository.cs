using N10.Data.Context;
using N10.Data.Entities;

namespace N10.Data.Repositories;

public interface IAuthorRepository : IRepositoryBase<Author>
{
}

public class AuthorRepository(BooksDbContext context) : RepositoryBase<Author>(context), IAuthorRepository
{
}

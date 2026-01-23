using N10.Data.Context;
using N10.Data.Entities;

namespace N10.Data.Repositories;

public interface IBookRepository : IRepositoryBase<Book>
{
}

public class BookRepository(BooksDbContext context) : RepositoryBase<Book>(context), IBookRepository
{
}

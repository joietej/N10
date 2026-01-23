namespace N10.Data.Init;

public interface IDbInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}

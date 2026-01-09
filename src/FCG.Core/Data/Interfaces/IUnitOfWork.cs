namespace FCG.Core.Data.Interfaces
{
    public interface IUnitOfWork
    {
        Task BeginTransaction();
        Task<bool> Commit();
        Task Rollback();
    }
}

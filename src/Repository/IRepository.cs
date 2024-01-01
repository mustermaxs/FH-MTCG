using System;

namespace MTCG;

public interface IRepository<T>
{
    public T? Get(Guid id);

    public IEnumerable<T> GetAll();

    public void Delete(T obj);

    public void Save(T obj);
}
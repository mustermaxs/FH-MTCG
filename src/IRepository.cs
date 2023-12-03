using System;

namespace MTCG;

public interface IRepository<T>
{
    // public IEnumerable<T> GetAll();
    public T? Get(int id);
}
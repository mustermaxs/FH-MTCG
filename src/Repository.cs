using System;

namespace MTCG;

public abstract class Repository<T> : IRepository<T> where T: class, new()
{
    virtual public T Get(int id)
    {
        return new T();
    }
    // virtual public IEnumerable<T> GetAll()
    // {
    //     return new List<T>{new T("Maximilian", "Software Engineer")};
    // }
}
using System;

namespace MTCG;

public interface IRepository
{
    public T GetAll<T>();
    public T GetById<T>(int id);
}
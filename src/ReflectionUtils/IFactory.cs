using System;
using System.Collections.Generic;


namespace MTCG;

public interface IFactory<TClass>
{
    TClass Build();
    TClass BuildInstance();
    void TrySetField<TFieldType>(TFieldType val);
}
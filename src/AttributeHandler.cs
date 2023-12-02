using System;
using System.Reflection;
using System.Reflection.Metadata;
using Npgsql.Replication;

namespace MTCG;

// public delegate T InstanceFactory<T>();

public class AttributeHandler : IAttributeHandler
{
    public AttributeHandler(Assembly? assembly = null)
    :base(assembly)
    {

    }
}
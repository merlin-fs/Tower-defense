using System;

namespace Common.Core
{
    public interface IIdentifiable<T>
    {
        T ID { get; }
    }
}
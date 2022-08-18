using System;

namespace Common.Defs
{
    using Core;

    public interface IDefineable
    {
    }

    public interface IDefineable<T>: IDefineable
        where T : IDef
    {
    }
}
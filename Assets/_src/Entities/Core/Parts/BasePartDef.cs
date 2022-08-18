using System;
using UnityEngine;
using Unity.Entities;
using Common.Defs;

namespace Game.Model.Units.Defs
{
    public interface IPartDef: IDef
    {
    }

    public abstract class BasePartDef<T> : ClassDef<T>, IPartDef
        where T : struct, IDefineable, IComponentData
    {
    }
}
using System;
using UnityEngine;
using St.Common.Core;

namespace Game.Entities
{
    public interface ITargetable: ICoreMonoObject
    {
    }

    public class Targetable: MonoBehaviour, ITargetable
    {
        GameObject ICoreMonoObject.GameObject => gameObject;
    }
}
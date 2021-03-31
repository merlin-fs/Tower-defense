using System;
using UnityEngine;
using St.Common.Core;

namespace TowerDefense.Targetting
{
    public interface ITargetable: ICoreGameObject
    {
    }

    public class Targetable: MonoBehaviour, ITargetable
    {
        GameObject ICoreGameObject.GameObject => gameObject;
    }
}
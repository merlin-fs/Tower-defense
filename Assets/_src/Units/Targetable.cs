using System;
using UnityEngine;
using Common.Core;

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
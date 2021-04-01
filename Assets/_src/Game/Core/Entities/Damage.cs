using System;
using System.Collections.Generic;

namespace Game.Entities
{
    public interface IDamage
    {
        float Value { get; }
    }

    public interface IDamaged
    {
        /// <summary>
        /// Поглощение урона.
        /// Список всех типов урона, который может поглотить.
        /// Результат, после вычисления поглощения, передается далее.
        /// </summary>
        IReadOnlyCollection<IDamage> Absorb { get; }

        /// <summary>
        /// Сопротивление урону.
        /// Список всех типов урона, к которым есть сопротивлекние.
        /// Результат, после вычисления сопротивления, нансит урон.
        /// </summary>
        IReadOnlyCollection<IDamage> Resist { get; }

        /// <summary>
        /// Урон который наноситься объекту.
        /// Значение должно быть расчитано в соответствии с Absorb и Resist
        /// </summary>
        /// <param name="value"></param>
        void AddDamage(IUnit sender, float value);
    }
}
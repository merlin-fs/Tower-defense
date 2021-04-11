using System;
using St.Common.Core;

namespace Game.Entities
{
    public interface ISlice : ICoreInstantiate
    {
        /// <summary>
        /// Заполнение свойств из другого слайса.
        /// </summary>
        /// <remarks>
        /// Например в SubWawe, заменяются свойства Enemy свойствами указаными в SubWawe.
        /// </remarks>
        void FillFrom(ISlice other);
    }

    public interface ISliceUpdate : ISlice
    {
        /// <summary>
        /// Вызывается из MonoBehaviour.Update(), в парент объекте
        /// </summary>
        void Update(IUnit unit, float deltaTime);
    }

    public interface ISliceInit : ISlice
    {
        /// <summary>
        /// Инициализация слайса
        /// </summary>
        /// <param name="unit"></param>
        void Init(IUnit unit);
        /// <summary>
        /// Финализация слайса
        /// </summary>
        void Done(IUnit unit);
    }
}
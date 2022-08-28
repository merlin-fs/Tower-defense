using System;
using Unity.Entities;
using Unity.Mathematics;
using Common.Core;


namespace Game.Model.Logics
{
    using Core;
    using World;
    public struct EnemyLogic : ILogic
    {
        public struct Target: IComponentData
        {
            public Entity Entity;
        }


        private ReferenceObject<ILogicDef> m_Def;
        public int currentJob;

        public struct State : ILogicState
        {
            public JobState value;
            public JobState Value { get => value; set => this.value = value; }
            public void SetState(EntityCommandBuffer.ParallelWriter writer, Entity entity, JobState state, int sortKey)
            {
                writer.SetComponent(sortKey, entity, new State { Value = state });
            }
        }

        #region ILogic
        public ILogicDef Def => m_Def.Link;
        public int CurrentJob { get => currentJob; set => currentJob = value; }
        #endregion

        public EnemyLogic(ReferenceObject<ILogicDef> def)
        {
            m_Def = def;
            currentJob = 0;
        }


        unsafe void GeneratePosition(Map map, ref int2 position)
        {
            /*
            using (var cells = map.GetCells(position.CurrentPosition, 5,
                (m, value) =>
                {
                    bool result = m.Tiles.EntityExist(value);
                    result |= IsNotPassable(map.Tiles.HeightTypes[map.At(value)].Value);

                    return !result;
                }))
            {
                position.TargetPosition = cells.RandomElement();
            }

            using (var cells = map.GetCells(position.TargetPosition, 5,
                (m, value) =>
                {
                    //m.GetCostTile
                    bool result = m.Tiles.EntityExist(value);
                    result |= IsNotPassable(m.Tiles.HeightTypes[map.At(value)].Value);

                    return !result;
                }))
            {
                position.TargetPosition = cells.RandomElement();
            }

            static bool IsNotPassable(Map.HeightType value)
            {
                switch (value.Value)
                {
                    case Map.HeightType.Type.Snow:
                    case Map.HeightType.Type.DeepWater:
                    case Map.HeightType.Type.ShallowWater:
                        return true;
                    default:
                        return false;
                }
            }
            */
        }
    }
}
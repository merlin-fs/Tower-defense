using System;
using Unity.Entities;
using Unity.Mathematics;
using Common.Defs;
using UnityEngine;
using Game.Core.Repositories;

namespace Game.Model
{
    using Core;
    using Units;
    using World;
    using Logics;
    using Properties;
    using Skills;

    public partial class Squad
    {
        public interface ISquadDef : IDef
        {

        }

        [Defineable(typeof(Data))]
        public class SquadDef : ClassDef<Data>, ISquadDef
        {
            public int Radius = 3;

            //TODO: �����!!! ���� ����� �������� ��� �� ������������ �����������! (����� ������� �������� � ������� ID � � �������� (�� �������) ����� ��� �� �����������)
            //[SerializeField]
            //private UnitDef m_Def;
            private IUnitDef m_Def;
            public IUnitDef Prefab
            {
                get {
                    if (m_Def == null)
                    {
                        var repo = Repositories.Instance.Repository<UnitDef>();
                        m_Def = repo?.FindByID("Tank");
                    }
                    return m_Def;
                }
            }

            public int Count = 3;

            [SerializeReference, Reference()]
            ITeamDef m_Team;

            [SerializeReference, Reference()]
            ILogicDef m_Logic;

            [SerializeReference, Reference()]
            Move.MovingDef m_Move;

            protected override void AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
            {
                base.AddComponentData(entity, manager, conversionSystem);
                manager.AddBuffer<UnitLink>(entity);
                manager.AddBuffer<UnitPosition>(entity);


                (m_Move as IDef).AddComponentData(entity, manager, conversionSystem);
                m_Team.AddComponentData(entity, manager, conversionSystem);
                m_Logic.AddComponentData(entity, manager, conversionSystem);
            }

            protected override void AddComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey)
            {
                base.AddComponentData(entity, writer, sortKey);
                writer.AddBuffer<UnitLink>(sortKey, entity);
                writer.AddBuffer<UnitPosition>(sortKey, entity);

                (m_Move as IDef).AddComponentData(entity, writer, sortKey);
                m_Team.AddComponentData(entity, writer, sortKey);
                m_Logic.AddComponentData(entity, writer, sortKey);
            }
        }
    }
}

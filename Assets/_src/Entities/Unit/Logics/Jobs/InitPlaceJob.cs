using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;

/*
namespace Game.Model.Logics
{
    using Core;
    using Skills;
    using World;

    public struct InitPlaceJob : ILogicJob
    {
        public static int ID { get; } = typeof(InitPlaceJob).GetHashCode();
        public float Weight => 1;

        private ComponentTypeHandle<Move.Moving> m_MoveHandle;
        private ComponentTypeHandle<Translation> m_TranslationHandle;
        private ComponentTypeHandle<Rotation> m_RotationHandle;

        public InitPlaceJob(LogicSystem system)
        {
            m_MoveHandle = system.GetComponentTypeHandle<Move.Moving>(false);
            m_TranslationHandle = system.GetComponentTypeHandle<Translation>(false);
            m_RotationHandle = system.GetComponentTypeHandle<Rotation>(false);
        }

        public void Execute(ExecuteContext context, FunctionPointer<StateCallback> callback)
        {
            var moving = context.GetData(m_MoveHandle);
            var position = moving.Def.Link.InitPosition;
            Map.GeneratePosition(Map.Singleton, ref position);
            Place(context, position);

            callback.Invoke(context.Entity, JobResult.Done);
        }

        public void Place(ExecuteContext context, int2 position)
        {
            var moving = context.GetData(m_MoveHandle);
            var translation = context.GetData(m_TranslationHandle);
            var rotation = context.GetData(m_RotationHandle);
            moving.TargetPosition = position;

            Move.SetToPoint(Map.Singleton, context.Entity, ref moving, ref translation, ref rotation);

            context.SetData(m_MoveHandle, ref moving);
            context.SetData(m_TranslationHandle, ref translation);
            context.SetData(m_RotationHandle, ref rotation);
        }
    }
}

*/
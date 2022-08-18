using System;
using System.Linq;
using System.Collections.Generic;
using Common.Defs;
using Common.Core;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using System.Runtime.InteropServices;

namespace Game.Core
{
    [UpdateInGroup(typeof(GameLogicInitSystemGroup), OrderFirst = true)]
    public partial class LogicSystem: CallbackSystem
    {
        protected EntityCommandBufferSystem m_CommandBuffer;
        protected EntityQuery m_Query;

        private FunctionPointer<StateCallback> m_Callback;
        protected override void OnCreate()
        {
            base.OnCreate();

            m_CommandBuffer = World.GetOrCreateSystem<GameLogicCommandBufferSystem>();
            m_StateMachine = new LogicStateMachine(EntityManager, this, m_CommandBuffer);

            m_Query = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadWrite<Logic>(), ComponentType.ReadWrite<Logic.State>() },
                Options = EntityQueryOptions.IncludeDisabled
            });
            m_Query.AddChangedVersionFilter(ComponentType.ReadWrite<Logic.State>());
            RequireForUpdate(m_Query);

            m_Callback = new FunctionPointer<StateCallback>(Marshal.GetFunctionPointerForDelegate<StateCallback>(SetState));
            //JobsUtility.JobDebuggerEnabled = false;
        }

        static void SetState(ref EntityCommandBuffer.ParallelWriter writer, ref Entity entity, JobResult result, int sortKey)
        {
            var state = result == JobResult.Error? JobState.Error : JobState.None;
            Logic.State.SetState(ref writer, ref entity, state, sortKey);
        }

        struct LogicJob : IJobEntityBatch
        {
            [ReadOnly] public uint LastSystemVersion;
            [ReadOnly] public EntityTypeHandle InputEntity;
            public FunctionPointer<StateCallback> Callback;

            public ComponentTypeHandle<Logic> InputLogic;
            public ComponentTypeHandle<Logic.State> InputState;
            public EntityCommandBuffer.ParallelWriter Writer;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var change = batchInChunk.DidChange(InputState, LastSystemVersion);
                if (!change)
                    return;

                var entities = batchInChunk.GetNativeArray(InputEntity);
                var datas = batchInChunk.GetNativeArray(InputLogic);
                var states = batchInChunk.GetNativeArray(InputState);

                for (var i = 0; i < batchInChunk.Count; i++)
                {
                    var state = states[i];
                    if (state.Value == JobState.Running)
                        return;

                    var iter = datas[i];
                    var entity = entities[i];
                    var result = state.Value == JobState.Error ? JobResult.Error : JobResult.Done;

                    var next = iter.Def.GetTransition(iter.CurrentJob, result);
                    if (next != 0)
                    {
                        if (iter.Def.Logic.TryGetJob(next, out ILogicPart logicJob))
                        {
                            state.Value = JobState.Running;
                            try
                            {
                                iter.CurrentJob = next;
                                var context = new ExecuteContext(Writer, batchInChunk, entity, i, Callback, batchIndex);
                                logicJob.Execute(context);
                            }
                            catch
                            {
                                state.Value = JobState.Error;
                                throw;
                            }
                            finally
                            {
                                datas[i] = iter;
                                Logic.State.SetState(ref Writer, ref entity, state.Value, batchIndex);
                            }
                        }
                    }
                }
            }
        }

        protected override void OnUpdate()
        {
            var job = new LogicJob
            {
                Writer = m_CommandBuffer.CreateCommandBuffer().AsParallelWriter(),
                LastSystemVersion = LastSystemVersion,
                Callback = m_Callback,
                InputLogic = GetComponentTypeHandle<Logic>(false),
                InputState = GetComponentTypeHandle<Logic.State>(false),
                InputEntity = GetEntityTypeHandle(),
            };

            foreach (var iter in StateMachine.Parts)
                iter.Init(this);
            Dependency = job.ScheduleParallel(m_Query, Dependency);
            //m_CommandBuffer.AddJobHandleForProducer(Dependency);
        }
    }
}
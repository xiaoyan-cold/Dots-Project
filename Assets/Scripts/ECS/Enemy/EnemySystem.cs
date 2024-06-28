using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct EnemySystem : ISystem
{
    public struct key1 { }
    public struct key2 { }
    public struct key3 { }
    public readonly static SharedStatic<int> createdCount = SharedStatic<int>.GetOrCreate<key1>();
    public readonly static SharedStatic<int> createCount = SharedStatic<int>.GetOrCreate<key2>();
    public readonly static SharedStatic<Random> random = SharedStatic<Random>.GetOrCreate<key3>();
    public float spawnEnemyTimer;
    public const int maxEnemys = 10000;

    public void OnCreate(ref SystemState state)
    {
        createdCount.Data = 0;
        createCount.Data = 0;
        random.Data = new Random((uint)System.DateTime.Now.GetHashCode());
        SharedData.gameSharedData.Data.deadCounter = 0;
    }
    public void OnUpdate(ref SystemState state)
    {
        spawnEnemyTimer -= SystemAPI.Time.DeltaTime;
        if (spawnEnemyTimer <= 0)
        {
            spawnEnemyTimer = SharedData.gameSharedData.Data.spawnInterval;
            createCount.Data += SharedData.gameSharedData.Data.spawnCount;
        }
        EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        float2 playerPos = SharedData.playerPos.Data;
        new EnemyJob()
        {
            deltaTime = SystemAPI.Time.DeltaTime,
            playerPos = playerPos,
            ecb = ecb,
            time = SystemAPI.Time.ElapsedTime,
        }.ScheduleParallel();
        state.CompleteDependency();

        if (createCount.Data > 0 && createdCount.Data < maxEnemys)
        {
            NativeArray<Entity> newEnemys = new NativeArray<Entity>(createCount.Data, Allocator.Temp);
            ecb.Instantiate(int.MinValue, SystemAPI.GetSingleton<GameConfigData>().enemyPortotype, newEnemys);
            for (int i = 0; i < newEnemys.Length && createdCount.Data < maxEnemys; i++)
            {
                createdCount.Data += 1;
                float2 offset = random.Data.NextFloat2Direction() * random.Data.NextFloat(5f, 10);
                ecb.SetComponent<LocalTransform>(newEnemys[i].Index, newEnemys[i], new LocalTransform()
                {
                    Position = new float3(playerPos.x + offset.x, playerPos.y + offset.y, 0),
                    Rotation = quaternion.identity,
                    Scale = 1,
                });
            }
            createCount.Data = 0;
            newEnemys.Dispose();
        }

    }

    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
    [BurstCompile]
    public partial struct EnemyJob : IJobEntity
    {
        public float deltaTime;
        public double time;
        public float2 playerPos;
        public EntityCommandBuffer.ParallelWriter ecb;
        private void Execute(EnabledRefRW<EnemyData> enableState, EnabledRefRW<RendererSortTag> rendererSortEnableState, EnabledRefRW<AnimationFrameIndex> aniamtionEnableState, ref EnemyData enemyData, in EnemySharedData enemySharedData, ref LocalTransform localTransform, ref LocalToWorld localToWorld)
        {
            if (enableState.ValueRO == false)
            {
                if (createCount.Data > 0)
                {
                    createCount.Data -= 1;
                    float2 offset = random.Data.NextFloat2Direction() * random.Data.NextFloat(5f, 10);
                    localTransform.Position = new float3(playerPos.x + offset.x, playerPos.y + offset.y, 0);
                    enableState.ValueRW = true;
                    rendererSortEnableState.ValueRW = true;
                    aniamtionEnableState.ValueRW = true;
                    localTransform.Scale = 1;
                }
                return;
            }
            if (enemyData.die)
            {
                SharedData.gameSharedData.Data.playHitAudio = true;
                SharedData.gameSharedData.Data.deadCounter += 1;
                SharedData.gameSharedData.Data.playHitAudioTime += time;
                enemyData.die = false;
                enableState.ValueRW = false;
                rendererSortEnableState.ValueRW = false;
                aniamtionEnableState.ValueRW = false;
                localTransform.Scale = 0;
                return;
            }

            float2 dir = math.normalize(playerPos - new float2(localTransform.Position.x, localTransform.Position.y));
            localTransform.Position += deltaTime * enemySharedData.moveSpeed * new float3(dir.x, dir.y, 0);
            localToWorld.Value.c0.x = localTransform.Position.x < playerPos.x ? -enemySharedData.scale.x : enemySharedData.scale.x;
            localToWorld.Value.c1.y = enemySharedData.scale.y;
        }
    }
}
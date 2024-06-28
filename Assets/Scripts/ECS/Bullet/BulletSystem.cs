using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Physics;
using Unity.Mathematics;
using static EnemyAuthoring;

public partial struct BulletSystem : ISystem
{
    public readonly static SharedStatic<int> createBulletCount = SharedStatic<int>.GetOrCreate<BulletSystem>();
    public void OnCreate(ref SystemState state)
    {
        createBulletCount.Data = 0;
        SharedData.singtonEnitty.Data = state.EntityManager.CreateEntity(typeof(BulletCreateInfo));
    }
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        DynamicBuffer<BulletCreateInfo> bulletCreateInfoBuffer = SystemAPI.GetSingletonBuffer<BulletCreateInfo>();
        createBulletCount.Data = bulletCreateInfoBuffer.Length;
        new BulletJob()
        {
            enemyLayerMask = 1 << 6,
            ecb = ecb,
            deltaTime = SystemAPI.Time.DeltaTime,
            bulletCreateInfoBuffer = bulletCreateInfoBuffer,
            collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld,
            // enemyLookUp = SystemAPI.GetComponentLookup<EnemyData>()
        }.ScheduleParallel();
        state.CompleteDependency();
        if (createBulletCount.Data > 0)         // 补充不足的部分
        {
            NativeArray<Entity> newBullets = new NativeArray<Entity>(createBulletCount.Data, Allocator.Temp);
            ecb.Instantiate(int.MinValue, SystemAPI.GetSingleton<GameConfigData>().bulletPortotype, newBullets);
            for (int i = 0; i < newBullets.Length; i++)
            {
                BulletCreateInfo info = bulletCreateInfoBuffer[i];
                ecb.SetComponent<LocalTransform>(newBullets[i].Index, newBullets[i], new LocalTransform()
                {
                    Position = info.position,
                    Rotation = info.rotation,
                    Scale = 1
                });
            }
            newBullets.Dispose();
        }

        bulletCreateInfoBuffer.Clear();
    }

    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
    [BurstCompile]
    public partial struct BulletJob : IJobEntity
    {
        public uint enemyLayerMask;
        public EntityCommandBuffer.ParallelWriter ecb;
        public float deltaTime;
        [ReadOnly] public DynamicBuffer<BulletCreateInfo> bulletCreateInfoBuffer;
        [ReadOnly] public CollisionWorld collisionWorld;
        // [ReadOnly] public ComponentLookup<EnemyData> enemyLookUp;
        public void Execute(EnabledRefRW<BulletData> bulletEnableState, EnabledRefRW<RendererSortTag> sortEnableState, ref BulletData bulletData, in BulletSharedData bulletSharedData, in Entity entity, ref LocalTransform localTransform)
        {
            // 当前子弹是非激活状态，同时需要创建子弹
            if (bulletEnableState.ValueRO == false)
            {
                if (createBulletCount.Data > 0)
                {
                    int index = createBulletCount.Data -= 1;
                    bulletEnableState.ValueRW = true;
                    sortEnableState.ValueRW = true;
                    localTransform.Position = bulletCreateInfoBuffer[index].position;
                    localTransform.Rotation = bulletCreateInfoBuffer[index].rotation;
                    localTransform.Scale = 1;
                    bulletData.destroyTimer = bulletSharedData.destroyTime;
                }
                return;
            }

            // 位置移动
            localTransform.Position += bulletSharedData.moveSpeed * deltaTime * localTransform.Up();
            // 销毁计时
            bulletData.destroyTimer -= deltaTime;
            if (bulletData.destroyTimer <= 0)
            {
                bulletEnableState.ValueRW = false;
                sortEnableState.ValueRW = false;
                localTransform.Scale = 0;
                return;
            }

            // 伤害检测
            NativeList<DistanceHit> hits = new NativeList<DistanceHit>(Allocator.Temp);
            CollisionFilter filter = new CollisionFilter()
            {
                BelongsTo = ~0u,
                CollidesWith = enemyLayerMask, // all 1s, so all layers, collide with everything
                GroupIndex = 0
            };

            if (collisionWorld.OverlapBox(localTransform.Position, localTransform.Rotation, bulletSharedData.colliderHalfExtents, ref hits, filter))
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    Entity temp = hits[i].Entity;
                    bulletData.destroyTimer = 0;
                    ecb.SetComponent<EnemyData>(temp.Index, temp, new EnemyData()
                    {
                        die = true,
                    });
                }
            }
            hits.Dispose();
        }
    }
}
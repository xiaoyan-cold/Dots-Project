using Unity.Entities;
using Unity.Mathematics;

public struct BulletSharedData : ISharedComponentData
{
    public float moveSpeed;
    public float destroyTime;
    public float2 colliderOffset;
    public float3 colliderHalfExtents;
}
using Unity.Entities;
using Unity.Mathematics;

public struct EnemySharedData : ISharedComponentData
{
    public float moveSpeed;
    public float2 scale;
}
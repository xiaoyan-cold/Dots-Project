using Unity.Entities;

public struct AnimationSharedData : ISharedComponentData
{
    public float frameRate;
    public int frameCount;
}
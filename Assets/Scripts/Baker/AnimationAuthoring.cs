using Unity.Entities;
using UnityEngine;

public class AnimationAuthoring : MonoBehaviour
{
    public float frameRate;
    public int frameMaxIndex;
    public class AnimationBaker : Baker<AnimationAuthoring>
    {
        public override void Bake(AnimationAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<AnimationFrameIndex>(entity);
            AddSharedComponent<AnimationSharedData>(entity, new AnimationSharedData()
            {
                frameRate = authoring.frameRate,
                frameCount = authoring.frameMaxIndex,
            });
        }
    }
}

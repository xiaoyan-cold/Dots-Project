using Unity.Burst;
using Unity.Entities;

public partial struct AnimationSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        new AnimationJob()
        {
            delaTime = state.WorldUnmanaged.Time.DeltaTime
        }.ScheduleParallel();
    }
    [BurstCompile]
    public partial struct AnimationJob : IJobEntity
    {
        public float delaTime;
        private void Execute(in AnimationSharedData animationData, ref AnimationFrameIndex frameIndex)
        {
            float newIndex = frameIndex.value + delaTime * animationData.frameRate;
            while (newIndex > animationData.frameCount)
            {
                newIndex -= animationData.frameCount;
            }
            frameIndex.value = newIndex;
        }
    }
}
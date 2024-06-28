using Unity.Entities;
using UnityEngine;

public class BulletAuthoring : MonoBehaviour
{
    public float moveSpeed;
    public float destroyTime;

    public class BulletBaker : Baker<BulletAuthoring>
    {
        public override void Bake(BulletAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<RendererSortTag>(entity);
            SetComponentEnabled<RendererSortTag>(entity, true);

            AddComponent<BulletData>(entity, new BulletData()
            {
                destroyTimer = authoring.destroyTime
            });
            SetComponentEnabled<BulletData>(entity, true);


            Vector2 colliderSize = authoring.GetComponent<BoxCollider2D>().size / 2;
            AddSharedComponent<BulletSharedData>(entity, new BulletSharedData()
            {
                moveSpeed = authoring.moveSpeed,
                destroyTime = authoring.destroyTime,
                colliderOffset = authoring.GetComponent<BoxCollider2D>().offset,
                colliderHalfExtents = new Unity.Mathematics.float3(colliderSize.x, colliderSize.y, 10000),
            }); ;
        }
    }
}
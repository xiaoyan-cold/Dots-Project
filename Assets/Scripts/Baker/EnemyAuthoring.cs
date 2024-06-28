using Unity.Entities;
using UnityEngine;

public class EnemyAuthoring : MonoBehaviour
{
    public float moveSpeed = 4;
    public class EnemyBaker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<RendererSortTag>(entity);
            SetComponentEnabled<RendererSortTag>(entity, true);

            AddComponent<EnemyData>(entity, new EnemyData() { die = false });
            SetComponentEnabled<EnemyData>(entity, true);

            AddSharedComponent<EnemySharedData>(entity, new EnemySharedData()
            {
                moveSpeed = authoring.moveSpeed,
                scale = (Vector2)authoring.transform.localScale,
            });
        }
    }
}

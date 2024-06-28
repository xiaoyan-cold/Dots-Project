using Unity.Entities;
using UnityEngine;

public class GameManagerAuthoring : MonoBehaviour
{
    public GameObject bulletPrefab;
    public GameObject enemyPrefab;
    public class GameManagerBaker : Baker<GameManagerAuthoring>
    {
        public override void Bake(GameManagerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            GameConfigData configData = new GameConfigData();
            configData.bulletPortotype = GetEntity(authoring.bulletPrefab, TransformUsageFlags.Dynamic);
            configData.enemyPortotype = GetEntity(authoring.enemyPrefab, TransformUsageFlags.Dynamic);
            AddComponent<GameConfigData>(entity, configData);
        }
    }
}
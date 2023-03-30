using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class FoodSpawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;

    private const int MaxPrefabCount = 50;

    // Start is called before the first frame update
    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += SpawnFoodStart;
    }

    private void SpawnFoodStart()
    {
        NetworkManager.Singleton.OnServerStarted -= SpawnFoodStart;
        NetworkObjectPool.Singleton.InitializePool();

        for (int i = 0; i < 30; ++i)
        {
            SpawnFood();
        }
        StartCoroutine(SpawnOverTime());
    }

    private void SpawnFood()
    {
        NetworkObject obj = NetworkObjectPool.Singleton.GetNetworkObject(prefab, GetRandomPositionOnMap(), Quaternion.identity);
        obj.GetComponent<Food>().prefab = prefab;
        if (!obj.IsSpawned) obj.Spawn(destroyWithScene: true);
    }

    private Vector3 GetRandomPositionOnMap()
    {
        return new Vector3(x: Random.Range(-18f, 18f), y: Random.Range(-10f, 10f), 0f); ;
    }

    private IEnumerator SpawnOverTime()
    {
        while (NetworkManager.Singleton.ConnectedClients.Count > 0)
        {
            yield return new WaitForSeconds(2f);
            if (NetworkObjectPool.Singleton.GetCurrentPrefabCount(prefab) < MaxPrefabCount)
            SpawnFood();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that creates decorations around the map.
/// </summary>
public class MapDecoration : MonoBehaviour
{
    [SerializeField] private float spawnAnimationSpeed = 7.5f;
    [SerializeField] private Vector2Int maxElementSize;
    [SerializeField] private GameObject[] grassBiomeObjects;
    [SerializeField] private GameObject[] desertBiomeObjects;
    [SerializeField] private GameObject[] winterBiomeObjects;
    [SerializeField] private GameObject[] rockBiomeObjects;

    private List<GameObject> spawnedElements = new List<GameObject>();

    /// <summary>
    /// Indicates whether decorations are spawned.
    /// </summary>
    public bool AreDecorationsSpawned => spawnedElements.Count > 0;

    /// <summary>
    /// Spawns random decorations in random positions around map. It uses animation.
    /// </summary>
    /// <param name="biomeType">Biome type used to select decorations.</param>
    /// <param name="mapSize">Size of map.</param>
    /// <param name="radius">Radius in which objects will be spawned.</param>
    /// <param name="border">Border around map in which objects won't be spawned.</param>
    /// <param name="createBottomDecoration">Indicates whether create decorations at bottom of map.</param>
    public void SpawnDecoration(Biomes biomeType, Vector2Int mapSize, int countOfElements)
    {
        Vector3 pos;

        // TOP
        for (int y = 0; y < countOfElements; y++)
        {
            pos = new Vector3(-countOfElements * maxElementSize.x, 0f, ((y + 1) * maxElementSize.y) + (maxElementSize.y * 0.75f));
            pos = new Vector3(Random.Range(pos.x - 1, pos.x + 1), pos.y, pos.z);

            for (int x = 0; x < (countOfElements * 2) + (mapSize.x / 2); x+=2)
            {
                spawnedElements.Add(Instantiate(GetRandomElement(biomeType), pos, Quaternion.identity));
                spawnedElements[spawnedElements.Count - 1].transform.localScale = Vector3.zero;
                pos += new Vector3(maxElementSize.x * 2.25f, 0, 0);
                pos = new Vector3(Random.Range(pos.x - 1, pos.x + 1), pos.y, pos.z);
            }
        }

        // LEFT
        for (int y = 0; y < countOfElements; y++)
        {
            pos = new Vector3(-maxElementSize.x - (maxElementSize.x * 0.75f), 0f, -y * maxElementSize.y);
            pos = new Vector3(Random.Range(pos.x - 1, pos.x + 1), pos.y, pos.z);

            for (int x = 0; x < countOfElements; x += 2)
            {
                spawnedElements.Add(Instantiate(GetRandomElement(biomeType), pos, Quaternion.identity));
                spawnedElements[spawnedElements.Count - 1].transform.localScale = Vector3.zero;
                pos -= new Vector3(maxElementSize.x * 4f, 0, 0);
                pos = new Vector3(Random.Range(pos.x - 1, pos.x + 1), pos.y, pos.z);
            }
        }

        // RIGHT
        for (int y = 0; y < countOfElements; y++)
        {
            pos = new Vector3(mapSize.x + (maxElementSize.x * 0.75f), 0f, -y * maxElementSize.y);
            pos = new Vector3(Random.Range(pos.x - 1, pos.x + 1), pos.y, pos.z);

            for (int x = 0; x < countOfElements; x += 2)
            {
                spawnedElements.Add(Instantiate(GetRandomElement(biomeType), pos, Quaternion.identity));
                spawnedElements[spawnedElements.Count - 1].transform.localScale = Vector3.zero;
                pos += new Vector3(maxElementSize.x * 4f, 0, 0);
                pos = new Vector3(Random.Range(pos.x - 1, pos.x + 1), pos.y, pos.z);
            }
        }

        // ANIMATION
        foreach (GameObject obj in spawnedElements)
        {
            StartCoroutine(ScaleAnimation(obj.transform));

            IEnumerator ScaleAnimation(Transform t)
            {
                while (t.localScale != Vector3.one)
                {
                    Vector3 scale = Vector3.Lerp(t.localScale, Vector3.one, Time.deltaTime * spawnAnimationSpeed);
                    t.localScale = scale;
                    yield return null;
                }
            }
        }

    }

    /// <summary>
    /// Removes all previously spawned decorations. It uses animation.
    /// </summary>
    public void ClearSpawnedDecorations()
    {
        // ANIMATION
        foreach (GameObject obj in spawnedElements)
        {
            StartCoroutine(ScaleAnimation(obj.transform));

            IEnumerator ScaleAnimation(Transform t)
            {
                while (t.localScale.x >= 0.01f)
                {
                    Vector3 scale = Vector3.Lerp(t.localScale, Vector3.zero, Time.deltaTime * spawnAnimationSpeed);
                    t.localScale = scale;
                    yield return null;
                }

                Destroy(obj);
            }
        }

        spawnedElements.Clear();
    }

    /// <summary>
    /// Returns random element from set that fits to given <seealso cref="Biomes"/>.
    /// </summary>
    /// <param name="biomeType">Type of biome - used to select apropiate decorations set.</param>
    /// <returns></returns>
    public GameObject GetRandomElement(Biomes biomeType)
    {
        GameObject[] objectsPool = null;

        switch (biomeType)
        {
            case Biomes.Grass:
                objectsPool = grassBiomeObjects;
                break;
            case Biomes.Desert:
                objectsPool = desertBiomeObjects;
                break;
            case Biomes.Winter:
                objectsPool = winterBiomeObjects;
                break;
            case Biomes.Rock:
                objectsPool = rockBiomeObjects;
                break;
        }

        int index = Random.Range(0, objectsPool.Length);
        return objectsPool[index];
    }
}

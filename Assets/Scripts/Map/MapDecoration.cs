using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that creates decorations around the map.
/// </summary>
public class MapDecoration : MonoBehaviour
{
    [SerializeField] private float eachElementDelay = 0.01f;
    [SerializeField] private float spawnAnimationSpeed = 7.5f;
    [SerializeField] private Vector2Int maxElementSize;
    [SerializeField] private GameObject[] grassBiomeObjects;

    private List<GameObject> spawnedElements = new List<GameObject>();

    /// <summary>
    /// Spawns random decorations in random positions around map. It uses animation.
    /// </summary>
    /// <param name="biomeType">Biome type used to select decorations.</param>
    /// <param name="mapSize">Size of map.</param>
    /// <param name="radius">Radius in which objects will be spawned.</param>
    /// <param name="border">Border around map in which objects won't be spawned.</param>
    /// <param name="createBottomDecoration">Indicates whether create decorations at bottom of map.</param>
    public void SpawnDecoration(Biomes biomeType, Vector2Int mapSize, int radius, int border = 0, bool createBottomDecoration = true)
    {
        if ((radius + border) % 2 == 1) radius--;

        mapSize = new Vector2Int(mapSize.x + border, mapSize.y + border);

        Vector3 pos;

        // TOP
        for (int y = 0; y < radius; y += maxElementSize.y)
        {
            pos = new Vector3(-radius - border, 0f, -y + radius + border);

            for (int x = 0; x < mapSize.x + (radius * 2); x += maxElementSize.x)
            {
                spawnedElements.Add(Instantiate(GetRandomElement(biomeType), pos, Quaternion.identity));
                spawnedElements[spawnedElements.Count - 1].transform.localScale = Vector3.zero;
                pos += new Vector3(maxElementSize.x, 0, 0);
            }
        }

        // LEFT
        for (int y = 0; y < mapSize.y; y += maxElementSize.y)
        {
            pos = new Vector3(-radius - border, 0f, -y);

            for (int x = 0; x < radius; x += maxElementSize.x)
            {
                spawnedElements.Add(Instantiate(GetRandomElement(biomeType), pos, Quaternion.identity));
                spawnedElements[spawnedElements.Count - 1].transform.localScale = Vector3.zero;
                pos += new Vector3(maxElementSize.x, 0, 0);
            }
        }

        // RIGHT
        for (int y = 0; y < mapSize.y; y += maxElementSize.y)
        {
            pos = new Vector3(mapSize.x, 0f, -y);

            for (int x = 0; x < radius; x += maxElementSize.x)
            {
                spawnedElements.Add(Instantiate(GetRandomElement(biomeType), pos, Quaternion.identity));
                spawnedElements[spawnedElements.Count - 1].transform.localScale = Vector3.zero;
                pos += new Vector3(maxElementSize.x, 0, 0);
            }
        }

        // BOTTOM
        if (createBottomDecoration)
        {
            for (int y = 0; y < radius; y += maxElementSize.y)
            {
                pos = new Vector3(-radius - border, 0f, -mapSize.y - y);

                for (int x = 0; x < mapSize.x + (radius * 2); x += maxElementSize.x)
                {
                    spawnedElements.Add(Instantiate(GetRandomElement(biomeType), pos, Quaternion.identity));
                    spawnedElements[spawnedElements.Count - 1].transform.localScale = Vector3.zero;
                    pos += new Vector3(maxElementSize.x, 0, 0);
                }
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
        }

        int index = Random.Range(0, objectsPool.Length);
        return objectsPool[index];
    }
}

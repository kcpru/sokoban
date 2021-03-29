using UnityEngine;
using System;

public class MapElementsManager : MonoBehaviour
{
    [SerializeField] private MapElementsSet[] mapElementsSets;

    public static MapElementsManager CurrentManager { get; private set; }

    private void Awake() => CurrentManager = this;

    /// <summary>
    /// Returns <seealso cref="GameObject"/> that fits to given <seealso cref="ElementType"/> and that appears in given <seealso cref="Biomes"/>.
    /// </summary>
    /// <returns></returns>
    public GameObject GetMapElement(ElementType elementType, Biomes biome)
    {
        MapElementsSet set = null;
        GameObject element = null;

        foreach(MapElementsSet s in mapElementsSets)
        {
            if(s.biome == biome)
            {
                set = s;
                break;
            }
        }

        switch (elementType)
        {
            case ElementType.Ground:
                element = set.groundElement;
                break;
            case ElementType.Player:
            case ElementType.PlayerOnTarget:
                element = set.playerElement;
                break;
            case ElementType.Air:
                element = null;
                break;
            case ElementType.Box:
            case ElementType.DoneTarget:
                element = set.boxElement;
                break;
            case ElementType.Target:
                element = set.targetElement;
                break;
        }

        return element;
    }

    [Serializable]
    public class MapElementsSet
    {
        public Biomes biome;
        public GameObject playerElement;
        public GameObject boxElement;
        public GameObject groundElement;
        public GameObject targetElement;
    }
}

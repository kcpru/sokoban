using UnityEngine;
using System.Xml;
using System;

/// <summary>
/// Class that serializes <seealso cref="Map"/> to xml file or deseralize <seealso cref="Map"/> from xml file.
/// </summary>
public class MapSerializer
{
    /// <summary>
    /// Path to xml file.
    /// </summary>
    public string Path { get; private set; }

    /// <summary>
    /// Create new instance of <seealso cref="MapSerializer"/> which will operate on given path.
    /// </summary>
    public MapSerializer(string path) => Path = path;

    public static string MapsPath => System.IO.Path.Combine(Application.dataPath, "Resources/Maps");

    /// <summary>
    /// Returns <seealso cref="Map"/> deserialized from previously given path.
    /// </summary>
    public Map Deserialize()
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(Path);
        XmlNode rootNode = doc.SelectSingleNode("SokobanLevel");
        XmlNodeList rows = rootNode.SelectSingleNode("LevelStructure").SelectNodes("Row");
        ElementType[,] elements;

        Biomes biomeType;
        Difficulty difficulty;
        Vector2Int size = Vector2Int.zero;
        int width, height;

        if(!Enum.TryParse(rootNode.Attributes.GetNamedItem("biome").Value, true, out biomeType))
        {
            Debug.LogError("biome attribute is not correct!");
            return null;
        }

        if (!Enum.TryParse(rootNode.Attributes.GetNamedItem("difficulty").Value, true, out difficulty))
        {
            Debug.Log("difficulty attribute is not correct!");
            return null;
        }

        if (!int.TryParse(rootNode.SelectSingleNode("LevelStructure").Attributes.GetNamedItem("width").Value, out width))
        {
            Debug.LogError("width attribute is not correct!");
            return null;
        }

        if (!int.TryParse(rootNode.SelectSingleNode("LevelStructure").Attributes.GetNamedItem("height").Value, out height))
        {
            Debug.LogError("height attribute is not correct!");
            return null;
        }

        size = new Vector2Int(width, height);
        Debug.Log(size);
        elements = new ElementType[size.y, size.x];

        for(int y = 0; y < size.y; y++)
        {
            string row = rows[y].InnerText.Trim();

            for (int x = 0; x < size.x; x++)
            {
                ElementType type = ElementType.Air;

                switch (row[x])
                {
                    case 'G':
                        type = ElementType.Ground;
                        break;
                    case 'P':
                        type = ElementType.Player;
                        break;
                    case 'A':
                        type = ElementType.Air;
                        break;
                    case 'B':
                        type = ElementType.Box;
                        break;
                    case 'T':
                        type = ElementType.Target;
                        break;
                    case 'D':
                        type = ElementType.DoneTarget;
                        break;
                    case 'O':
                        type = ElementType.PlayerOnTarget;
                        break;
                }

                elements[y, x] = type;
            }
        }

        Map map = new Map(elements, biomeType, difficulty);
        return map;
    }

    /// <summary>
    /// Serialize given <seealso cref="Map"/> and saves it to previously given file.
    /// </summary>
    /// <param name="map"><seealso cref="Map"/> to serialize and save</param>
    /// <returns>Path to saved map</returns>
    public string Serialize(Map map)
    {
        if(!map.IsMapDefined)
        {
            Debug.LogError("Given map is not defined!");
            return string.Empty;
        }

        XmlDocument doc = new XmlDocument();
        XmlNode declarationNode = doc.CreateXmlDeclaration("1.0", null, null);
        doc.AppendChild(declarationNode);

        XmlNode rootNode = doc.CreateNode(XmlNodeType.Element, "SokobanLevel", null);
        XmlAttribute attr = doc.CreateAttribute("difficulty");
        attr.Value = map.difficulty.ToString();
        rootNode.Attributes.Append(attr);

        attr = doc.CreateAttribute("biome");
        attr.Value = map.biomeType.ToString();
        rootNode.Attributes.Append(attr);

        doc.AppendChild(rootNode);

        XmlNode levelStructure = doc.CreateNode(XmlNodeType.Element, "LevelStructure", null);
        attr = doc.CreateAttribute("width");
        attr.Value = map.mapSize.x.ToString();
        levelStructure.Attributes.Append(attr);

        attr = doc.CreateAttribute("height");
        attr.Value = map.mapSize.y.ToString();
        levelStructure.Attributes.Append(attr);

        for (int i = 0; i < map.mapSize.y; i++)
        {
            string val = "";

            for (int j = 0; j < map.mapSize.x; j++)
            {
                char element = 'A';

                switch(map.mapDefinition[i, j])
                {
                    case ElementType.Air:
                        element = 'A';
                        break;
                    case ElementType.Box:
                        element = 'B';
                        break;
                    case ElementType.DoneTarget:
                        element = 'D';
                        break;
                    case ElementType.Ground:
                        element = 'G';
                        break;
                    case ElementType.Player:
                        element = 'P';
                        break;
                    case ElementType.PlayerOnTarget:
                        element = 'O';
                        break;
                    case ElementType.Target:
                        element = 'T';
                        break;
                }

                val += element;
            }

            XmlNode row = doc.CreateNode(XmlNodeType.Element, "Row", null);
            row.InnerText = val;
            levelStructure.AppendChild(row);
        }

        rootNode.AppendChild(levelStructure);

        doc.Save(Path);
        return Path;
    }

    /// <summary>
    /// Converts level from given normalized xml file to acceptable format.
    /// </summary>
    public string ConvertNormalizedFile(string normalizedFile, string levelId, Biomes biomeType, Difficulty difficulty)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(normalizedFile);
        XmlNode rootNode = doc.SelectSingleNode("SokobanLevels/LevelCollection");
        XmlNodeList levels = rootNode.SelectNodes("Level");
        XmlNode levelNode = null;

        for(int i = 0; i < levels.Count; i++)
        {
            if (levels[i].Attributes.GetNamedItem("Id").Value == levelId)
            {
                levelNode = levels[i];
                break;
            }
        }

        if(levelNode == null)
        {
            Debug.LogError("Level with given ID does not exist in given file!");
            return string.Empty;
        }

        int width, height;
        Vector2Int size;

        if (!int.TryParse(levelNode.Attributes.GetNamedItem("Width").Value, out width))
        {
            Debug.LogError("Width attribute is not correct!");
            return null;
        }

        if (!int.TryParse(levelNode.Attributes.GetNamedItem("Height").Value, out height))
        {
            Debug.LogError("Height attribute is not correct!");
            return null;
        }

        size = new Vector2Int(width, height);

        XmlDocument newDoc = new XmlDocument();
        XmlNode declarationNode = newDoc.CreateXmlDeclaration("1.0", null, null);
        newDoc.AppendChild(declarationNode);

        XmlNode newRootNode = newDoc.CreateNode(XmlNodeType.Element, "SokobanLevel", null);
        XmlAttribute attr = newDoc.CreateAttribute("difficulty");
        attr.Value = difficulty.ToString();
        newRootNode.Attributes.Append(attr);

        attr = newDoc.CreateAttribute("biome");
        attr.Value = biomeType.ToString();
        newRootNode.Attributes.Append(attr);

        newDoc.AppendChild(newRootNode);

        XmlNode levelStructure = newDoc.CreateNode(XmlNodeType.Element, "LevelStructure", null);
        attr = newDoc.CreateAttribute("width");
        attr.Value = size.x.ToString();
        levelStructure.Attributes.Append(attr);

        attr = newDoc.CreateAttribute("height");
        attr.Value = size.y.ToString();
        levelStructure.Attributes.Append(attr);

        for (int i = 0; i < levelNode.ChildNodes.Count; i++)
        {
            string val = "";
            bool wasChar = false;

            for (int j = 0; j < levelNode.ChildNodes[i].InnerText.Length; j++)
            {
                char c = levelNode.ChildNodes[i].InnerText[j];

                if(c == ' ')
                {
                    if(!wasChar)
                        val += 'A';
                    else
                        val += 'G';
                }
                else
                {
                    wasChar = true;

                    switch (c)
                    {
                        case '#':
                            val += 'A';
                            break;
                        case '$':
                            val += 'B';
                            break;
                        case '.':
                            val += 'T';
                            break;
                        case '*':
                            val += 'D';
                            break;
                        case '@':
                            val += 'P';
                            break;
                        case '+':
                            val += "O";
                            break;
                    }
                }
            }

            while (val.Length < size.x)
                val += 'A';

            XmlNode row = newDoc.CreateNode(XmlNodeType.Element, "Row", null);
            row.InnerText = val;
            levelStructure.AppendChild(row);
        }

        newRootNode.AppendChild(levelStructure);
        newDoc.Save(Path);
        return Path;
    }
}

using System.Xml;
using System.IO;
using UnityEngine;

public static class SaveLoadManager
{
    public static string PathToSaveFiles => Path.Combine(Application.dataPath, "Save");

    public static void SaveLevelProgress(Map mapToSave, int movesCount)
    {
        PrepareDirectory();
        string path = GetPathFromMap(mapToSave);
        MapSerializer serializer = new MapSerializer(path);
        serializer.Serialize(mapToSave);

        XmlDocument doc = new XmlDocument();
        doc.Load(path);

        XmlNode rootNode = doc.SelectSingleNode("SokobanLevel");
        XmlAttribute attr = doc.CreateAttribute("moves");
        attr.Value = movesCount.ToString();
        rootNode.Attributes.Append(attr);

        doc.Save(path);
    }

    public static Map LoadLevelProgress(Map defaultMap, out int movesCount)
    {
        string path = GetPathFromMap(defaultMap);

        if (!SaveExists(defaultMap))
        {
            movesCount = 0;
            return null;
        }

        MapSerializer serializer = new MapSerializer(path);
        Map savedProgress = serializer.Deserialize(false);

        XmlDocument doc = new XmlDocument();
        doc.Load(path);
        XmlNode rootNode = doc.SelectSingleNode("SokobanLevel");
        movesCount = int.Parse(rootNode.Attributes.GetNamedItem("moves").Value);

        return savedProgress;
    }

    public static void ClearSave(Map mapToClear)
    {
        if(SaveExists(mapToClear))
            File.Delete(GetPathFromMap(mapToClear));
    }

    private static void PrepareDirectory()
    {
        if (!Directory.Exists(PathToSaveFiles))
            Directory.CreateDirectory(PathToSaveFiles);
    }

    public static bool SaveExists(Map mapToCheck) => File.Exists(GetPathFromMap(mapToCheck));

    public static string GetPathFromMap(Map map) => Path.Combine(PathToSaveFiles, map.name + "_save.xml");


}

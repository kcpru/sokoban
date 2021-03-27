using UnityEngine;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Linq;

public static class RankingManager
{
    public static string PathToRankFile => Path.Combine(Application.dataPath, "Save/Ranking.xml");

    public static void AddRecord(Map map, float time, int movesCount)
    {
        PrepareFile();

        XmlDocument doc = new XmlDocument();
        doc.Load(PathToRankFile);

        XmlNode rootNode = doc.SelectSingleNode("Ranking");

        XmlNode record = doc.CreateNode(XmlNodeType.Element, "Record", null);
        XmlAttribute attr = doc.CreateAttribute("map");
        attr.Value = map.name;
        record.Attributes.Append(attr);

        attr = doc.CreateAttribute("moves");
        attr.Value = movesCount.ToString();
        record.Attributes.Append(attr);

        attr = doc.CreateAttribute("time");
        attr.Value = time.ToString();
        record.Attributes.Append(attr);

        rootNode.AppendChild(record);
        doc.Save(PathToRankFile);
    }

    public static Record[] GetRecords(string mapName)
    {
        if (!File.Exists(PathToRankFile)) return null;

        XmlDocument doc = new XmlDocument();
        doc.Load(PathToRankFile);

        XmlNodeList records = doc.SelectSingleNode("Ranking").SelectNodes("Record");
        List<Record> entries = new List<Record>();

        for(int i = 0; i < records.Count; i++)
        {
            if(records[i].Attributes.GetNamedItem("map").Value.Trim() == mapName)
            {
                Record record = new Record(
                    mapName, 
                    int.Parse(records[i].Attributes.GetNamedItem("moves").Value), 
                    float.Parse(records[i].Attributes.GetNamedItem("time").Value)
                    );

                entries.Add(record);
            }
        }

        entries = entries.OrderBy(item => item.time).ToList();
        return entries.ToArray();
    }

    private static void PrepareFile()
    {
        if (!Directory.Exists(Path.GetDirectoryName(PathToRankFile)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PathToRankFile));
        }

        if (!File.Exists(PathToRankFile))
        {
            XmlDocument doc = new XmlDocument();

            XmlNode declarationNode = doc.CreateXmlDeclaration("1.0", null, null);
            doc.AppendChild(declarationNode);

            XmlNode rootNode = doc.CreateNode(XmlNodeType.Element, "Ranking", null);
            doc.AppendChild(rootNode);

            doc.Save(PathToRankFile);
        }
    }

    public struct Record
    {
        public string name;
        public int moves;
        public float time;

        public Record(string name, int moves, float time)
        {
            this.name = name;
            this.moves = moves;
            this.time = time;
        }
    }
}

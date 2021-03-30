using UnityEngine;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Class to manage file that stores ranking.
/// </summary>
public static class RankingManager
{
    public static string PathToRankFile => Path.Combine(Application.dataPath, "Save/Ranking.xml");
    public const int MAX_RECORDS = 20;

    /// <summary>
    /// Adds new <seealso cref="Record"/> to ranking file.
    /// </summary>
    /// <param name="r"><seealso cref="Record"/> record to add.</param>
    public static void AddRecord(Record r)
    {
        PrepareFile();

        XmlDocument doc = new XmlDocument();
        doc.Load(PathToRankFile);

        XmlNode rootNode = doc.SelectSingleNode("Ranking");

        XmlNode record = doc.CreateNode(XmlNodeType.Element, "Record", null);
        XmlAttribute attr = doc.CreateAttribute("map");
        attr.Value = r.name;
        record.Attributes.Append(attr);

        attr = doc.CreateAttribute("moves");
        attr.Value = r.moves.ToString();
        record.Attributes.Append(attr);

        attr = doc.CreateAttribute("points");
        attr.Value = r.points.ToString();
        record.Attributes.Append(attr);

        attr = doc.CreateAttribute("date");
        attr.Value = r.date.ToString();
        record.Attributes.Append(attr);

        rootNode.AppendChild(record);

        Record[] records = GetRecords(r.name);

        if(records.Length >= MAX_RECORDS)
        {
            records = records.OrderByDescending(item => item.points).ThenByDescending(item => item.date).ToArray();
            string date = records[records.Length - 1].date.ToString();

            XmlNodeList recs = doc.SelectSingleNode("Ranking").SelectNodes("Record");

            for (int i = 0; i < recs.Count; i++)
            {
                if (recs[i].Attributes.GetNamedItem("date").Value.Trim() == date)
                {
                    doc.SelectSingleNode("Ranking").RemoveChild(recs[i]);
                    break;
                }
            }
        }

        doc.Save(PathToRankFile);
    }

    /// <summary>
    /// Returns best result in given map. If there is no any record, then returns null.
    /// </summary>
    /// <param name="mapName">Map from which records will be checked.</param>
    /// <returns></returns>
    public static Record? GetTheBestRecord(string mapName)
    {
        Record[] records = GetRecords(mapName);
        if (records == null || records.Length == 0) return null;
        return records[0];
    }

    /// <summary>
    /// Returns array with all records from given map.
    /// </summary>
    /// <param name="mapName">Map from which records will be returned.</param>
    /// <returns></returns>
    public static Record[] GetRecords(string mapName)
    {
        if (!File.Exists(PathToRankFile)) return new Record[0];

        XmlDocument doc = new XmlDocument();
        doc.Load(PathToRankFile);

        XmlNodeList records = doc.SelectSingleNode("Ranking").SelectNodes("Record");
        List<Record> entries = new List<Record>();

        for(int i = 0; i < records.Count; i++)
        {
            if(records[i].Attributes.GetNamedItem("map").Value.Trim() == mapName)
            {
                Record record = new Record(
                    records[i].Attributes.GetNamedItem("map").Value, 
                    int.Parse(records[i].Attributes.GetNamedItem("moves").Value),
                    int.Parse(records[i].Attributes.GetNamedItem("points").Value),
                    DateTime.Parse(records[i].Attributes.GetNamedItem("date").Value)
                    );

                entries.Add(record);
            }
        }

        entries = entries.OrderByDescending(item => item.points).ThenByDescending(item => item.date).ToList();
        return entries.ToArray();
    }

    /// <summary>
    /// Removes all records linked with the map with given name.
    /// </summary>
    public static void RemoveAllRecords(string mapName)
    {
        if (!File.Exists(PathToRankFile)) return;

        XmlDocument doc = new XmlDocument();
        doc.Load(PathToRankFile);

        XmlNode rootNode = doc.SelectSingleNode("Ranking");
        XmlNodeList records = rootNode.SelectNodes("Record");

        for (int i = 0; i < records.Count; i++)
        {
            if (records[i].Attributes.GetNamedItem("map").Value.Trim() == mapName)
            {
                doc.SelectSingleNode("Ranking").RemoveChild(records[i]);
            }
        }

        doc.Save(PathToRankFile);
    }

    /// <summary>
    /// If directory or file in which ranking is stored don't exist, then creates them and additionaly prepares xml file.
    /// </summary>
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

    /// <summary>
    /// Structure which stores information about one result.
    /// </summary>
    public struct Record
    {
        public string name;
        public int moves;
        public int points;
        public DateTime date;

        public Record(string name, int moves, int points, DateTime date)
        {
            this.name = name;
            this.moves = moves;
            this.points = points;
            this.date = date;
        }
    }
}

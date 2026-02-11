using UnityEngine;
using System.Collections.Generic;

public static class PersonCSVLoader
{
    public static List<PersonData> LoadFromCSV(string csvName)
    {
        List<PersonData> list = new List<PersonData>();

        TextAsset csv = Resources.Load<TextAsset>(csvName);

        if (csv == null)
        {
            Debug.LogError($"CSV not found: {csvName}");
            return list;
        }

        string[] lines = csv.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] c = line.Split(',');

            if (c.Length < 10)
            {
                Debug.LogError($"CSV format error: {line}");
                continue;
            }

            PersonData d = new PersonData();
            d.name = c[0];
            d.baseSpriteName = c[1];

            string[] docs = c[2].Split('|');
            d.relatedDocumentIndices = new int[docs.Length];
            for (int j = 0; j < docs.Length; j++)
                int.TryParse(docs[j], out d.relatedDocumentIndices[j]);

            float.TryParse(c[3], out d.angerTime);

            d.enterLine = c[4];
            d.exitLine = c[5];
            d.stampPattern = c[6];
            d.docAngleMode = c[7];
            float.TryParse(c[8], out d.docAngleMin);
            float.TryParse(c[9], out d.docAngleMax);

            list.Add(d);
        }

        return list;
    }
}

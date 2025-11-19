using UnityEngine;
using System.Collections.Generic;

public static class PersonCSVLoader
{
    public static List<PersonData> LoadFromCSV(string csvName)
    {
        TextAsset csv = Resources.Load<TextAsset>(csvName);

        if (csv == null)
        {
            Debug.LogError($"CSVファイルが見つかりません: {csvName}");
            return new List<PersonData>();
        }

        List<PersonData> list = new List<PersonData>();
        string[] lines = csv.text.Split('\n');

        // 行 0 はヘッダ行
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] c = line.Split(',');

            PersonData data = new PersonData();
            data.name = c[0];
            data.baseSpriteName = c[1];

            // Documents (例: 0|1)
            string[] docs = c[2].Split('|');
            data.relatedDocumentIndices = new int[docs.Length];
            for (int j = 0; j < docs.Length; j++)
                int.TryParse(docs[j], out data.relatedDocumentIndices[j]);

            float.TryParse(c[3], out data.angerTime);

            data.enterLine = c[4];
            data.exitLine = c[5];
            data.stampPattern = c[6];
            data.docAngleMode = c[7];

            float.TryParse(c[8], out data.docAngleMin);
            float.TryParse(c[9], out data.docAngleMax);

            list.Add(data);
        }

        return list;
    }
}
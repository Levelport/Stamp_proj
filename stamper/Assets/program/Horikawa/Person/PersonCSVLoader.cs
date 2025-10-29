using UnityEngine;
using System.Collections.Generic;

public static class PersonCSVLoader
{
    public static List<PersonData> LoadFromCSV(string fileName)
    {
        List<PersonData> list = new List<PersonData>();
        TextAsset csvFile = Resources.Load<TextAsset>(fileName);

        if (csvFile == null)
        {
            Debug.LogError($"❌ CSVファイルが見つかりません: Resources/{fileName}.csv");
            return list;
        }

        string[] lines = csvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] cols = SplitCSV(line);
            if (cols.Length < 10) continue;

            PersonData data = ScriptableObject.CreateInstance<PersonData>();
            data.name = cols[0].Trim();
            data.baseSprite = Resources.Load<Sprite>(cols[1].Trim());

            string[] docStrs = cols[2].Split(';');
            int[] docs = new int[docStrs.Length];
            for (int d = 0; d < docStrs.Length; d++)
                int.TryParse(docStrs[d], out docs[d]);
            data.relatedDocumentIndices = docs;

            float.TryParse(cols[3], out data.angerTime);
            data.enterLine = cols[4].Trim('"');
            data.exitLine = cols[5].Trim('"');
            data.stampPattern = cols[6].Trim();

            data.docAngleMode = cols[7].Trim();
            float.TryParse(cols[8], out data.docAngleMin);
            float.TryParse(cols[9], out data.docAngleMax);

            list.Add(data);
        }

        Debug.Log($"✅ CSVから人物データ {list.Count}件 読み込み完了");
        return list;
    }

    // カンマ・クォート対応パーサー
    private static string[] SplitCSV(string line)
    {
        List<string> result = new List<string>();
        bool inQuote = false;
        string current = "";

        foreach (char c in line)
        {
            if (c == '"') { inQuote = !inQuote; continue; }
            if (c == ',' && !inQuote)
            {
                result.Add(current);
                current = "";
            }
            else current += c;
        }
        result.Add(current);
        return result.ToArray();
    }
}

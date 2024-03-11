using System.Globalization;
using System.Numerics;

namespace ComputerGraphics.Algorithms;

public class ObjFileParser(string fullObjFilePath)
{
    public readonly List<Vector4> Vertices = [];

    public readonly List<List<int>> Polygons = [];

    private string FilePath { get; init; } = fullObjFilePath;
    
    public void ParseFile()
    {
        foreach (var line in File.ReadLines(FilePath))
        {
            if (line.Length <= 4) continue;

            switch (line[0])
            {
                case 'v' when line[1] == ' ' && line.Length > 7:
                {
                    string[] spaceParts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    Vector4 v3 = new(
                        float.Parse(spaceParts[1], NumberFormatInfo.InvariantInfo),
                        float.Parse(spaceParts[2], NumberFormatInfo.InvariantInfo),
                        float.Parse(spaceParts[3], NumberFormatInfo.InvariantInfo),
                        1);

                    Vertices.Add(v3);

                    break;
                }
                case 'f' when line[1] == ' ' && line.Length > 7:
                {
                    List<int> polygonVertices = [];

                    string[] spaceParts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 1; i < spaceParts.Length; i++)
                    {
                        string[] slashParts = spaceParts[i].Split('/', StringSplitOptions.RemoveEmptyEntries);

                        polygonVertices.Add(int.Parse(slashParts[0]));
                    }

                    Polygons.Add(polygonVertices);

                    break;
                }
            }
        }

    }
}
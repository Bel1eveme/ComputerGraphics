using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;

namespace ComputerGraphics.Algorithms;

public class ObjFileParser(string fullObjFilePath)
{
    public readonly List<Vector4> Vertices = [];
    public readonly List<Vector4> Normals = [];

    public readonly List<List<int>> Polygons = [];

    private string FilePath { get; init; } = fullObjFilePath;
    
    public void ParseFile1()
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
                case 'v' when line[1] == 'n' && line.Length > 7:
                {
                    string[] spaceParts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    Vector4 v3 = new(
                        float.Parse(spaceParts[1], NumberFormatInfo.InvariantInfo),
                        float.Parse(spaceParts[2], NumberFormatInfo.InvariantInfo),
                        float.Parse(spaceParts[3], NumberFormatInfo.InvariantInfo),
                        1);

                    Normals.Add(v3);

                    break;
                }
            }
        }

    }
    public void ParseFile()
    {
        using var sr = new StreamReader(FilePath);
        string? line;
        while ((line = sr.ReadLine()) != null)
        {
            line = Regex.Replace(line.Trim().Replace('.', ','), @"\s+", " ");
            if (line.Length > 0)
            {
                var split = line.Split(' ');
                var values = split[1..];
                if (split[0] == "v")
                {
                    var floats = values.Where(x => x != "").Select(float.Parse);
                    var arr = floats.ToArray();
                    Vertices.Add(new Vector4(arr[0], arr[1], arr[2], 1));
                }
                else if (split[0] == "vn")
                {
                    var floats = values.Where(x => x != "").Select(float.Parse);
                    var arr = floats.ToArray();
                    Normals.Add(new Vector4(arr[0], arr[1], arr[2], 1));
                }
                else if (split[0] == "f")
                {
                    var vertexIndexes = new List<int>();
                    var normalsIndexes = new List<int>();
                    foreach (string value in values)
                    {
                        var indexes = value.Split('/');
                        var vertexIndex = int.Parse(indexes[0]);
                        var normalIndex = int.Parse(indexes[2]);
                        vertexIndexes.Add(vertexIndex > 0 ? vertexIndex - 1 : Vertices.Count - vertexIndex);
                        normalsIndexes.Add(normalIndex > 0 ? normalIndex - 1 : Normals.Count - normalIndex);
                    }
                    Polygons.Add(vertexIndexes);
                }
            }
               
        }
    }
}
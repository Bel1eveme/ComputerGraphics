using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;

namespace ComputerGraphics.Algorithms;

public class ObjFileParser(string fullObjFilePath)
{
    public readonly List<Vector4> Vertices = [];
    public readonly List<Vector3> Normals = [];
    public readonly List<List<int>> PolygonsNormals = [];
    
    public readonly List<List<int>> Polygons = [];
    public Bitmap textureFile;
    public Bitmap mirrorMap;
    public Bitmap normalMap;
    public List<Vector2> textures = [];
    public List<List<int>> PolygonsTextures = [];
    public Vector3[,] fileNormals;
    

    private string FilePath { get; init; } = fullObjFilePath;
    
    // public void ParseFile1()
    // {
    //     foreach (var line in File.ReadLines(FilePath))
    //     {
    //         if (line.Length <= 4) continue;
    //
    //         switch (line[0])
    //         {
    //             case 'v' when line[1] == ' ' && line.Length > 7:
    //             {
    //                 string[] spaceParts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    //
    //                 Vector4 v3 = new(
    //                     float.Parse(spaceParts[1], NumberFormatInfo.InvariantInfo),
    //                     float.Parse(spaceParts[2], NumberFormatInfo.InvariantInfo),
    //                     float.Parse(spaceParts[3], NumberFormatInfo.InvariantInfo),
    //                     1);
    //
    //                 Vertices.Add(v3);
    //
    //                 break;
    //             }
    //             case 'f' when line[1] == ' ' && line.Length > 7:
    //             {
    //                 List<int> polygonVertices = [];
    //
    //                 string[] spaceParts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    //
    //                 for (int i = 1; i < spaceParts.Length; i++)
    //                 {
    //                     string[] slashParts = spaceParts[i].Split('/', StringSplitOptions.RemoveEmptyEntries);
    //
    //                     polygonVertices.Add(int.Parse(slashParts[0]));
    //                 }
    //
    //                 Polygons.Add(polygonVertices);
    //
    //                 break;
    //             }
    //             case 'v' when line[1] == 'n' && line.Length > 7:
    //             {
    //                 string[] spaceParts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    //
    //                 Vector3 v3 = new(
    //                     float.Parse(spaceParts[1], NumberFormatInfo.InvariantInfo),
    //                     float.Parse(spaceParts[2], NumberFormatInfo.InvariantInfo),
    //                     float.Parse(spaceParts[3], NumberFormatInfo.InvariantInfo));
    //
    //                 Normals.Add(v3);
    //
    //                 break;
    //             }
    //         }
    //     }
    //
    // }
    public void ParseFile(string diffuseMapPath, string mirrorMapPath, string normalMapPath)
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
                else if (split[0] == "vt")
                {
                    var floats = values.Where(x => x != "").Select(float.Parse);
                    textures.Add(new Vector2(floats.ToArray()));
                }
                else if (split[0] == "vn")
                {
                    var floats = values.Where(x => x != "").Select(float.Parse);
                    var arr = floats.ToArray();
                    Normals.Add(new Vector3(floats.ToArray()));
                }
                else if (split[0] == "f")
                {
                    var vertexIndexes = new List<int>();
                    var textureIndexes = new List<int>();
                    var normalsIndexes = new List<int>();
                    foreach (string value in values)
                    {
                        var indexes = value.Split('/');
                        var vertexIndex = int.Parse(indexes[0]);
                        var normalIndex = int.Parse(indexes[2]);
                        var textureIndex = int.Parse(indexes[1]);
                        vertexIndexes.Add(vertexIndex > 0 ? vertexIndex - 1 : Vertices.Count - vertexIndex);
                        textureIndexes.Add(textureIndex > 0 ? textureIndex - 1 : textures.Count - textureIndex);
                        normalsIndexes.Add(normalIndex > 0 ? normalIndex - 1 : Normals.Count - normalIndex);
                    }
                    Polygons.Add(vertexIndexes);
                    PolygonsNormals.Add(normalsIndexes);
                    PolygonsTextures.Add(textureIndexes);
                }
            }
               
        }
        try
        {
            textureFile = (Bitmap)Bitmap.FromFile(diffuseMapPath);
        }
        catch (Exception ex)
        {
            textureFile = null;
        }

        try
        {
            mirrorMap = (Bitmap)Bitmap.FromFile(mirrorMapPath);
        }
        catch (Exception ex)
        {
            mirrorMap = null;
        }

        try
        {
            normalMap = (Bitmap)Bitmap.FromFile(normalMapPath);
            fileNormals = new Vector3[normalMap.Width, normalMap.Height];
                    
            for (int i = 0; i < normalMap.Width; i++)
            {
                for (int j = 0; j < normalMap.Height; j++)
                {
                    Color normalColor = normalMap.GetPixel(i, j);
                    Vector3 normal = new Vector3(normalColor.R / 255f, normalColor.G / 255f, normalColor.B / 255f);
                    normal = (normal * 2) - Vector3.One;
                    normal = Vector3.Normalize(normal);
                }
            }
        }
        catch (Exception ex)
        {
            normalMap = null;
        }
    }
}
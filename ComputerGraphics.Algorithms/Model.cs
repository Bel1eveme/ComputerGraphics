using System.Drawing;
using System.Numerics;

namespace ComputerGraphics.Algorithms;

public class Model
{
    private const float FiledOfView = (float)(Math.PI / 2);
    
    private const float AspectRatio = (float)1920 / 1080;
    
    private const float NearPlaneDistance = 0.1f;
    
    private const float FarPlaneDistance = 100f;
    public Vector3 LightSource = new(0, 0, 200);

    public List<Vector4> Vertices { get; }
    public List<Vector4> WorldVertices { get; }
    public List<List<int>> PolygonsNormals = [];
    public  List<Vector3> Normals = [];
    
    public List<List<int>> Polygons { get; private set; }
    public Vector3 _source = new(40, 0, 10);

    private readonly int _viewportWidth;
    
    private readonly int _viewportHeight;

    public Matrix4x4 _worldMatrix;
    
    private readonly Matrix4x4 _viewMatrix;
    
    private readonly Matrix4x4 _projectionMatrix;

    private readonly Converter _converter;

    private float _scalingCoefficient;
    
    private float _step;
    
    private readonly List<Vector4> _modelVertices;
    public Bitmap textureFile;
    public Bitmap mirrorMap;
    public Bitmap normalMap;
    public List<Vector2> textures;
    public Vector3[,] fileNormals;
    public List<List<int>> PolygonsTextures = [];

    public Model(ObjFileParser parser, Converter converter, int viewportWidth, int viewportHeight)
    {
        _converter = converter;
        _viewportWidth = viewportWidth;
        _viewportHeight = viewportHeight;

        _modelVertices = parser.Vertices;
        Vertices =  Enumerable.Range(1, _modelVertices.Count)
                            .Select(_ => new Vector4()).ToList();
        Polygons = parser.Polygons;
        Normals = parser.Normals;
        PolygonsTextures = parser.PolygonsTextures;
        PolygonsNormals = parser.PolygonsNormals;
        textureFile = parser.textureFile;
        mirrorMap = parser.mirrorMap;
        normalMap = parser.normalMap;
        textures = parser.textures;
        fileNormals = parser.fileNormals;
        Console.WriteLine("Started drawing");
        
        var cameraPosition = new Vector3(1f, 1f, -MathF.PI);
        var cameraTarget = new Vector3(0, 0, 0);
        var cameraUpVector = new Vector3(0, 1, 0);

        _scalingCoefficient = 0.05f;
        _step = (float)Math.PI / 15.0f;
        _worldMatrix = Matrix4x4.Identity;
        _viewMatrix = Matrix4x4.CreateLookAt(cameraPosition, cameraTarget, cameraUpVector);
        _projectionMatrix =
            Matrix4x4.CreatePerspectiveFieldOfView(FiledOfView, AspectRatio, NearPlaneDistance, FarPlaneDistance);
        
        
    }

    public void Update(Vector3 angle, float scale, Vector3 move)
    {
        var scaleModel = Matrix4x4.CreateScale(scale);                                                       
        var rotation = Matrix4x4.CreateFromYawPitchRoll(angle.Y, angle.X, angle.Z);                        
        var translation = Matrix4x4.CreateTranslation(move);
        _worldMatrix = scaleModel * rotation * translation;
        _converter.ApplyTransformations(_modelVertices, Vertices, _scalingCoefficient,
            _worldMatrix, _viewMatrix, _projectionMatrix, _viewportWidth, _viewportHeight, Normals);
    }

   
}
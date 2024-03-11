using System.Numerics;

namespace ComputerGraphics.Algorithms;

public class Model
{
    private const float FiledOfView = (float)(Math.PI / 4);
    
    private const float AspectRatio = (float)1080 / 720;
    
    private const float NearPlaneDistance = 0.1f;
    
    private const float FarPlaneDistance = 100f;

    public List<Vector4> Vertices { get; }
    
    public List<List<int>> Polygons { get; private set; }

    private readonly int _viewportWidth;
    
    private readonly int _viewportHeight;

    private readonly Matrix4x4 _worldMatrix;
    
    private readonly Matrix4x4 _viewMatrix;
    
    private readonly Matrix4x4 _projectionMatrix;

    private readonly Converter _converter;

    private float _scalingCoefficient;
    
    private float _step;
    
    private readonly List<Vector4> _modelVertices;

    public Model(ObjFileParser parser, Converter converter, int viewportWidth, int viewportHeight)
    {
        _converter = converter;
        _viewportWidth = viewportWidth;
        _viewportHeight = viewportHeight;

        _modelVertices = parser.Vertices;
        Vertices =  Enumerable.Range(1, _modelVertices.Count)
                            .Select(_ => new Vector4()).ToList();
        Polygons = parser.Polygons;
        
        Console.WriteLine("Started drawing");
        
        var cameraPosition = new Vector3(1f, 1f, -MathF.PI);
        var cameraTarget = Vector3.Zero;
        var cameraUpVector = Vector3.UnitY;

        _scalingCoefficient = 0.00005f;
        _step = (float)Math.PI / 15.0f;
        _worldMatrix = Matrix4x4.Identity;
        _viewMatrix = Matrix4x4.CreateLookAt(cameraPosition, cameraTarget, cameraUpVector);
        _projectionMatrix =
            Matrix4x4.CreatePerspectiveFieldOfView(FiledOfView, AspectRatio, NearPlaneDistance, FarPlaneDistance);
        
        Update();
    }

    private void Update()
    {
        _converter.ApplyTransformations(_modelVertices, Vertices, _scalingCoefficient,
            _worldMatrix, _viewMatrix, _projectionMatrix, _viewportWidth, _viewportHeight);
    }

    public void ChangeScalingCoefficient(float delta)
    {
        _scalingCoefficient += delta;
        
        Update();
    }
    
    public void ChangeStep(float delta)
    {
        _step += delta;
    }

    public void RotateXPos()
    {
        _converter.Transform(_modelVertices, Matrix4x4.CreateRotationX(_step));
        
        Update();
    }
    
    public void RotateXNeg()
    {
        _converter.Transform(_modelVertices, Matrix4x4.CreateRotationX(-_step));
        
        Update();
    }
    
    public void RotateYPos()
    {
        _converter.Transform(_modelVertices, Matrix4x4.CreateRotationY(_step));
        
        Update();
    }
    
    public void RotateYNeg()
    {
        _converter.Transform(_modelVertices, Matrix4x4.CreateRotationY(-_step));
        
        Update();
    }
    
    public void RotateZPos()
    {
        _converter.Transform(_modelVertices, Matrix4x4.CreateRotationZ(_step));
        
        Update();
    }
    
    public void RotateZNeg()
    {
        _converter.Transform(_modelVertices, Matrix4x4.CreateRotationZ(-_step));
        
        Update();
    }
}
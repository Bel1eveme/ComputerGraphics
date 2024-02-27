using System.Numerics;

namespace ComputerGraphics.Algorithms;

public class Model
{
    private const float FiledOfView = (float)(Math.PI / 4);
    
    private const float AspectRatio = (float)1080 / 720;
    
    private const float NearPlaneDistance = 0.1f;
    
    private const float FarPlaneDistance = 1000f;

    private const float Step = (float)Math.PI / 15;

    public List<Vector4> Vertices { get; private set; }
    
    public List<List<int>> Polygons { get; private set; }

    private readonly int _viewportWidth;
    
    private readonly int _viewportHeight;

    private readonly Matrix4x4 _worldMatrix;
    
    private readonly Matrix4x4 _viewMatrix;
    
    private readonly Matrix4x4 _projectionMatrix;

    private readonly Converter _converter;

    private float _scalingCoefficient;
    
    private List<Vector4> _modelVertices;

    public Model(ObjFileParser parser, Converter converter, int viewportWidth, int viewportHeight)
    {
        _converter = converter;
        _viewportWidth = viewportWidth;
        _viewportHeight = viewportHeight;

        _modelVertices = parser.Vertices;
        Vertices = parser.Vertices;
        Polygons = parser.Polygons;
        
        var cameraPosition = new Vector3(1f, 1f, -MathF.PI);
        var cameraTarget = Vector3.Zero;
        var cameraUpVector = Vector3.UnitY;

        _scalingCoefficient = 0.00009f;
        _worldMatrix = Matrix4x4.Identity;
        _viewMatrix = Matrix4x4.CreateLookAt(cameraPosition, cameraTarget, cameraUpVector);
        _projectionMatrix =
            Matrix4x4.CreatePerspectiveFieldOfView(FiledOfView, AspectRatio, NearPlaneDistance, FarPlaneDistance);
        
        Update();
    }

    private void Update()
    {
        Vertices = _converter.ModelToWorld(_converter.Scale(_modelVertices, _scalingCoefficient), _worldMatrix);
        
        Vertices = _converter.WorldToView(Vertices, _viewMatrix);
        Vertices = _converter.ViewToProjection(Vertices, _projectionMatrix);
        Vertices = _converter.ProjectionToViewport(Vertices, _viewportWidth, _viewportHeight);

        /*Vertices = _converter.ApplyAllTransformations(_modelVertices, _scalingCoefficient,
            _worldMatrix, _projectionMatrix, _viewMatrix, _viewportWidth, _viewportHeight);*/
    }

    public void ChangeScalingCoefficient(float delta)
    {
        _scalingCoefficient += delta;
        
        Update();
    }

    public void RotateRight()
    {
        _modelVertices = _converter.Transform(_modelVertices, Matrix4x4.CreateRotationX(-Step));
        
        Update();
    }
    
    public void RotateLeft()
    {
        _modelVertices = _converter.Transform(_modelVertices, Matrix4x4.CreateRotationX(Step));
        
        Update();
    }
    
    public void RotateDown()
    {
        _modelVertices = _converter.Transform(_modelVertices, Matrix4x4.CreateRotationY(-Step));
        
        Update();
    }
    
    public void RotateUp()
    {
        _modelVertices = _converter.Transform(_modelVertices, Matrix4x4.CreateRotationY(Step));
        
        Update();
    }
    
    public void MoveRight()
    {
        _modelVertices = _converter.Transform(_modelVertices, Matrix4x4.CreateRotationZ(-Step));
        
        Update();
    }
    
    public void MoveLeft()
    {
        _modelVertices = _converter.Transform(_modelVertices, Matrix4x4.CreateRotationZ(Step));
        
        Update();
    }
    
}
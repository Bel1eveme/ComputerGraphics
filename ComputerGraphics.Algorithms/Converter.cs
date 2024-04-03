using System.Numerics;
using System.Runtime.InteropServices;

namespace ComputerGraphics.Algorithms;

public class Converter
{
    public List<Vector4> WorldVertices { get; set; }

    public void ApplyTransformations(List<Vector4> modelVertices, List<Vector4> vertices, Matrix4x4 worldMatrix,
        Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, int width, int height)
    {
        Span<Vector4> verticesAsSpan = CollectionsMarshal.AsSpan(vertices);
        Span<Vector4> modelVerticesAsSpan = CollectionsMarshal.AsSpan(modelVertices);
        
        for (int i = 0; i < modelVertices.Count; i++)
        {
            verticesAsSpan[i] = Vector4.Transform(modelVerticesAsSpan[i], worldMatrix);
            
            verticesAsSpan[i] = Vector4.Transform(verticesAsSpan[i], viewMatrix);
            
            verticesAsSpan[i] = Vector4.Transform(verticesAsSpan[i], projectionMatrix);
            verticesAsSpan[i] /= verticesAsSpan[i].W;

            verticesAsSpan[i].X = (verticesAsSpan[i].X + 1) * width / 2;
            verticesAsSpan[i].Y = (-verticesAsSpan[i].Y + 1) * height / 2;
        }
    }
    
    public void ApplyTransformations(List<Vector4> modelVertices, List<Vector4> vertices, float scale,
        Matrix4x4 worldMatrix, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, int width, int height)
    {
        Span<Vector4> verticesAsSpan = CollectionsMarshal.AsSpan(vertices);
        Span<Vector4> modelVerticesAsSpan = CollectionsMarshal.AsSpan(modelVertices);
        WorldVertices = new(modelVertices.Count);
        var scaleMatrix = Matrix4x4.CreateScale(scale);
        
        for (int i = 0; i < modelVertices.Count; i++)
        {
            verticesAsSpan[i] = Vector4.Transform(modelVerticesAsSpan[i], scaleMatrix);
            
            verticesAsSpan[i] = Vector4.Transform(verticesAsSpan[i], worldMatrix);
            WorldVertices.Add(new Vector4(verticesAsSpan[i].X, verticesAsSpan[i].Y, verticesAsSpan[i].Z, verticesAsSpan[i].W));

            verticesAsSpan[i] = Vector4.Transform(verticesAsSpan[i], viewMatrix);
            
            verticesAsSpan[i] = Vector4.Transform(verticesAsSpan[i], projectionMatrix);
            verticesAsSpan[i] /= verticesAsSpan[i].W;

            verticesAsSpan[i].X = (verticesAsSpan[i].X + 1) * width / 2;
            verticesAsSpan[i].Y = (-verticesAsSpan[i].Y + 1) * height / 2;
            
        }
    }
    
    public void Transform(List<Vector4> vertices, Matrix4x4 matrix)
    {
        Span<Vector4> verticesAsSpan = CollectionsMarshal.AsSpan(vertices);
        
        for (int i = 0; i < vertices.Count; i++)
        {
            verticesAsSpan[i] = Vector4.Transform(verticesAsSpan[i], matrix);
        }
    }
}
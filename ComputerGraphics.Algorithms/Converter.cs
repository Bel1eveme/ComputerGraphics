using System.Numerics;
using System.Runtime.InteropServices;

namespace ComputerGraphics.Algorithms;

public class Converter
{
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
        
        for (int i = 0; i < modelVertices.Count; i++)
        {
            verticesAsSpan[i] = Vector4.Transform(modelVerticesAsSpan[i], Matrix4x4.CreateScale(scale));
            
            verticesAsSpan[i] = Vector4.Transform(verticesAsSpan[i], worldMatrix);
            
            verticesAsSpan[i] = Vector4.Transform(verticesAsSpan[i], viewMatrix);
            
            verticesAsSpan[i] = Vector4.Transform(verticesAsSpan[i], projectionMatrix);
            verticesAsSpan[i] /= verticesAsSpan[i].W;

            verticesAsSpan[i].X = (verticesAsSpan[i].X + 1) * width / 2;
            verticesAsSpan[i].Y = (-verticesAsSpan[i].Y + 1) * height / 2;
        }
    }

    public List<Vector4> ApplyTransformation(List<Vector4> vertices, Matrix4x4 matrix)
    {
        Span<Vector4> verticesAsSpan = CollectionsMarshal.AsSpan(vertices);
        
        for (int i = 0; i < vertices.Count; i++)
        {
            verticesAsSpan[i] = Vector4.Transform(verticesAsSpan[i], matrix);
        }
        
        return vertices;
    }
    
    public List<Vector4> ApplyTransformationWithDivision(List<Vector4> vertices, Matrix4x4 matrix)
    {
        Span<Vector4> verticesAsSpan = CollectionsMarshal.AsSpan(vertices);
        
        for (int i = 0; i < vertices.Count; i++)
        {
            verticesAsSpan[i] = Vector4.Transform(verticesAsSpan[i], matrix);
            verticesAsSpan[i] /= vertices[i].W;
        }
        
        return vertices;
    }
    
    public List<Vector4> ModelToWorld(List<Vector4> vertices, Matrix4x4 transformationMatrix)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = Vector4.Transform(vertices[i], transformationMatrix);
        }
        
        return vertices;
    }
    
    public List<Vector4> WorldToView(List<Vector4> vertices, Matrix4x4 viewMatrix)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = Vector4.Transform(vertices[i], viewMatrix);
        }
        
        return vertices;
    }
    
    public List<Vector4> ViewToProjection(List<Vector4> vertices, Matrix4x4 projectionMatrix)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = Vector4.Transform(vertices[i], projectionMatrix);
            vertices[i] /= vertices[i].W;
        }
        
        return vertices;
    }
    
    public List<Vector4> ProjectionToViewport(List<Vector4> vertices, int width, int height)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            float x = (vertices[i].X + 1) * width / 2;
            float y = (-vertices[i].Y + 1) * height / 2;
            
            vertices[i] = new(x, y, vertices[i].Z, vertices[i].W);
        }

        return vertices;
    }
    
    public List<Vector4> Transform(List<Vector4> vertices, Matrix4x4 matrix)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = Vector4.Transform(vertices[i], matrix);
        }
        
        return vertices;
    }
    
    public List<Vector4> Scale(List<Vector4> vertices, float scale)
    {
        List<Vector4> scaledVertices = [];
        Matrix4x4 scaleMatrix = new Matrix4x4(
            scale, 0, 0, 0, 
            0, scale, 0, 0,
            0, 0, scale, 0,
            0, 0, 0, 1);

        foreach (var vertex in vertices)
        {
            Vector4 input = new (vertex.X, vertex.Y, vertex.Z, vertex.W);
            Vector4 result = Vector4.Transform(input, scaleMatrix);

            scaledVertices.Add(new Vector4(result.X, result.Y, result.Z, result.W));
        }
        
        return scaledVertices;
    }

    private Matrix4x4 GetScaleMatrix(float scale) => new Matrix4x4
    (
        scale, 0, 0, 0,
        0, scale, 0, 0,
        0, 0, scale, 0,
        0, 0, 0, 1
    );
}
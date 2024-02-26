using System.Numerics;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;

namespace ComputerGraphics.Algorithms;

public class Converter
{
    public List<Vector4> ModelToWorld(List<Vector4> vertices, Matrix4x4 transformationMatrix)
    {
        Span<Vector4> verticesAsSpan = CollectionsMarshal.AsSpan(vertices);
        
        for (int i = 0; i < verticesAsSpan.Length; i++)
        {
            verticesAsSpan[i] = Vector4.Transform(verticesAsSpan[i], transformationMatrix);
        }
        
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
    
    public List<Vector4> Transform(List<Vector4> vertices, Matrix4x4 translationMatrix)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = Vector4.Transform(vertices[i], translationMatrix);
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
}
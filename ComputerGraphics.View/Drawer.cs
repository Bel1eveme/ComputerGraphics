using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ComputerGraphics.Algorithms;
using Color = System.Windows.Media.Color;

namespace ComputerGraphics.View;

public class Drawer
{
    // possible optimizations:
    // 1. cache matrices
    // 2. do not scale every time
    // 3. Clear method to pure pointers without marshaling
    // 4. draw line alg?
    // 5. Try readonly span?
    
    public WriteableBitmap Bitmap { get; }

    private readonly Color _foregroundColor;
    
    private readonly Color _backgroundColor;

    private readonly Model _model;

    private readonly int _bytesPerPixel;
    private float[,] _zBuffer;
    private readonly Vector3 _eye = new(1f, 1f, -MathF.PI);
    private readonly Vector3 _source = new(30, 0, 50);
    private readonly Converter _converter;

    public Drawer(int width, int height, Color foregroundColor, Color backgroundColor, Model model, Converter converter)
    {
        _converter = converter;
        _foregroundColor = foregroundColor;
        _backgroundColor = backgroundColor;
        Bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
        _model = model;
        _zBuffer = new float[Bitmap.PixelHeight, Bitmap.PixelWidth];
        
        _bytesPerPixel = (Bitmap.Format.BitsPerPixel + 7) / 8;
        
        Update();
    }

    private void DrawLineDda(PointF firstPoint, PointF secondPoint)
    {
        int stride = Bitmap.PixelWidth *_bytesPerPixel;

        int startX = (int)firstPoint.X;
        int startY = (int)firstPoint.Y;
        int endX = (int)secondPoint.X;
        int endY = (int)secondPoint.Y;
        
        int dx = endX - startX;
        int dy = endY - startY;

        int steps = Math.Max(Math.Abs(dx), Math.Abs(dy));
        
        float xIncrement = (float)(endX - startX) / steps;
        float yIncrement = (float)(endY - startY) / steps;
        
        byte[] pixels = new byte[Bitmap.PixelHeight * stride];

        float x = startX;
        float y = startY;
        
        for (int i = 0; i <= steps; i++)
        {
            int pixelX = (int)Math.Round(x);
            int pixelY = (int)Math.Round(y);

            if (pixelX >= 0 && pixelX < Bitmap.PixelWidth && pixelY >= 0 && pixelY < Bitmap.PixelHeight)
            {
                int pixelIndex = pixelY * stride + pixelX * _bytesPerPixel;
                pixels[pixelIndex] = _foregroundColor.B;
                pixels[pixelIndex + 1] = _foregroundColor.G;
                pixels[pixelIndex + 2] = _foregroundColor.R;
                pixels[pixelIndex + 3] = _foregroundColor.A;
            }

            x += xIncrement;
            y += yIncrement;
        }

        Bitmap.WritePixels(new Int32Rect(0, 0, Bitmap.PixelWidth, Bitmap.PixelHeight), pixels, stride, 0);
    }

    private void DrawPixel(int x, int y, Color color)
    {
        unsafe
        {
            byte* data = (byte*)Bitmap.BackBuffer + y * Bitmap.BackBufferStride + x * _bytesPerPixel;
            if (x >= 0 && x < Bitmap.PixelWidth && y >= 0 && y < Bitmap.PixelHeight)
            {
                data[0] = color.B;
                data[1] = color.G;
                data[2] = color.R;
                data[3] = color.A;
            }
        }
    }

    private void DrawLineBresenham(int x0, int y0, int x1, int y1)
    {
        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            DrawPixel(x0, y0, _foregroundColor);

            if (x0 == x1 && y0 == y1)
                break;

            int e2 = 2 * err;
            
            if (e2 > -dy)
            {   
                err -= dy;
                x0 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    private void Clear()
    {
        byte[] pixels = new byte[Bitmap.PixelHeight * Bitmap.BackBufferStride];
        
        for (int i = 0; i < pixels.Length; i += _bytesPerPixel)
        {
            pixels[i + 0] = _backgroundColor.B;
            pixels[i + 1] = _backgroundColor.G;
            pixels[i + 2] = _backgroundColor.R;
            pixels[i + 3] = _backgroundColor.A;
        }
        
        Bitmap.WritePixels(new Int32Rect(0, 0, Bitmap.PixelWidth, Bitmap.PixelHeight), pixels, Bitmap.BackBufferStride, 0);
    }
    
    private void Draw()
    {
        Span<Vector4> verticesAsSpan = CollectionsMarshal.AsSpan(_model.Vertices);
        Span<List<int>> polygonsAsSpan = CollectionsMarshal.AsSpan(_model.Polygons);
        
        for (int i = 0; i < _model.Polygons.Count; i++)
        {
            Span<int> indexesAsSpan = CollectionsMarshal.AsSpan(_model.Polygons[i]);
            
            for (int j = 0; j < polygonsAsSpan[i].Count - 1; j++)
            {
                int index1 = indexesAsSpan[j];
                int index2 = indexesAsSpan[j + 1];
                
                DrawLineBresenham((int)verticesAsSpan[index1 - 1].X, (int)verticesAsSpan[index1 - 1].Y,
                    (int)verticesAsSpan[index2 - 1].X, (int)verticesAsSpan[index2 - 1].Y);
            }

            int lastIndex = indexesAsSpan[^1];
            int firstIndex = indexesAsSpan[0];
				 
            DrawLineBresenham((int)verticesAsSpan[lastIndex - 1].X, (int)verticesAsSpan[lastIndex - 1].Y,
                (int)verticesAsSpan[firstIndex - 1].X, (int)verticesAsSpan[firstIndex - 1].Y);
        }
    }

    int ToInterval(float value, int max, int min = 0)
    {
        int result = Math.Max(min, (int)MathF.Round(value));
        if (result > max)
        {
            result = max;
        }
        return result;
    }

    void DrawLine(int y, Vector4 left, Vector4 right, Color color)
    {
        var deltaX = (right - left) / (right.X - left.X);
        int xMin = ToInterval(left.X, Bitmap.PixelWidth - 1);
        int xMax = ToInterval(right.X, Bitmap.PixelWidth - 1);
        if (xMin > xMax)
        {
            (xMin, xMax) = (xMax, xMin);
        }
        var point = left;
        for (int x = xMin; x <= xMax; x++)
        {
            point = left + deltaX * (x - left.X);
            if (point.Z <= _zBuffer[y, x])
            {
                _zBuffer[y, x] = point.Z;
                DrawPixel(x, y, color);
            }
        }
    }

    private Color Lighting(Vector3 center, Vector3 centerNormal)
    {
        var lightVector = Vector3.Normalize(_source - center);
        var intensity = Vector3.Dot(centerNormal, lightVector);
        return Color.Multiply(Colors.White, intensity);
    }

    private void Rasterization(Color color, Vector4[] vertices)
    {

        if (vertices[0].Y > vertices[1].Y)
        {
            (vertices[0], vertices[1]) = (vertices[1], vertices[0]);
        }
        if (vertices[1].Y > vertices[2].Y)
        {
            (vertices[1], vertices[2]) = (vertices[2], vertices[1]);
        }
        if (vertices[0].Y > vertices[1].Y)
        {
            (vertices[0], vertices[1]) = (vertices[1], vertices[0]);
        }

        Vector4 delta02 = vertices[2] - vertices[0];
        Vector4 delta01 = vertices[1] - vertices[0];
        Vector4 delta12 = vertices[2] - vertices[1];
        if (delta02.Y > 1)
        {
            delta02 /= delta02.Y;
        }
        if (delta01.Y > 1)
        {
            delta01 /= delta01.Y;
        }
        if (delta12.Y > 1)
        {
            delta12 /= delta12.Y;
        }
        Vector4 deltaLeft1, deltaLeft2, deltaRight1, deltaRight2;
        if (vertices[1].X < vertices[2].X)
        {
            deltaLeft1 = delta01;
            deltaLeft2 = delta12;
            deltaRight1 = delta02;
            deltaRight2 = delta02;
        }
        else
        {
            deltaLeft1 = delta02;
            deltaLeft2 = delta02;
            deltaRight1 = delta01;
            deltaRight2 = delta12;
        }

        int start = ToInterval(vertices[0].Y, Bitmap.PixelHeight - 1);
        int stop = ToInterval(vertices[1].Y, Bitmap.PixelHeight - 1);
        for (int y = start; y < stop; y++)
        {
            var left = vertices[0] + deltaLeft1 * (y - vertices[0].Y);
            var right = vertices[0] + deltaRight1 * (y - vertices[0].Y);
            DrawLine(y, left, right, color);
        }

        start = stop;
        stop = ToInterval(vertices[2].Y, Bitmap.PixelHeight - 1);

        Vector4 leftStart, rightStart;
        if (vertices[1].X < vertices[2].X)
        {
            leftStart = vertices[1];
            rightStart = vertices[0] + deltaRight1 * (start - vertices[0].Y);
        }
        else
        {
            leftStart = vertices[0] + deltaLeft1 * (start - vertices[0].Y);
            rightStart = vertices[1];
        }
        for (int y = start; y <= stop; y++)
        {
            var left = leftStart + deltaLeft2 * (y - vertices[1].Y);
            var right = rightStart + deltaRight2 * (y - vertices[1].Y);
            DrawLine(y, left, right, color);
        }
    }
    private void Draw2()
    {
        for (int i = 0; i < Bitmap.PixelHeight; i++)
        {
            for (int j = 0; j < Bitmap.PixelWidth; j++)
            {
                _zBuffer[i, j] = float.MaxValue;
            }
        }

        int k = 0;
        foreach (List<int> corners in _model.Polygons)
        {
            k++;
            Vector3[] triangle =
            [
                new Vector3(_converter.WorldVertices[corners[0]].X, _converter.WorldVertices[corners[0]].Y, _converter.WorldVertices[corners[0]].Z),
                new Vector3(_converter.WorldVertices[corners[1]].X, _converter.WorldVertices[corners[1]].Y, _converter.WorldVertices[corners[1]].Z),
                new Vector3(_converter.WorldVertices[corners[2]].X, _converter.WorldVertices[corners[2]].Y, _converter.WorldVertices[corners[2]].Z)
            ];
            var center = new Vector3
            (
                (triangle[0].X + triangle[1].X + triangle[2].X) / 3,
                (triangle[0].Y + triangle[1].Y + triangle[2].Y) / 3,
                (triangle[0].Z + triangle[1].Z + triangle[2].Z) / 3
            );
            var centerNormal = Vector3.Normalize(Vector3.Cross(triangle[0] - center, triangle[1] - center));
            if (Vector3.Dot(centerNormal, _eye) < 0)
            {
                continue;
            }

            var color = Lighting(center, centerNormal);
            Vector4[] vertices =
            [
                _model.Vertices[corners[0]],
                _model.Vertices[corners[1]],
                _model.Vertices[corners[2]],
            ];
            Rasterization(color, vertices);

        }
    }

    public void Update()
    {
        Clear();
        
        Draw2();
    }
}

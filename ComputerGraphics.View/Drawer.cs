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

    public Drawer(int width, int height, Color foregroundColor, Color backgroundColor, Model model)
    {
        _foregroundColor = foregroundColor;
        _backgroundColor = backgroundColor;
        Bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
        _model = model;
        
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
    
    private void DrawLineBresenham(int x0, int y0, int x1, int y1)
    {
        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            unsafe
            {
                byte* data = (byte*)Bitmap.BackBuffer + y0 * Bitmap.BackBufferStride
                                                      + x0 * _bytesPerPixel;

                if (x0 >= 0 && x0 < Bitmap.PixelWidth && y0 >= 0 && y0 < Bitmap.PixelHeight)
                {
                    data[0] = _foregroundColor.B;
                    data[1] = _foregroundColor.G;
                    data[2] = _foregroundColor.R;
                    data[3] = _foregroundColor.A;
                }
                
            }

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

    public void Update()
    {
        Clear();
        
        Draw();
    }
}

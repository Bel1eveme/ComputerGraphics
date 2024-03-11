using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ComputerGraphics.Algorithms;
using Color = System.Windows.Media.Color;

namespace ComputerGraphics.View;

public class Drawer
{
    public WriteableBitmap Bitmap { get; }

    private readonly Color _foregroundColor;
    
    private readonly Color _backgroundColor;

    private readonly Model _model;

    private readonly byte[] _colorRgbArray;

    private readonly int _bytesPerPixel;

    public Drawer(int width, int height, Color foregroundColor, Color backgroundColor, Model model)
    {
        _foregroundColor = foregroundColor;
        _backgroundColor = backgroundColor;
        Bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
        _model = model;

        _colorRgbArray = [foregroundColor.B, _foregroundColor.G, _foregroundColor.R, 255];
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
    
    private void DrawLineBresenham(PointF startPoint, PointF endPoint)
    {
        unsafe
        {
            int startX = (int)startPoint.X;
            int startY = (int)startPoint.Y;
            int endX = (int)endPoint.X;
            int endY = (int)endPoint.Y;

            int dx = Math.Abs(endX - startX);
            int dy = Math.Abs(endY - startY);

            int sx = startX < endX ? 1 : -1;
            int sy = startY < endY ? 1 : -1;

            int err = dx - dy;

            int index = (startY * Bitmap.BackBufferStride) + (startX * _bytesPerPixel);
            byte* byteBufferPtr = (byte*)Bitmap.BackBuffer;

            while (true)
            {
                byteBufferPtr[index] = _foregroundColor.B;
                byteBufferPtr[index + 1] = _foregroundColor.G;
                byteBufferPtr[index + 2] = _foregroundColor.R;
                byteBufferPtr[index + 3] = _foregroundColor.A;

                if (startX == endX && startY == endY)
                    break;

                int err2 = 2 * err;

                if (err2 > -dy)
                {
                    err -= dy;
                    startX += sx;
                }

                if (err2 < dx)
                {
                    err += dx;
                    startY += sy;
                }

                index = startY * Bitmap.BackBufferStride + startX * _bytesPerPixel;
            }
        }
    }
    
    private void DrawLineBresenham1(PointF startPoint, PointF endPoint)
    {
        unsafe
        {
            int x0 = (int)Math.Round(startPoint.X);
            int y0 = (int)Math.Round(startPoint.Y);
            int x1 = (int)Math.Round(endPoint.X);
            int y1 = (int)Math.Round(endPoint.Y);

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
        
            while (true)
            {
                byte* data = (byte*)Bitmap.BackBuffer + y0 * Bitmap.BackBufferStride
                                                      + x0 * _bytesPerPixel;

                if (data != null && x0 >= 0 && x0 < Bitmap.PixelWidth && y0 >= 0 && y0 < Bitmap.PixelHeight)
                {
                    data[0] = _foregroundColor.B;
                    data[1] = _foregroundColor.G;
                    data[2] = _foregroundColor.R;
                    data[2] = _foregroundColor.A;
                }
                else
                {
                    Console.WriteLine();
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
    }

    private void Clear()
    {
        int stride = Bitmap.PixelWidth * _bytesPerPixel;
        
        byte[] pixels = new byte[Bitmap.PixelHeight * stride];
        
        for (int i = 0; i < pixels.Length; i += _bytesPerPixel)
        {
            pixels[i + 0] = _backgroundColor.B;
            pixels[i + 1] = _backgroundColor.G;
            pixels[i + 2] = _backgroundColor.R;
            pixels[i + 3] = _backgroundColor.A;
        }
        
        // test vs array copy
        
        Bitmap.WritePixels(new Int32Rect(0, 0, Bitmap.PixelWidth, Bitmap.PixelHeight), pixels, stride, 0);
    }
    
    private void Draw()
    {
        PointF[] pointsArray = new PointF[_model.Vertices.Count];
        for (int i = 0; i < _model.Vertices.Count; i++)
        {
            pointsArray[i] = new PointF(_model.Vertices[i].X, _model.Vertices[i].Y);
        }
        
        foreach (List<int> polygon in _model.Polygons)
        {
            for (int i = 0; i < polygon.Count - 1; i++)
            {
                int index1 = polygon[i];
                int index2 = polygon[i + 1];

                PointF point1 = pointsArray[index1 - 1];
                PointF point2 = pointsArray[index2 - 1];

                //DrawLineDda(point1, point2);
                DrawLineBresenham1(point1, point2);
            }

            int lastIndex = polygon[^1];
            int firstIndex = polygon[0];
				 
            PointF lastPoint = pointsArray[lastIndex - 1];
            PointF firstPoint = pointsArray[firstIndex - 1];
				 
            //DrawLineDda(lastPoint, firstPoint);
            DrawLineBresenham1(lastPoint, firstPoint);
        }
    }

    public void Update()
    {
        Clear();
        
        Draw();
    }
}

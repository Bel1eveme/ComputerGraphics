using System.Drawing;
using System.Drawing.Imaging;
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

        _colorRgbArray = [foregroundColor.B, _foregroundColor.G, _foregroundColor.R];
        _bytesPerPixel = (Bitmap.Format.BitsPerPixel + 7) / 8;
        
        Update();
    }

    private unsafe void DrawLineDda(PointF firstPoint, PointF secondPoint)
    {
        float dx = secondPoint.X - firstPoint.X;
        float dy = secondPoint.Y - firstPoint.Y;

        int stepCount = (int)Math.Max(Math.Abs(dx), Math.Abs(dy));

        float xIncrement = dx / stepCount;
        float yIncrement = dy / stepCount;

        float x = firstPoint.X;
        float y = firstPoint.Y;

        for (int i = 0; i <= stepCount; i++)
        {
            byte* pixel = (byte*) data.Scan0
                           + (int)float.Round(y) * data.Stride + 
                           (int)float.Round(x) * _bytesPerPixel;
            
            if (pixel != null  && x < data.Width - 20 && x >= 0 && y < data.Height - 20 && y >= 0)
            {
                pixel[0] = _foregroundColor.B;
                pixel[1] = _foregroundColor.G;
                pixel[2] = _foregroundColor.R;
                pixel[3] = _foregroundColor.A;
                //Marshal.Copy(_colorRgbArray, 0, pixel, _colorRgbArray.Length);
            }
            
            x += xIncrement;
            y += yIncrement;
        }
    }

    private void Clear()
    {
        Color clearColor = Colors.Black;
        
        int stride = Bitmap.PixelWidth * _bytesPerPixel;
        
        byte[] pixels = new byte[Bitmap.PixelHeight * stride];
        
        for (int i = 0; i < pixels.Length; i += _bytesPerPixel)
        {
            pixels[i + 0] = clearColor.B;
            pixels[i + 1] = clearColor.G;
            pixels[i + 2] = clearColor.R;
            pixels[i + 3] = clearColor.A;
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

        Bitmap.Lock();
			 
        foreach (List<int> polygon in _model.Polygons)
        {
            for (int i = 0; i < polygon.Count - 1; i++)
            {
                int index1 = polygon[i];
                int index2 = polygon[i + 1];

                PointF point1 = pointsArray[index1 - 1];
                PointF point2 = pointsArray[index2 - 1];

                DrawLineDda(point1, point2);
            }

            int lastIndex = polygon[^1];
            int firstIndex = polygon[0];
				 
            PointF lastPoint = pointsArray[lastIndex - 1];
            PointF firstPoint = pointsArray[firstIndex - 1];
				 
            DrawLineDda(lastPoint, firstPoint);
        }
        
        Bitmap.Unlock();
    }

    public void Update()
    {
        Clear();
        
        Draw();
    }
}

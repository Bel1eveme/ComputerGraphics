using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ComputerGraphics.Algorithms;

using Rectangle = System.Drawing.Rectangle;

namespace ComputerGraphics.View;

public class Drawer
{
    public Bitmap Bitmap { get; }

    private readonly Color _foregroundColor;
    
    private readonly Color _backgroundColor;

    private readonly Model _model;

    private readonly byte[] _colorRgbArray;

    private readonly int _bytesInPixel;

    public Drawer(int width, int height, Color foregroundColor, Color backgroundColor, Model model)
    {
        _foregroundColor = foregroundColor;
        _backgroundColor = backgroundColor;
        Bitmap = new Bitmap(width, height);
        _model = model;

        _colorRgbArray = [_foregroundColor.B, _foregroundColor.G, _foregroundColor.R];
        _bytesInPixel = Image.GetPixelFormatSize(Bitmap.PixelFormat) / 8;
        
        Update();
    }

    private unsafe void DrawLineDda(BitmapData data, PointF firstPoint, PointF secondPoint)
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
                           (int)float.Round(x) * _bytesInPixel;
            
            if (pixel != null  && x < data.Width && x >= 0 && y < data.Height && y >= 0)
            {
                pixel[0] = _foregroundColor.B;
                pixel[1] = _foregroundColor.G;
                pixel[2] = _foregroundColor.R;
                pixel[3] = 255;
                //Marshal.Copy(_colorRgbArray, 0, pixel, _colorRgbArray.Length);
            }
            
            x += xIncrement;
            y += yIncrement;
        }
    }

    private void Clear()
    {
        using var graphics = Graphics.FromImage(Bitmap);
        
        graphics.Clear(_backgroundColor);
    }
    
    private void Draw()
    {
        PointF[] pointsArray = new PointF[_model.Vertices.Count];
        for (int i = 0; i < _model.Vertices.Count; i++)
        {
            pointsArray[i] = new PointF(_model.Vertices[i].X, _model.Vertices[i].Y);
        }
        
        BitmapData bitmapData = Bitmap.LockBits(new Rectangle(0, 0, Bitmap.Width, Bitmap.Height),
                                        ImageLockMode.WriteOnly, Bitmap.PixelFormat);
			 
        foreach (List<int> polygon in _model.Polygons)
        {
            for (int i = 0; i < polygon.Count - 1; i++)
            {
                int index1 = polygon[i];
                int index2 = polygon[i + 1];

                PointF point1 = pointsArray[index1 - 1];
                PointF point2 = pointsArray[index2 - 1];

                DrawLineDda(bitmapData, point1, point2);
            }

            int lastIndex = polygon[^1];
            int firstIndex = polygon[0];
				 
            PointF lastPoint = pointsArray[lastIndex - 1];
            PointF firstPoint = pointsArray[firstIndex - 1];
				 
            DrawLineDda(bitmapData, lastPoint, firstPoint);
        }
        
        Bitmap.UnlockBits(bitmapData);
    }

    public void Update()
    {
        Clear();
        
        Draw();
    }
}

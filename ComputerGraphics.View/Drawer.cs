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

    private readonly byte[] ColorRgbArray;

    private readonly int BytesInPixel;

    public Drawer(int width, int height, Color foregroundColor, Color backgroundColor, Model model)
    {
        _foregroundColor = foregroundColor;
        _backgroundColor = backgroundColor;
        Bitmap = new Bitmap(width, height);
        _model = model;

        ColorRgbArray = new[] { _foregroundColor.B, _foregroundColor.G, _foregroundColor.R };
        BytesInPixel = Image.GetPixelFormatSize(Bitmap.PixelFormat) / 8;
        
        Update();
    }

    private void DrawLineDda(BitmapData data, PointF point1, PointF point2)
    {
        const int bitsInByte = 8;
        
        float dx = point2.X - point1.X;
        float dy = point2.Y - point1.Y;

        int steps = (int)Math.Max(Math.Abs(dx), Math.Abs(dy));

        float xIncrement = dx / steps;
        float yIncrement = dy / steps;

        float x = point1.X;
        float y = point1.Y;

        for (int i = 0; i <= steps; i++)
        {
            if (x < data.Width && x >= 0 && y < data.Height && y >= 0)
            {
                IntPtr pixel = data.Scan0
                              + (int)float.Round(y) * data.Stride + 
                              (int)float.Round(x) * BytesInPixel;
                
                Marshal.Copy(ColorRgbArray, 0, pixel, ColorRgbArray.Length);
            }
            x += xIncrement;
            y += yIncrement;    
        }
    }

    private void Clear()
    {
        using Graphics g = Graphics.FromImage(Bitmap);
        
        g.Clear(_backgroundColor);
        
        //g.DrawLine(new Pen(_foregroundColor), new Point(1, 1), new Point(1000, 1000));
    }
    
    private void Draw()
    {
        PointF[] pointsArray = new PointF[_model.Vertices.Count];
        for (int i = 0; i < _model.Vertices.Count; i++)
        {
            pointsArray[i] = new PointF(_model.Vertices[i].X, _model.Vertices[i].Y);
        }
        
        BitmapData bData = Bitmap.LockBits(new Rectangle(0, 0, Bitmap.Width, Bitmap.Height),
            
            ImageLockMode.ReadWrite, Bitmap.PixelFormat); // writeOnly
			 
        foreach (List<int> polygon in _model.Polygons)
        {
            for (int i = 0; i < polygon.Count - 1; i++)
            {
                int index1 = polygon[i];
                int index2 = polygon[i + 1];

                PointF point1 = pointsArray[index1 - 1];
                PointF point2 = pointsArray[index2 - 1];

                DrawLineDda(bData, point1, point2);
            }

            int lastIndex = polygon[^1];
            int firstIndex = polygon[0];
				 
            PointF lastPoint = pointsArray[lastIndex - 1];
            PointF firstPoint = pointsArray[firstIndex - 1];
				 
            DrawLineDda(bData, lastPoint, firstPoint);
        }
        
        Bitmap.UnlockBits(bData);
    }

    public void Update()
    {
        Clear();
        
        Draw();
    }
}

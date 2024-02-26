using System.Drawing;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ComputerGraphics.Algorithms;

namespace ComputerGraphics.View;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly Drawer _drawer;

    private readonly Model _model;

    private void Update()
    {
        _drawer.Update();
        //var bitmap = _drawer.Bitmap;
        ImageView.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            _drawer.Bitmap.GetHbitmap(),
            IntPtr.Zero,
            System.Windows.Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());
        
        ImageView.UpdateLayout();
    }
    
    public MainWindow()
    {
        InitializeComponent();

        //var pathToObjFile = "D:\\downloads\\FinalBaseMesh.obj";
        var pathToObjFile = @"D:\downloads\cube.obj";
        //var pathToObjFile = "D:\\downloads\\ImageToStl.com_datsun240k.obj";
        //var pathToObjFile = "D:\\downloads\\Napoleon.obj";
        
        _model = new Model(new ObjFileParser(pathToObjFile), new Converter(), (int) ImageView.Width, (int) ImageView.Height);
        
        _drawer = new Drawer((int)ImageView.Width, (int) ImageView.Height,
            Color.White, Color.Black, _model);
        
        Update();
    }

    private void WindowKeyDownEventHandler(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Right)
        {
            _model.RotateRight();
            
            Console.WriteLine("Up");
        }
        else if (e.Key == Key.Down)
        {
            _model.RotateDown();
            
            Console.WriteLine("Down");
        }
        
        Update();
    }

    private void MouseWheelScrollEventHandler(object sender, MouseWheelEventArgs e)
    {
        if (e.Delta > 0)
        {
            _model.ChangeScalingCoefficient(0.001f);
        }
        else
        {
            _model.ChangeScalingCoefficient(-0.001f);
        }
        
        Update();
    }
    
}
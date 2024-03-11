using System.Windows.Input;
using System.Windows.Media;
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
    }
    
    public MainWindow()
    {
        InitializeComponent();

        //var pathToObjFile = "D:\\downloads\\FinalBaseMesh.obj";
        //var pathToObjFile = @"D:\downloads\cube.obj";
        //var pathToObjFile = @"D:\downloads\ImageToStl.com_datsun240k.obj";
        var pathToObjFile = @"D:\downloads\Napoleon.obj";

        ObjFileParser parser = new (pathToObjFile);
        parser.ParseFile();
        
        Console.WriteLine("Parsed");
        
        _model = new Model(parser, new Converter(), (int)ImageView.Width, (int)ImageView.Height);
        
        Console.WriteLine("Model created");
        
        _drawer = new Drawer((int)ImageView.Width, (int)ImageView.Height, Colors.White, Colors.Black, _model);
        
        Update();
        
        Console.WriteLine("Model drawn");

        ImageView.Source = _drawer.Bitmap;
    }

    private void WindowKeyDownEventHandler(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Q:
                _model.RotateXNeg();
            
                Console.WriteLine("Rotation X-");
                break;
            case Key.W:
                _model.RotateXPos();
            
                Console.WriteLine("Rotation X+");
                break;
            
            case Key.A:
                _model.RotateYNeg();
            
                Console.WriteLine("Rotation Y-");
                break;
            case Key.S:
                _model.RotateYPos();
            
                Console.WriteLine("Rotation Y+");
                break;
            
            case Key.Z:
                _model.RotateZNeg();
            
                Console.WriteLine("Rotation Z-");
                break;
            case Key.X:
                _model.RotateZPos();
            
                Console.WriteLine("Rotation Z+");
                break;
            
            case Key.OemPlus:
                _model.ChangeStep(0.1f);
            
                Console.WriteLine("Step increased");
                break;
            case Key.OemMinus:
                _model.ChangeStep(-0.1f);
            
                Console.WriteLine("Step decreased");
                break;
            default:
                Console.WriteLine("Unknown operation");
                
                break;
        }

        Update();
    }

    private void MouseWheelScrollEventHandler(object sender, MouseWheelEventArgs e)
    {
        if (e.Delta > 0)
        {
            _model.ChangeScalingCoefficient(0.0001f);
            
            Console.WriteLine("Scaling increased");
        }
        else
        {
            _model.ChangeScalingCoefficient(-0.0001f);
            
            Console.WriteLine("Scaling decreased");
        }
        
        Update();
    }
    
}
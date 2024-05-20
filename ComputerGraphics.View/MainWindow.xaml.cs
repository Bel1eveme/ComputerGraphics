using System.Numerics;
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
    private readonly Converter _converter;
    private Vector3 angle = Vector3.Zero;
    private float scale = 0.03f;
    private const float scale_step = 0.01f;
    private Vector3 move = Vector3.Zero;
    private float step = 0.2f;
    

    private void Update()
    {
        _drawer.Update();
    }
    
    public MainWindow()
    {
        InitializeComponent();

        //var pathToObjFile = "D:\\Downloads\\ComputerGraphics-main\\objects\\Model.obj";
        var pathToObjFile = "D:\\Downloads\\ComputerGraphics-main\\objects\\1\\plane.obj";
        
        //var pathToObjFile = @"D:\downloads\cube.obj";
        //var pathToObjFile = @"D:\downloads\ImageToStl.com_datsun240k.obj";
        //var pathToObjFile = @"D:\Downloads\teamugobj.j";

        ObjFileParser parser = new (pathToObjFile);
        // parser.ParseFile("D:\\Downloads\\ComputerGraphics-main\\objects\\diffuse.png",
        //     "D:\\Downloads\\ComputerGraphics-main\\objects\\specular.png",
        //     "D:\\Downloads\\ComputerGraphics-main\\objects\\normal.png");
        
        parser.ParseFile("D:\\Downloads\\ComputerGraphics-main\\objects\\1\\diffuseMap.png",
            "D:\\Downloads\\ComputerGraphics-main\\objects\\1\\reflectMap.png",
            "D:\\Downloads\\ComputerGraphics-main\\objects\\1\\normalMap.png");
        
        Console.WriteLine("Parsed");
        _converter = new Converter();
        _model = new Model(parser, _converter, (int)ImageView.Width, (int)ImageView.Height);
        _model.Update(angle, scale, move);
        Console.WriteLine("Model created");
        
        _drawer = new Drawer((int)ImageView.Width, (int)ImageView.Height, Colors.White, Colors.Black, _model, _converter);
        
        Update();
        
        Console.WriteLine("Model drawn");

        ImageView.Source = _drawer.Bitmap;
    }

    private void WindowKeyDownEventHandler(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.E:
                _model._source.X -= 5f;
            
                Console.WriteLine(_model._source.X);
                break;
            case Key.R:
                _model._source.Y += 5f;
            
                Console.WriteLine("Source Y+");
                break;
            case Key.T:
                _model._source.Z += 5f;
            
                Console.WriteLine("Source Z+");
                break;
            case Key.D:
                _model._source.X += 5f;
            
                Console.WriteLine(_model._source.X);
                break;
            case Key.F:
                _model._source.Y -= 5f;
            
                Console.WriteLine("Source Y-");
                break;
            case Key.G:
                _model._source.Z -= 5f;
            
                Console.WriteLine("Source Z-");
                break;
            case Key.Q:
                angle.X -= step;
                Console.WriteLine("Rotation X-");
                break;
            case Key.W:
                angle.X += step;
                Console.WriteLine("Rotation X+");
                break;
            
            case Key.A:
                angle.Y -= step;
                Console.WriteLine("Rotation Y-");
                break;
            case Key.S:
                angle.Y += step;
                Console.WriteLine("Rotation Y+");
                break;
            
            case Key.Z:
                angle.Z -= step;
                Console.WriteLine("Rotation Z-");
                break;
            case Key.X:
                angle.Z += step;
                Console.WriteLine("Rotation Z+");
                break;
            
            case Key.OemPlus:
                step += 0.1f;
                Console.WriteLine("Step increased");
                break;
            case Key.OemMinus:
                step -= 0.1f;
                Console.WriteLine("Step decreased");
                break;
            case Key.Left:
                move.X += step;
                Console.WriteLine("X pos");
                break;
            case Key.Right:
                move.X -= step;
                Console.WriteLine("X neg");
                break;
            case Key.Up:
                move.Y += step;
                Console.WriteLine("Y pos");
                break;
            case Key.Down:
                move.Y -= step;
                Console.WriteLine("Y neg");
                break;
            case Key.Add:
                scale += scale_step;
                break;
            case Key.Subtract:
                scale -= scale_step;
                break;
            default:
                Console.WriteLine("Unknown operation");
                
                break;
        }
        _model.Update(angle, scale, move);
        Update();
    }

    
    
}
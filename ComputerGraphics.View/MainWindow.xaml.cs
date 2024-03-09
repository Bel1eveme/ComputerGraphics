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
        
        ImageView.UpdateLayout();
    }
    
    public MainWindow()
    {
        InitializeComponent();

        //var pathToObjFile = "D:\\downloads\\FinalBaseMesh.obj";
        //var pathToObjFile = @"D:\downloads\cube.obj";
        //var pathToObjFile = "D:\\downloads\\ImageToStl.com_datsun240k.obj";
        var pathToObjFile = "D:\\downloads\\Napoleon.obj";


        ObjFileParser parser = new ObjFileParser(pathToObjFile);
        parser.ParseFile();

        
        Console.WriteLine("Parsed");
        
        
        _model = new Model(parser, new Converter(), (int)ImageView.Width - 30, (int)ImageView.Height - 30);
        
        _drawer = new Drawer((int)ImageView.Width, (int)ImageView.Height, Colors.White, Colors.Black, _model);
        
        Update();

        ImageView.Source = _drawer.Bitmap;
    }

    private void WindowKeyDownEventHandler(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Right)
        {
            _model.RotateRight();
            
            Console.WriteLine("Right");
        }
        else if (e.Key == Key.Left)
        {
            _model.RotateLeft();
            
            Console.WriteLine("Left");
        }
        else if (e.Key == Key.Down)
        {
            _model.RotateDown();
            
            Console.WriteLine("Down");
        }
        else if (e.Key == Key.Up)
        {
            _model.RotateUp();
            
            Console.WriteLine("Up");
        }
        else if (e.Key == Key.A)
        {
            _model.MoveLeft();
            
            Console.WriteLine("To left");
        }
        else if (e.Key == Key.D)
        {
            _model.MoveRight();
            
            Console.WriteLine("To right");
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
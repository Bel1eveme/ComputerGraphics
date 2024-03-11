using System.Runtime.InteropServices;
using System.Windows.Media;
using BenchmarkDotNet.Attributes;

namespace ComputerGraphics.Benchmarks;

public class CopyVsAssign
{
    private byte[] _pixels;

    private IntPtr _pointerToPixels;

    private Color1 _color;

    private byte[] _colorArray;

    private int _bytesPerPixel;
    
    [Params(1000, 10000)]
    public int N;

    [GlobalSetup]
    public void Setup()
    {
        _pixels = new byte[N];
        _color = new Color1();
        _colorArray = [5, 5, 5, 255];
        _pointerToPixels = Marshal.AllocHGlobal(N);
        _bytesPerPixel = 8;
    }

    [Benchmark]
    public byte[] GetArrayWithCopy()
    {
        for (int i = 0; i < _pixels.Length; i += _bytesPerPixel)
        {
            Marshal.Copy(_colorArray, 0, _pointerToPixels, _colorArray.Length);
        }
        
        return _pixels;
    }

    [Benchmark]
    public byte[] GetArrayWithAssign()
    {
        for (int i = 0; i < _pixels.Length; i += _bytesPerPixel)
        {
            _pixels[i + 0] = _color.n1;
            _pixels[i + 1] = _color.n2;
            _pixels[i + 2] = _color.n3;
            _pixels[i + 3] = _color.n4;
        }
        
        return _pixels;
    }

    class Color1
    {
        public byte n1 = 5;
        
        public byte n2 = 5;
        
        public byte n3 = 5;
        
        public byte n4 = 5;
    }
}
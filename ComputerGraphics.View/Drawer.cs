using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ComputerGraphics.Algorithms;
using Color = System.Windows.Media.Color;
using Vector = System.Windows.Vector;

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
    private float[,] _zBuffer;
    private readonly Vector3 _eye = new(1f, 1f, -MathF.PI);
    private readonly Converter _converter;
    private readonly Vector3 ambientColor = new Vector3(201, 208, 95);
    private readonly Vector3 diffuseColor = new Vector3(201, 208, 95);
    private readonly Vector3 specularColor = new Vector3(255, 255, 255);
    private readonly float ambientCoef = 0.2f;
    private readonly float diffuseCoef = 1f;
    private readonly float specularCoef = 0.3f;
    private readonly float shininess = 100f;
    

    public Drawer(int width, int height, Color foregroundColor, Color backgroundColor, Model model, Converter converter)
    {
        _converter = converter;
        _foregroundColor = foregroundColor;
        _backgroundColor = backgroundColor;
        Bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
        _model = model;
        _zBuffer = new float[Bitmap.PixelHeight, Bitmap.PixelWidth];
        
        _bytesPerPixel = (Bitmap.Format.BitsPerPixel + 7) / 8;
        
        Update();
    }
    

    private void DrawPixel(int x, int y, Color color)
    {
        unsafe
        {
            byte* data = (byte*)Bitmap.BackBuffer + y * Bitmap.BackBufferStride + x * _bytesPerPixel;
            if (x >= 0 && x < Bitmap.PixelWidth && y >= 0 && y < Bitmap.PixelHeight)
            {
                data[0] = color.B;
                data[1] = color.G;
                data[2] = color.R;
                data[3] = color.A;
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
    

    int ToInterval(float value, int max, int min = 0)
    {
        int result = Math.Max(min, (int)MathF.Round(value));
        if (result > max)
        {
            result = max;
        }
        return result;
    }

    private Vector3 AmbientLighting(Vector3 color)
    {
	    Vector3 ambient = new();
	    ambient.X = Math.Min(color.X * ambientCoef, 255);
	    ambient.Y = Math.Min(color.Y * ambientCoef, 255);
	    ambient.Z = Math.Min(color.Z * ambientCoef, 255);
	    return ambient;
    }

    private Vector3 DiffuseLighting(Vector3 normal, Vector3 lightVector, Vector3 color)
    {
	    Vector3 diffuse = new();
	    diffuse.X = Math.Min(color.X * diffuseCoef * Math.Max(0, Vector3.Dot(lightVector, normal)),255);
	    diffuse.Y = Math.Min(color.Y * diffuseCoef * Math.Max(0,  Vector3.Dot(lightVector, normal)),255);
	    diffuse.Z = Math.Min(color.Z * diffuseCoef * Math.Max(0,  Vector3.Dot(lightVector, normal)),255);
	    return diffuse;
    }

    private Vector3 SpecularLighting(Vector3 lightVector, Vector3 normal, Vector3 pointWorld, Vector3 color)
    {
	    var reflected = Vector3.Normalize(Vector3.Reflect(-lightVector, normal));
	    //var reflected = lightVector - 2 * Vector3.Dot(-lightVector, normal) * normal;
	    var RV = Math.Max(0, Vector3.Dot(reflected, Vector3.Normalize(_eye - pointWorld)));
	    Vector3 specular = new();
	    specular.X = Math.Min(color.X * specularCoef * MathF.Pow(RV, shininess),255);
	    specular.Y = Math.Min(color.Y * specularCoef * MathF.Pow(RV, shininess),255);
	    specular.Z = Math.Min(color.Z * specularCoef * MathF.Pow(RV, shininess),255);
	    return specular;


    }
    private Vector3 MultiplyColor(Vector3 color, float x)
    {
        color.X = Math.Min(color.X * x, 255);
        color.Y = Math.Min(color.Y * x, 255);
        color.Z = Math.Min(color.Z * x, 255);
        return color;
    }
    
    private Color Lighting(Vector3 center, Vector3 centerNormal)
    {
        var lightVector = Vector3.Normalize(_model._source - center);
        var intensity = Vector3.Dot(centerNormal, lightVector);
        return Color.Multiply(_foregroundColor, intensity);
    }

    
    public void Draw()
		{
			float?[,] zBuffer = new float?[Bitmap.PixelHeight, Bitmap.PixelWidth];

			int curr = -1;
			foreach (List<int> vector in _model.Polygons)
			{
				curr++;
				Vector4[] screenTriangle = 
				[ 
					_model.Vertices[vector[0]],
					_model.Vertices[vector[1]], 
					_model.Vertices[vector[2]] 
				];
				Vector3[] worldTriangle = 
				[ 
					new Vector3(_converter.WorldVertices[vector[0]].X, _converter.WorldVertices[vector[0]].Y, _converter.WorldVertices[vector[0]].Z),
					new Vector3(_converter.WorldVertices[vector[1]].X, _converter.WorldVertices[vector[1]].Y, _converter.WorldVertices[vector[1]].Z),
					new Vector3(_converter.WorldVertices[vector[2]].X, _converter.WorldVertices[vector[2]].Y, _converter.WorldVertices[vector[2]].Z)
				];
				Vector4 edge1 = screenTriangle[2] - screenTriangle[0];
				Vector4 edge2 = screenTriangle[1] - screenTriangle[0];
				if (edge1.X * edge2.Y - edge1.Y * edge2.X <= 0)
				{
					continue;
				}
				Vector3 vertexNormal0 = new Vector3(
					Vector3.Normalize(_converter.WorldNormals[_model.PolygonsNormals[curr][0]]).X,
					Vector3.Normalize(_converter.WorldNormals[_model.PolygonsNormals[curr][0]]).Y,
					Vector3.Normalize(_converter.WorldNormals[_model.PolygonsNormals[curr][0]]).Z);
				Vector3 vertexNormal1 = new Vector3(
					Vector3.Normalize(_converter.WorldNormals[_model.PolygonsNormals[curr][1]]).X,
					Vector3.Normalize(_converter.WorldNormals[_model.PolygonsNormals[curr][1]]).Y,
					Vector3.Normalize(_converter.WorldNormals[_model.PolygonsNormals[curr][1]]).Z);
				Vector3 vertexNormal2 = new Vector3(
					Vector3.Normalize(_converter.WorldNormals[_model.PolygonsNormals[curr][2]]).X,
					Vector3.Normalize(_converter.WorldNormals[_model.PolygonsNormals[curr][2]]).Y,
					Vector3.Normalize(_converter.WorldNormals[_model.PolygonsNormals[curr][2]]).Z);
				
				var texture0 = _model.textures[_model.PolygonsTextures[curr][0]] / screenTriangle[0].Z;
				var texture1 = _model.textures[_model.PolygonsTextures[curr][1]] / screenTriangle[1].Z;
				var texture2 = _model.textures[_model.PolygonsTextures[curr][2]] / screenTriangle[2].Z;

				var r0 = 1 / screenTriangle[0].Z;
				var r1 = 1 / screenTriangle[1].Z;
				var r2 = 1 / screenTriangle[2].Z;
				
				if (screenTriangle[0].Y > screenTriangle[1].Y)
				{
					(screenTriangle[0], screenTriangle[1]) = (screenTriangle[1], screenTriangle[0]);
					(worldTriangle[0], worldTriangle[1]) = (worldTriangle[1], worldTriangle[0]);
					(vertexNormal0, vertexNormal1) = (vertexNormal1, vertexNormal0);
					(texture0, texture1) = (texture1, texture0);
					(r0, r1) = (r1, r0);
				}
				if (screenTriangle[0].Y > screenTriangle[2].Y)
				{
					(screenTriangle[0], screenTriangle[2]) = (screenTriangle[2], screenTriangle[0]);
					(worldTriangle[0], worldTriangle[2]) = (worldTriangle[2], worldTriangle[0]);
					(vertexNormal0, vertexNormal2) = (vertexNormal2, vertexNormal0);
					(texture0, texture2) = (texture2, texture0);
					(r0, r2) = (r2, r0);
				}
				if (screenTriangle[1].Y > screenTriangle[2].Y)
				{
					(screenTriangle[1], screenTriangle[2]) = (screenTriangle[2], screenTriangle[1]);
					(worldTriangle[1], worldTriangle[2]) = (worldTriangle[2], worldTriangle[1]);
					(vertexNormal1, vertexNormal2) = (vertexNormal2, vertexNormal1);
					(texture1, texture2) = (texture2, texture1);
					(r1, r2) = (r2, r1);
				}
				Vector4 screenKoeff01 = (screenTriangle[1] - screenTriangle[0]) / (screenTriangle[1].Y - screenTriangle[0].Y);
				Vector3 worldKoeff01 = (worldTriangle[1] - worldTriangle[0]) / (screenTriangle[1].Y - screenTriangle[0].Y);
				Vector3 vertexNormalKoeff01 = (vertexNormal1 - vertexNormal0) / (screenTriangle[1].Y - screenTriangle[0].Y);
				Vector2 textureKoeff01 = (texture1 - texture0 )/ (screenTriangle[1].Y - screenTriangle[0].Y);
				var rKoef01 = (r1 - r0) / (screenTriangle[1].Y - screenTriangle[0].Y);

				Vector4 screenKoeff02 = (screenTriangle[2] - screenTriangle[0]) / (screenTriangle[2].Y - screenTriangle[0].Y);
				Vector3 worldKoeff02 = (worldTriangle[2] - worldTriangle[0]) / (screenTriangle[2].Y - screenTriangle[0].Y);
				Vector3 vertexNormalKoeff02 = (vertexNormal2 - vertexNormal0) / (screenTriangle[2].Y - screenTriangle[0].Y);
				Vector2 textureKoeff02      = (texture2 - texture0) / (screenTriangle[2].Y - screenTriangle[0].Y);
				var rKoef02 = (r2 - r0) / (screenTriangle[2].Y - screenTriangle[0].Y);

				Vector4 screenKoeff03 = (screenTriangle[2] - screenTriangle[1]) / (screenTriangle[2].Y - screenTriangle[1].Y);
				Vector3 worldKoeff03 = (worldTriangle[2] - worldTriangle[1]) / (screenTriangle[2].Y - screenTriangle[1].Y);
				Vector3 vertexNormalKoeff03 = (vertexNormal2 - vertexNormal1) / (screenTriangle[2].Y - screenTriangle[1].Y);
				Vector2 textureKoeff03      = (texture2 - texture1 )/ (screenTriangle[2].Y - screenTriangle[1].Y);
				var rKoef03 = (r2 - r1) / (screenTriangle[2].Y - screenTriangle[1].Y);

				int minY = Math.Max((int)MathF.Ceiling(screenTriangle[0].Y), 0);
				int maxY = Math.Min((int)MathF.Ceiling(screenTriangle[2].Y), Bitmap.PixelHeight);

				for (int y = minY; y < maxY; y++)
				{
					Vector4 screenA = y < screenTriangle[1].Y ? screenTriangle[0] + (y - screenTriangle[0].Y) * screenKoeff01 :
																 screenTriangle[1] + (y - screenTriangle[1].Y) * screenKoeff03;
					Vector4 screenB = screenTriangle[0] + (y - screenTriangle[0].Y) * screenKoeff02;

					Vector3 worldA = y < screenTriangle[1].Y ? worldTriangle[0] + (y - screenTriangle[0].Y) * worldKoeff01 :
																worldTriangle[1] + (y - screenTriangle[1].Y) * worldKoeff03;
					Vector3 worldB = worldTriangle[0] + (y - screenTriangle[0].Y) * worldKoeff02;

					Vector3 normalA = y < screenTriangle[1].Y ? vertexNormal0 + (y - screenTriangle[0].Y) * vertexNormalKoeff01 :
															   vertexNormal1 + (y - screenTriangle[1].Y) * vertexNormalKoeff03;
					Vector3 normalB = vertexNormal0 + (y - screenTriangle[0].Y) * vertexNormalKoeff02;
					
					Vector2 textureA = y < screenTriangle[1].Y ? texture0 + (y - screenTriangle[0].Y) * textureKoeff01 :
						texture1 + (y - screenTriangle[1].Y) * textureKoeff03;
					Vector2 textureB = texture0 + (y - screenTriangle[0].Y) * textureKoeff02;
					var rA = y < screenTriangle[1].Y ? r0 + (y - screenTriangle[0].Y) * rKoef01 :
						r1 + (y - screenTriangle[1].Y) * rKoef03;
					var rB = r0 + (y - screenTriangle[0].Y) * rKoef02;

					if (screenA.X > screenB.X)
					{
						(screenA, screenB) = (screenB, screenA);
						(worldA, worldB) = (worldB, worldA);
						(normalA, normalB) = (normalB, normalA);
						(textureA, textureB) = (textureB, textureA);
						(rA, rB) = (rB, rA);
					}

					int minX = Math.Max((int)MathF.Ceiling(screenA.X), 0);
					int maxX = Math.Min((int)MathF.Ceiling(screenB.X), Bitmap.PixelWidth);

					Vector4 screenKoeff = (screenB - screenA) / (screenB.X - screenA.X);
					Vector3 worldKoeff = (worldB - worldA) / (screenB.X - screenA.X);
					Vector3 normalKoeff = (normalB - normalA) / (screenB.X - screenA.X);
					Vector2 textureKoeff = (textureB - textureA) / (screenB.X - screenA.X);
					var rKoef = (rB - rA) / (screenB.X - screenA.X);

					for (int x = minX; x < maxX; x++)
					{ 
						Vector4 pointScreen = screenA + (x - screenA.X) * screenKoeff;
						Vector3 pointWorld = worldA + (x - screenA.X) * worldKoeff;
						if (!(pointScreen.Z > zBuffer[y, x]))
						{
							zBuffer[y, x] = pointScreen.Z;
							Vector3 lightDirection = Vector3.Normalize(_model.LightSource - pointWorld);
							Vector2 texture = textureA + (x - screenA.X) * textureKoeff;
							var r = rA + (x - screenA.X) * rKoef;
							texture /= r;
							System.Drawing.Color objColor = _model.textureFile.GetPixel(Convert.ToInt32(texture.X * (_model.textureFile.Width - 1)), Convert.ToInt32((1 - texture.Y) * (_model.textureFile.Height - 1)));
							var color = new Vector3(objColor.R, objColor.G, objColor.B);
							System.Drawing.Color spcColor = _model.mirrorMap.GetPixel(Convert.ToInt32(texture.X * (_model.mirrorMap.Width - 1)), Convert.ToInt32((1 - texture.Y) * (_model.mirrorMap.Height - 1)));
							var specular = new Vector3(spcColor.R, spcColor.G, spcColor.B);
							//System.Drawing.Color normalColor = _model.normalMap.GetPixel(Convert.ToInt32(texture.X * (_model.normalMap.Width - 1)), Convert.ToInt32((1 - texture.Y) * (_model.normalMap.Height - 1)));
							//var normal = new Vector3(normalColor.R/255f, normalColor.G/255f, normalColor.B/255f);
							Vector3 normal = Vector3.One;
							normal = normalA + (x - screenA.X) * normalKoeff;
							//normal = normal * 2 - Vector3.One;
							normal = Vector3.Normalize(normal);
							var ambientLightingColor = AmbientLighting(color);
							var diffuseLightingColor = DiffuseLighting(normal, -lightDirection, color);
							var specularLightingColor = SpecularLighting(-lightDirection, normal, pointWorld, specular);
							var resColor = ambientLightingColor + diffuseLightingColor + specularLightingColor;
							resColor.X = Math.Min(resColor.X, 255);
							resColor.Y = Math.Min(resColor.Y, 255);
							resColor.Z = Math.Min(resColor.Z, 255);
							DrawPixel(x, y, Color.FromRgb((byte)resColor.X, (byte)resColor.Y, (byte)resColor.Z));
						}
					}
				}
			}

		}
    
    public void Update()
    {
        Clear();
        
        Draw();
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SFML.Graphics;

namespace imgconvert
{
    class Program
    {
        static Vector3f[] palVec;

        static void Main(string[] args)
        {
            var palette = new Image("palette.png");
            palVec = new Vector3f[256];

            for (uint x = 0; x < palVec.Length; x++)
            {
                palVec[x] = (Vector3f)palette.GetPixel(x, 0);
            }

            var image = new Image("test.png");
            var imgVec = new Vector3f[image.Size.X, image.Size.Y];
            var imgRes = new byte[image.Size.X, image.Size.Y];

            for (uint y = 0; y < image.Size.Y; y++)
            {
                for (uint x = 0; x < image.Size.X; x++)
                {
                    imgVec[x, y] = (Vector3f)image.GetPixel(x, y);
                }
            }

            for (uint y = 0; y < image.Size.Y; y++)
            {
                for (uint x = 0; x < image.Size.X; x++)
                {
                    if (x == 0 || y == 0 || x == image.Size.X - 1 || y == image.Size.Y - 1)
                    {
                        imgRes[x, y] = FindClosestColor(imgVec[x, y]);
                        continue;
                    }

                    var oldPix = imgVec[x, y];
                    var newCol = FindClosestColor(oldPix);
                    var newPix = palVec[newCol];
                    var error = oldPix - newPix;

                    imgRes[x, y] = newCol;

                    imgVec[x + 1, y + 0] += error * (7.0f / 16.0f);
                    imgVec[x - 1, y + 1] += error * (3.0f / 16.0f);
                    imgVec[x + 0, y + 1] += error * (5.0f / 16.0f);
                    imgVec[x + 1, y + 1] += error * (1.0f / 16.0f);
                }
            }

            for (uint y = 0; y < image.Size.Y; y++)
            {
                for (uint x = 0; x < image.Size.X; x++)
                {
                    image.SetPixel(x, y, (Color)palVec[imgRes[x, y]]);
                }
            }

            image.SaveToFile("out.png");

            var o = File.OpenWrite("out.pic");
            for (uint y = 0; y < image.Size.Y; y++)
            {
                for (uint x = 0; x < image.Size.X; x++)
                {
                    o.WriteByte(imgRes[x, y]);
                }
            }
            o.Dispose();
        }

        static byte FindClosestColor(Vector3f col)
        {
            byte closest = 0;
            float closestError = float.MaxValue;

            for (var i = 0; i < palVec.Length; i++)
            {
                var dr =  col.R - palVec[i].R;
                var dg = col.G - palVec[i].G;
                var db =  col.B - palVec[i].B;
                var error = dr * dr + dg * dg + db * db;

                if (error < closestError)
                {
                    closestError = error;
                    closest = (byte)i;
                }
            }

            return closest;
        }
    }

    public struct Vector3f
    {
        public float R, G, B;

        public Vector3f(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }

        public static Vector3f operator +(Vector3f v1, Vector3f v2)
        {
            return new Vector3f(v1.R + v2.R, v1.G + v2.G, v1.B + v2.B);
        }

        public static Vector3f operator -(Vector3f v1, Vector3f v2)
        {
            return new Vector3f(v1.R - v2.R, v1.G - v2.G, v1.B - v2.B);
        }

        public static Vector3f operator *(Vector3f v, float div)
        {
            return new Vector3f(v.R * div, v.G * div, v.B * div);
        }

        public static explicit operator Color(Vector3f v)
        {
            return new Color((byte)(v.R * 256), (byte)(v.G * 256), (byte)(v.B * 256));
        }

        public static explicit operator Vector3f(Color c)
        {
            return new Vector3f((float)c.R / 256, (float)c.G / 256, (float)c.B / 256);
        }
    }
}

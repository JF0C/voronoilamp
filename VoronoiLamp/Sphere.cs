using System;
using System.Collections.Generic;
using System.Numerics;

namespace VoronoiLamp
{
    public class Sphere : I3dObject
    {
        private float radius;
        private string material;
        private int slices;
        private Vector3 origin = new Vector3(0, 0, 0);

        public Sphere(float r, int slices = 9)
        {
            radius = r;
            this.slices = slices;
            Render();
        }

        private void Render()
        {
            Verteces = new List<Vector3>();
            Triangles = new List<Triangle>();
            if(slices % 2 == 0)
            {
                slices += 1;
            }
            if(slices < 3)
            {
                slices = 3;
            }
            var slicePoints = new List<int>();
            for(var k = 0; k < slices; k++)
            {
                if(k < (slices + 1) / 2)
                {
                    if (k == 0) { slicePoints.Add(1); }
                    else if (k == 1) { slicePoints.Add(6); }
                    else { slicePoints.Add(slicePoints[k - 1] * 2); }
                }
                else
                {
                    slicePoints.Add(slicePoints[slices - k - 1]);
                }
            }

            var upperSlice = new List<Vector3>() { new Vector3(0, 0, Radius) + origin };
            Verteces.AddRange(upperSlice);
            for(var k = 1; k < slicePoints.Count; k++)
            {
                var phi = Math.PI * (k / (double)(slicePoints.Count - 1));
                var h = (float)Math.Cos(phi) * Radius;
                var lowerSlice = new List<Vector3>();
                var lastSlice = slicePoints[k - 1];
                var currSlice = slicePoints[k];
                var rl = (float)Math.Sin(phi) * Radius;

                for (var l = 0; l < currSlice; l++)
                {
                    var alpha = 2.0 * Math.PI * (l / (double)currSlice);
                    var v = new Vector3((float)Math.Sin(alpha) * rl, (float)Math.Cos(alpha) * rl, h);
                    lowerSlice.Add(v + origin);
                }
                if (currSlice > slicePoints[k - 1])
                {
                    var sliceRatio = lastSlice / (double)currSlice;
                    for (var l = 0; l < currSlice; l++)
                    {
                        var topIndex = l * sliceRatio;
                        var top = upperSlice[(int)topIndex];
                        if (Math.Abs(Math.Floor(topIndex) - topIndex) < 0.0001f && slicePoints[k-1] > 1)
                        {
                            var ot = new Triangle(upperSlice[((int)topIndex + upperSlice.Count - 1) % upperSlice.Count],
                                top, lowerSlice[l], this);
                            Triangles.Add(ot);
                        }
                        var t = new Triangle(lowerSlice[(l + 1) % currSlice], lowerSlice[l], top, this);
                        Triangles.Add(t);
                    }
                }
                else
                {
                    var sliceRatio = currSlice / (double)lastSlice;
                    for (var l = 0; l < lastSlice; l++)
                    {
                        var botIndex = l * sliceRatio;
                        var bot = lowerSlice[(int)botIndex];
                        if(Math.Abs(Math.Floor(botIndex) - botIndex) < 0.0001f && slicePoints[k] > 1)
                        {
                            var lt = new Triangle(lowerSlice[(((int)botIndex) + lowerSlice.Count - 1) % lowerSlice.Count],
                                upperSlice[l], bot, this);
                            Triangles.Add(lt);
                        }
                        var t = new Triangle(upperSlice[l], upperSlice[(l + 1) % lastSlice], bot, this);
                        Triangles.Add(t);
                    }
                }
                Verteces.AddRange(lowerSlice);
                upperSlice = new List<Vector3>(lowerSlice);
            }

        }

        public float Radius
        {
            get => radius;
            set
            {
                radius = value;
                Render();
            }
        }

        public List<Vector3> Verteces { get; private set; }

        public List<Triangle> Triangles { get; private set; }

        public bool Hide { get; set; }

        public string Name { get; set; }
        public string Material
        {
            get => material;
            set
            {
                material = value;
                foreach(var t in Triangles)
                {
                    t.Material = material;
                }
            }
        }
        public void Move(Vector3 v)
        {
            origin += v;
            Render();
        }
        public Vector3 Center { get => origin; }

        public Tuple<Vector3, Vector3> PlaneIntersect(Vector3 origin, Vector3 normal)
        {
            throw new NotImplementedException();
        }
    }
}

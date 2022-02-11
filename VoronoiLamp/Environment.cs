using System;
using System.IO;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
namespace VoronoiLamp
{
    public class Environment
    {
        public List<I3dObject> Meshes { get; }

        public Environment()
        {
            Meshes = new List<I3dObject>();
        }
        public void AddCoord(float len = 1.0f)
        {
            var ux = PolygonSheet.Line(Vector3Util.Origin, Vector3Util.UX * len);
            ux.Material = "red";
            ux.Name = "x-coord";
            Meshes.Add(ux);
            var uy = PolygonSheet.Line(Vector3Util.Origin, Vector3Util.UY * len);
            uy.Name = "y-coord";
            uy.Material = "green";
            Meshes.Add(uy);
            var uz = PolygonSheet.Line(Vector3Util.Origin, Vector3Util.UZ * len);
            uz.Name = "z-coord";
            uz.Material = "blue";
            Meshes.Add(uz);

        }

        public void ToObj(string path)
        {
            Console.WriteLine("generating obj");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            var file = new StreamWriter(File.OpenWrite(path));
            if(MaterialLib != null)
            {
                file.WriteLine("mtllib " + MaterialLib);
            }
            var verteces = Meshes.Aggregate(new List<Vector3>(), (v, m) =>
            {
                if (!m.Hide)
                {
                    v.AddRange(m.Verteces);
                }
                return v;
            }).Distinct().ToList();

            var counter = 0;
            foreach(var v in verteces)
            {
                counter++;
                if(counter % 1000 == 0)
                {
                    Console.WriteLine("Verteces: " + counter + " / " + verteces.Count);
                }
                file.WriteLine($"v {v.X.ToString().Replace(',', '.')} {v.Y.ToString().Replace(',', '.')} {v.Z.ToString().Replace(',', '.')}");
            }

            var triangles = Meshes.Aggregate(new List<Triangle>(), (t, m) =>
            {
                if (!m.Hide)
                {
                    t.AddRange(m.Triangles);
                }
                return t;
            });
            var material = "";
            counter = 0;
            foreach (var trig in triangles)
            {
                counter++;
                if (counter % 1000 == 0)
                {
                    Console.WriteLine("Triangles: " + counter + " / " + triangles.Count);
                }
                if (trig.Material != material)
                {
                    file.WriteLine("usemtl " + trig.Material);
                    material = trig.Material;
                }
                file.Write("f");
                foreach (var vert in trig.Verteces)
                {
                    var found = false;
                    for (var k = 0; k < verteces.Count; k++)
                    {
                        if (Vector3.DistanceSquared(verteces[k], vert) < 0.0000001f)
                        {
                            found = true;
                            file.Write(" " + (k + 1));
                            break;
                        }
                    }
                    if (!found)
                    {
                        throw new Exception($"could not find vertex ({vert.X}, {vert.Y}, {vert.Z})");
                    }
                }
                file.WriteLine();
            }
            //return res;
            file.Close();
        }

        public string MaterialLib { get; set; }
    }
}

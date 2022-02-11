using System;
using System.IO;
using System.Numerics;
using System.Collections.Generic;
namespace VoronoiLamp
{
    public class TestIntersect
    {
        public TestIntersect()
        {
        }
        public void Run()
        {
            var env = new Environment();
            env.MaterialLib = "materials.mtl";
            env.AddCoord(0.1f);

            var holed = new HoleSheet(new List<Vector3>
            {
                new Vector3(0, 0, 0),
                new Vector3(0.1f, 0, 0),
                new Vector3(0.1f, 0.1f, 0),
                new Vector3(0, 0.1f, 0)
            }, new Vector3(0.025f, 0.025f, 0), 0.01f, 0.005f)
            {
                Material = "yellow"
            };
            //holed.Move(new Vector3(0.1f, 0, 0.1f));
            holed.Rotate(new Vector3(0, 0, 0.1f), new Vector3(1, 0, 0), (float)Math.PI / 2f);
            env.Meshes.Add(holed);
            //var planes = new List<PolygonSheet>();

            //var plane = PolygonSheet.Regular(5, 0.1f, 0.001f);
            //plane.Material = "yellowop";
            //plane.Move(new Vector3(0.1f, 0.1f, 0));
            //planes.Add(plane);
            //env.Meshes.Add(plane);

            //var p2 = new PolygonSheet(plane);
            //p2.Move(new Vector3(0.05f, 0f, 0.1f));
            //planes.Add(p2);
            //env.Meshes.Add(p2);

            //var p3 = new PolygonSheet(plane);
            //p3.Move(new Vector3(-0.1f, -0.1f, 0));
            //planes.Add(p3);
            //env.Meshes.Add(p3);

            //var p4 = new PolygonSheet(p3);
            //p4.Move(new Vector3(0, 0, 0.041f));
            //planes.Add(p4);
            //env.Meshes.Add(p4);

            //var point1 = new Vector3(0.02f, 0.02f, -0.05f);
            //var point2 = new Vector3(0.02f, 0.0f, 0.04f);
            //var line = PolygonSheet.Line(point1, point2);
            //line.Material = "white";
            //env.Meshes.Add(line);

            //foreach(var p in planes)
            //{
            //    if(p.LineItersects(point1, point2))
            //    {
            //        p.Material = "red";
            //    }
            //}

            env.ToObj("./../../intersects.obj");
        }
    }
}

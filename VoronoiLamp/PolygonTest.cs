using System;
using System.Numerics;
using System.Collections.Generic;
using System.IO;
namespace VoronoiLamp
{
    public class PolygonTest
    {
        public PolygonTest()
        {
        }
        public void Run()
        {
            var env = new Environment();
            env.MaterialLib = "materials.mtl";

            var hideWalls = true;
            var col0 = true;
            var surfThickness = 0.0006f;

            var verts = new List<Vector3>();
            verts.Add(new Vector3(0f, 0f, 0f));
            verts.Add(new Vector3(0f, 0.1f, 0f));
            verts.Add(new Vector3(0f, 0.1f, 0.2f));
            verts.Add(new Vector3(0f, 0f, 0.2f));

            var pol1 = new PolygonSheet(verts.ToArray(), 0.005f);
            pol1.Move(new Vector3(0.1f, 0f, 0f));
            pol1.Material = "redop";
            pol1.Hide = hideWalls;
            env.Meshes.Add(pol1);

            var pol2 = new PolygonSheet(verts.ToArray(), 0.005f);
            pol2.Rotate(pol2.Center, new Vector3(0f, 0f, 1f), (float)(Math.PI));
            pol2.Material = "redop";
            pol2.Hide = hideWalls;
            env.Meshes.Add(pol2);

            var pol3 = new PolygonSheet(verts.ToArray(), 0.005f);
            pol3.Rotate(new Vector3(), new Vector3(0, 0, 1), (float)(3 * Math.PI / 2));
            pol3.Material = "redop";
            pol3.Hide = hideWalls;
            env.Meshes.Add(pol3);

            var pol4 = new PolygonSheet(verts.ToArray(), 0.005f);
            pol4.Rotate(new Vector3(0f, 0.1f, 0f), new Vector3(0, 0, 1), (float)(Math.PI / 2));
            pol4.Material = "redop";
            pol4.Hide = hideWalls;
            //pol4.Rotate(pol4.Center, new Vector3(0, 0, 1), (float)(Math.PI));
            env.Meshes.Add(pol4);

            var polb = PolygonSheet.Regular(4, (float)(0.1 / Math.Sqrt(2.0)), 0.005f);
            polb.Move(new Vector3(0.05f, 0.05f, 0));
            polb.Rotate(polb.Center, new Vector3(0, 0, 1), (float)(Math.PI / 4.0));
            polb.Material = "white";
            env.Meshes.Add(polb);

            var polt = new PolygonSheet(polb);
            polt.Move(new Vector3(0f, 0f, 0.2f));
            polt.Material = "redop";
            polt.Hide = hideWalls;
            env.Meshes.Add(polt);

            var pillar1 = PolygonSheet.Regular(4, 0.002f, 0.2f);
            pillar1.Material = "grey";
            pillar1.Move(new Vector3(0, 0, 0.1f));
            pillar1.Intersect = false;
            pillar1.Rotate(pillar1.Center, new Vector3(0, 0, 1f), (float)(Math.PI / 4));
            env.Meshes.Add(pillar1);

            var pillar2 = new PolygonSheet(pillar1);
            pillar2.Move(new Vector3(0.1f, 0, 0));
            env.Meshes.Add(pillar2);

            var pillar3 = new PolygonSheet(pillar1);
            pillar3.Move(new Vector3(0.1f, 0.1f, 0));
            env.Meshes.Add(pillar3);

            var pillar4 = new PolygonSheet(pillar1);
            pillar4.Move(new Vector3(0, 0.1f, 0));
            env.Meshes.Add(pillar4);

            //var pillar1top = new PolygonSheet()

            var pol5 = PolygonSheet.Regular(4, 0.2f, surfThickness);
            pol5.Rotate(new Vector3(0.025f, 0f, 0.025f), new Vector3(1f, 0f, 0f), (float)(-Math.PI / 6));
            pol5.Move(new Vector3(0f, 0.05f, 0.1f));
            pol5.PlaneIntersect(pol2);
            pol5.PlaneIntersect(pol1);
            pol5.PlaneIntersect(pol4);
            pol5.PlaneIntersect(pol3);
            pol5.Material = "blue";
            env.Meshes.Add(pol5);

            var r = 0.15f;
            //var pol6 = PolygonSheet.Regular(6, r, 0.003f);
            //pol6.Material = "white";
            //pol6.Move(new Vector3(0.05f, 0.05f, 0.18f));
            //env.Meshes.Add(pol6);

            var pol7 = PolygonSheet.Regular(4, r, surfThickness);
            pol7.Material = "red";
            pol7.Move(new Vector3(r / 4, r / 4, 0.11f));
            pol7.Rotate(pol7.Center, new Vector3(1, 1, 0), (float)(Math.PI / 7));
            pol7.PlaneIntersect(pol2);
            pol7.PlaneIntersect(pol1);
            pol7.PlaneIntersect(pol4);
            pol7.PlaneIntersect(pol3);
            pol7.PlaneIntersect(pol5);
            env.Meshes.Add(pol7);


            var pol8 = PolygonSheet.Regular(4, r, surfThickness);
            pol8.Material = "green";
            pol8.Move(new Vector3(0.05f, 0.05f, 0.1f));
            pol8.Rotate(pol8.Center, new Vector3(1, -1, 0), (float)(Math.PI / 3));
            foreach (var m in env.Meshes)
            {
                var pol = m as PolygonSheet;
                if (pol != null)
                {
                    pol8.PlaneIntersect(pol);
                }
            }


            pol5.PlaneIntersect(pol7);
            pol5.PlaneIntersect(pol8);
            pol7.PlaneIntersect(pol8);
            pol7.PlaneIntersect(pol5);
            pol8.PlaneIntersect(pol7);
            pol8.PlaneIntersect(pol5);

            var funCircle = PolygonSheet.Regular(100, 0.05f, surfThickness);
            funCircle.Move(new Vector3(0.05f, 0.05f, 0.2f));
            funCircle.Material = "redop";
            env.Meshes.Add(funCircle);

            env.Meshes.Add(pol8);
            if (col0)
            {
                pol5.Material = "darkgrey";
                pol7.Material = "darkgrey";
                pol8.Material = "darkgrey";
            }

            env.ToObj("./../../mesh.obj");
        }
    }
}

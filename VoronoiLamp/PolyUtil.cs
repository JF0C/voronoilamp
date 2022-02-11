using System;
using System.Numerics;
using System.IO;
using Newtonsoft.Json;

namespace VoronoiLamp
{
    public class PolyUtil
    {
        public PolyUtil()
        {
        }
        public void Run()
        {
            var env = new Environment();
            env.MaterialLib = "materials.mtl";
            env.AddCoord(0.1f);
            /*
            var sphere = new Sphere(0.1f, 13);
            sphere.Material = "redop";
            env.Meshes.Add(sphere);
            */

            var param = new Config
            {
                Seeds = 100,
                Steps = 200,
                StepSize = 0.001f,
                MaxDist = 0.2f
            };
            if (File.Exists("../../config.json"))
            {
                param = JsonConvert.DeserializeObject<Config>(File.ReadAllText("../../config.json"));
            }
            var voronoiGen = new VoronoiGenerator(param, env);
            //env.Meshes.AddRange(voronoiGen.SurfGrid(10, 0.06f, 0.1f));
            voronoiGen.Run();
            voronoiGen.Render();

            /*
            var p1 = new Vector3(0, 0.1f, 0.1f);
            var p2 = new Vector3(0.2f, 0.05f, 0);

            var c1 = PolygonSheet.Cube(0.01f);
            c1.Move(p1);
            c1.Material = "red";
            c1.Name = "cube1";
            env.Meshes.Add(c1);

            var c2 = PolygonSheet.Cube(0.01f);
            c2.Move(p2);
            c2.Material = "red";
            c2.Name = "cube2";
            env.Meshes.Add(c2);

            var line = PolygonSheet.Line(p1, p2);
            line.Material = "white";
            line.Name = "line1";
            env.Meshes.Add(line);

            var cube = PolygonSheet.Cube(0.15f);
            cube.InvertNormals = true;
            cube.Material = "greyop";
            cube.Name = "bigcube";
            cube.Move(new Vector3(0, 0.1f, 0.1f));
            env.Meshes.Add(cube);
            */

            

            env.ToObj("./../../meshgs.obj");
        }
    }
}

using System;
using System.IO;
using System.Numerics;
using System.Collections.Generic;

namespace VoronoiLamp
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Voronoi Object Builder");
            //var polyTest = new PolygonTest();
            var polyTest = new PolyUtil();
            //var polyTest = new TestIntersect();
            polyTest.Run();
            
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;



/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance : GH_ScriptInstance
{
#region Utility functions
  /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
  /// <param name="text">String to print.</param>
  private void Print(string text) { /* Implementation hidden. */ }
  /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
  /// <param name="format">String format.</param>
  /// <param name="args">Formatting parameters.</param>
  private void Print(string format, params object[] args) { /* Implementation hidden. */ }
  /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj) { /* Implementation hidden. */ }
  /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj, string method_name) { /* Implementation hidden. */ }
#endregion

#region Members
  /// <summary>Gets the current Rhino document.</summary>
  private readonly RhinoDoc RhinoDocument;
  /// <summary>Gets the Grasshopper document that owns this script.</summary>
  private readonly GH_Document GrasshopperDocument;
  /// <summary>Gets the Grasshopper script component that owns this script.</summary>
  private readonly IGH_Component Component;
  /// <summary>
  /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
  /// Any subsequent call within the same solution will increment the Iteration count.
  /// </summary>
  private readonly int Iteration;
#endregion

  /// <summary>
  /// This procedure contains the user code. Input parameters are provided as regular arguments,
  /// Output parameters as ref arguments. You don't have to assign output parameters,
  /// they will have a default value.
  /// </summary>
  private void RunScript(Polyline poly, int levels, ref object koch, ref object segments)
  {
    //Algorithms
    Polyline kStar = KochStar(poly, levels);

    // Outputs
    koch = kStar;
    segments = kStar.SegmentCount;

  }

  // <Custom additional code> 
  //Get segment count property


  //Performs Koch Subdivision recursively based on number of levels
  Polyline KochStar(Polyline pol, int levels)
  {
    Polyline star = pol;


    for (int i = 0; i < levels; i++)
    {
      star = KochSubdivision(star);
    }

    return star;
  }

  //Performs Koch Subdivision on a Polyline
  Polyline KochSubdivision(Polyline pol)
  {
    int pointCount = pol.Count;

    Polyline sub = new Polyline(); //new empty polyline
    sub.Add(pol.First); //add first point of pol to sub

    //iterate over each edge of the polyline
    for (int i = 1; i < pointCount; i++)
    {
      Point3d start = pol[i - 1];
      Point3d end = pol[i];

      // Compute point at 1/3 of this segment
      Point3d p13 = PointAlongLine(start, end, 1 / 3.0);
      sub.Add(p13);

      // Compute point at perp triangle tip of this segment
      Vector3d axis = end - start;
      axis.Unitize();
      Vector3d norm = Vector3d.ZAxis;
      Vector3d perp = Vector3d.CrossProduct(axis, norm);
      double a = Distance2pt(start, end);
      double a3 = a / 3.0;
      double h = a3 * Math.Sqrt(3) / 2.0;
      perp = h * perp;
      Point3d mid = PointAlongLine(start, end, 0.5);
      mid += perp;
      sub.Add(mid);

      // Compute point at 2/3 of this segment
      Point3d p23 = PointAlongLine(start, end, 2 / 3.0);
      sub.Add(p23);

      // Compute point at end of this segment
      sub.Add(end);

    }

    return sub;
  }

  // Calculates Point along Line at relative distance
  Point3d PointAlongLine(Point3d a, Point3d b, double t)
  {
    Vector3d AB = b - a;
    Vector3d AC = t * AB;
    Point3d c = a + AC;

    return c;
  }

  //Calculates the distance between two points
  double Distance2pt(Point3d a, Point3d b)
  {
    double d = Math.Sqrt(((b.X - a.X) * (b.X - a.X)) + ((b.Y - a.Y) * (b.Y - a.Y)) + ((b.Z - a.Z) * (b.Z - a.Z)));
    return d;
  }

  // </Custom additional code> 
}

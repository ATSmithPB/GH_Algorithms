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
  private void RunScript(List<Point3d> pts, int n, ref object A)
  {


    pts.Add(pts[0]);
    var route = pts.ToArray();
    var shortest = Length(route);
    for (var k = 0; k < n; k++)
      for (var i = 1; i < route.Length - 1; i++)
        for (var j = i + 1; j < route.Length; j++)
          route = Swap(route, ref shortest, i, j);
    A = route;

  }

  // <Custom additional code> 

  double Length(Point3d[] route)
  {
    var d = 0.0;
    for(var i = 0; i < route.Length - 1; i++)
      d += route[i].DistanceTo(route[i + 1]);
    return d;
  }
  Point3d[] Swap(Point3d[] route, ref double shortest, int i, int j)
  {
    var swapped = (Point3d[]) route.Clone();
    Array.Reverse(swapped, i, j - i);
    var len = Length(swapped);
    if (len < shortest)
    {
      shortest = len;
      return swapped;
    }
    return route;
  }
  // </Custom additional code> 
}

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
  private void RunScript(Polyline poly, int levels, ref object sierpinski)
  {

    sierpinski = SierpinskiStar(poly, levels);
  }

  // <Custom additional code> 
  //Loops Sierpinski subdivision on a polyline

  List<Polyline> SierpinskiStar(Polyline pol, int levels){
    //Declare empty list of polylines
    List<Polyline> triangles = new List<Polyline>();

    //add origional polyline to start of list
    triangles.Add(pol);

    for (int i = 0; i < levels; i++){
      List <Polyline> subdivisions = new List<Polyline>();

      foreach (Polyline p in triangles){
        List<Polyline> subs = SierpinskiSubdivision(p);
        subdivisions.AddRange(subs);
      }

      triangles = subdivisions;

    }
    return triangles;
  }

  //Performs Sierpinski subdivision on a polyline
  List<Polyline> SierpinskiSubdivision(Polyline pol){

    List<Polyline> subdivisions = new List<Polyline>();

    for (int i = 0; i < pol.Count - 1; i++){
      Polyline pl = new Polyline();

      //Current and Next point variables for clarity
      Point3d currentPt = pol[i];
      Point3d nextPt = pol[i + 1];

      Point3d prevPt;
      if (i != 0){
        prevPt = pol[i - 1];
      }
      else{
        prevPt = pol[pol.Count - 2];
      }

      // Add current vertex we are checking
      pl.Add(currentPt);

      //Add middle points
      Point3d midNext = PointAlongLineRelative(currentPt, nextPt, 0.5);
      pl.Add(midNext);

      Point3d midPrev = PointAlongLineRelative(currentPt, prevPt, 0.5);
      pl.Add(midPrev);

      //Close polyline by adding the same point again
      pl.Add(currentPt);

      subdivisions.Add(pl);

    }

    return subdivisions;
  }

  // Point Along Line (Geometry Gem #7)
  Point3d PointAlongLineRelative(Point3d a, Point3d b, double t){
    //Vector Algebra method
    Vector3d AB = b - a;
    Vector3d AC = t * AB;
    Point3d c = a + AC;

    return c;
  }
  // </Custom additional code> 
}

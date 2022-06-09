using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System.Linq;

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
  private void RunScript(List<Point3d> pts, ref object Cpts, ref object Chull)
  {
    //Algorithm
    pts = pts.OrderBy(pt => pt.Y).ToList(); //Sorts points by Y
    pts = pts.OrderBy(pt => Math.Atan2(pt.Y - pts[0].Y, pt.X - pts[0].X)).ToList(); //arctangent of the quotent of the two arguments

    //Graham Scan
    List<Point3d> selPts = new List<Point3d>();

    while (pts.Count > 0)
    {
      GrahamScan(ref pts, ref selPts);
    }

    selPts.Add(selPts[0]);

    //Outputs
    Cpts = selPts;
    Chull = new Polyline(selPts);

  }

  // <Custom additional code> 
  void GrahamScan(ref List<Point3d> pts, ref List<Point3d> selPts)
  {
    if(pts.Count > 0) //if there are more than 0 points in list
    {
      var pt = pts[0]; //pt = first point in pts list

      if(selPts.Count <= 1)
      {
        selPts.Add(pt); //add first point in pts list to selPts
        pts.RemoveAt(0); //remove first point in pts from list
      }
      else
      {
        var pt1 = selPts[selPts.Count - 1]; //pt1 = last item in selPts list
        var pt2 = selPts[selPts.Count - 2]; //pt2 = second to last item in selPts list
        Vector3d dir1 = pt1 - pt2;
        Vector3d dir2 = pt - pt1;
        var cross = Vector3d.CrossProduct(dir1, dir2); //calc cross product

        if(cross.Z < 0)
        {
          selPts.RemoveAt(selPts.Count - 1);
        }
        else
        {
          selPts.Add(pt);
          pts.RemoveAt(0);
        }
      }
    }
  }
  // </Custom additional code> 
}

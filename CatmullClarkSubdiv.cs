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
  private void RunScript(Mesh MeshIn, int Levels, ref object MeshOut, ref object A, ref object B, ref object C, ref object D)
  {

    //for (int i = 0; i < Levels; i++){

    //Get Subdivided Points
    Point3d[] OrigPtsArray = MeshIn.Vertices.ToPoint3dArray();
    List<Point3d> OrigPts = OrigPtsArray.ToList();
    List<Point3d> facePts = GetMeshFacePts(MeshIn);
    List<Point3d> edgePts = GetMeshEdgePts(MeshIn);
    List<Point3d> baryPts = GetMeshBaryPts(MeshIn);

    //Construct Mesh from new points
    Mesh mSubD = new Mesh();
    for (int i = 0; i < MeshIn.Faces.Count; i++)
    {
      mSubD.Vertices.Add(facePts[i]);
      for (int j = 0; j < MeshIn.Faces.GetConnectedFaces(i).Length; j++)
        mSubD.Vertices.Add(edgePts[j]);

    }


    //}

    //Outputs
    A = facePts;
    B = edgePts;
    C = OrigPts;
    D = baryPts;
  }

  // <Custom additional code> 

  /// <summary>
  /// Returns a list of "Edge Points" per Catmull Clark method.
  /// </summary>
  /// <param name="m"></param>
  /// <returns></returns>
  public List<Point3d> GetMeshEdgePts(Mesh m){

    //Create empty list to store final edge points
    List<Point3d> edgePts = new List<Point3d>();
    //Calculate Edge Points and add them to Edge Point List
    for (int i = 0; i < m.TopologyEdges.Count; i++){
      //Calculate Midpoint of Edge
      Line edgeLine = m.TopologyEdges.EdgeLine(i);
      Point3d edgeMidPt = edgeLine.PointAt(0.5);

      //Calculate list of mesh face center points
      List<Point3d> facePts = new List<Point3d>();
      for (int f = 0; f < m.Faces.Count; f++)
      {
        Point3d facePt = m.Faces.GetFaceCenter(f);
        facePts.Add(facePt);
      }

      //Retrieve Face Points of connected Faces
      List<Point3d> conFacePts = new List<Point3d>();
      //Array of connected face indices
      int[] conFaceIndex = m.TopologyEdges.GetConnectedFaces(i);
      for (int j = 0; j < conFaceIndex.Length; j++){
        conFacePts.Add(facePts[conFaceIndex[j]]);
      }

      //Lists to contain XYZs
      List<double> kX = new List<double>();
      List<double> kY = new List<double>();
      List<double> kZ = new List<double>();

      // Coordinate Values to Lists
      for (int k = 0; k < conFacePts.Count; k++)
      {
        kX.Add(conFacePts[k].X);
        kY.Add(conFacePts[k].Y);
        kZ.Add(conFacePts[k].Z);
      }

      //Calculate Averages of FacePts XYZ values
      double kXavg = kX.Average();
      double kYavg = kY.Average();
      double kZavg = kZ.Average();

      //Calculate EdgePt (Average of edge middle and connected face middles)
      Point3d edgePt = new Point3d(((kXavg + edgeMidPt.X) / 2),
        ((kYavg + edgeMidPt.Y) / 2),
        ((kZavg + edgeMidPt.Z) / 2));
      edgePts.Add(edgePt);
    }
    return edgePts;
  }

  /// <summary>
  /// Returns a list of the face center points of a mesh.
  /// </summary>
  /// <param name="m"></param>
  /// <returns></returns>
  public List<Point3d> GetMeshFacePts(Mesh m)
  {
    List<Point3d> ctrPts = new List<Point3d>();

    for (int i = 0; i < m.Faces.Count; i++)
    {
      Point3d ctrPt = m.Faces.GetFaceCenter(i);
      ctrPts.Add(ctrPt);
    }
    return ctrPts;
  }

  /// <summary>
  /// Returns a list of weighted baypoints for a mesh per Catmull Clark subdivision
  /// </summary>
  /// <param name="m"></param>
  /// <returns></returns>
  List<Point3d> GetMeshBaryPts(Mesh m)
  {
    List<Point3d> facePts = GetMeshFacePts(m);
    List<Point3d> edgeMidPts = GetMeshEdgeMidPts(m);
    List<Point3d> bayPts = new List<Point3d>();

    for (int i = 0; i < m.Vertices.Count; i++)
    {
      int[] conVertIndex = m.TopologyVertices.ConnectedFaces(i);
      int conFaceCount = conVertIndex.Length;
      List<Point3d> conFacePts = new List<Point3d>();
      Point3d currentVertexPt = m.TopologyVertices.ElementAt(i);

      //Add all conncected face points to a list
      for (int j = 0; j < conVertIndex.Length; j++)
      {
        conFacePts.Add(facePts[conVertIndex[j]]);
      }

      //Calculate average all connected face points...
      Point3d conFacePtAvg = GetAveragePt(conFacePts);

      int[] conEdgeIndex = m.TopologyVertices.ConnectedEdges(i);
      List<Point3d> conEdgePts = new List<Point3d>();

      //Add all conncected edge points to a list
      for (int k = 0; k < conEdgeIndex.Length; k++)
      {
        conEdgePts.Add(edgeMidPts[conEdgeIndex[k]]);
      }
      Point3d conEdgePtsAvg = GetAveragePt(conEdgePts);

      Point3d bayPt = new Point3d(((conFacePtAvg.X + (2 * conEdgePtsAvg.X) + ((conFaceCount - 3) * currentVertexPt.X)) / conFaceCount),
        ((conFacePtAvg.Y + (2 * conEdgePtsAvg.Y) + ((conFaceCount - 3) * currentVertexPt.Y)) / conFaceCount),
        ((conFacePtAvg.Z + (2 * conEdgePtsAvg.Z) + ((conFaceCount - 3) * currentVertexPt.Z)) / conFaceCount));
      bayPts.Add(bayPt);
    }
    return bayPts;
  }

  /// <summary>
  /// Returns the Average Point from a list of points
  /// </summary>
  /// <param name="pts"></param>
  /// <returns></returns>
  Point3d GetAveragePt(List<Point3d> pts)
  {
    double[] ptsX = new double[pts.Count];
    double[] ptsY = new double[pts.Count];
    double[] ptsZ = new double[pts.Count];
    Point3d AvgPt = new Point3d();

    for (int i = 0; i < pts.Count; i++)
    {
      ptsX[i] = (pts[i].X);
      ptsY[i] = (pts[i].Y);
      ptsZ[i] = (pts[i].Z);

      AvgPt = new Point3d(ptsX.Average(), ptsY.Average(), ptsZ.Average());

    }
    return AvgPt;
  }

  /// <summary>
  /// Returns a list of mid points for all edges on a mesh.
  /// </summary>
  /// <param name="m"></param>
  /// <returns></returns>
  List<Point3d> GetMeshEdgeMidPts(Mesh m)
  {
    List<Point3d> EdgeMidPts = new List<Point3d>();

    for (int i = 0; i < m.TopologyEdges.Count; i++)
    {
      Line edgeLine = m.TopologyEdges.EdgeLine(i);
      Point3d edgeMidPt = edgeLine.PointAt(0.5);
      EdgeMidPts.Add(edgeMidPt);
    }
    return EdgeMidPts;
  }
  // </Custom additional code> 
}

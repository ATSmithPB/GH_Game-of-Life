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
  private void RunScript(List<bool> cells, int rows, int columns, List<int> birthRule, List<int> survivalRule, bool run, bool reset, ref object previous, ref object newIteration, ref object B)
  {
    //Onnff
    if (run == false){
      return;
    }

    if (cells.Count != rows * columns){
      Component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Something wrong with dimensions of grid");
      return;
    }

    //Initialize calculations
    if (reset == true){
      //Turn the list of bools into a 2D array
      prevCells = new bool[columns, rows];
      int it = 0;
      for (int j = 0; j < columns; j++){
        for (int i = 0; i < rows; i++){
          prevCells[j, i] = cells[it];
          it++;
        }
      }
    }

    //New Array of Alive/Dead cells for next iteration
    int[,] nCount = new int[columns, rows];


    //Switch previous cells with new ones
    bool[,] newCells = new bool[columns, rows];

    for (int j = 0; j < columns; j++){
      for (int i = 0; i < rows; i++){

        //Compute alive neighbors
        int aliveNeighbors = 0;
        int ni, nj;

        //Top-Left (wrapping grid)
        ni = i + 1;
        nj = j - 1;
        if (ni > rows - 1) ni = 0;
        else if (ni < 0) ni = rows - 1;
        if (nj > columns - 1) nj = 0;
        else if (nj < 0) nj = columns - 1;
        if (prevCells[nj, ni] == true) aliveNeighbors++;

        //Top
        ni = i + 1;
        nj = j;
        if (ni > rows - 1) ni = 0;
        else if (ni < 0) ni = rows - 1;
        if (nj > columns - 1) nj = 0;
        else if (nj < 0) nj = columns - 1;
        if (prevCells[nj, ni] == true) aliveNeighbors++;

        //Top-Right
        ni = i + 1;
        nj = j + 1;
        if (ni > rows - 1) ni = 0;
        else if (ni < 0) ni = rows - 1;
        if (nj > columns - 1) nj = 0;
        else if (nj < 0) nj = columns - 1;
        if (prevCells[nj, ni] == true) aliveNeighbors++;

        //Left
        ni = i;
        nj = j - 1;
        if (ni > rows - 1) ni = 0;
        else if (ni < 0) ni = rows - 1;
        if (nj > columns - 1) nj = 0;
        else if (nj < 0) nj = columns - 1;
        if (prevCells[nj, ni] == true) aliveNeighbors++;

        //Right
        ni = i;
        nj = j + 1;
        if (ni > rows - 1) ni = 0;
        else if (ni < 0) ni = rows - 1;
        if (nj > columns - 1) nj = 0;
        else if (nj < 0) nj = columns - 1;
        if (prevCells[nj, ni] == true) aliveNeighbors++;

        //Bottom-Left
        ni = i - 1;
        nj = j - 1;
        if (ni > rows - 1) ni = 0;
        else if (ni < 0) ni = rows - 1;
        if (nj > columns - 1) nj = 0;
        else if (nj < 0) nj = columns - 1;
        if (prevCells[nj, ni] == true) aliveNeighbors++;

        //Bottom
        ni = i - 1;
        nj = j;
        if (ni > rows - 1) ni = 0;
        else if (ni < 0) ni = rows - 1;
        if (nj > columns - 1) nj = 0;
        else if (nj < 0) nj = columns - 1;
        if (prevCells[nj, ni] == true) aliveNeighbors++;

        //Bottom-Right
        ni = i - 1;
        nj = j + 1;
        if (ni > rows - 1) ni = 0;
        else if (ni < 0) ni = rows - 1;
        if (nj > columns - 1) nj = 0;
        else if (nj < 0) nj = columns - 1;
        if (prevCells[nj, ni] == true) aliveNeighbors++;

        Print("Alive Neighbors: " + aliveNeighbors);

        //Compute the new state
        bool previousState = prevCells[j, i];
        bool newState = false;

        // If cell was dead
        if (previousState == false){
          foreach (int b in birthRule){
            if (b == aliveNeighbors){
              newState = true;
              break;
            }
          }
        }
          // If cell was alive
        else {
          foreach (int s in survivalRule){
            if (s == aliveNeighbors){
              newState = true;
              break;
            }
          }
        }
        //Store new state in new array
        newCells[j, i] = newState;

      }
    }

    //Output
    previous = prevCells;
    newIteration = newCells;

    //Store new cells for next iteration
    prevCells = newCells;
  }

  // <Custom additional code> 

  //Create Persistant variables
  bool[,] prevCells;

  // </Custom additional code> 
}

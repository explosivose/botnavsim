# Developing a Navigation Algorithm for BotNavSim v0.2.0
Instructions for developing a plugin that implements the INavigation interface.

https://github.com/explosivose/botnavsim/blob/master/Assets/botnavsim/Scripts/INavigation/INavigation.cs

### Table of Contents
**[Setup](#setup)**<br>
**[INavigation](#INavigation)**<br>
**[Order of Execution](#order-of-execution)**<br>
**[Interface Reference](#interface-reference)**<br>

## Setup
This quick guide will explain how to set up a C# project with Monodevelop (the version that ships with Unity 4.6), 
which assemblies need to be referenced and using Monodevelop to generate the functions specified in INavigation.

1. Create a new C# library solution
* Edit the project references (Project>Edit References...)
* The required assemblies can be found in the BotNavSim installation directory: 
` <botnavsim installation directory>\botnavsim_Data\Managed`
* The assemblies you need to add to your project references are as follows:
  * Assembly-CSharp.dll - BotNavSim assembly (contains INavigation)
  * UnityEngine.dll - contains a lot of useful classes like `Vector3`
* At the top of your code include the UnityEngine library: `using UnityEngine;`
* In your code have a class that implements INavigation: `public class MyClass : INavigation`

To auto-generate the INavigation interface members do the following

1. Place the cursor inside the class and outside of any method declaration
* From this point you can bring up the Monodevelop code generation window (Alt+Insert)
* Tick all the boxes for Implement Interface Members and hit Enter

Your skeleton class might look something like this: [link pending]()

## INavigation

### Order of Execution

1. Set once: `Bounds searchBounds`
* Set once: `Vector3 origin`
* Set once: `Vector3 destination`
* Get every frame: `bool pathFound`
* Called every frame: `void Proximity(from, to, obstructed)`
* Called every frame `if (pathFound == true) Vector3 direction = PathDirection(myLocation)`
* Called every frame `void DrawGizmos()`
* Called every frame `void DrawDebugInfo()`
* `StartCoroutine( SearchForPath(origin, destination) )`

### Interface Reference

    Bounds searchBounds
We use a [bounding box](http://docs.unity3d.com/ScriptReference/Bounds.html) to specify the search space. Use this information to setup a search graph of nodes, for example. 

    Space spaceRelativeTo
We use the [Space](http://docs.unity3d.com/ScriptReference/Space.html) enumeration to specify a frame of reference when interfacing with the simulation. `Space.World` indicates that all Vector3 coordinates and rotations are relative to a point in space whose value is (0,0,0) with pitch, roll and yaw (0,0,0). `Space.Self` indicates that all Vector3 coordinates and rotations are relative to the robot whose position is always (0,0,0) and whose forward direction is (0,0,0). To avoid confusion, use the same frame of reference throughout a simulation.

    Vector3 origin
The start location of the navigation path. This is always where the robot is now. 

    Vector3 destination
The location of the destination. 

    IEnumerator SearchForPath()
    IEnumerator SearchForPath(Vector3 start, Vector3 end)
In this version the main search routine must be implemented as a [coroutine](http://docs.unity3d.com/ScriptReference/Coroutine.html). This is likely to change in future. Coroutines work by yielding execution at certain points to avoid hanging up UnityEngine execution. The problem with this is that yielding execution will slow down your search routine. The engine and your search routine must take it in turns to execute code. 

If you want to write your search routine just like an ordinary function then you can write something like this

    public IEnumerator SearchForPath () {
        StartSearch();
        yield break;
    }


    bool pathFound
This boolean flag should be a readonly property. Set to true when a path has been found.

    Vector3 PathDirection(Vector3 myLocation)
Return a direction along the path to the destination given the current location of the robot.

    void Proximity(Vector3 from, Vector3 to, bool obstructed)
This statement is saying that at position `from` there is a sensor, and it has "seen" all the way to position `to`. The `obstructed` flag indicates whether position `to` is obstructed. 

    void DrawDebugInfo()
Use this function to draw lines or cubes in the simulation space with the following functions

    Draw.Instance.Line(Vector3 start, Vector3 end, Color color)
    Draw.Instance.Cube(Vector3 position, Vector3 size, Color color)


    void DrawGizmos()
Use this function to draw lines or cubes in with the Unity3D Editor via the Gizmos class. Not useful for standalone builds. 


# Developing a Navigation Algorithm for BotNavSim
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



### Interface Reference

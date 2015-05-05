using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Draws stuff, useful for displaying debug data in the simulation.
/// Note that GL implementations require that this Monobehaviour
/// be attached to a the rendering camera (OnPostRender event)
/// </summary>
public class Draw : MonoBehaviour {

	// data structure for drawing a line
	private class LineData {
		public Vector3 start;
		public Vector3 end;
		public Color color;
		public LineData(Vector3 from, Vector3 to, Color c) {
			start = from;
			end = to;
			color = c;
		}
	}
	
	// data structure for drawing a cube
	private class CubeData {
		public Vector3 size;
		public Vector3 center;
		public Color color;
		public CubeData(Vector3 Size, Vector3 Center, Color c) {
			size = Size;
			center = Center;
			color = c;
		}
	}

	/// <summary>
	/// Reference to the MonoBehaviour instance.
	/// </summary>
	public static Draw Instance;

	// private members
	// -~-~-~-~-~-~-~-~
	
	private Material glLineMaterial;
	private List<LineData> gl_lines = new List<LineData>();		// list of lines to draw in the next frame 
	private List<CubeData> gl_cubes = new List<CubeData>();		// list of cubes to draw in the next frame
	
	// initialisation
	void Awake() {
		if (Instance == null){
			Instance = this;
		}
		else {	
			Destroy(this);
		}

		glLineMaterial = Resources.Load<Material>("Materials/Line");
	}
	
	// called every frame
	void Update() {
		// clear lists while the camera isn't being rendered
		if (Camera.current != camera) {
			gl_lines.Clear();
			gl_cubes.Clear();
		}
	}
	
	/// <summary>
	/// Draw a line between two positions, start and end, in a specified color. 
	/// </summary>
	/// <param name="start">Start.</param>
	/// <param name="end">End.</param>
	/// <param name="color">Color.</param>
	/// <param name="relativeTo">Choose world-space or Robot local-space.</param>
	public void Line(Vector3 start, Vector3 end, Color color, Space relativeTo = Space.World) {
		if (relativeTo == Space.Self && Simulation.robot != null) {
			start = Simulation.robot.transform.TransformPoint(start);
			end = Simulation.robot.transform.TransformPoint(end);
		}
		gl_lines.Add(new LineData(start, end, color));
	}
	
	/// <summary>
	/// Draw a line in a direction starting at a specified location, in a specified color. 
	/// </summary>
	/// <param name="origin">Origin.</param>
	/// <param name="direction">Direction.</param>
	/// <param name="color">Color.</param>
	public void Bearing(Vector3 origin, Vector3 direction, Color color) {
		Vector3 end = origin + direction;
		gl_lines.Add(new LineData(origin, end, color));
	}
	
	/// <summary>
	/// Draw any sized cube at a location in any color. 
	/// </summary>
	/// <param name="center">Center.</param>
	/// <param name="size">Size.</param>
	/// <param name="color">Color.</param>
	public void Cube(Vector3 center, Vector3 size, Color color) {
		//if (Simulation.robot.navigation.spaceRelativeTo == Space.Self) {
		//	center = Simulation.robot.transform.TransformPoint(center);
		//}
		gl_cubes.Add(new CubeData(size, center, color));
	}
	
	// draw a GL Line
	private void GLLine(LineData line) {
		GL.Color(line.color);
		GL.Vertex(line.start);
		GL.Vertex(line.end);
	}
	
	// draw a cube in GL Lines
	private void GLCube(CubeData cube) {
		float w = cube.size.x;
		float h = cube.size.y;
		float d = cube.size.z;
		float x = cube.center.x;
		float y = cube.center.y;
		float z = cube.center.z;
		
		GL.Color(cube.color);
		
		GL.Vertex3(x - w/2f, y - h/2f, z - d/2f); // 4
		GL.Vertex3(x - w/2f, y - h/2f, z + d/2f); // 1
		GL.Vertex3(x - w/2f, y + h/2f, z + d/2f); // 2
		GL.Vertex3(x - w/2f, y + h/2f, z - d/2f); // 3

		GL.Vertex3(x + w/2f, y - h/2f, z - d/2f); // 5
		GL.Vertex3(x - w/2f, y - h/2f, z - d/2f); // 4
		GL.Vertex3(x - w/2f, y + h/2f, z - d/2f); // 3
		GL.Vertex3(x + w/2f, y + h/2f, z - d/2f); // 8

		GL.Vertex3(x + w/2f, y - h/2f, z + d/2f); // 6
		GL.Vertex3(x + w/2f, y - h/2f, z - d/2f); // 5
		GL.Vertex3(x + w/2f, y + h/2f, z - d/2f); // 8
		GL.Vertex3(x + w/2f, y + h/2f, z + d/2f); // 7
		
		GL.Vertex3(x - w/2f, y - h/2f, z + d/2f); // 1
		GL.Vertex3(x + w/2f, y - h/2f, z + d/2f); // 6
		GL.Vertex3(x + w/2f, y + h/2f, z + d/2f); // 7
		GL.Vertex3(x - w/2f, y + h/2f, z + d/2f); // 2
	}
	
	/// <summary>
	/// Raises the post render event. Use this for GL stuff.
	/// </summary>
	void OnPostRender() {
		if (CamController.renderMode == CamController.RenderMode.Normal) return;
		GL.PushMatrix();
		glLineMaterial.SetPass(0);
		GL.Begin(GL.LINES);
		foreach(LineData line in gl_lines) {
			GLLine(line);
		}
		foreach(CubeData cube in gl_cubes) {
			GLCube(cube);
		}
		GL.End();
		GL.PopMatrix();
		gl_lines.Clear();
		gl_cubes.Clear();
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Draws stuff, useful for displaying debug data in world space.
/// Note that GL implementations require that this Monobehaviour
/// be attached to a the rendering camera (OnPostRender event)
/// </summary>
public class Draw : MonoBehaviour {

/*

This line renderer implementation is super slow for many lines
Do GL.Lines instead...

*/
	
	public static Draw Instance;

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

	List<Transform> instances = new List<Transform>();
	Material glLineMaterial;
	List<LineData> gl_lines = new List<LineData>();
	List<CubeData> gl_cubes = new List<CubeData>();
	
	void Awake() {
		if (Instance == null){
			Instance = this;
		}
		else {	
			Destroy(this);
		}

		glLineMaterial = Resources.Load<Material>("Materials/Line");
	}
		
	
	public void Line(Vector3 start, Vector3 end, Color color, Space relativeTo = Space.World) {
		if (relativeTo == Space.Self && Simulation.robot != null) {
			start = Simulation.robot.transform.TransformPoint(start);
			end = Simulation.robot.transform.TransformPoint(end);
		}
		gl_lines.Add(new LineData(start, end, color));
	}
	
	public void Bearing(Vector3 origin, Vector3 direction, Color color) {
		Vector3 end = origin + direction;
		gl_lines.Add(new LineData(origin, end, color));
	}
	
	public void Cube(Vector3 center, Vector3 size, Color color) {
		//if (Simulation.robot.navigation.spaceRelativeTo == Space.Self) {
		//	center = Simulation.robot.transform.TransformPoint(center);
		//}
		gl_cubes.Add(new CubeData(size, center, color));
	}
	
	public void Clear() {
		foreach(Transform t in instances) {
			t.Recycle();
		}
		instances.Clear();
	}
	

	private void GLLine(LineData line) {
		GL.Color(line.color);
		GL.Vertex(line.start);
		GL.Vertex(line.end);
	}
	
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

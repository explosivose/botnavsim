using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// BotPath displays information about a recorded path taken or calculated.
/// </summary>
[System.Serializable]
public class BotPath : IObservable  {
	
	public BotPath() {
		_nodes = new List<Vector3>();
		_times = new List<float>();
		color = Color.blue;
		visible = true;
	}
	
	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="BotPath"/> is visible.
	/// </summary>
	/// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
	public bool visible { get; set; }
	
	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="BotPath"/> is highlighted.
	/// When highlighted, the path is drawn in white.
	/// </summary>
	/// <value><c>true</c> if highlight; otherwise, <c>false</c>.</value>
	public bool highlight { get; set; }
	
	/// <summary>
	/// Gets or sets the name of the csv.
	/// </summary>
	/// <value>The name of the csv.</value>
	public string csvName { get; set; }
	
	/// <summary>
	/// Gets the total distance this BotPath covers.
	/// </summary>
	/// <value>The distance.</value>
	public float distance { get; private set; }
	
	/// <summary>
	/// Gets the start of the path.
	/// </summary>
	/// <value>The start.</value>
	public Vector3 start {
		get {
			return _nodes[0];
		}
	}
	
	/// <summary>
	/// Gets the end of the path.
	/// </summary>
	/// <value>The end.</value>
	public Vector3 end {
		get {
			return _nodes[_nodes.Count-1];
		}
	}
	
	public Bounds bounds {
		get; private set;
	}
	
	public string name {
		get { return csvName; }
	}
	
	/// <summary>
	/// Gets or sets the color used in drawing the path via Draw.
	/// </summary>
	/// <value>The draw color.</value>
	public Color color { get; set; }
	
	
	private List<Vector3> _nodes;
	private List<float> _times;
	
	/// <summary>
	/// Adds a node to the path. 
	/// </summary>
	/// <param name="node">Node.</param>
	/// <param name="time">Time.</param>
	public void AddNode(Vector3 node, float time) {
		if (_nodes.Count > 1)
			distance += Vector3.Distance(_nodes[_nodes.Count-1], node);
		
		_nodes.Add(node);
		_times.Add(time);
		Bounds b = new Bounds();
		b.Encapsulate(bounds);
		b.Encapsulate(node);
		bounds = b;
	}
	
	/// <summary>
	/// Draws the path on screen using Draw class and text labels.
	/// </summary>
	public void DrawPath() {
		Color c = highlight ? Color.white : color;
		for(int i = 1; i < _nodes.Count-1; i++) {
			Draw.Instance.Line(
				_nodes[i-1],
				_nodes[i],
				c);
		}
		if (_nodes.Count > 2) {
			// draw start node
			Draw.Instance.Cube(
				_nodes[0],
				Vector3.one * 0.5f,
				c);
			// draw end node
			Draw.Instance.Cube(
				_nodes[_nodes.Count-1],
				Vector3.one,
				c);
			Draw.Instance.Cube(
				_nodes[_nodes.Count-1],
				Vector3.one * 0.5f,
				c);
		}

	}
}

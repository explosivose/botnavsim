using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Astar : MonoBehaviour {

	[System.Serializable]
	public class GraphData {
		public Transform nodePrefab;
		public int X = 25;
		public int Y = 25;
		public float spacing = 1f;
		public node[,] graph;
		
		public void Initialise() {
			nodePrefab.CreatePool();
			graph = new node[X,Y];
		}
		public void BuildGraph() {

				for (int x = 0; x < X; x++) {
					for (int y = 0; y < Y; y++) {
						Vector3 position = new Vector3(x * spacing, 0, y * spacing);
						node n = new node(position, this);
						graph[x,y] = n;
					}
				}
			
			ConnectNodes();
		}
		
		
		/* 2D */
		void ConnectNodes() {
			for (int x = 0; x < X; x++) {
				for (int y = 0; y < Y; y++) {
					node n = graph[x,y];
					if (y > 0) {
						n.connected.Add(graph[x,y-1]);
						if (x > 1)
							n.connected.Add(graph[x-1,y-1]);
						if (x < X-1)
							n.connected.Add(graph[x+1,y-1]);
					}
					if (x > 0) {
						n.connected.Add(graph[x-1,y]);
						if (y < Y-1)
							n.connected.Add(graph[x-1,y+1]);
					}
					if (x < X-1) {
						n.connected.Add(graph[x+1,y]);
						if (y < Y-1)
							n.connected.Add(graph[x+1,y+1]);
					}
					if (y < Y-1) {
						n.connected.Add(graph[x,y+1]);
					}
				}
			}
		}
		
		
		public node NearestNode(Vector3 position) {
			node nearestNode = graph[0,0];
			float d1 = Mathf.Infinity;
			for (int x = 0; x < X; x++) {
				for (int y = 0; y < Y; y++) {
					float d2 = Vector3.Distance(graph[x,y].position, position);
					if (d2 < d1) {
						nearestNode = graph[x,y];
						d1 = d2;
					}
				}
			}
			return nearestNode;
		}
		
		public void DrawGizmos() {
			for (int x = 0; x < X; x++) {
				for (int y = 0; y < Y; y++) {
					graph[x,y].DrawGizmos();
				}
			}
		}
	}
	
	[System.Serializable]
	public class node {

		public enum State {
			regular,
			start,
			destination,
			path,
			open,
			closed
		}
		public enum Type {
			walkable,
			obstructed,
			unexplored
		}
		
		public GraphData graph;
		public bool highlight;
		public List<node> connected = new List<node>();
		
		public State state {
			get {
				return _state;
			}
			set {
				_state = value;
				switch(_state) {
				case State.regular:
					type = _type;
					break;
				case State.start:
					_color = Color.Lerp(Color.clear, Color.yellow, 0.75f);
					break;
				case State.destination:
					_color = Color.green;
					break;
				case State.path:
					_color = Color.Lerp(Color.clear, Color.green, 0.75f);
					break;
				case State.closed:
					_color = Color.Lerp(Color.clear, Color.cyan, 0.75f);
					break;
				case State.open:
					_color = Color.Lerp(Color.clear, Color.magenta, 0.75f);
					break;
				default:
					_color = Color.magenta;
					break; 
				}
			}
		}
		
		public Type type {
			get{
				return _type;
			}
			set {
				_type = value;
				if (state != State.regular) return;
				switch(_type){
				case Type.obstructed:
					_color = Color.Lerp(Color.clear, Color.red, 0.75f);
					break;
				case Type.walkable:
					_color = Color.Lerp(Color.clear, Color.green, 0.25f);
					break;
				case Type.unexplored:
					_color = Color.Lerp(Color.clear, Color.black, 0.25f);
					break;
				}
			}
		}
		
		public int index {
			get; private set;
		}
		public node parent {
			get {
				return _parent;
			}
			set {
				_parent = value;
				if (_parent) {
					G = _parent.G;
					G += Vector3.Distance(position, _parent.position);
				}
				else {
					G = 0f;
					type = type;
				}
				
			}
		}
		public node child {
			get; set;
		}
		public node destination {
			get {
				return _destination;
			}
			set {
				_destination = value;
				if (_destination) {
					H = Mathf.Abs(position.x - _destination.position.x);
					H += Mathf.Abs(position.y - _destination.position.y);
					H += Mathf.Abs(position.z - _destination.position.z);
				}
				else {
					H = 0f;
					type = type;
				}
			}
		}
		
		public float G {
			get; private set;
		}
		
		public float H {
			get; private set;
		}
		
		public float F {
			get {
				return G + H;
			}
		}
		
		public Vector3 position {
			get; private set;
		}
		
		private static int node_count;
		private node _parent;
		private node _destination;
		private State _state;
		private Type _type;
		private Color _color;
		
		public static implicit operator bool(node n) {
			return n != null;
		}
		
		public node(Vector3 location, GraphData g) {
			index = node_count++;
			position = location;
			graph = g;
			state = State.regular;
			type = Type.unexplored;
			Explore();
		}
		
		public void Explore() {
			if (Physics.CheckSphere(position, graph.spacing-0.5f))
				type = Type.obstructed;
			else
				type = Type.walkable;
		}
		
		public float TentativeG(node potentialParent) {
			float tG = potentialParent.G;
			tG += Vector3.Distance(position, potentialParent.position);
			return tG;
		}
		
		public void DrawGizmos() {
			
			if (state == State.regular) return;
			
			Gizmos.color = highlight ? Color.white : _color;
			Gizmos.DrawWireCube(
				position, 
				Vector3.one * graph.spacing * 0.25f
				);
			if (parent) {
				Gizmos.DrawLine(position, parent.position);
			}
			else {
				foreach(node n in connected) {
					Gizmos.DrawLine(position, n.position);
				}
			}
			
			
			if (child) {
				Gizmos.color = Color.green;
				Gizmos.DrawLine(position, child.position);
			}
		}
	}
	
	public bool search;
	public bool repeatSearch;
	public float steptime;
	public Transform target;
	public GraphData graphData = new GraphData();
	
	private List<node> open = new List<node>();
	private List<node> closed = new List<node>();
	
	public node destinationNode {
		get; private set;
	}

	void Awake() {
		graphData.Initialise();
		graphData.BuildGraph();
	}
	
	IEnumerator Start() {
		yield return new WaitForSeconds(1f);
		while(true) {
			if (search || repeatSearch) {
				yield return StartCoroutine(Path(target.position));
				search = false;
			}
			yield return new WaitForFixedUpdate();
		}
	}
	
	
	void InitialiseSearch() {
		foreach (node n in closed) {
			n.child = null;
			n.parent = null;
			n.destination = null;
			n.state = node.State.regular;
		}
		closed.Clear();
		foreach (node n in open) {
			n.child = null;
			n.parent = null;
			n.destination = null;
			n.state = node.State.regular;
		}
		open.Clear();
	}
	
	node LowestFscoreInOpen() {
		node lowest = open[0];
		foreach (node n in open) {
			if (n.F < lowest.F) lowest = n;
		}
		return lowest;
	}
	
	IEnumerator Path(Vector3 destination) {
		InitialiseSearch();

		bool success = false;
		
		node start = graphData.NearestNode(transform.position);
		start.state = node.State.start;
		destinationNode = graphData.NearestNode(destination);
		destinationNode.state = node.State.destination;
				
		open.Add(start);
		node current;
		while( open.Count > 0 ) {
			current = LowestFscoreInOpen();
			if (current.state == node.State.destination) {
				yield return StartCoroutine(ReconstructPath());
				success = true;
				break;
			}
			
			current.destination = destinationNode;
			
			open.Remove(current);
			closed.Add(current);
			if (current.state != node.State.start)
				current.state = node.State.closed;
			
			foreach(node n in current.connected) {
				if (closed.Contains(n)) continue;
				if (n.type == node.Type.obstructed) continue;
				
				n.destination = destinationNode;

				if (!open.Contains(n) || n.TentativeG(current) < n.G) {
					n.parent = current;
					if (!open.Contains(n)) {
						open.Add(n);
						if (n.state != node.State.destination)
							n.state = node.State.open;
					}
				}
			}
			
			yield return new WaitForSeconds(steptime);
		}
		
		if (success) {
			Debug.Log ("Path completed.");
		}
		else {
			// failure! destination was not found.
			Debug.LogWarning("Could not find path to destination.");
		}
		
	}
	
	IEnumerator ReconstructPath() {
		node current = destinationNode;
		/*foreach (node n in closed) {
			if (n.state != node.State.start)
				n.state = node.State.regular;
		}*/
		while (current.state != node.State.start) {
			if (current.parent) {
				current.parent.child = current;
				current = current.parent;
				if (current.state == node.State.closed)
				current.state = node.State.path;
				
			}
			yield return new WaitForSeconds(steptime);
		}
		yield return new WaitForSeconds(1f);
	}
	
	
	void OnDrawGizmos() {
		if (Application.isPlaying)
		graphData.DrawGizmos();
	}
}

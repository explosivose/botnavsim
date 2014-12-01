using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AstarNative : MonoBehaviour, INavigation {

	// public fields
	// ~-~-~-~-~-~-~-~-
	public bool showsteps;
	public float steptime;
	public GraphData graphData = new GraphData();

	// private members
	// ~-~-~-~-~-~-~-~-
	private List<node> open = new List<node>();
	private List<node> closed = new List<node>();
	private node pathnode;
	private bool pathready;

	// public properties
	// ~-~-~-~-~-~-~-~-
	public node destinationNode { get; private set;	}
	
	public node startNode {	get; private set; }

	public Bounds searchBounds {get; set;}
	public Vector3 origin {get; set;}
	public Vector3 destination {get; set;}
	public bool pathFound { get {return pathready;} }	

	// public methods
	// ~-~-~-~-~-~-~-~-


	/// <summary>
	/// Part of the INavigation interface.
	/// </summary>
	/// <returns>Direction to next node in path.</returns>
	/// <param name="currentPosition">Current position of mobile robot.</param>
	public Vector3 PathDirection(Vector3 myLocation) {
		if (!pathready) return Vector3.zero;
		if (pathnode.state != node.State.destination)
			if (Vector3.Distance(myLocation, pathnode.position) < graphData.spacing * 0.6f) 
				pathnode = pathnode.child;
		
		Debug.DrawLine(myLocation, pathnode.position, Color.red);
		return pathnode.position - myLocation;
	}
	
	/// <summary>
	/// Part of the INavigation interface.
	/// </summary>
	/// <param name="start">Sensor world position.</param>
	/// <param name="end">End of sensor range.</param>
	/// <param name="obstructed"><c>true</c> indicates obstruction at
	/// the end of the sensor range.</param>
	public void Proximity(Vector3 from, Vector3 to, bool obstructed) {
		Vector3 mark = from;
		float length = Vector3.Distance(from, to);
		for (float dist = 0f; dist < length; dist += graphData.spacing) {
			mark = Vector3.Lerp(from, to, dist/length);
			node n = graphData.NearestNode(mark);
			if (n.type != node.Type.obstructed) {
				n.type = node.Type.walkable;
			}
			Debug.DrawRay(mark, Vector3.up, Color.magenta);
		}
		if (obstructed) {
			node n = graphData.NearestNode(to);
			n.type = node.Type.obstructed;
			if (NodeInPath(n)) StartSearch();
		}
	}
	
	public IEnumerator SearchForPath() {
		StartSearch();
		yield break;
	}
	
	public IEnumerator SearchForPath(Vector3 start, Vector3 end) {
		origin = start;
		destination = end;
		StartSearch();
		yield break;
	}

	public void DrawGizmos () {
		throw new System.NotImplementedException ();
	}
	
	public void DrawDebugInfo() {
		
	}

	// private methods
	// ~-~-~-~-~-~-~-~-

	/// <summary>
	/// Awake this instance. (Monobehaviour)
	/// </summary>
	void Awake() {
		graphData.Initialise();
		graphData.BuildGraph(transform.position);
	}

	/// <summary>
	/// Starts the A* search to target.
	/// </summary>
	void StartSearch() {
		if (showsteps) {
			StartCoroutine( BuildPath(destination) );
		}
		else {
			BuildPathInstant(destination);
		}
	}

	/// <summary>
	/// Deletes any recorded data from previous searches.
	/// </summary>
	void InitialiseSearch() {
		pathready = false;
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
	
	/// <summary>
	/// Gets the lowest f score in open list.
	/// </summary>
	/// <returns>The lowest f score in open list.</returns>
	node LowestFscoreInOpen() {
		node lowest = open[0];
		foreach (node n in open) {
			if (n.F < lowest.F) lowest = n;
		}
		return lowest;
	}
	
	/// <summary>
	/// A* Search routine with wait states to demonstrate steps.
	/// </summary>
	/// <param name="destination">Destination.</param>
	IEnumerator BuildPath(Vector3 destination) {
		InitialiseSearch();

		bool success = false;
		
		pathnode = graphData.NearestUnobstructedNode(transform.position);
		startNode = pathnode;
		pathnode.state = node.State.start;
		destinationNode = graphData.NearestUnobstructedNode(destination);
		destinationNode.state = node.State.destination;
				
		open.Add(pathnode);
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
			
			//yield return new WaitForSeconds(steptime);
		}
		
		if (success) {
			Debug.Log ("A*: Path completed.");
		}
		else {
			// failure! destination was not found.
			Debug.LogWarning("A*: Could not find path to destination.");
		}
		
	}
	
	/// <summary>
	/// Reconstructs the path with wait states to demonstrate steps
	/// </summary>
	IEnumerator ReconstructPath() {
		node current = destinationNode;
		while (current != startNode) {
			if (current.parent) {
				current.parent.child = current;
				current = current.parent;
				if (current.state == node.State.closed)
				current.state = node.State.path;
			}
			yield return new WaitForSeconds(steptime);
		}
		pathready = true;
		yield return new WaitForSeconds(0.5f);
	}
	
	/// <summary>
	/// A* Search routine with no wait states.
	/// </summary>
	/// <param name="destination">Destination.</param>
	void BuildPathInstant(Vector3 destination) {
		InitialiseSearch();
		
		bool success = false;
		
		pathnode = graphData.NearestUnobstructedNode(transform.position);
		startNode = pathnode;
		pathnode.state = node.State.start;
		destinationNode = graphData.NearestUnobstructedNode(destination);
		destinationNode.state = node.State.destination;
		
		open.Add(pathnode);
		node current;
		while( open.Count > 0 ) {
			current = LowestFscoreInOpen();
			if (current.state == node.State.destination) {
				ReconstructPathInstantly();
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
		}
		
		if (success) {
			Debug.Log ("A*: Path completed.");
		}
		else {
			// failure! destination was not found.
			Debug.LogWarning("A*: Could not find path to destination.");
		}
		
	}
	
	/// <summary>
	/// Reconstructs the path with no wait states.
	/// </summary>
	void ReconstructPathInstantly() {
		node current = destinationNode;
		while (current != startNode) {
			if (current.parent) {
				current.parent.child = current;
				current = current.parent;
				if (current.state == node.State.closed)
					current.state = node.State.path;
			}
		}
		pathready = true;
	}
	
	/// <summary>
	/// Check if node n is featured in the current A* path.
	/// </summary>
	/// <returns><c>true</c>, if node is in path, <c>false</c> otherwise.</returns>
	/// <param name="n">N.</param>
	bool NodeInPath(node n) {
		if (!pathready) return false;
		node current = destinationNode;
		while(current.state != node.State.start) {
			if (current.index == n.index) return true;
			if (!current.parent) return false;
			current = current.parent;
		}
		return false;
	} 
	
	/// <summary>
	/// Raises the draw gizmos event. (Monobehaviour)
	/// </summary>
	void OnDrawGizmos() {
		if (Application.isPlaying)
		graphData.DrawGizmos();
	}
	
	
	[System.Serializable]
	public class GraphData {
		public bool drawDebug;
		public int X = 25;
		public int Y = 25;
		public float spacing = 1f;
		public node[,] graph {get;set;}
		public bool detectObstacles;
		public LayerMask obstacleMask;
		public Transform nodeRenderPrefab;
		
		public void Initialise() {
			graph = new node[X,Y];
		}
		public void BuildGraph(Vector3 location) {
			
			for (int x = 0; x < X; x++) {
				for (int y = 0; y < Y; y++) {
					Vector3 position = new Vector3(x * spacing, 0, y * spacing);
					position += location;
					node n = new node(position, nodeRenderPrefab, this);
					if (detectObstacles) n.Explore();
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
		
		public node NearestUnobstructedNode(Vector3 position) {
			node nearestNode = graph[0,0];
			float d1 = Mathf.Infinity;
			for (int x = 0; x < X; x++) {
				for (int y = 0; y < Y; y++) {
					if (graph[x,y].type != node.Type.obstructed) {
						float d2 = Vector3.Distance(graph[x,y].position, position);
						if (d2 < d1) {
							nearestNode = graph[x,y];
							d1 = d2;
						}
					}
				}
			}
			return nearestNode;
		}
		
		public node RandomUnobstructedNode() {
			int x = Random.Range(0,X);
			int y = Random.Range(0,Y);
			if (graph[x,y].type == node.Type.obstructed) 
				return RandomUnobstructedNode();
			else
				return graph[x,y];
		}
		
		public void DrawGizmos() {
			if (!drawDebug) return;
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
		public Color color {
			get {
				return _color;
			}
		}
		public List<node> connected = new List<node>();
		
		/// <summary>
		/// Set true if node state, type, parent or child is changed.
		/// </summary>
		/// <value><c>true</c> if has changed; otherwise, <c>false</c>.</value>
		public bool hasChanged {
			get; set;
		}
		
		public State state {
			get {
				return _state;
			}
			set {
				_state = value;
				hasChanged = true;
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
				hasChanged = true;
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
				hasChanged = true;
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
			get  {
				return _child;
			}
			set {
				_child = value;
				hasChanged = true;
			}
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
		private node _child;
		private node _destination;
		private State _state;
		private Type _type;
		private Color _color;
		private Transform _renderPrefab;
		private Transform _renderInstance;
		public static implicit operator bool(node n) {
			return n != null;
		}
		
		public node(Vector3 location, Transform nodePrefab, GraphData g) {
			index = node_count++;
			position = location;
			graph = g;
			state = State.regular;
			type = Type.unexplored;
			if (nodePrefab) {
				_renderPrefab = nodePrefab;
				_renderPrefab.CreatePool();
				_renderInstance = _renderPrefab.Spawn(location);
				_renderInstance.parent = Simulation.Instance.transform;
				_renderInstance.GetComponent<AstarNodeRender>().myNode = this;
			}
		}
		
		public void Explore() {
			if (Physics.CheckSphere(position, graph.spacing-0.5f, graph.obstacleMask))
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
			
			if (type == Type.obstructed) {
				Gizmos.color = Color.red;
				Gizmos.DrawWireCube(
					position,
					Vector3.one
					);
				return;
			}
			
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
	
}

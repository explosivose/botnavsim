using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Astar : MonoBehaviour {

	public bool search;
	public bool repeatSearch;
	public float steptime;
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
						node n = nodePrefab.Spawn(position, Quaternion.identity).GetComponent<node>();
						n.graph = this;
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
					float d2 = Vector3.Distance(graph[x,y].tr.position, position);
					if (d2 < d1) {
						nearestNode = graph[x,y];
						d1 = d2;
					}
				}
			}
			return nearestNode;
		}
	}
	
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
				current.state = node.State.path;
			
			foreach(node n in current.connected) {
				if (closed.Contains(n)) continue;
				if (n.type == node.Type.obstructed) continue;
				
				n.destination = destinationNode;

				if (!open.Contains(n) || n.TentativeG(current) < n.G) {
					n.parent = current;
					if (!open.Contains(n)) {
						open.Add(n);
						if (n.state != node.State.destination)
							n.state = node.State.pathHead;
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
				if (current.state == node.State.path)
				current.state = node.State.regular;
				
			}
			yield return new WaitForSeconds(steptime);
		}
		foreach (node n in open) {
			if (n.state != node.State.destination)
				n.state = node.State.regular;
		}
	}
	
}

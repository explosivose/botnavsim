using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class node : MonoBehaviour {

	public Astar.GraphData graph;
	
	public enum State {
		regular,
		start,
		destination,
		path,
		pathHead
	}
	
	public enum Type {
		walkable,
		obstructed,
		unexplored
	}
	
	public bool highlight;
	
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
				_text.text = "S";
				break;
			case State.destination:
				_color = Color.green;
				_text.text = "D";
				break;
			case State.path:
				_color = Color.Lerp(Color.clear, Color.cyan, 0.75f);
				break;
			case State.pathHead:
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
				_text.text = "#";
				break;
			case Type.walkable:
				_color = Color.Lerp(Color.clear, Color.green, 0.25f);
				_text.text = "W";
				break;
			case Type.unexplored:
				_color = Color.Lerp(Color.clear, Color.black, 0.25f);
				_text.text = "";
				break;
			}
		}
	}
	
	public List<node> connected = new List<node>();
	
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
				G += Vector3.Distance(tr.position, _parent.tr.position);
				_text.text = F.ToString("G2");
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
				H = Mathf.Abs(tr.position.x - _destination.tr.position.x);
				H += Mathf.Abs(tr.position.y - _destination.tr.position.y);
				H += Mathf.Abs(tr.position.z - _destination.tr.position.z);
				_text.text = F.ToString("G2");
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
	
	public Transform tr {
		get; private set;
	}
	
	private static int node_count;
	private node _parent;
	private node _destination;
	private State _state;
	private Type _type;
	private Color _color;
	private TextMesh _text;
	
	void Awake() {
		tr = this.transform;
		_text = transform.Find("text").GetComponent<TextMesh>();
		index = node_count++;
	}
	
	void Start() {
		state = State.regular;
		type = Type.unexplored;
		Explore();
	}
	
	public void Explore() {
		if (Physics.CheckSphere(transform.position, graph.spacing-0.5f))
			type = Type.obstructed;
		else
			type = Type.walkable;
	}
	
	public float TentativeG(node potentialParent) {
		float tG = potentialParent.G;
		tG += Vector3.Distance(tr.position, potentialParent.tr.position);
		return tG;
	}
	
	void OnDrawGizmos() {
		
		Gizmos.color = highlight ? Color.white : _color;
		Gizmos.DrawWireCube(
			transform.position, 
			Vector3.one
			);
		if (parent) {
			Gizmos.DrawLine(tr.position, parent.tr.position);
		}
		else {
			foreach(node n in connected) {
				Gizmos.DrawLine(tr.position, n.tr.position);
			}
		}

		
		if (child) {
			Gizmos.color = Color.green;
			Gizmos.DrawLine(tr.position, child.tr.position);
		}
	}
}

using UnityEngine;
using System.Collections;

public class AstarNodeRender : MonoBehaviour {
	
	public Material obstructedMat;
	public Material openMat;
	public Material closedMat;
	public Material pathMat;
	public Material startMat;
	public Material destinationMat;
	
	public AstarNative.node myNode {get; set;}
	
	private MeshRenderer _renderer;
	private LineRenderer _line;
	private bool _render;
	
	void Awake() {
		_renderer = GetComponent<MeshRenderer>();
		_line = GetComponent<LineRenderer>();
	}
	
	void Update() {
		if (!myNode) return;
		if (myNode.hasChanged) {
			myNode.hasChanged = false;
			UpdateMaterial();
		}
	}
	
	void UpdateMaterial() {
		
		if (myNode.type == AstarNative.node.Type.obstructed) {
			_renderer.enabled = true;
			_renderer.material = obstructedMat;
			_line.enabled = false;
			return;
		}
		
		if (myNode.state == AstarNative.node.State.regular) {
			_renderer.enabled = false;
			_line.enabled = false;
			return;
		}
		else {
			_renderer.enabled = true;
		}
		
		if (myNode.state == AstarNative.node.State.start) 
			_renderer.material = startMat;
		else if (myNode.state == AstarNative.node.State.destination)
			_renderer.material = destinationMat;
		else if (myNode.state == AstarNative.node.State.closed)
			_renderer.material = closedMat;
		else if (myNode.state == AstarNative.node.State.open) 
			_renderer.material = openMat;
		else if (myNode.state == AstarNative.node.State.path)
			_renderer.material = pathMat;
		
		
		if (myNode.child) {
			_line.enabled = true;
			_line.SetVertexCount(2);
			_line.SetPosition(0, myNode.position);
			_line.SetPosition(1, myNode.child.position);
			_line.material = pathMat;
		}
		else if (myNode.parent) {
			_line.enabled = true;
			_line.SetVertexCount(2);
			_line.SetPosition(0, myNode.position);
			_line.SetPosition(1, myNode.parent.position);
			_line.material = closedMat;
		}
		else {
			_line.enabled = true;
			_line.SetVertexCount(myNode.connected.Count * 2);
			for (int i = 0; i < myNode.connected.Count * 2 - 2; i+=2) {
				_line.SetPosition(i, myNode.position);
				_line.SetPosition(i+1, myNode.connected[i/2].position);
			}
			_line.material = openMat;
		}
		
	}
}
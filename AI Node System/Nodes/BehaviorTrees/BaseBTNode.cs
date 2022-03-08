using System;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using UnityEngine;
using UnityEngine.Serialization;
using System.Linq;

namespace NodeEditorFramework.Standard
{
	/// <summary>
	///  Basic Behavior Tree Node class, all other Behavior Tree Nodes (Root nodes, Selectors, Sequences) are derived from this.
	/// </summary>
	[Serializable]
	[Node(true, "Behavior Tree/BaseBehaviorTreeNode", new Type[] { typeof(BehaviorTreeCanvas) })]
	public abstract class BaseBTNode : Node
	{
		public virtual Type GetObjectType { get { return typeof(BaseBTNode); } }
		

		#region Fields

		[NonSerialized]
		protected Vector2 offset = Vector2.zero;

		protected TaskResult status = TaskResult.NOT_ACTIVE;

		public bool debug = false;
		[NonSerialized]
		public bool HasBeenInit = false;
		[NonSerialized]
		public bool HasBeenCalled = false;

		[SerializeField]
		public BaseBTNode[] children = new BaseBTNode[0];
		[SerializeField]
		public BaseBTNode parent = null;

		public string title = "";

		#endregion

		#region Node Methods

		///check if the first connection of the specified port points to something
		protected bool IsAvailable(ConnectionPort port)
		{
			return port != null
				&& port.connections != null && port.connections.Count > 0
				&& port.connections[0].body != null
				&& port.connections[0].body != default(Node);
		}

		public virtual void Init(BehaviorTreeManager owner) { }
		public virtual void Begin(BehaviorTreeManager owner) { }
		
		public virtual TaskResult Tick(BehaviorTreeManager owner)
        {
			if (!HasBeenInit)
			{
				Init(owner);
				HasBeenInit = true;
			}
			if (!HasBeenCalled)
			{
				Begin(owner);
				HasBeenCalled = true;
			}
			status = ProcessTick(owner);
			if (status == TaskResult.SUCCESS || status == TaskResult.FAILURE)
			{
				HasBeenCalled = false;
			}
			return status;
		}

		public abstract TaskResult ProcessTick(BehaviorTreeManager owner);

		public TaskResult GetStatus()
        {
			return status;
        }

		public void RecursivelyFindChildren()
		{
			if (inputPorts.Count > 0)
				parent = inputPorts[0].connections[0].body as BaseBTNode;
			if (outputPorts.Count < 1)
			{
				children = new BaseBTNode[0];
				return;
			}
			children = outputPorts[0].connections.Select(connection => connection.body as BaseBTNode).ToArray();
			Array.Sort(children, (o1, o2) => o1.position.x.CompareTo(o2.position.x));
			foreach (BaseBTNode child in children)
			{
				child.RecursivelyFindChildren();
			}
		}

		public void DeactivateStatus()
		{
			status = TaskResult.NOT_ACTIVE;
			if (children == null)
				return;
			foreach (BaseBTNode child in children)
			{
				if (child != null)
					child.DeactivateStatus();
			}
		}

		#endregion

		#region Input

		public void SetNodeOffset(Node node)
		{
			this.offset = this.position - node.position;
			for (int i = 0; i < children.Length; i++)
			{
				children[i].SetNodeOffset(node);
			}
		}

		public void UpdatePosition(Node node)
		{
			//Debug.Log(offset);
			this.position = node.position + offset;
			for (int i = 0; i < children.Length; i++)
			{
				if (children[i] != null)
					children[i].UpdatePosition(node);
			}
		}

		[EventHandlerAttribute(EventType.MouseDown, 109)]
		private static void HandleNodeStartDrag(NodeEditorInputInfo inputInfo)
		{
			//Debug.Log("Running OnMoveNode callback");
			if (GUIUtility.hotControl > 0)
				return; // GUI has control

			NodeEditorState state = inputInfo.editorState;
			if (inputInfo.inputEvent.button == 0 && !state.dragNode)
			{
				var n = NodeEditor.NodeAtPosition(NodeEditor.ScreenToCanvasSpace(inputInfo.inputPos)) as BaseBTNode;
				if (n != null)
				{
					for (int i = 0; i < n.children.Length; i++)
					{
						n.children[i].SetNodeOffset(n);
					}
					state.StartDrag("recursiveDrag", inputInfo.inputPos, n.position);
					state.dragNode = true;
				}
			}
		}

		[EventHandlerAttribute(EventType.MouseDrag)]
		private static void HandleNodeDragging(NodeEditorInputInfo inputInfo)
		{
			NodeEditorState state = inputInfo.editorState;
			var n = state.selectedNode as BaseBTNode;
			if (n != null)
			{
				if (state.dragUserID != "recursiveDrag")
				{
					state.selectedNode = null;
					state.dragNode = false;
					return;
				}
				//Update drag operation
				Vector2 dragChange = state.UpdateDrag("recursiveDrag", inputInfo.inputPos);
				Vector2 newPos = state.dragObjectPos;

				// Update positions
				n.position = newPos;
				for (int i = 0; i < n.children.Length; i++)
				{
					n.children[i].UpdatePosition(n);
				}

				inputInfo.inputEvent.Use();
				NodeEditor.RepaintClients();
			}
		}

		[EventHandlerAttribute(EventType.MouseUp)]
		private static void HandleDraggingEnd(NodeEditorInputInfo inputInfo)
		{
			if (inputInfo.editorState.dragUserID == "recursiveDrag")
			{
				inputInfo.editorState.EndDrag("recursiveDrag");
				NodeEditor.RepaintClients();
			}

			inputInfo.editorState.selectedNode = null;
			inputInfo.editorState.dragNode = false;
		}
		#endregion

		#region Drawing

		private GUIStyle style;

		protected override void DrawNode()
		{
			//Debug.Log("Drawing " + Title);
			// Create a rect that is adjusted to the editor zoom and pixel perfect
			Rect nodeRect = rect;
			Vector2 pos = NodeEditor.curEditorState.zoomPanAdjust + NodeEditor.curEditorState.panOffset;
			nodeRect.position = new Vector2((int)(nodeRect.x + pos.x), (int)(nodeRect.y + pos.y));
			Vector2 contentOffset = new Vector2(0, 20);

			// Create a headerRect out of the previous rect and draw it, marking the selected node as such by making the header bold
			GUI.color = Color.black;
			Rect headerRect = new Rect(nodeRect.x, nodeRect.y, nodeRect.width, contentOffset.y);
			GUI.Box(headerRect, GUIContent.none);

			// Draw highlight based on node status
			GUI.color = Color.white;
			style = new GUIStyle(GUI.skin.box);
			style.border = new RectOffset(3, 3, 3, 3);

			if (status == TaskResult.RUNNING)
			{
				style.normal.background = HighlightTextures.GetInstance().runningHighlight;
			}
			else if (status == TaskResult.FAILURE)
			{
				style.normal.background = HighlightTextures.GetInstance().failureHighlight;
			}
			else if (status == TaskResult.SUCCESS)
			{
				style.normal.background = HighlightTextures.GetInstance().successHighlight;
			}
			else
			{
				style.normal.background = HighlightTextures.GetInstance().notActiveHighlight;
			}
			if (HighlightTextures.GetInstance().successHighlight == null)
				Debug.Log("No success texture for " + Title);
			if (HighlightTextures.GetInstance().failureHighlight == null)
				Debug.Log("No failure texture for " + Title);
			if (HighlightTextures.GetInstance().runningHighlight == null)
				Debug.Log("No running texture for " + Title);
			GUI.Box(headerRect, GUIContent.none, style);
			GUI.color = Color.white;
			GUI.Label(headerRect, Title, GUI.skin.GetStyle(NodeEditor.curEditorState.selectedNode == this ? "labelBoldCentered" : "labelCentered"));

			// Begin the body frame around the NodeGUI
			Rect bodyRect = new Rect(nodeRect.x, nodeRect.y + contentOffset.y, nodeRect.width, nodeRect.height - contentOffset.y);
			GUI.color = backgroundColor;
			//GUI.color = Color.white;
			GUI.BeginGroup(bodyRect, GUIContent.none, GUI.skin.box);
			GUI.color = Color.white;
			bodyRect.position = Vector2.zero;
			GUILayout.BeginArea(bodyRect);

			// Call NodeGUI
			GUI.changed = false;

#if UNITY_EDITOR // Record changes done in the GUI function
			UnityEditor.Undo.RecordObject(this, "Node GUI");
#endif
			NodeGUI();
#if UNITY_EDITOR // Make sure it doesn't record anything else after this
			UnityEditor.Undo.FlushUndoRecordObjects();
#endif

			//if (Event.current.type == EventType.Repaint)
			// nodeGUIHeight = GUILayoutUtility.GetLastRect().max + contentOffset;

			// End NodeGUI frame
			GUILayout.EndArea();
			GUI.EndGroup();

			// Automatically node if desired
			AutoLayoutNode();

		}


		#endregion

	}
}
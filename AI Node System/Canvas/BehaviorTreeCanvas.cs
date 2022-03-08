using UnityEngine;
using NodeEditorFramework;
using System.Collections.Generic;
using System;
using System.Linq;

namespace NodeEditorFramework.Standard
{
	[NodeCanvasType("BehaviorTree")]
	public class BehaviorTreeCanvas : NodeCanvas
	{
		public override string canvasName { get { return "Behavior Tree Canvas"; } }
		public override bool allowRecursion { get { return false; } }


		private string rootNodeID { get { return "rootNode"; } }
		public RootNode rootNode;

		protected override void OnCreate()
		{
			Traversal = new BehaviorTreeCanvasTraversal(this);
			ValidateSelf();
		}

		public void OnEnable()
		{
			if(Traversal == null)
            {
				Traversal = new BehaviorTreeCanvasTraversal(this);
            }
			ValidateSelf();
			// Register to other callbacks, f.E.:
			//NodeEditorCallbacks.OnDeleteNode += OnDeleteNode;
		}

		public override void OnBeforeSavingCanvas()
        {
			rootNode.RecursivelyFindChildren();
		}

		protected override void ValidateSelf()
		{
			
			if (Traversal == null)
				Traversal = new BehaviorTreeCanvasTraversal(this);
			if(rootNode == null && (rootNode = nodes.Find((Node n) => n!=null && n.GetID.Equals(rootNodeID)) as RootNode) == null)
				rootNode = Node.Create(rootNodeID, Vector2.zero, this, null, true) as RootNode;
		}
	}
}

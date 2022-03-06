using System;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using UnityEngine;
using UnityEngine.Serialization;
using System.Linq;

namespace NodeEditorFramework.AI
{
	/// <summary>
	///  Basic Behavior Tree Node class, all other Behavior Tree Nodes (Root nodes, Selectors, Sequences) are derived from this.
	/// </summary>
	public abstract class BaseBTNode : Node
	{
		public enum TaskResult { NOT_ACTIVE, SUCCESS, FAILURE, RUNNING };

		#region Fields

		public bool HasBeenCalled = false; // will be reset when parent finishes
		public bool HasBeenInit = false; // will be set only after first init

		protected bool debug = false; // toggle printing of ticks to console

		[SerializeField]
		public BaseBTNode[] children = new BaseBTNode[0];
		[SerializeField]
		public BaseBTNode parent;
		[SerializeField]
		public TaskResult status;

		protected Transform owner;

        #endregion

        #region Base Methods

        public virtual void Init()
		{

		}

		public virtual void Start()
		{

		}

		public virtual TaskResult Tick()
		{
			if (!HasBeenInit)
			{
				Init();
				HasBeenInit = true;
			}
			if (!HasBeenCalled)
			{
				Start();
				HasBeenCalled = true;
			}
			status = ProcessTick();
			if (status == TaskResult.SUCCESS || status == TaskResult.FAILURE)
			{
				HasBeenCalled = false;
			}
			return status;
		}

		public abstract TaskResult ProcessTick();

		public TaskResult GetStatus()
		{
			return status;
		}

		void OnDisable()
		{
			HasBeenCalled = false;
			HasBeenInit = false;
		}

        #endregion

        #region Node Methods

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

		public Manager GetManager()
		{
			return ((BehaviorTreeCanvas)canvas).rootNode.manager;
		}

#endregion
	}
}
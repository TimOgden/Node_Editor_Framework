using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(true, "Behavior Tree/Composites/Composite", new Type[] { typeof(BehaviorTreeCanvas) })]
    public abstract class BaseComposite : BaseBTNode
    {
        #region GUI Fields

        protected Color color_ = new Color(.86f, .12f, .07f, .75f);

        public override Vector2 MinSize { get { return new Vector2(100, 25); } }

        public override bool AutoLayout { get { return false; } }

        [ConnectionKnobAttribute("", Direction.Out, "Flow", NodeSide.Bottom)]
        public ConnectionKnob outputKnob;

        #endregion

        #region Logic Fields

        [System.NonSerialized]
        public Queue<BaseBTNode> childrenToRun = new Queue<BaseBTNode>();

        #endregion

        void OnEnable()
        {
            backgroundColor = color_;
        }

        public override void NodeGUI()
        {
            title = RTEditorGUI.TextField(title);
        }

        void ResetChildrenCalled()
        {
            // reset HasBeenCalled for all children left to run
            // so that once we abort and rerun them, they will still call
            // Begin()
            Queue<BaseBTNode> temp = new Queue<BaseBTNode>();
            while (childrenToRun.Count > 0)
            {
                BaseBTNode node = childrenToRun.Dequeue();
                if (node != null)
                {
                    node.HasBeenCalled = false;
                    temp.Enqueue(node);
                }
            }

            while (temp.Count > 0)
            {
                childrenToRun.Enqueue(temp.Dequeue());
            }
        }

        public void ResetChildrenStatuses()
        {
            ResetChildrenCalled();
            childrenToRun = new Queue<BaseBTNode>(); // clear queue of children to be run
            foreach (BaseBTNode child in children)
            {
                childrenToRun.Enqueue(child); // re-add all children of Composite
            }
        }

        public void StartAtNode(BaseBTNode node)
        {
            ResetChildrenCalled();
            childrenToRun = new Queue<BaseBTNode>();
            for (int i = Array.IndexOf(children, node); i < children.Length; i++)
            {
                childrenToRun.Enqueue(children[i]);
            }
        }

        public override void Init(BehaviorTreeManager owner)
        {
            foreach (BaseBTNode child in children)
            {
                childrenToRun.Enqueue(child);
            }
        }

        public override void Begin(BehaviorTreeManager owner)
        {
            ResetChildrenStatuses();
        }
    }

    [Node(false, "Behavior Tree/Composites/Root Node", new Type[] { typeof(BehaviorTreeCanvas) })]
    public class RootNode : BaseComposite
    {
        public const string ID = "rootNode";
        public override string GetID { get { return ID; } }
        public override string Title { get { return title == "" ? "Root Node" : title + " (Root)"; } }

        public void SetStatus(TaskResult status)
        {
            this.status = status;
        }

        void OnEnable()
        {
            backgroundColor = color_;
            // DeactivateStatus();
        }

        public override void NodeGUI()
        {
            title = RTEditorGUI.TextField(title);
            outputKnob.DisplayLayout();
        }

        // Same as selector code.
        public override TaskResult ProcessTick(BehaviorTreeManager owner)
        {
            if (debug)
                Debug.Log("Ticking " + Title);

            if (childrenToRun.Count == 0)
            {
                //RecursivelyFindChildren();
            }

            Debug.Log("Children count: " + children.Length);
            Debug.Log("Child has value: " + (children.Length > 0 ? (children[0] != null).ToString() : "no array"));
            while (childrenToRun.Count > 0)
            {
                BaseBTNode child = childrenToRun.Peek();
                
                TaskResult childResult = child.Tick(owner);

                if (childResult == TaskResult.RUNNING || childResult == TaskResult.SUCCESS)
                {
                    return childResult;
                }
                else
                {
                    childrenToRun.Dequeue();
                }
            }
            return TaskResult.FAILURE;



        }

        [Node(false, "Behavior Tree/Composites/Selector", new Type[] { typeof(BehaviorTreeCanvas) })]
        public class Selector : BaseComposite
        {
            [ConnectionKnobAttribute("", Direction.In, "Flow", NodeSide.Top)]
            public ConnectionKnob inputKnob;
            public const string ID = "selector";
            public override string GetID { get { return ID; } }
            public override string Title { get { return title == "" ? "Selector" : title + " (?)"; } }

            public override void NodeGUI()
            {
                title = RTEditorGUI.TextField(title);
                inputKnob.DisplayLayout();
                outputKnob.DisplayLayout();
            }

            public override TaskResult ProcessTick(BehaviorTreeManager owner)
            {
                if (debug)
                    Debug.Log("Ticking " + Title);

                while (childrenToRun.Count > 0)
                {
                    BaseBTNode child = childrenToRun.Peek();
                    TaskResult childResult = child.Tick(owner);

                    if (childResult == TaskResult.RUNNING || childResult == TaskResult.SUCCESS)
                    {
                        return childResult;
                    }
                    else
                    {
                        childrenToRun.Dequeue();
                    }
                }
                return TaskResult.FAILURE;
            }
        }
    }

    [Node(false, "Behavior Tree/Composites/Sequence", new Type[] { typeof(BehaviorTreeCanvas) })]
    public class Sequence : BaseComposite
    {
        [ConnectionKnobAttribute("", Direction.In, "Flow", NodeSide.Top)]
        public ConnectionKnob inputKnob;
        public const string ID = "sequence";
        public override string GetID { get { return ID; } }
        public override string Title { get { return title == "" ? "Sequence" : title + " (=>)"; } }

        public override void NodeGUI()
        {
            title = RTEditorGUI.TextField(title);
            inputKnob.DisplayLayout();
            outputKnob.DisplayLayout();
        }

        public override TaskResult ProcessTick(BehaviorTreeManager owner)
        {
            if (debug)
                Debug.Log("Ticking " + Title);

            while (childrenToRun.Count > 0)
            {
                BaseBTNode child = childrenToRun.Peek();
                TaskResult childResult = child.Tick(owner);

                if (childResult == TaskResult.RUNNING || childResult == TaskResult.FAILURE)
                {
                    return childResult;
                }
                else
                {
                    childrenToRun.Dequeue();
                }
            }
            return TaskResult.SUCCESS;
        }
    }
}
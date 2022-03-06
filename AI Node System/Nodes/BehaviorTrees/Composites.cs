using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Standard;

namespace NodeEditorFramework.AI
{
    [Node(true, "Behavior Tree/Composites/Composite", new Type[] { typeof(BehaviorTreeCanvas) })]
    public abstract class BaseComposite : BaseBTNode
    {
        [System.NonSerialized]
        public Queue<BaseBTNode> childrenToRun = new Queue<BaseBTNode>();

        void ResetChildrenCalled()
        {
            // reset HasBeenCalled for all children left to run
            // so that once we abort and rerun them, they will still call
            // Start()
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

        public override void Init()
        {
            foreach (BaseBTNode child in children)
            {
                childrenToRun.Enqueue(child);
            }
        }

        public override void Start()
        {
            ResetChildrenStatuses();
        }
    }

    public class RootNode : BaseComposite
    {
        public const string ID = "rootNode";
        public override string GetID { get { return ID; } }
        public override string Title { get { return "Root Node"; } }
        public float tickingFrequency = .2f;

        public Manager manager;

        void OnEnable()
        {
            DeactivateStatus();
        }

        public override void Init()
        {
            ((BehaviorTreeCanvas)canvas).manager = manager;
        }

        // Same as selector code.
        public override TaskResult ProcessTick()
        {
            if (debug)
                Debug.Log("Ticking " + Title);
            if (children.Length == 0)
                RecursivelyFindChildren();
            TaskResult childResult = children[0].Tick();
            if (childResult == TaskResult.RUNNING)
            {
                status = TaskResult.RUNNING;
                return status;
            }
            status = childResult;
            return status;
        }

        public IEnumerator EvalTree()
        {
            status = TaskResult.NOT_ACTIVE;
            Debug.Log("Starting evaluation of tree with status: " + status);
            while (true)
            {
                status = Tick();
                if (!(status == TaskResult.NOT_ACTIVE || status == TaskResult.RUNNING))
                {
                    if (debug)
                        Debug.Log("Root returned: " + status);
                    yield return status;
                    yield return new WaitForSeconds(tickingFrequency); // pause after full tree traversal
                    DeactivateStatus();
                }
                yield return new WaitForSeconds(tickingFrequency); // ticking frequency
            }
        }
    }

    public class Selector : BaseComposite
    {
        public const string ID = "selector";
        public override string GetID { get { return ID; } }
        public override string Title { get { return "Selector"; } }

        public override TaskResult ProcessTick()
        {
            if (debug)
                Debug.Log("Ticking " + Title);

            while (childrenToRun.Count > 0)
            {
                BaseBTNode child = childrenToRun.Peek();
                TaskResult childResult = child.Tick();

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

    public class Sequence : BaseComposite
    {
        public const string ID = "sequence";
        public override string GetID { get { return ID; } }
        public override string Title { get { return "Sequence"; } }
        public override TaskResult ProcessTick()
        {
            if (debug)
                Debug.Log("Ticking " + Title);

            while (childrenToRun.Count > 0)
            {
                BaseBTNode child = childrenToRun.Peek();
                TaskResult childResult = child.Tick();

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
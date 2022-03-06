using System;
using System.Collections;
using System.Collections.Generic;
using NodeEditorFramework;
using NodeEditorFramework.Standard;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.AI
{
    [Node(true, "Behavior Tree/Decorators/BODs/BlackboardObserver", new Type[] { typeof(BehaviorTreeCanvas) })]
    public abstract class BaseBlackboardObserver : BaseDecorator
    {
        
        public override Vector2 MinSize { get { return new Vector2(130, 25); } }

        public string blackboardKey = "";
        protected string[] notify_rules = { "OnValueChange", "OnResultChange" };
        public int notify_rule = 0;
        protected string[] aborts = { "NONE", "SELF", "LOWER_PRIORITY", "BOTH" };
        public int abort_rule = 0;

        protected Blackboard blackboard;
        private BaseComposite parentCompsite;
        private bool observedCorrectly = false;

        /*
         * Stop rules
         * 
         * NONE:
         *  On Init(), check blackboard[key]
         *      if true:
         *          tick child
         *      else:
         *          return TaskResult.FAILURE
         * 
         * SELF:
         *  On Init(), check blackboard[key]
         *      if true:
         *          create observer on blackboard
         *          tick child until child returns value or condition becomes false
         *      else:
         *          return TaskResult.FAILURE
         * 
         * LOWER_PRIORITY:
         *  On Init(), check blackboard[key]
         *      if true:
         *          tick child
         *      else:
         *          create observer on blackboard
         *          skip child
         *          if condition ever becomes true:
         *              composite aborts running node(s) and restarts the decorator
         * 
         * BOTH:
         *  On Init(), check blackboard[key]
         *      if true:
         *          create observer on blackboard
         *          tick child until child returns value or condition becomes false
         *      else:
         *          create observer on blackboard
         *          skip child
         *          if condition ever becomes true:
         *              composite aborts running node(s) and restarts the decorator
         * 
        */

        public override void NodeGUI()
        {
            inputKnob.DisplayLayout();
            outputKnob.DisplayLayout();


            blackboardKey = RTEditorGUI.TextField(blackboardKey);

            notify_rule = RTEditorGUI.Popup("Notify on:" + notify_rules[notify_rule], notify_rule, notify_rules);
            abort_rule = RTEditorGUI.Popup("Rule:" + aborts[abort_rule], abort_rule, aborts);
        }

        public override void Init()
        {
            blackboard = GameObject.FindWithTag("Monster").GetComponent<Blackboard>();

            BaseBTNode p = parent;
            if ((p as BaseComposite) != null)
            {
                parentCompsite = (BaseComposite)p;
                return;
            }
            while (p.parent != null && (p.parent as BaseComposite) != null)
            {
                p = p.parent;
            }
            parentCompsite = (BaseComposite)p;
        }

        public override void Start()
        {
            observedCorrectly = CheckCondition();

            if (observedCorrectly && (abort_rule == 1 || abort_rule == 3))
            {
                // SELF or BOTH
                blackboard.AddObserver(blackboardKey, this);
            }
            if (!observedCorrectly && (abort_rule == 2 || abort_rule == 3))
            {
                // LOWER_PRIORITY or BOTH
                blackboard.AddObserver(blackboardKey, this);
            }

        }

        public abstract bool CheckCondition();

        public override TaskResult ProcessTick()
        {
            if (debug)
                Debug.Log("Ticking " + Title);
            if (!observedCorrectly)
                return TaskResult.FAILURE;
            TaskResult childResult = children[0].Tick();
            if (childResult == TaskResult.FAILURE || childResult == TaskResult.SUCCESS)
            {
                blackboard.RemoveObserver(blackboardKey, this);
            }
            return childResult;
        }

        public void ReceiveNotification(bool condition)
        {
            if (debug)
                Debug.Log("Observer was notified.");
            if ((abort_rule == 1 || abort_rule == 3) && !condition)
                AbortSelf();
            else if ((abort_rule == 2 || abort_rule == 3) && condition)
                AbortLowerPriority();
        }

        private void AbortSelf()
        {
            if (debug)
                Debug.Log("Aborting " + Title);
            blackboard.RemoveObserver(blackboardKey, this);
            parentCompsite.DeactivateStatus();
            parentCompsite.childrenToRun.Dequeue(); // remove current node from parent composite's queue
        }

        private void AbortLowerPriority()
        {
            if (debug)
                Debug.Log(Title + " aborting lower priority nodes");
            blackboard.RemoveObserver(blackboardKey, this);
            parentCompsite.DeactivateStatus();
            parentCompsite.StartAtNode(this);
        }

    }

    [Node(false, "Behavior Tree/Decorators/BODs/CheckBool", new Type[] { typeof(BehaviorTreeCanvas) })]
    public class CheckBool : BaseBlackboardObserver
    {
        public const string ID = "CheckBool";
        public override string GetID { get { return ID; } }
        public override string Title { get { return blackboardKey; } }


        public override bool CheckCondition()
        {
            if (!blackboard.HasValue(blackboardKey))
            {
                return false;
            }
            return blackboard.GetValue<bool>(blackboardKey);
        }

    }

    [Node(false, "Behavior Tree/Decorators/BODs/CheckHasValue", new Type[] { typeof(BehaviorTreeCanvas) })]
    public class CheckHasValue : BaseBlackboardObserver
    {
        public const string ID = "CheckHasValue";
        public override string GetID { get { return ID; } }
        public override string Title { get { return "E: " + blackboardKey; } }


        public override bool CheckCondition()
        {
            return blackboard.HasValue(blackboardKey);
        }

    }

    [Node(false, "Behavior Tree/Decorators/BODs/GreaterThan", new Type[] { typeof(BehaviorTreeCanvas) })]
    public class GreaterThan : BaseBlackboardObserver
    {
        public const string ID = "GreaterThan";
        public override string GetID { get { return ID; } }
        public override string Title { get { return blackboardKey + ">" + x; } }

        [SerializeField]
        private float x = 1f;

        public override void NodeGUI()
        {
            inputKnob.DisplayLayout();
            outputKnob.DisplayLayout();

            blackboardKey = RTEditorGUI.TextField(blackboardKey);
            x = RTEditorGUI.FloatField("x:", x);

            notify_rule = RTEditorGUI.Popup("Notify on:" + notify_rules[notify_rule], notify_rule, notify_rules);
            abort_rule = RTEditorGUI.Popup("Rule:" + aborts[abort_rule], abort_rule, aborts);
        }

        public override bool CheckCondition()
        {
            if (!blackboard.HasValue(blackboardKey))
                return false;
            return blackboard.GetValue<float>(blackboardKey) > x;
        }

    }

    [Node(false, "Behavior Tree/Decorators/BODs/PathExists", new Type[] { typeof(BehaviorTreeCanvas) })]
    public class PathExists : BaseBlackboardObserver
    {
        public const string ID = "PathExists";
        public override string GetID { get { return ID; } }
        public override string Title { get { return blackboardKey.Length < 7 ? blackboardKey + "<" + maxLength : blackboardKey.Substring(0, 7) + "...<" + maxLength; } }

        private UnityEngine.AI.NavMeshAgent agent;
        private UnityEngine.AI.NavMeshPath path;
        [SerializeField]
        private float maxLength = 10f;

        public override void Init()
        {
            blackboard = GetManager().blackboard;
            agent = blackboard.GetComponent<UnityEngine.AI.NavMeshAgent>();
        }

        public override void NodeGUI()
        {
            inputKnob.DisplayLayout();
            outputKnob.DisplayLayout();

            blackboardKey = RTEditorGUI.TextField(blackboardKey);
            maxLength = RTEditorGUI.FloatField("len:" + maxLength, maxLength);

            notify_rule = RTEditorGUI.Popup("Notify on:" + notify_rules[notify_rule], notify_rule, notify_rules);
            abort_rule = RTEditorGUI.Popup("Rule:" + aborts[abort_rule], abort_rule, aborts);
        }

        public static float GetPathLength(UnityEngine.AI.NavMeshPath path)
        {
            float lng = 0.0f;

            if (path.status != UnityEngine.AI.NavMeshPathStatus.PathInvalid)
            {
                for (int i = 1; i < path.corners.Length; ++i)
                {
                    lng += Vector3.Distance(path.corners[i - 1], path.corners[i]);
                }
            }

            return lng;
        }

        public override bool CheckCondition()
        {
            path = new UnityEngine.AI.NavMeshPath();
            agent.CalculatePath(blackboard.GetValue<Vector3>(blackboardKey), path);
            return path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete && (maxLength == -1f || GetPathLength(path) <= maxLength);
        }

    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Standard;
using NodeEditorFramework.Utilities;
using System;
using System.Linq;

namespace NodeEditorFramework.Standard
{
    [Node(true, "Behavior Tree/Decorators/Decorator", new Type[] { typeof(BehaviorTreeCanvas) })]
    public abstract class BaseDecorator : BaseBTNode
    {
        public virtual Type GetObjectType { get { return this.GetType(); } }
        [ConnectionKnobAttribute("", Direction.In, "Flow", NodeSide.Top)]
        public ConnectionKnob inputKnob;
        [ConnectionKnobAttribute("", Direction.Out, "Flow", NodeSide.Bottom)]
        public ConnectionKnob outputKnob;
        private Color color_ = new Color(.03f, .034f, .55f, .75f);
        //public override Color backgroundColor { get { return color; } }
        public override Vector2 MinSize { get { return new Vector2(100, 25); } }
        public override bool AutoLayout { get { return true; } }

        void OnEnable()
        {
            backgroundColor = color_;
        }

        public override void NodeGUI()
        {
            inputKnob.DisplayLayout();
            outputKnob.DisplayLayout();
        }
        

    }

    [Node(false, "Behavior Tree/Decorators/Inverter", new Type[] { typeof(BehaviorTreeCanvas) })]
    public class Inverter : BaseDecorator
    {
        public const string ID = "inverter";
        public override string GetID { get { return ID; } }
        public override string Title { get { return "Inverter"; } }

        public override TaskResult ProcessTick(BehaviorTreeManager owner)
        {
            if (debug)
                Debug.Log("Ticking " + Title);
            TaskResult childStatus = children[0].Tick(owner);
            if (childStatus == TaskResult.SUCCESS)
            {
                status = TaskResult.FAILURE;
                return status;
            }
            else if (childStatus == TaskResult.FAILURE)
            {
                status = TaskResult.SUCCESS;
                return status;
            }
            status = TaskResult.RUNNING;
            return status;
        }

    }

    [Node(false, "Behavior Tree/Decorators/Cooldown", new Type[] { typeof(BehaviorTreeCanvas) })]
    public class Cooldown : BaseDecorator
    {
        public const string ID = "Cooldown";
        public override string GetID { get { return ID; } }
        public override string Title { get { return "Cooldown"; } }

        private float x = .5f;

        private float lastTime = 0f;
        private bool canRun = true;

        public override void NodeGUI()
        {
            inputKnob.DisplayLayout();
            outputKnob.DisplayLayout();

            x = RTEditorGUI.FloatField("X:" + x, x);
        }

        public override void Init(BehaviorTreeManager owner)
        {
            canRun = true;
        }

        public override void Begin(BehaviorTreeManager owner)
        {
            if (canRun || Time.time - lastTime >= x)
            {
                canRun = true;
                lastTime = Time.time;
            }
            else
            {
                canRun = false;
            }
        }

        public override TaskResult ProcessTick(BehaviorTreeManager owner)
        {
            if (debug)
                Debug.Log("Ticking " + Title);
            if (canRun)
            {
                canRun = false;
                return TaskResult.SUCCESS;
            }
            return TaskResult.FAILURE;
        }

    }

    [Node(false, "Behavior Tree/Decorators/RepeatUntilSuccess", new Type[] { typeof(BehaviorTreeCanvas) })]
    public class RepeatUntilSuccess : BaseDecorator
    {
        public const string ID = "RepeatUntilSuccess";
        public override string GetID { get { return ID; } }
        public override string Title { get { return "RepeatUntilSuccess"; } }

        public override void NodeGUI()
        {

        }

        public override void Init(BehaviorTreeManager owner)
        {

        }

        public override TaskResult ProcessTick(BehaviorTreeManager owner)
        {
            if (debug)
                Debug.Log("Ticking " + Title);

            TaskResult childResult = children[0].Tick(owner);
            if (childResult != TaskResult.SUCCESS)
            {
                status = TaskResult.RUNNING;
                return status;
            }
            status = TaskResult.SUCCESS;
            return status;
        }

    }

    [Node(false, "Behavior Tree/Decorators/TimeLimit", new Type[] { typeof(BehaviorTreeCanvas) })]
    public class TimeLimit : BaseDecorator
    {
        public const string ID = "TimeLimit";
        public override string GetID { get { return ID; } }
        public override string Title { get { return "TimeLimit"; } }

        private float x = 5f;

        private float start_time = 0f;

        public override void NodeGUI()
        {
            inputKnob.DisplayLayout();
            outputKnob.DisplayLayout();

            x = RTEditorGUI.FloatField("X:" + x, x);
        }

        public override void Begin(BehaviorTreeManager owner)
        {
            start_time = Time.time;
        }

        public override TaskResult ProcessTick(BehaviorTreeManager owner)
        {
            if (debug)
                Debug.Log("Ticking " + Title);
            if (Time.time - start_time > x)
            {
                var composite = children[0] as BaseComposite;
                if (composite != null)
                {
                    composite.ResetChildrenStatuses();
                }
                return TaskResult.FAILURE;
            }
            return children[0].Tick(owner);
        }

    }


}
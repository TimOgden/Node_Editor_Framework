using System;
using System.Collections;
using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(true, "Behavior Tree/Decorators/Services/Service", new Type[] { typeof(BehaviorTreeCanvas) })]
    public abstract class BaseService : BaseDecorator
    {
        public const string ID = "Service";
        public override string GetID { get { return ID; } }
        public override string Title { get { return "Service"; } }

        public float frequency;
        protected IEnumerator method;

        protected BaseComposite childComposite;
        public Blackboard blackboard;

        public override void NodeGUI()
        {
            inputKnob.DisplayLayout();
            outputKnob.DisplayLayout();
            frequency = RTEditorGUI.FloatField("freq:" + frequency, frequency);
        }

        public override void Init(BehaviorTreeManager owner)
        {
            childComposite = children[0] as BaseComposite;
            childComposite.Init(owner);
        }

        public override void Begin(BehaviorTreeManager owner)
        {
            method = Method();
            blackboard.StartCoroutine(method);
        }

        public override TaskResult ProcessTick(BehaviorTreeManager owner)
        {
            if (debug)
              Debug.Log("Ticking " + Title);

            TaskResult childResult = childComposite.Tick(owner);
            if(childResult==TaskResult.SUCCESS || childResult==TaskResult.FAILURE)
            {
                blackboard.StopCoroutine(method);
                return childResult;
            }
            
            return TaskResult.RUNNING;
        }

        public virtual IEnumerator Method()
        {
            while (true)
            {
                yield return null; // Run code
                yield return new WaitForSeconds(frequency);
            }
        }

    }
}
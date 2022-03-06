using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Inputs/Boolean")]
    public class BooleanInput : Node
    {
        public const string ID = "BooleanInput";
        public override string GetID { get { return ID; } }
        public override Vector2 MinSize { get { return new Vector2(200, 100); } }

        [ValueConnectionKnob("Output", Direction.Out, "Boolean", NodeSide.Right)]
        public ValueConnectionKnob outputKnob;

        bool isTrue = false;

        public override void NodeGUI()
        {
            outputKnob.DisplayLayout();
            isTrue = RTEditorGUI.Toggle(isTrue, "Value");

            if (GUI.changed)
                NodeEditor.curNodeCanvas.OnNodeChange(this);
        }

        public override bool Calculate()
        {
            outputKnob.SetValue<bool>(isTrue);
            return true;
        }
    }
}

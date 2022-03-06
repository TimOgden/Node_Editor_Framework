using UnityEngine;
using NodeEditorFramework;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Displays/Boolean")]
    public class BooleanDisplay : Node
    {
        public const string ID = "BooleanDisplay";
        public override string GetID { get { return ID; } }
        public override bool AutoLayout { get { return true; } }

        [ValueConnectionKnob("Input", Direction.In, "Boolean", NodeSide.Left)]
        public ValueConnectionKnob inputKnob;

        public override void NodeGUI()
        {
            inputKnob.DisplayLayout();
            Color temp = GUI.color;
            if (inputKnob.GetValue<bool>())
            {
                GUI.color = Color.green;
                GUILayout.Label("True");
            }
            else
            {
                GUI.color = Color.red;
                GUILayout.Label("False");
            }
            GUI.color = temp;

        }

    }

    
}

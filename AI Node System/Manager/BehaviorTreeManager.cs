using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Standard;

public enum TaskResult { NOT_ACTIVE, SUCCESS, FAILURE, RUNNING };

public class BehaviorTreeManager : MonoBehaviour
{
    public BehaviorTreeCanvas behaviorTree;
    public float tickingFrequency = .2f;
    public bool debug = false;

    private IEnumerator coroutine;

    void Awake()
    {
        coroutine = EvalTree();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(coroutine);
    }

    public void AssureCanvas()
    {
        if (behaviorTree == null)
            throw new UnityException("No canvas specified to calculate on " + name + "!");
    }

    private void SetupTree()
    {
        AssureCanvas();
        NodeEditor.checkInit(false);
        behaviorTree.Validate();
    }

    public IEnumerator EvalTree()
    {
        if(debug)
            Debug.Log("Starting evaluation of tree with status: " + behaviorTree.rootNode.GetStatus());
        while (true)
        {
            SetupTree();
            TaskResult status = behaviorTree.rootNode.Tick(this);
            
            if (!(status == TaskResult.NOT_ACTIVE || status == TaskResult.RUNNING))
            {
                if (debug)
                    Debug.Log(transform.name + " (" + behaviorTree.name + " behavior tree) returned: " + status);
                yield return status;
                yield return new WaitForSeconds(tickingFrequency); // pause after full tree traversal
                behaviorTree.rootNode.DeactivateStatus();
            }
            yield return new WaitForSeconds(tickingFrequency); // ticking frequency
        }
    }


}

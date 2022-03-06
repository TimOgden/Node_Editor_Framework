using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework.Standard;
using NodeEditorFramework.AI;

public class Manager : MonoBehaviour
{
    public BehaviorTreeCanvas behaviorTree;
    private IEnumerator coroutine;
    [HideInInspector]
    public Blackboard blackboard;

    void Awake()
    {
        blackboard = GetComponent<Blackboard>();
        behaviorTree.rootNode.manager = this;
        coroutine = behaviorTree.rootNode.EvalTree();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(coroutine);
    }

    void OnEnable()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }


}

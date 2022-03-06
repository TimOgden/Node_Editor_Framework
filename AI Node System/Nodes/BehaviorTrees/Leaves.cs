using System;
using NodeEditorFramework;
using NodeEditorFramework.Standard;
using UnityEngine;
using UnityEngine.Serialization;


/// <summary>
///  Leaf node to execute actions.
/// </summary>
namespace NodeEditorFramework.AI
{
	[Node(true, "Behavior Tree/Leaves/Leaf", new Type[] { typeof(BehaviorTreeCanvas) })]
	public abstract class BaseLeaf : BaseBTNode
	{
		public virtual Type GetObjectType { get { return this.GetType(); } }
		private Color color_ = new Color(0f, 1f, 0f, .5f);
		[ConnectionKnobAttribute("", Direction.In, "Flow", NodeSide.Top)]
		public ConnectionKnob inputKnob;
		void OnEnable()
		{
			backgroundColor = color_;
		}
		//public override Color backgroundColor { get { return color; } }
		public override Vector2 MinSize { get { return new Vector2(75, 35); } }
		public override bool AutoLayout { get { return true; } }

	}

    [Node(false, "Behavior Tree/Leaves/MoveTo", new Type[] { typeof(BehaviorTreeCanvas) })]
    public class MoveTo : BaseLeaf
    {
        public const string ID = "moveTo";
        public override string GetID { get { return ID; } }
        public override string Title { get { return "MoveTo"; } }

        private Blackboard blackboard;

        [SerializeField]
        private string blackboardKey;
        [SerializeField]
        private float speed;
        [SerializeField]
        private bool continuous;

        private Vector3 destination;
        private UnityEngine.AI.NavMeshPath path;

        public override void Init()
        {
            blackboard = GetManager().blackboard;
        }

        public override void Start()
        {
            destination = blackboard.GetValue<Vector3>(blackboardKey);
            monster.agent.speed = speed;
            monster.agent.SetDestination(destination);
        }

        public override void NodeGUI()
        {
            inputKnob.DisplayLayout();
            blackboardKey = RTEditorGUI.TextField(blackboardKey);
            speed = RTEditorGUI.FloatField("speed:" + speed, speed);
            continuous = RTEditorGUI.Toggle(continuous, "continuous:" + continuous);
        }


        public override TaskResult ProcessTick()
        {
            if (debug)
                Debug.Log("Ticking " + Title);

            if (continuous)
                monster.agent.SetDestination(blackboard.GetValue<Vector3>(blackboardKey));
            path = new UnityEngine.AI.NavMeshPath();
            monster.agent.CalculatePath(destination, path);
            if (path.status != UnityEngine.AI.NavMeshPathStatus.PathComplete)
                return TaskResult.FAILURE;

            if (!monster.agent.pathPending)
            {
                if (monster.agent.remainingDistance <= monster.agent.stoppingDistance)
                {
                    if (!monster.agent.hasPath || monster.agent.velocity.sqrMagnitude == 0f)
                    {
                        return TaskResult.SUCCESS;
                    }
                }
            }
            return TaskResult.RUNNING;
        }
    }

    [Node(false, "Behavior Tree/Leaves/NullBlackboardKey", new Type[] { typeof(BehaviorTreeCanvas) })]
    public class NullBlackboardKey : BaseLeaf
    {
        public const string ID = "NullBlackboardKey";
        public override string GetID { get { return ID; } }
        public override string Title { get { return "NullBlackboardKey"; } }

        [SerializeField]
        private string blackboardKey = "";

        private Blackboard blackboard;

        public override void NodeGUI()
        {
            inputKnob.DisplayLayout();

            blackboardKey = RTEditorGUI.TextField(blackboardKey);
        }

        public override void Init()
        {
            blackboard = GetManager().blackboard;

        }

        public override void Start()
        {
            blackboard.RemoveKey(blackboardKey);
        }

        public override TaskResult ProcessTick()
        {
            if (debug)
                Debug.Log("Ticking " + Title);
            return TaskResult.SUCCESS;
        }

    }

    [Node(false, "Behavior Tree/Leaves/RotateToFace", new Type[] { typeof(BehaviorTreeCanvas) })]
    public class RotateToFace : BaseLeaf
    {
        public const string ID = "RotateToFace";
        public override string GetID { get { return ID; } }
        public override string Title { get { return "RotateToFace"; } }


        public string blackboardKey;
        public float speed = 1f;
        private Quaternion desiredRotation;
        private Blackboard blackboard;
        private UnityEngine.AI.NavMeshAgent agent;

        public override void NodeGUI()
        {
            inputKnob.DisplayLayout();
            blackboardKey = RTEditorGUI.TextField(blackboardKey);
            speed = RTEditorGUI.FloatField("Speed:" + speed, speed);
        }

        public override void Init()
        {
            blackboard = GetManager().blackboard;
            //agent = blackboard.GetComponent<NavMeshAgent>();
            desiredRotation = Quaternion.LookRotation(blackboard.GetValue<Vector3>(blackboardKey) - blackboard.transform.position, Vector3.up);
            //agent.speed = 0f;
            //agent.SetDestination(blackboard.GetValue<Vector3>(blackboardKey));
        }

        public override TaskResult ProcessTick()
        {
            if (debug)
                Debug.Log("Ticking " + Title);
            if (Quaternion.Angle(blackboard.transform.rotation, desiredRotation) <= 15f)
            {
                return TaskResult.SUCCESS;
            }
            blackboard.transform.rotation = Quaternion.Slerp(blackboard.transform.rotation, desiredRotation, speed * Time.deltaTime);
            return TaskResult.RUNNING;
        }

    }

    [Node(false, "Behavior Tree/Leaves/SetRandomWalkPoint", new Type[] { typeof(BehaviorTreeCanvas) })]
    public class SetRandomWalkPoint : BaseLeaf
    {
        public const string ID = "setRandomWalkPoint";
        public override string GetID { get { return ID; } }
        public override string Title { get { return "SetRandomWalkPoint"; } }


        public float walkRadius = 2f;

        private Blackboard blackboard;
        [SerializeField]
        private string blackboardKey;

        public override void Init()
        {
            blackboard = GetManager().blackboard;
        }

        public override void Start()
        {
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * walkRadius;
            randomDirection += monster.transform.position;
            UnityEngine.AI.NavMeshHit hit;
            UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out hit, walkRadius, 1);
            blackboard.SetValue(blackboardKey, hit.position);
        }

        public override void NodeGUI()
        {
            inputKnob.DisplayLayout();
            walkRadius = RTEditorGUI.FloatField("r:" + walkRadius, walkRadius);
            blackboardKey = RTEditorGUI.TextField(blackboardKey);
        }
        public override TaskResult ProcessTick()
        {
            if (debug)
                Debug.Log("Ticking " + Title);
            return TaskResult.SUCCESS;
        }
    }

    [Node(false, "Behavior Tree/Leaves/Succeeder", new Type[] { typeof(BehaviorTreeCanvas) })]
    public class Succeeder : BaseLeaf
    {
        public const string ID = "succeeder";
        public override string GetID { get { return ID; } }
        public override string Title { get { return "Succeeder"; } }

        public override void NodeGUI()
        {
            inputKnob.DisplayLayout();
        }

        public override TaskResult ProcessTick()
        {
            if (debug)
                Debug.Log("Ticking " + Title);
            status = TaskResult.SUCCESS;
            return status;
        }
    }

    [Node(false, "Behavior Tree/Leaves/WaitNTicks", new Type[] { typeof(BehaviorTreeCanvas) })]
    public class WaitNTicks : BaseLeaf
    {
        public const string ID = "waitNTicks";
        public override string GetID { get { return ID; } }
        public override string Title { get { return "WaitNTicks"; } }
        public int n = 0;
        private int counter = 0; // counting up to n before returning SUCCESS

        public override void Start()
        {
            counter = 0;
        }

        public override void NodeGUI()
        {
            inputKnob.DisplayLayout();
            n = RTEditorGUI.IntField("N:" + n, n);
        }

        public override TaskResult ProcessTick()
        {
            if (debug)
                Debug.Log("Ticking " + Title);
            if (!HasBeenCalled)
            {
                Init();
                HasBeenCalled = true;
            }
            if (counter >= n)
            {
                counter = 0;
                status = TaskResult.SUCCESS;
                return TaskResult.SUCCESS;
            }
            counter++;
            status = TaskResult.RUNNING;
            return status;
        }
    }

    [Node(false, "Behavior Tree/Leaves/WaitXSeconds", new Type[] { typeof(BehaviorTreeCanvas) })]
    public class WaitXSeconds : BaseLeaf
    {
        public const string ID = "waitXSeconds";
        public override string GetID { get { return ID; } }
        public override string Title { get { return "WaitXSeconds"; } }
        public float x = 0f;
        private float start_time = 0; // counting up to x before returning SUCCESS

        public override void Start()
        {
            start_time = Time.time;
        }

        public override void NodeGUI()
        {
            inputKnob.DisplayLayout();
            x = RTEditorGUI.FloatField("X:" + x, x);
        }

        public override TaskResult ProcessTick()
        {
            if (debug)
                Debug.Log("Ticking " + Title);
            if (Time.time - start_time >= x)
            {
                status = TaskResult.SUCCESS;
                return TaskResult.SUCCESS;
            }
            status = TaskResult.RUNNING;
            return status;
        }
    }
}
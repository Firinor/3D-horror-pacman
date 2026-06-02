using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Enemy
{
    public class Biha_Enemy : MonoBehaviour
    {
        [FormerlySerializedAs("player")] public Transform Target;
        public NavMeshAgent NavMeshAgent;
        
        public List<Transform> patrolPoitns;
        public int patrolPointIndex = 0;
        
        public float amogusTime = 60f;
        public float amogusTimer;

        public float warningTime = 4f;
        public float warningTimer;
        
        public float patrolSpeed = 3f;
        public float runSpeed = 5f;

        private EnemyBehaviour behaviour;

        private Dictionary<behaviours, EnemyBehaviour> allBehaviours;

        private enum behaviours
        {
            Patrol,
            Warning,
            Banishment
        }

        private void Awake()
        {
            allBehaviours = new()
            {
                { behaviours.Patrol, new Patrol(this) },
                { behaviours.Warning, new Warning(this) },
                { behaviours.Banishment, new Banishment(this) },
            };

            behaviour = allBehaviours[behaviours.Patrol];
            behaviour.Initialize();
        }

        public void ToBunishment()
        {
            NavMeshAgent.speed = patrolSpeed;
            behaviour = allBehaviours[behaviours.Banishment];
        }

        public void ToWarning()
        {
            NavMeshAgent.speed = runSpeed;
            behaviour = allBehaviours[behaviours.Warning];
        }

        void Update()
        {
            amogusTimer -= Time.deltaTime;
            if (amogusTimer < 0)
            {
                amogusTimer = amogusTime;
                Transform newPoint = new GameObject("EnemyPatrolPoint" + patrolPoitns.Count).transform;
                newPoint.position = Target.position;
                patrolPoitns.Add(newPoint);
            }

            behaviour.Update();
        }
    }

    public abstract class EnemyBehaviour
    {
        public readonly Biha_Enemy Main;

        public EnemyBehaviour(Biha_Enemy owner)
        {
            Main = owner;
        }
        
        public abstract void Initialize();
        public abstract void Update();
    }

    public class Patrol : EnemyBehaviour
    {
        private float idleTime = 8f;
        private float idleTimer;

        public Patrol(Biha_Enemy owner) : base(owner) { }

        public override void Initialize()
        {
            Main.NavMeshAgent.SetDestination(Main.patrolPoitns[0].position);
        }

        public override void Update()
        {
            if (Main.NavMeshAgent.hasPath) 
                return;
            
            idleTimer -= Time.deltaTime;
            if (idleTimer > 0)
                return;

            idleTimer = idleTime;

            Main.patrolPointIndex++;
            Main.patrolPointIndex %= Main.patrolPoitns.Count;
            Main.NavMeshAgent.SetDestination(Main.patrolPoitns[Main.patrolPointIndex].position);

        }
    }

    public class Warning : EnemyBehaviour
    {
        public Warning(Biha_Enemy owner) : base(owner) { }

        public override void Initialize()
        {
            
        }
        
        public override void Update()
        {
            Main.transform.LookAt(Main.Target);
        }
    }

    public class Banishment : EnemyBehaviour
    {
        public Banishment(Biha_Enemy owner) : base(owner) { }
        
        public override void Initialize()
        {
            
        }
        
        public override void Update()
        {

        }
    }
}
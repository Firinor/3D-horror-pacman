using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy
{
    public class Biha_Enemy : MonoBehaviour
    {
        public Transform Target;
        
        public NavMeshAgent NavMeshAgent;
        
        public List<Transform> patrolPoitns;
        public Transform patrolParent;
        public int patrolPointIndex = 0;
        
        public Material behaviourMat;
        
        public Transform Head;
        public EnemyVision Vision;
        
        public float amogusTime = 60f;
        public float amogusTimer;

        public float idleTime = 8f;
        public float idleTimer;
        
        public float warningTime = 4f;
        public float warningTimer;
        
        public float runTime = 4f;
        public float runTimer;
        
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

            ToPatrol();
        }

        public void ToBunishment()
        {
            NavMeshAgent.speed = patrolSpeed;
            behaviour = allBehaviours[behaviours.Banishment];
            behaviour.Initialize();
        }

        public void ToWarning()
        {
            NavMeshAgent.isStopped = true;
            NavMeshAgent.speed = runSpeed;
            behaviour = allBehaviours[behaviours.Warning];
            behaviour.Initialize();
            
            behaviourMat.color = Color.red;
        }

        public void ToPatrol()
        {
            Head.rotation = Quaternion.identity;
            
            NavMeshAgent.isStopped = false;
            
            NavMeshAgent.speed = patrolSpeed;
            behaviour = allBehaviours[behaviours.Patrol];
            behaviour.Initialize();
            
            behaviourMat.color = Color.green;
        }
        
        void Update()
        {
            /*amogusTimer -= Time.deltaTime;
            if (amogusTimer < 0)
            {
                amogusTimer = amogusTime;
                Transform newPoint = new GameObject("EnemyPatrolPoint" + patrolPoitns.Count).transform;
                newPoint.SetParent(patrolParent);
                newPoint.position = Target.position;
                patrolPoitns.Add(newPoint);
            }*/

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
        public Patrol(Biha_Enemy owner) : base(owner) { }

        public override void Initialize()
        {
            Main.NavMeshAgent.SetDestination(Main.patrolPoitns[Main.patrolPointIndex].position);
        }

        public override void Update()
        {
            if (Main.Vision.IsCanSeeTarget())
            {
                Main.ToWarning();
                return;
            }
            
            if (Main.NavMeshAgent.hasPath) 
                return;
            
            Main.idleTimer -= Time.deltaTime;
            if (Main.idleTimer > 0)
                return;

            Main.idleTimer = Main.idleTime;

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
            Main.warningTimer = Main.warningTime;
            Main.runTimer = Main.runTime;
        }
        
        public override void Update()
        {
            Vector3 direction = Main.Target.position -  Main.transform.position;
            direction.y = 0;
            Main.transform.LookAt( Main.transform.position + direction);
            Main.Head.LookAt(Main.Target);
            
            Main.warningTimer -= Time.deltaTime;
            if (!(Main.warningTimer <= 0)) 
                return;
            
            Main.NavMeshAgent.isStopped = false;
            Main.NavMeshAgent.SetDestination(Main.Target.position);

            Main.runTimer -= Time.deltaTime;
            if(Main.runTimer <= 0)
                Main.ToPatrol();
        }
    }

    public class Banishment : EnemyBehaviour
    {
        public Banishment(Biha_Enemy owner) : base(owner) { }
        
        public override void Initialize()
        {
            Main.NavMeshAgent.Warp(GetFarPoint());
        }

        private Vector3 GetFarPoint()
        {
            Vector3 result = new();
            float maxDistance = 0;
            
            for (int i = 0; i < 4; i++)//only first 4 patrol points
            {
                float distance = Vector3.Distance(Main.Target.position, Main.patrolPoitns[i].position);
                
                if(distance <= maxDistance)
                    continue;

                maxDistance = distance;
                result = Main.patrolPoitns[i].position;
            }

            Debug.Log("NewPoint: " + result + " maxDistance: " + maxDistance);
            return result;
        }


        public override void Update()
        {
            Main.ToPatrol();
        }
    }
}
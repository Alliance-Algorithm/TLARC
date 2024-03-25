using UnityEngine;
using UnityEngine.AI;

namespace RMUL
{
    public abstract class PathFinder:MonoBehaviour
    {
        internal static NavMeshAgent agent;
        internal static NavMeshPath _path;
        internal static Vector3 _targetPos;

        abstract internal void Work(DecisionMaker.Status status);

        internal abstract Vector3 targetPos{get;set;}
        internal abstract  Vector2 dest{get;}
    }
}
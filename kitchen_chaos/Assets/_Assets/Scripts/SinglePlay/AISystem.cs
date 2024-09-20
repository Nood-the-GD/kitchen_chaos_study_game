using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class AISystem : MonoBehaviour
{
    // [SerializeField] private AIController _aiBot;
    [SerializeField] private NavMeshSurface _navMeshSurface;

    public void Init(Vector3 position)
    {
        this.transform.position = position;
        StartCoroutine(BakeSurface_CR(position));
    }

    IEnumerator BakeSurface_CR(Vector3 position)
    {
        yield return new WaitForSeconds(2f);
        //_navMeshSurface.BuildNavMesh();
        //_aiBot.transform.position = position;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestAI : MonoBehaviour
{
    [SerializeField] private AIController _agent;
    [SerializeField] private BaseCounter _baseCounter;
    [SerializeField] private RecipeSO _recipeSO;

    private void Start()
    {
    }

    void Update()
    {
        if (_agent.botStage == BotStage.Empty)
        {
            _agent.SetRecipeSO(_recipeSO);        
        }
        // Vector3 dir = _baseCounter.transform.position - this.transform.position;
        // dir.Normalize();
        // Vector3 newPos = this.transform.position + dir * 2f;
        // if(NavMesh.SamplePosition(_baseCounter.transform.position, out NavMeshHit hit, 100f, NavMesh.AllAreas))
        // {
        //     Debug.Log("True");
        //     _agent.SetDestination(hit.position);
        //     // _agent.transform.position = hit.position;
        // }
    }
}

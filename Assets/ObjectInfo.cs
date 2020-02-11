using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ObjectInfo : MonoBehaviour
{
    public TaskList task;
    public ResourceManager RM;

    GameObject targetNode;
    public enum HeldResources { Food}
    public NodeManager.ResourceTypes heldResourceType;
    public bool isSelected = false;
    public bool isGathering = false;
    public int maxHeldResource;

    GameObject[] drops;

    public string ObjectName;

    private NavMeshAgent agent;

    public int heldResource;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GatherTick());
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1) && isSelected)
        {
            RightClick();
        }
        if(heldResource >= maxHeldResource)
        {
            drops = GameObject.FindGameObjectsWithTag("Drop");
            agent.destination = GetClosestDropOff(drops).transform.position;
            drops = null;
            task = TaskList.Delivering;
        }
        if(targetNode == null)
        {
            if(heldResource != 0)
            {
                drops = GameObject.FindGameObjectsWithTag("Drop");
                agent.destination = GetClosestDropOff(drops).transform.position;
                drops = null;
                task = TaskList.Delivering;
            }
            else
            {
              task = TaskList.Idle;
            }
            
        }
       
    }
    GameObject GetClosestDropOff(GameObject[] dropOffs)
    {
        GameObject closestDrop = null;
        float closestDistance = Mathf.Infinity;
        Vector3 position = transform.position;
     
        foreach(GameObject targetDrop in dropOffs)
        {
           
            Vector3 direction = targetDrop.transform.position - position;
            float distance = direction.sqrMagnitude;
            if(distance < closestDistance)
            {
                closestDistance = distance;
                closestDrop = targetDrop;

            }
        }
        return closestDrop;
   
    }
    public void RightClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray,out hit, 100))
        {
            if(hit.collider.tag == "Ground")
            {
                agent.destination = hit.point;
                task = TaskList.Moving;
            }else if (hit.collider.tag == "Resource")
            {
                agent.destination = hit.collider.gameObject.transform.position;
                task = TaskList.Gathering;
                targetNode = hit.collider.gameObject;
            }
            
        }
       
    }
    public void OnTriggerEnter(Collider other)
    {
        GameObject hitObject = other.gameObject;
        if(hitObject.tag == "Resource" && task == TaskList.Gathering)
        {
            isGathering = true;
            hitObject.GetComponent<NodeManager>().gatherers++;
            heldResourceType = hitObject.GetComponent<NodeManager>().resourceType;
        }
        else if( hitObject.tag == "Drop" && task == TaskList.Delivering)
        {
            if (RM.food >= RM.maxFood)
            {
                task = TaskList.Idle;
                Debug.Log("idle");
            }
            else
            {
                RM.food += heldResource;
                heldResource = 0;
                task = TaskList.Gathering;
                agent.destination = targetNode.transform.position;
            }
        }
    }
    public void OnTriggerExit(Collider other)
    {
        GameObject hitObject = other.gameObject;

        if(hitObject.tag == "Resource")
        {
            isGathering = false;
            hitObject.GetComponent<NodeManager>().gatherers--;
            
        }
    }
    IEnumerator GatherTick()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (isGathering)
            {
               heldResource++;
            }
            
        }
    }
}

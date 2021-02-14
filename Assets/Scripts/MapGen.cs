using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGen : MonoBehaviour
{
    public GameObject[] Rooms;
    public GameObject[] Corridors;
    public GameObject[] MapElements;
    public float CorridorToRoomRation = 0.75f;
    public int IterationThreshold = 10;
    private List<Anchor> newUnconnectedExits;

    // Start is called before the first frame update
    void Start()
    {
        newUnconnectedExits = new List<Anchor>();
        GenerateDungeon();
    }

    public void GenerateDungeon(){
        
        //place start room at Vector3.Zero
        GameObject firstRoom = Instantiate(Rooms[0], Vector3.zero, Quaternion.identity, this.transform);

        GameObject newCorridor = Corridors[Random.Range(0, Corridors.Length)];

        Anchor[] allExits = firstRoom.GetComponentsInChildren<Anchor>();
        Anchor anchor = allExits[0];
        GameObject anchorGO = anchor.gameObject;

        var newlySpawnedGO = SpawnNextGameObject(anchorGO, newCorridor);

        //start recursive iteration
        for (int i = 0; i < IterationThreshold; i++)
        {
            FillListOfExists();
        }
    }

    void FillListOfExists(){

        //check if previous element was a Room - then spawn corridor - PRESUMES THERE IS ONLY ONE EXIT
        //else if it is a corridor, check exits - if there is only one, spawn another corridor
        //if there is more than one exit, make one an exit and random assign the rest (use threshold)
        FindAllOpenExits();

        for (int i = 0; i < newUnconnectedExits.Count; i++)
        {
            Anchor anchor = newUnconnectedExits[i];
            GameObject anchorGO = anchor.gameObject;


            if(anchor.transform.parent.tag == "Room"){

                //spawn corridor
                GameObject newCorridor = Corridors[Random.Range(0, Corridors.Length)];
                var newlySpawnedGO = SpawnNextGameObject(anchorGO, newCorridor);
            }

            if(anchor.transform.parent.tag == "Corridor")
            {               
                //THIS IS JUST A TEST 
                if(Random.Range(0, 5) < 4){
                    GameObject newCorridor = Corridors[Random.Range(0, Corridors.Length)];
                    var newlySpawnedGO = SpawnNextGameObject(anchorGO, newCorridor);
                }
                else {
                    GameObject newRoom = Rooms[Random.Range(0, Rooms.Length)];
                    var newlySpawnedGO = SpawnNextGameObject(anchorGO, newRoom);
                }
            }
        }
    }

    GameObject SpawnNextGameObject(GameObject oldAnchorGO, GameObject gameObjectToPlace){

        //instantiate corridor
        GameObject newAnchorGO;
        GameObject newGO = Instantiate(gameObjectToPlace, oldAnchorGO.transform.position, Quaternion.identity, this.transform);

        //find entry anchor and rotate
        foreach(Transform child in newGO.transform)
        {
            if(child.tag == "Entry"){
                newAnchorGO = child.gameObject;

                float AngleDiff = 0f;
                if(oldAnchorGO.transform.rotation.y == 0)
                    AngleDiff =  180 + Quaternion.Angle(newAnchorGO.transform.rotation, oldAnchorGO.transform.rotation);
                else
                    AngleDiff =  180 - Quaternion.Angle(newAnchorGO.transform.rotation, oldAnchorGO.transform.rotation);               

                newGO.transform.RotateAround(newAnchorGO.transform.position, Vector3.up, AngleDiff);

                //reposition
                var offset = oldAnchorGO.transform.position - newAnchorGO.transform.position;
                newGO.transform.position = newGO.transform.position + offset;

                //check if collides - if so, then mark anchor as blocked else make it connected
                // RaycastHit m_Hit;
                // float m_MaxDistance = 0.5f;
                // var goCollider = newGO.GetComponent<BoxCollider>();
                // Debug.Log(newGO.transform.localScale);
                // bool m_HitDetect = Physics.BoxCast(goCollider.bounds.center, newGO.transform.localScale, newGO.transform.forward, 
                // out m_Hit, newGO.transform.rotation, m_MaxDistance);

                // if (m_HitDetect)
                // {
                //     //Output the name of the Collider your Box hit
                //     // Debug.Log("Hit : " + m_Hit.collider.name);
                //     // Debug.Log("Hit : " + m_Hit.collider.transform.parent.name);

                //     var r = m_Hit.collider.transform.gameObject.GetComponent<Renderer>();

                //     //Call SetColor using the shader property name "_Color" and setting the color to red
                //     r.material.SetColor("_Color", Color.red);

                //     // m_Hit.collider.transform.gameObject.GetComponent<Material>().color = Color.red;
                // }

                //Use the OverlapBox to detect if there are any other colliders within this box area.
                //Use the GameObject's centre, half the size (as a radius) and rotation. This creates an invisible box around your GameObject.
                LayerMask m_LayerMask = LayerMask.GetMask("Terrain");
                Collider[] hitColliders = Physics.OverlapBox(newGO.transform.position, newGO.transform.localScale / 2, Quaternion.identity, m_LayerMask);
                int i = 0;
                //Check when there is a new collider coming into contact with the box
                while (i < hitColliders.Length)
                {
                    //Output all of the collider names
                    Debug.Log("Hit : " + hitColliders[i].name + i);

                    //Increase the number of Colliders in the array
                    i++;
                }

                //set if OK
                SetAnchorAsConnected(oldAnchorGO);
            }
        }

        return newGO;
    }

    void SetAnchorAsConnected(GameObject anchorGO){
        Anchor anchor = anchorGO.GetComponent<Anchor>();
        anchor.IsConnected = true;
        anchor.IsBlocked = false;
    }

    void SetAnchorAsBloced(GameObject anchorGO){
        Anchor anchor = anchorGO.GetComponent<Anchor>();
        anchor.IsBlocked = true;
        anchor.IsConnected = false;
    }


    void FindAllOpenExits(){
        newUnconnectedExits.Clear();
        Anchor[] allExits = gameObject.GetComponentsInChildren<Anchor>();

        foreach(Anchor anchor in allExits){
            if(!anchor.IsConnected && !anchor.IsBlocked && anchor.transform.tag == "Exit")
                newUnconnectedExits.Add(anchor);
        }   
    }
}

using UnityEngine;

public class FishInstantiator : MonoBehaviour
{
    public GameObject fishPrefab;
    public GameObject[] units;
    public int numUnits = 10;
    public Vector3 range = new Vector3(5, 5, 5);
    public GameObject hiddelLeader;

    public Joystick joystick;
    public float speed = 40f;

    public GameObject obstacle;




    private void Start()
    {
        units = new GameObject[numUnits];
        for (int i = 0; i < numUnits; i++)
        {
            Vector3 unitPos = new Vector3(Random.Range(-range.x, range.x), Random.Range(-range.y, range.y), Random.Range(0, 0));
            units[i]= Instantiate(fishPrefab, this.transform.position + unitPos, Quaternion.identity) as GameObject;
            units[i].GetComponent<Flocking>().manager = this.gameObject;
            units[i].GetComponent<Flocking>().Obstacle = obstacle;
        }

        hiddelLeader.transform.position = new Vector2(-0.5f,3f);
        

    }

    private void Update()
    {
        

        Vector3 movement = new Vector3(joystick.Horizontal / 80, joystick.Vertical /80,0);

        // movement *= speed ;

        // hiddelLeader.transform.Translate(movement *speed* Time.deltaTime);
        hiddelLeader.transform.position += movement;

        for (int i = 0; i < numUnits; i++)
        {
            if (movement.magnitude < 0.01f)
            {

                units[i].GetComponent<Flocking>().maxSpeed = 1.5f;
                units[i].GetComponent<Flocking>().separationAmount = 1.5f;
            }
            else
            {
                units[i].GetComponent<Flocking>().maxSpeed = 5.48f;
                units[i].GetComponent<Flocking>().separationAmount = 1.4f;
            }   
        }
        
    }
}
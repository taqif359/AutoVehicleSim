using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NeuralNetwork))]
public class drive : MonoBehaviour
{
    private Vector3 startPos, startRot;
    private NeuralNetwork network;

    [Range(-1f,1f)]
    public float a,t;

    public float timeSinceStart = 0f;

    [Header("Fitness")]
    public float fitness;
    //following values indicate which of them is more important to the fitness
    public float distMultiplier = 1.4f;
    public float avgSpeedMultiplier = 0.2f;
    public float sensorMultiplier = 0.1f;   //tells car how important it is to staw awy from obstacles

    [Header("Network Options")]
    public int LAYERS = 1;
    public int NEURONS = 10;


    private Vector3 lastPos;
    private float totalDistance;
    private float avgSpeed;

    private float aSensor, bSensor, cSensor;

    private void Awake() {
        startPos = transform.position;
        startRot = transform.eulerAngles;
        network = GetComponent<NeuralNetwork>();

        
    }

    private void Death()
    {
        GameObject.FindObjectOfType<GeneticManager>().Death(fitness,network);
    }

    public void ResetWithNetwork(NeuralNetwork net){
        network = net;
        Reset();
    }

    public void Reset() {

        

        timeSinceStart = 0f;
        totalDistance = 0f;
        avgSpeed = 0f;
        lastPos = startPos;
        fitness = 0f;
        transform.position = startPos;
        transform.eulerAngles = startRot;
    }

    private void OnCollisionEnter (Collision collision) {
        if (collision.gameObject.transform.parent.parent.name == "Shoulder")
        {
            // print("Collision");
            Death();
        }
        // print(collision.gameObject.name);
        // Reset();
    }

    private void FixedUpdate() {
        InputSensors();
        lastPos = transform.position;

        //Neural network code here
        (a,t) = network.RunNetwork(aSensor,bSensor,cSensor);

        MoveCar(a,t);

        timeSinceStart += Time.deltaTime;

        CalculateFitness();

        // a=0;
        // t=0;

    }

    private void CalculateFitness() {
        //distance between lastPos and current is added to total distance
        totalDistance += Vector3.Distance(transform.position, lastPos);
        avgSpeed = totalDistance/timeSinceStart;

        fitness = (totalDistance * distMultiplier) + (avgSpeed*avgSpeedMultiplier) + (((aSensor + bSensor + cSensor)/3)*sensorMultiplier);

        if (timeSinceStart > 20 && fitness < 40){   //NOTE: for our course we may need to tweak these values
            Death();
        }

        if(fitness >= 1000){    //NOTE: same as above note
            //saves network to JSON
            Death();
        }
    }

    private void InputSensors() {
        //transform.forward... because its relative to the car
        Vector3 a = (transform.forward + transform.right);
        Vector3 b = (transform.forward);
        Vector3 c = (transform.forward - transform.right);

        Ray r = new Ray(transform.position, a);
        RaycastHit hit;

        if (Physics.Raycast(r, out hit)){
            aSensor = hit.distance/400;  //normalize the value before passing it to the neural network
            Debug.DrawLine(r.origin, hit.point, Color.red);
            // print("A:" + aSensor);
        }

        r.direction = b;
        if (Physics.Raycast(r, out hit)){
            bSensor = hit.distance/400;  //normalize the value before passing it to the neural network
            Debug.DrawLine(r.origin, hit.point, Color.blue);
            // print("B:" + bSensor);
        }

        r.direction = c;
        if (Physics.Raycast(r, out hit)){
            cSensor = hit.distance/400;  //normalize the value before passing it to the neural network
            Debug.DrawLine(r.origin, hit.point, Color.green);
            // print("C:" + cSensor);
        }
    }

    private Vector3 inp;
    public void MoveCar(float v, float h){
        inp = Vector3.Lerp(Vector3.zero, new Vector3(0,0,v*11.4f), 0.02f);
        inp = transform.TransformDirection(inp);    //to make input direction relative to car
        transform.position += inp;

        transform.eulerAngles += new Vector3(0, (h*90)*0.02f,0);
    }
    // public float speed = 50f;
    // public float torque = 5f;

    // // Start is called before the first frame update
    // void Start()
    // {
        
    // }

    // // Update is called once per frame
    // void Update()
    // {
    //     float vertical = Input.GetAxis("Vertical");
    //     float horizontal = Input.GetAxis("Horizontal");
    //     float dtime = Time.deltaTime;
    //     MoveCar(horizontal, vertical, dtime);
    // }

    // private void MoveCar (float horizontal, float vertical, float dtime)
    // {
    //     // Linear Movement
    //     float distance_moved = speed * vertical;
    //     transform.Translate(dtime * distance_moved * Vector3.forward);

    //     // Rotational Movement
    //     float rotation = horizontal * torque * 90f;
    //     transform.Rotate(0f, rotation * dtime, 0f);
    // }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NeuralNetwork))]
public class drive : MonoBehaviour
{
    private Vector3 startPos, startRot;
    private NeuralNetwork network;
    [Range(-1f,1f)]
    public float a,t;   //acceleration and turning values
    public float elapsedTimeSinceStart = 0f;

    [Header("Fitness")]
    public float fitness;
    //following values indicate which of them is more important to the fitness
    public float distMultiplier = 1.4f;
    public float speedMultiplier = 0.2f;
    public float sensorMultiplier = 1.5f;   //tells car how important it is to staw away from obstacles

    [Header("Network Options")]
    public int LAYERS = 1;
    public int NEURONS = 10;
    private Vector3 lastPos;
    private float totalDistance;
    private float speed;
    private float sensorA, sensorB, sensorC;

    private void Awake() {
        startPos = transform.position;
        startRot = transform.eulerAngles;
        network = GetComponent<NeuralNetwork>(); 
    }
    public void ResetByNetwork(NeuralNetwork net){
        network = net;
        Reset();
    }

    public void Reset() {
        elapsedTimeSinceStart = 0f;
        totalDistance = 0f;
        speed = 0f;
        lastPos = startPos;
        fitness = 0f;
        transform.position = startPos;
        transform.eulerAngles = startRot;
    }

    private void OnCollisionEnter (Collision collision) {
        if (collision.gameObject.transform.parent.parent.name == "Shoulder")
        {
            // print("Collision");
            Respawn();
        }
        // print(collision.gameObject.transform.name);
    }

    private void FixedUpdate() {
        SensorInput();
        lastPos = transform.position;

        //Run Neural Network
        (a,t) = network.RunNetwork(sensorA,sensorB,sensorC);
        MoveCar(a,t);
        elapsedTimeSinceStart += Time.deltaTime;
        CalculateFitness();
    }
    private void CalculateFitness() {
        //distance between lastPos and current is added to total distance
        totalDistance += Vector3.Distance(transform.position, lastPos);
        speed = totalDistance/elapsedTimeSinceStart;
        fitness = (totalDistance * distMultiplier) + (speed*speedMultiplier) + (((sensorA + sensorB + sensorC)/3)*sensorMultiplier);
        // print(fitness);
        if (elapsedTimeSinceStart > 20 && fitness < 40)
        { 
            Respawn();
        }
    }

    private void Respawn()
    {
        GameObject.FindObjectOfType<GeneticManager>().Respawn(fitness,network);
    }

    private void SensorInput() {
        //transform.forward... because its relative to the car
        Vector3 a = (transform.forward + transform.right);
        Vector3 b = (transform.forward);
        Vector3 c = (transform.forward - transform.right);

        Ray r = new Ray(transform.position, a);
        RaycastHit hit;

        if (Physics.Raycast(r, out hit)){
            sensorA = hit.distance/500;  //normalize the value before passing it to the neural network
            Debug.DrawLine(r.origin, hit.point, Color.red);
            // print("A:" + sensorA);
        }

        r.direction = b;
        if (Physics.Raycast(r, out hit)){
            sensorB = hit.distance/500;  //normalize the value before passing it to the neural network
            Debug.DrawLine(r.origin, hit.point, Color.blue);
            // print("B:" + sensorB);
        }

        r.direction = c;
        if (Physics.Raycast(r, out hit)){
            sensorC = hit.distance/500;  //normalize the value before passing it to the neural network
            Debug.DrawLine(r.origin, hit.point, Color.green);
            // print("C:" + sensorC);
        }
    }

    private Vector3 input;
    public void MoveCar(float v, float h){
        input = Vector3.Lerp(Vector3.zero, new Vector3(0,0,v*11.4f), 0.02f);
        input = transform.TransformDirection(input);    //to make input direction relative to car
        transform.position += input;
        transform.eulerAngles += new Vector3(0, (h*90)*0.02f,0);
    }
}

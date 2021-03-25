using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra;

public class GeneticManager : MonoBehaviour
{
    [Header("References")]
    public drive carController;

    [Header("Controls")]
    public int initialPopulation = 85;
    [Range(0.0f,1.0f)]
    public float mutationRate = 0.055f;

    [Header("Crossover Controls")]
    public int bestAgentSelection = 8;  //how many of the best
    public int workAgentSelection = 3;  //how many of the worst
    public int numberToCrossover;

    private List<int> genePool = new List<int>();

    private int naturallySelected;  //shows how many were selected instead of randomly generated

    private NeuralNetwork[] population;

    [Header("Public View")]
    public int currentGeneration;
    public int currentGenome = 0;

    private void Start() {
        CreatePopulation();
    }

    private void CreatePopulation()
    {
        population = new NeuralNetwork[initialPopulation];
        FillPopulationWithRandomValues(population,0);
        ResetToCurrentGenome();
    }

    private void ResetToCurrentGenome(){
        
        carController.ResetWithNetwork(population[currentGenome]);
    }

    //anything that hasnt been accounted for by crossover will be randomized
    private void FillPopulationWithRandomValues(NeuralNetwork[] newPopulation, int startIndex)
    {
        while(startIndex < initialPopulation)
        {
            // newPopulation[startIndex] = new NeuralNetwork();
            newPopulation[startIndex] = gameObject.AddComponent<NeuralNetwork>();

            newPopulation[startIndex].Initialize(carController.LAYERS, carController.NEURONS);
            startIndex++;
        }
        
    }

    public void Death (float fitness, NeuralNetwork network)
    {
        if (currentGenome < population.Length -1)
        {
            population[currentGenome].fitness = fitness;
            currentGenome++;
            ResetToCurrentGenome();
        }
        else
        {
            Repopulate();
        }
    }

    private void Repopulate()
    {
        genePool.Clear();
        currentGeneration++;
        naturallySelected=0;
        SortPopulation();

        NeuralNetwork[] newPopulation = pickBestPopulation();

        Crossover(newPopulation);
        Mutate(newPopulation);

        FillPopulationWithRandomValues(newPopulation, naturallySelected);

        population = newPopulation;
        currentGenome = 0;
        ResetToCurrentGenome();
    }

    private void Mutate (NeuralNetwork[] newPopulation)
    {
        for (int i =0; i<naturallySelected; i++)
        {
            for(int c=0; c<newPopulation[i].weights.Count; c++)
            {
                if(Random.Range(0.0f,1.0f) < mutationRate)
                {
                    newPopulation[i].weights[c] = MutateMatrix(newPopulation[i].weights[c]);
                }
            }
        }
    }

    Matrix<float> MutateMatrix(Matrix<float> A)
    {
        //the 7 is optional. leaving it blank the function will divide it by a value nonetheless
        int randomPoints = Random.Range(1, (A.RowCount * A.ColumnCount) / 7);

        Matrix<float> C = A;

        for(int i=0; i<randomPoints; i++)
        {
            int randomColumn = Random.Range(0, C.ColumnCount);
            int randomRow = Random.Range(0,C.RowCount);

            C[randomRow,randomColumn] = Mathf.Clamp(C[randomRow,randomColumn] + Random.Range(-1f,1f), -1f, 1f);
        }
        return C;
    }

    private void Crossover (NeuralNetwork[] newPopulation)
    {
        for(int i =0; i <numberToCrossover; i+=2)
        {
            int AIndex = i;
            int BIndex = i+1;

            if(genePool.Count >=1)
            {
                for(int c=0; c<100; c++)
                {
                    AIndex = genePool[Random.Range(0, genePool.Count)];
                    BIndex = genePool[Random.Range(0, genePool.Count)];

                    if(AIndex!= BIndex)
                        break;
                }
            }

            NeuralNetwork Child1 = new NeuralNetwork();
            NeuralNetwork Child2 = new NeuralNetwork();

            Child1.Initialize(carController.LAYERS, carController.NEURONS);
            Child2.Initialize(carController.LAYERS, carController.NEURONS);

            Child1.fitness =0;
            Child2.fitness =0;

            for(int w = 0; w<Child1.weights.Count; w++)
            {
                if(Random.Range(0.0f,1.0f) < 0.5f)
                {
                    Child1.weights[w] = population[AIndex].weights[w];
                    Child2.weights[w] = population[BIndex].weights[w];
                }
                else
                {
                    Child2.weights[w] = population[AIndex].weights[w];
                    Child1.weights[w] = population[BIndex].weights[w];
                }
            }

            for(int w = 0; w<Child1.biases.Count; w++)
            {
                if(Random.Range(0.0f,1.0f) < 0.5f)
                {
                    Child1.biases[w] = population[AIndex].biases[w];
                    Child2.biases[w] = population[BIndex].biases[w];
                }
                else
                {
                    Child2.biases[w] = population[AIndex].biases[w];
                    Child1.biases[w] = population[BIndex].biases[w];
                }
            }

            newPopulation[naturallySelected] = Child1;
            naturallySelected++;

            newPopulation[naturallySelected] = Child2;
            naturallySelected++;

        }
    }

    private NeuralNetwork[] pickBestPopulation()
    {
        NeuralNetwork[] newPopulation = new NeuralNetwork[initialPopulation];

        for(int i=0; i<bestAgentSelection; i++)
        {
            newPopulation[naturallySelected] = population[i].InitializeCopy(carController.LAYERS, carController.NEURONS);
            newPopulation[naturallySelected].fitness = 0;
            naturallySelected++;

            int f = Mathf.RoundToInt(population[i].fitness *10);

            for(int c=0; c<f; c++)
            {
                genePool.Add(i);
            }

        }

        for(int i=0; i< workAgentSelection; i++)
        {
            int last = population.Length - 1;
            last -= i;

            int f = Mathf.RoundToInt(population[last].fitness *10);

            for(int c=0; c<f; c++)
            {
                genePool.Add(last);
            }

        }

        return newPopulation;
    }

    private void SortPopulation()
    {
        //bubble sort highest to lowest fitness
        for(int i =0; i<population.Length; i++)
        {
            for(int j=i; j<population.Length; j++)
            {
                if(population[i].fitness < population[j].fitness)
                {
                    NeuralNetwork temp = population[i];
                    population[i] = population[j];
                    population[j] = temp;
                }
            }
        }
    }

}

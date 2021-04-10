using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra;

public class GeneticManager : MonoBehaviour
{
    [Header("References")]
    public drive vehicleController;

    [Header("Controls")]
    public int initPopulation = 15;
    [Range(0.0f,1.0f)]
    public float mutationRate = 0.055f;

    [Header("Crossover Controls")]
    public int numberOfBestAgents = 8;  //how many of the best
    public int numberOfWorstAgents = 1;  //how many of the worst
    public int numberToCrossover;
    private List<int> genePool = new List<int>();
    private int numberToNaturallySelect;  //shows how many were selected instead of randomly generated
    private NeuralNetwork[] population;

    [Header("Public View")]
    public int currentGeneration;
    public int currentGenome = 0;

    private void Start() 
    {
        CreatePopulation();
    }

    private void CreatePopulation()
    {
        population = new NeuralNetwork[initPopulation];
        RandomizePopulation(population,0);
        ResetToCurrentGenome();
    }
    //anything that hasnt been accounted for by crossover will be randomized
    private void RandomizePopulation(NeuralNetwork[] nextPopulation, int startIndex)
    {
        while(startIndex < initPopulation)
        {
            nextPopulation[startIndex] = gameObject.AddComponent<NeuralNetwork>();
            nextPopulation[startIndex].Initialize(vehicleController.LAYERS, vehicleController.NEURONS);
            startIndex++;
        }    
    }
    private void ResetToCurrentGenome()
    {    
        vehicleController.ResetByNetwork(population[currentGenome]);
    }

    public void Respawn (float fitness, NeuralNetwork network)
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
        numberToNaturallySelect=0;
        SortPopulation();
        NeuralNetwork[] nextPopulation = pickBestPopulation();
        Crossover(nextPopulation);
        Mutate(nextPopulation);
        RandomizePopulation(nextPopulation, numberToNaturallySelect);
        population = nextPopulation;
        currentGenome = 0;
        ResetToCurrentGenome();
    }
    Matrix<float> MutateMatrix(Matrix<float> M)
    {
        int randomPoints = Random.Range(1, (M.RowCount * M.ColumnCount));
        Matrix<float> C = M;

        for(int i=0; i<randomPoints; i++)
        {
            int randomColumn = Random.Range(0, C.ColumnCount);
            int randomRow = Random.Range(0,C.RowCount);
            C[randomRow,randomColumn] = Mathf.Clamp(C[randomRow,randomColumn] + Random.Range(-1f,1f), -1f, 1f);
        }
        return C;
    }

    private void Mutate (NeuralNetwork[] nextPopulation)
    {
        for (int i =0; i<numberToNaturallySelect; i++)
        {
            for(int j=0; j<nextPopulation[i].weightValues.Count; j++)
            {
                if(Random.Range(0.0f,1.0f) < mutationRate)
                {
                    nextPopulation[i].weightValues[j] = MutateMatrix(nextPopulation[i].weightValues[j]);
                }
            }
        }
    }

    private void Crossover (NeuralNetwork[] nextPopulation)
    {
        for(int i =0; i <numberToCrossover; i+=2)
        {
            int indexA = i;
            int indexB = i+1;

            if(genePool.Count > 0)
            {
                for(int c=0; c<100; c++)
                {
                    indexA = genePool[Random.Range(0, genePool.Count)];
                    indexB = genePool[Random.Range(0, genePool.Count)];
                    if(indexA!= indexB)
                        break;
                }
            }

            NeuralNetwork firstChild = new NeuralNetwork();
            NeuralNetwork secondChild = new NeuralNetwork();

            firstChild.Initialize(vehicleController.LAYERS, vehicleController.NEURONS);
            secondChild.Initialize(vehicleController.LAYERS, vehicleController.NEURONS);

            firstChild.fitness = 0;
            secondChild.fitness = 0;

            for(int w = 0; w < firstChild.weightValues.Count; w++)
            {
                if(Random.Range(0.0f,1.0f) < 0.5f)
                {
                    firstChild.weightValues[w] = population[indexA].weightValues[w];
                    secondChild.weightValues[w] = population[indexB].weightValues[w];
                }
                else
                {
                    secondChild.weightValues[w] = population[indexA].weightValues[w];
                    firstChild.weightValues[w] = population[indexB].weightValues[w];
                }
            }

            for(int w = 0; w < firstChild.biasValues.Count; w++)
            {
                if(Random.Range(0.0f,1.0f) < 0.5f)
                {
                    firstChild.biasValues[w] = population[indexA].biasValues[w];
                    secondChild.biasValues[w] = population[indexB].biasValues[w];
                }
                else
                {
                    secondChild.biasValues[w] = population[indexA].biasValues[w];
                    firstChild.biasValues[w] = population[indexB].biasValues[w];
                }
            }

            nextPopulation[numberToNaturallySelect] = firstChild;
            numberToNaturallySelect++;

            nextPopulation[numberToNaturallySelect] = secondChild;
            numberToNaturallySelect++;

        }
    }

    private void SortPopulation()
    {
        for(int i =0; i<population.Length -1; i++)
        {
            for(int j=0; j<population.Length - i - 1; j++)
            {
                if(population[j].fitness < population[j+1].fitness)
                {
                    NeuralNetwork temp = population[j];
                    population[j] = population[j+1];
                    population[j+1] = temp;
                }
            }
        }
    }
    private NeuralNetwork[] pickBestPopulation()
    {
        NeuralNetwork[] nextPopulation = new NeuralNetwork[initPopulation];

        for(int i=0; i<numberOfBestAgents; i++)
        {
            nextPopulation[numberToNaturallySelect] = population[i].InitializeCopy(vehicleController.LAYERS, vehicleController.NEURONS);
            nextPopulation[numberToNaturallySelect].fitness = 0;
            numberToNaturallySelect++;
            int f = Mathf.RoundToInt(population[i].fitness *10);
            for(int c=0; c<f; c++)
            {
                genePool.Add(i);
            }
        }

        for(int i=0; i< numberOfWorstAgents; i++)
        {
            int last = population.Length - 1;
            last -= i;
            int f = Mathf.RoundToInt(population[last].fitness *10);
            for(int c=0; c<f; c++)
            {
                genePool.Add(last);
            }
        }
        return nextPopulation;
    }

}

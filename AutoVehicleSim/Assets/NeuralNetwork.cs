using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using System;

using Random = UnityEngine.Random;

public class NeuralNetwork : MonoBehaviour
{
    public Matrix<float> inputLayer = Matrix<float>.Build.Dense(1,3);
    public List<Matrix<float>> hiddenLayers = new List<Matrix<float>>();
    public Matrix<float> outputLayer = Matrix<float>.Build.Dense(1,2);
    public List<Matrix<float>> weightValues = new List<Matrix<float>>();
    public List<float> biasValues = new List<float>();
    public float fitness;

    public void Initialize (int numberOfHiddenLayers, int numberOfHiddenNeurons) {
        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();
        weightValues.Clear();
        biasValues.Clear();

        for (int i=0; i<= numberOfHiddenLayers; i++)
        {
            Matrix<float> f = Matrix<float>.Build.Dense(1,numberOfHiddenNeurons);
            hiddenLayers.Add(f);
            biasValues.Add(Random.Range(-1f,1f));
            //weightValues
            if(i == 0)
            {
                Matrix<float> inputToFirstHiddenLayer = Matrix<float>.Build.Dense(3,numberOfHiddenNeurons);
                weightValues.Add(inputToFirstHiddenLayer);
            }
            Matrix<float> HiddenLayerToHiddenLayer = Matrix<float>.Build.Dense(numberOfHiddenNeurons, numberOfHiddenNeurons);
            weightValues.Add(HiddenLayerToHiddenLayer);
        }

        Matrix<float> outputWeight = Matrix<float>.Build.Dense(numberOfHiddenNeurons, 2);
        weightValues.Add(outputWeight);
        biasValues.Add(Random.Range(-1f,1f));
        RandomizeweightValues();
    }
    public void InitializeHidden(int numberOfHiddenLayers, int numberOfHiddenNeurons)
    {
        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();

        for(int i =0; i <= numberOfHiddenLayers ; i++)
        {
            Matrix<float> newHiddenLayer = Matrix<float>.Build.Dense(1, numberOfHiddenNeurons);
            hiddenLayers.Add(newHiddenLayer);
        }
    }
    
    public NeuralNetwork InitializeCopy (int numberOfHiddenLayers,int numberOfHiddenNeurons)
    {
        NeuralNetwork NNet = gameObject.AddComponent<NeuralNetwork>();
        List<Matrix<float>> newWeightValues = new List<Matrix<float>>();

        for(int i = 0; i<this.weightValues.Count; i++)
        {
            Matrix<float> currentWeight = Matrix<float>.Build.Dense(weightValues[i].RowCount, weightValues[i].ColumnCount);
            for(int x=0; x<currentWeight.RowCount; x++)
            {
                for(int y =0; y<currentWeight.ColumnCount; y++)
                {
                    currentWeight[x,y] = weightValues[i][x,y];
                }
            }
            newWeightValues.Add(currentWeight);
        }
        List<float> newbiasValues = new List<float>();
        newbiasValues.AddRange(biasValues);
        NNet.weightValues = newWeightValues;
        NNet.biasValues  = newbiasValues;
        NNet.InitializeHidden(numberOfHiddenLayers,numberOfHiddenNeurons);
        return NNet;
    }


    public void RandomizeweightValues(){
        for(int i =0; i< weightValues.Count; i++)
        {
            for (int x=0; x<weightValues[i].RowCount; x++)
            {
                for (int y =0; y< weightValues[i].ColumnCount; y++)
                {
                    weightValues[i][x,y] = Random.Range(-1f,1f);
                }
            }
        }
    }

    public (float,float) RunNetwork (float a, float b, float c)
    {
        //feed values into the inputLayer
        inputLayer[0,0] = a;
        inputLayer[0,1] = b;
        inputLayer[0,2] = c;
        inputLayer = inputLayer.PointwiseTanh();
        hiddenLayers[0] = ((inputLayer*weightValues[0]) + biasValues[0]).PointwiseTanh();
        for(int i=1; i<hiddenLayers.Count; i++)
        {
            hiddenLayers[i] = ((hiddenLayers[i-1]* weightValues[i]) + biasValues[i]).PointwiseTanh();
        }
        outputLayer = ((hiddenLayers[hiddenLayers.Count-1] * weightValues[weightValues.Count-1]) + biasValues[biasValues.Count-1]).PointwiseTanh();
        //first output is acceleration, second is steering value
        return (Sigmoid(outputLayer[0,0]), (float)Math.Tanh(outputLayer[0,1]));
    }

    private float Sigmoid(float s)
    {
        return (1/(1+ Mathf.Exp(-s)));
    }
}

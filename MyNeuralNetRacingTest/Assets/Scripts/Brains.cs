using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brains : MonoBehaviour {

    private NeuralNet m_neuralNet;
    private GeneticAlg m_geneticAlg;

    private void Start ()
    {
        m_geneticAlg = new GeneticAlg ();
        int totalWeights = 8 * 8 + 8 * 2 + 
    }
}

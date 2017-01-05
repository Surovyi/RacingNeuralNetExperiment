using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Brains : MonoBehaviour {

    private NeuralNet m_neuralNet;
    private GeneticAlg m_geneticAlg;

    private float m_currentFitness = 0f;
    private float m_bestFitness = 0f;

    private Vector3 m_defaultPosition;
    private Quaternion m_defaultRotation;

    private int m_populationCount = 15;

    private List<Waypoint> m_waypoints = new List<Waypoint> ();

    private void Start ()
    {
        m_geneticAlg = new GeneticAlg ();
        int totalWeights = 8 * 8 + 8 * 2;
        m_geneticAlg.GeneratePopulation (m_populationCount, totalWeights);

        m_neuralNet = new NeuralNet ();
        //m_neuralNet.CreateNeuralNet (1, 8, 8, 2);
        GeneticAlg.Genome genome = m_geneticAlg.GetNextGenome ();
        m_neuralNet.CreateNetFromGenome (genome, 8, 8, 2);

        m_defaultPosition = transform.position;
        m_defaultRotation = transform.rotation;

        m_waypoints = FindObjectsOfType <Waypoint> ().ToList ();
    }

    private void Update()
    {
         
    }
}

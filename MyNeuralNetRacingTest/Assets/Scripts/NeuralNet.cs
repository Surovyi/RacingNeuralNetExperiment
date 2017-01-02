using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent (typeof (UnityStandardAssets.Vehicles.Car.CarUserControl))]
public class NeuralNet : MonoBehaviour {

    // http://stevenmiller888.github.io/mind-how-to-build-a-neural-network/
    // http://nn.cs.utexas.edu/downloads/papers/stanley.ec02.pdf

    private UnityStandardAssets.Vehicles.Car.CarUserControl carControl;
    private Raycast raycast;

    private bool m_hasFailed = false;

    private List<Neuron> m_network;

    private List <float> m_inputs = new List<float>();
    private List <float> m_outputs = new List<float>(); //h, v
    public int m_outputsCount = 2;

    private int m_passedCheckpointsCount = 0;
    private float m_distance = 0f;

    public void Awake()
    {
        carControl = GetComponent<UnityStandardAssets.Vehicles.Car.CarUserControl> ();
        raycast = GetComponent<Raycast> ();
    }

    public void Start()
    {
        NeuralNet neuralNet = new NeuralNet ();
        neuralNet.CreateNeuralNet (1, 8, 8, 2);

        m_inputs = GetInputs ();
    }

    private void CreateNeuralNet (int hiddenLayersCount, int inputNeurons, int hiddenNeurons, int outputNeurons)
    {
        for (int i = 0; i < inputNeurons - 1; i++) {
            Neuron neuron = new Neuron (Neuron.NeuronType.INPUT);
            m_network.Add (neuron);
        }

        for (int i = 0; i < hiddenLayersCount - 1; i++) {
            for (int j = 0; j < hiddenNeurons - 1; j++) {
                Neuron neuron = new Neuron (Neuron.NeuronType.HIDDEN);
                neuron.SetRandomWeights (inputNeurons);
                m_network.Add (neuron);
            }
        }

        for (int i = 0; i < outputNeurons - 1; i++) {
            Neuron neuron = new Neuron (Neuron.NeuronType.OUTPUT);
            neuron.SetRandomWeights (hiddenNeurons);
            m_network.Add (neuron);
        }
    }

    public void Update()
    {
        m_hasFailed = carControl.crash;
        if (m_hasFailed == false) {
            m_distance += Time.deltaTime;

            m_inputs = GetInputs ();

            carControl.h = m_outputs[0];
            carControl.v = m_outputs[1];
        } else {
            m_distance = 0f;
        }
    }


    public List<float> GetInputs()
    {
        // 8 raycasts
        m_inputs = new List<float> ();
        m_inputs.Add (raycast.dis_l / raycast.raycastLength);
        m_inputs.Add (raycast.dis_flO / raycast.raycastLength);
        m_inputs.Add (raycast.dis_flT / raycast.raycastLength);
        m_inputs.Add (raycast.dis_f / raycast.raycastLength);
        m_inputs.Add (raycast.dis_frO / raycast.raycastLength);
        m_inputs.Add (raycast.dis_frT / raycast.raycastLength);
        m_inputs.Add (raycast.dis_r / raycast.raycastLength);
        m_inputs.Add (raycast.dis_b / raycast.raycastLength);
        return m_inputs;
    }

    public class Neuron
    {
        private float m_inputValue;
        private float m_outputValue;
        private NeuronType m_neuronType;

        private List<float> m_weights = new List<float> ();

        public Neuron (NeuronType neuronType)
        {
            m_neuronType = neuronType;
        }

        private float Sigmoid (float x)
        {
            return 1 / (1 + Mathf.Exp (-x));
        }

        private float HeperbolicTangent (float x)
        {
            return (1 - Mathf.Exp (-2 * x)) / (1 + Mathf.Exp (2 * x));
        }

        private float Normalize (float x, float max)
        {
            return x / max;
        }

        private float GetOutputValue ()
        {
            if (m_neuronType == NeuronType.INPUT) {
                return m_inputValue;
            }
            m_outputValue = HeperbolicTangent (m_inputValue);
            return m_outputValue;
        }

        public void SetRandomWeights (int capacity)
        {
            for (int i = 0; i < capacity - 1; i++) {
                m_weights.Add (GetRandomWeight ());
            }
        }

        private float GetRandomWeight ()
        {
            return UnityEngine.Random.Range (0f, 1f);
        }

        public enum NeuronType
        {
            INPUT,
            HIDDEN,
            OUTPUT
        }

    }
}

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
    private Raycast m_raycast;

    private bool m_hasFailed = false;

    private List<Neuron> m_network = new List<Neuron>();

    private List <float> m_inputs = new List<float>();
    private List <float> m_outputs = new List<float>(); //h, v
    private int m_outputsCount = 2;

    private int m_passedCheckpointsCount = 0;
    private float m_distance = 0f;

    public void Awake()
    {
        carControl = GetComponent<UnityStandardAssets.Vehicles.Car.CarUserControl> ();
        m_raycast = GetComponent<Raycast> ();
    }

    public void Start()
    {
        m_inputs = GetInputs ();

        CreateNeuralNet (1, 8, 8, 2);
    }

    private void CreateNeuralNet (int hiddenLayersCount, int inputNeurons, int hiddenNeurons, int outputNeurons)
    {
        for (int i = 0; i < inputNeurons; i++) {
            Neuron neuron = new Neuron (Neuron.NeuronType.INPUT);
            neuron.m_inputValue = m_inputs[i];
            m_network.Add (neuron);
        }

        for (int i = 0; i < hiddenLayersCount; i++) {
            for (int j = 0; j < hiddenNeurons; j++) {
                Neuron neuron = new Neuron (Neuron.NeuronType.HIDDEN);
                neuron.SetRandomWeights (inputNeurons);
                m_network.Add (neuron);
            }
        }

        for (int i = 0; i < outputNeurons; i++) {
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
            Refresh ();

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
        m_inputs.Add (m_raycast.dis_l / m_raycast.raycastLength);
        m_inputs.Add (m_raycast.dis_flO / m_raycast.raycastLength);
        m_inputs.Add (m_raycast.dis_flT / m_raycast.raycastLength);
        m_inputs.Add (m_raycast.dis_f / m_raycast.raycastLength);
        m_inputs.Add (m_raycast.dis_frO / m_raycast.raycastLength);
        m_inputs.Add (m_raycast.dis_frT / m_raycast.raycastLength);
        m_inputs.Add (m_raycast.dis_r / m_raycast.raycastLength);
        m_inputs.Add (m_raycast.dis_b / m_raycast.raycastLength);

        return m_inputs;
    }

    private void Refresh ()
    {
        m_outputs.Clear ();

        List<Neuron> hiddenNeurons = new List<Neuron> ();
        List<Neuron> inputNeurons = new List<Neuron> ();
        List<Neuron> outputNeurons = new List<Neuron> ();

        inputNeurons = m_network.FindAll (x => x.m_neuronType == Neuron.NeuronType.INPUT);
        hiddenNeurons = m_network.FindAll (x => x.m_neuronType == Neuron.NeuronType.HIDDEN);
        outputNeurons = m_network.FindAll (x => x.m_neuronType == Neuron.NeuronType.OUTPUT);

        for (int i = 0; i < hiddenNeurons.Count; i++) {
            float value = 0f;
            for (int j = 0; j < inputNeurons.Count; j++) {
                value += m_inputs[j] * hiddenNeurons[i].m_weights[j];
            }
            hiddenNeurons[i].m_inputValue = value;
        }

        for (int i = 0; i < outputNeurons.Count; i++) {
            float value = 0f;
            for (int j = 0; j < hiddenNeurons.Count; j++) {
                value += hiddenNeurons[j].GetOutputValue () * outputNeurons[i].m_weights[j] ;
            }
            outputNeurons[i].m_inputValue = value;
            m_outputs.Add (outputNeurons[i].GetOutputValue ());
        }


    }

    public class Neuron
    {
        public float m_inputValue;
        public float m_outputValue;
        public NeuronType m_neuronType;

        public List<float> m_weights = new List<float> ();

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

        public float GetOutputValue ()
        {
            if (m_neuronType == NeuronType.INPUT) {
                return m_inputValue;
            }
            m_outputValue = Sigmoid (m_inputValue);
            return m_outputValue;
        }

        public void SetRandomWeights (int capacity)
        {
            for (int i = 0; i < capacity; i++) {
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

using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (UnityStandardAssets.Vehicles.Car.CarUserControl))]
public class NeuralNet {

    // http://stevenmiller888.github.io/mind-how-to-build-a-neural-network/
    // http://nn.cs.utexas.edu/downloads/papers/stanley.ec02.pdf

    [HideInInspector]
    public bool m_hasFailed = false;

    private List<Neuron> m_network = new List<Neuron>(); // Does not contain input neurons

    private List <float> m_inputs = new List<float>();
    public List <float> m_outputs = new List<float>(); //h, v

    private int m_passedCheckpointsCount = 0;
    public float m_spentTime = 0f;
    private float m_timeThreshold = 4f;

    public void CreateNetFromGenome (GeneticAlg.Genome genome, int numOfInputs, int numOfHidden, int numOfOutputs)
    {
        m_network.Clear ();
        m_network.AddRange (GetInputNeurons ());

        for (int i = 0; i < numOfHidden; i++) {
            Neuron hiddenNeuron = new Neuron (Neuron.NeuronType.HIDDEN);

            for (int j = 0; j < numOfInputs; j++) {
                hiddenNeuron.m_weights.Add (genome.weights[i * numOfHidden + j]);
            }

            m_network.Add (hiddenNeuron);
        }

        for (int i = numOfHidden; i < numOfHidden + numOfOutputs; i++) {
            Neuron outputNeuron = new Neuron (Neuron.NeuronType.OUTPUT);

            for (int j = 0; j < numOfHidden; j++) {
                outputNeuron.m_weights.Add (genome.weights[i * numOfHidden + j]);
            }

            m_network.Add (outputNeuron);
        }
    }

    public void MakeUpdate(Raycast raycast, int pastWaypoints)
    {
        if (pastWaypoints == m_passedCheckpointsCount) {
            m_spentTime += Time.deltaTime;
        } else {
            m_passedCheckpointsCount = pastWaypoints;
            m_spentTime = Time.deltaTime;
            m_timeThreshold += 0.5f;
        }

        m_hasFailed = raycast.m_crash;
        if (m_hasFailed == false) {
            Refresh (raycast);
        }

        if (m_spentTime >= m_timeThreshold) {
            m_hasFailed = true;
            m_spentTime = Time.deltaTime;
        }
    }

    public void ClearFailure ()
    {
        m_hasFailed = false;
        m_spentTime = 0f;
        m_passedCheckpointsCount = 0;
        m_timeThreshold = 4f;
    }

    public List<float> GetInputs(Raycast raycast)
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

    public List <Neuron> GetInputNeurons ()
    {
        List<Neuron> inputNeurons = new List<Neuron> ();
        for (int i = 0; i < m_inputs.Count; i++) {
            Neuron neuron = new Neuron (Neuron.NeuronType.INPUT);
            neuron.m_inputValue = m_inputs[i];
            inputNeurons.Add (neuron);
        }

        return inputNeurons;
    }

    private void Refresh (Raycast raycast)
    {
        m_inputs = GetInputs (raycast);
        m_outputs.Clear ();
        
        List<Neuron> hiddenNeurons = new List<Neuron> ();
        List<Neuron> outputNeurons = new List<Neuron> ();
        
        hiddenNeurons = m_network.FindAll (x => x.m_neuronType == Neuron.NeuronType.HIDDEN);
        outputNeurons = m_network.FindAll (x => x.m_neuronType == Neuron.NeuronType.OUTPUT);

        for (int i = 0; i < hiddenNeurons.Count; i++) {
            float value = 0f;
            for (int j = 0; j < m_inputs.Count; j++) {
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
            return (Mathf.Exp (x) - Mathf.Exp (-x)) / (Mathf.Exp (x) + Mathf.Exp (-x));
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
            m_outputValue = HeperbolicTangent (m_inputValue);
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

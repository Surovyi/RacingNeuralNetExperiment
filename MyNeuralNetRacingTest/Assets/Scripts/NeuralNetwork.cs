using System.Collections.Generic;
using UnityEngine;

// Useful info to read
// http://stevenmiller888.github.io/mind-how-to-build-a-neural-network/
// http://nn.cs.utexas.edu/downloads/papers/stanley.ec02.pdf

[RequireComponent (typeof (UnityStandardAssets.Vehicles.Car.CarUserControl))]
public class NeuralNetwork {

    [HideInInspector]
    public bool m_hasFailed = false;

    private List<Neuron> m_network = new List<Neuron>(); // Does not contain input neurons

    private List <float> m_inputs = new List<float>();
    public List <float> m_outputs = new List<float>(); //h, v

    private int m_passedCheckpointsCount = 0;
    public float m_spentTime = 0f;
    private float m_timeThreshold = 4f;

	public float H { get { return (m_outputs.Count > 1) ? m_outputs [0] : 0f; } }
	public float V { get { return (m_outputs.Count > 1) ? m_outputs [1] : 0f; } }

    public void CreateNetFromGenome (GeneticAlgorithm.Genome genome, int[] neuralMap)
    {
        m_network.Clear ();
        m_network.AddRange (GetInputNeurons ());

		int hiddenLayersCount = neuralMap.Length - 2;
		int synapseIndex = 0;
		for (int layer = 1; layer <= hiddenLayersCount; layer++) {
			int neuronsInCurrentLayer = neuralMap [layer];
			for (int i = 0; i < neuronsInCurrentLayer; i++) {
				Neuron hiddenNeuron = new Neuron (Neuron.NeuronType.HIDDEN, layer);
				int neuronsInPreviousLayer = neuralMap [layer - 1];
				for (int j = 0; j < neuronsInPreviousLayer; j++) {
					hiddenNeuron.m_weights.Add (genome.weights [synapseIndex]);
					synapseIndex++;
				}
				m_network.Add (hiddenNeuron);
			}
		}

		int numOfHiddenNeurons = neuralMap [neuralMap.Length - 2];
		int numOfOutputNeurons = neuralMap [neuralMap.Length - 1];

		for (int i = 0; i < numOfOutputNeurons; i++) {
			Neuron outputNeuron = new Neuron (Neuron.NeuronType.OUTPUT, neuralMap.Length - 1);

			for (int j = 0; j < numOfHiddenNeurons; j++) {
				outputNeuron.m_weights.Add (genome.weights[synapseIndex]);
				synapseIndex++;
            }
            m_network.Add (outputNeuron);
        }
    }

	public void MakeUpdate(Eyes raycast, int[] neuralMap, int pastWaypoints, float normalizedSpeed)
    {
        if (pastWaypoints == m_passedCheckpointsCount) {
            m_spentTime += Time.deltaTime;
        } else {
            m_passedCheckpointsCount = pastWaypoints;
            m_spentTime = Time.deltaTime;
            if (m_timeThreshold < 4.9f) {
                m_timeThreshold += 0.2f;
            }
        }

        m_hasFailed = raycast.m_crash;
        if (m_hasFailed == false) {
			Refresh (raycast, neuralMap, normalizedSpeed);
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

    public List<float> GetInputs(Eyes raycast, float normalizedSpeed)
    {
        // 8 raycasts
        m_inputs = new List<float> ();
        m_inputs.Add (raycast.dis_l);
        m_inputs.Add (raycast.dis_flO);
        m_inputs.Add (raycast.dis_flT);
        m_inputs.Add (raycast.dis_f);
        m_inputs.Add (raycast.dis_frO);
        m_inputs.Add (raycast.dis_frT);
        m_inputs.Add (raycast.dis_r);
        m_inputs.Add (raycast.dis_b);

        // and current speed
        m_inputs.Add (normalizedSpeed);

        return m_inputs;
    }

    public List <Neuron> GetInputNeurons ()
    {
        List<Neuron> inputNeurons = new List<Neuron> ();
        for (int i = 0; i < m_inputs.Count; i++) {
            Neuron neuron = new Neuron (Neuron.NeuronType.INPUT, 0);
            neuron.m_inputValue = m_inputs[i];
            inputNeurons.Add (neuron);
        }

        return inputNeurons;
    }

	private void Refresh (Eyes raycast, int[] neuralMap, float normalizedSpeed)
    {
        m_inputs = GetInputs (raycast, normalizedSpeed);
        m_outputs.Clear ();
        
		List<Neuron> inputLayerNeurons = new List<Neuron> ();
        List<Neuron> previousLayerNeurons = new List<Neuron> ();
        List<Neuron> layerNeurons = new List<Neuron> ();

		inputLayerNeurons = GetInputNeurons ();
		for (int layerIndex = 1; layerIndex < neuralMap.Length; layerIndex++) {
			if (layerIndex == 1) {
				previousLayerNeurons = inputLayerNeurons;
			} else {
				previousLayerNeurons = m_network.FindAll (x => x.m_layerID == (layerIndex - 1));
			}
			layerNeurons = m_network.FindAll (x => x.m_layerID == layerIndex);



			for (int i = 0; i < layerNeurons.Count; i++) {
				float value = 0f;
				for (int j = 0; j < previousLayerNeurons.Count; j++) {
					value += previousLayerNeurons [j].GetOutputValue() * layerNeurons [i].m_weights [j];
				}
				layerNeurons [i].m_inputValue = value;
			}
		}

		for (int k = 0; k < neuralMap [neuralMap.Length - 1]; k++) {
			m_outputs.Add (layerNeurons [k].GetOutputValue());
		}
    }

    public class Neuron
    {
        public float m_inputValue;
        public float m_outputValue;
        public NeuronType m_neuronType;
		public int m_layerID;

        public List<float> m_weights = new List<float> ();

		public Neuron (NeuronType neuronType, int layerID)
        {
            m_neuronType = neuronType;
			m_layerID = layerID;
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

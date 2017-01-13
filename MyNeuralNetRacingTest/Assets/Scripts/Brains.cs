using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Brains : MonoBehaviour {
	public static Brains instance;


	private Text generation;
	private Text genome;
	private Text bestFitness;
	private Text fitness;
	private Text h;
	private Text v;
	private Text time;

    public bool loadBestGenome = false;

    private NeuralNet m_neuralNet;
    private GeneticAlg m_geneticAlg;
    private Raycast m_raycaster;
    protected UnityStandardAssets.Vehicles.Car.CarUserControl m_carControl;

	private Dictionary <string, Text> m_mapToText = new Dictionary<string, Text> ();

    protected float m_currentFitness = 0f;
    protected float m_bestFitness = 0f;

    private Vector3 m_defaultPosition;
    private Quaternion m_defaultRotation;

    private bool m_canGo = true;

    private int m_populationCount = 15;
    protected int m_currentGenomeIndex = 0;
    protected int m_currentGenerationIndex = 0;

    [HideInInspector]
    public List<Waypoint> m_waypoints = new List<Waypoint> ();
    [HideInInspector]
    public int m_waypointsPast = 0;

	private Brains () {
		m_mapToText.Add ("Generation Num", generation);
		m_mapToText.Add ("Genome Num", genome);
		m_mapToText.Add ("Best Fit Num", bestFitness);
		m_mapToText.Add ("Fitness Num", fitness);
		m_mapToText.Add ("H Num", h);
		m_mapToText.Add ("V Num", v);
		m_mapToText.Add ("Time Num", time);

		Debug.Log ("mapToText initialized");
	}

    private void Awake ()
    {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad (this.gameObject);
		}

		m_carControl = FindObjectOfType<UnityStandardAssets.Vehicles.Car.CarUserControl> ();
        m_waypoints = FindObjectsOfType<Waypoint> ().ToList ();
		m_raycaster = m_carControl.GetComponent<Raycast> ();

		GameObject canvas = GameObject.FindGameObjectWithTag ("UICanvas");
		foreach (KeyValuePair <string, Text> kvp in m_mapToText) {
			m_mapToText [kvp.Key] = canvas.transform.FindChild (kvp.Key).GetComponent<Text> ();
		}
    }

    private void Start ()
    {
        m_geneticAlg = new GeneticAlg ();
        int totalWeights = 8 * 8 + 8 * 2;
        m_geneticAlg.GeneratePopulation (m_populationCount, totalWeights);

        m_neuralNet = new NeuralNet ();
        if (loadBestGenome == false) {
            GeneticAlg.Genome genome = m_geneticAlg.GetNextGenome ();
            m_currentGenomeIndex = m_geneticAlg.GetCurrentGenomeIndex ();
            m_neuralNet.CreateNetFromGenome (genome, 8, 8, 2);
        } else {
            GeneticAlg.Genome genome = SaveLoad.LoadRun ();
            m_neuralNet.CreateNetFromGenome (genome, 8, 8, 2);
            m_currentFitness = genome.fitness;
        }
        
        m_defaultPosition = transform.position;
        m_defaultRotation = transform.rotation;
    }

    private IEnumerator WaitGenomeStart()
    {
        m_canGo = false;
        yield return new WaitForSeconds (0.3f);
        m_canGo = true;
    }

    private void Update()
    {
        if (m_canGo == false) {
            return;
        }

        m_neuralNet.MakeUpdate (m_raycaster, m_waypointsPast);

        m_carControl.H = m_neuralNet.m_outputs[0];
        m_carControl.V = m_neuralNet.m_outputs[1];

        m_currentFitness = CalculateFitness ();

        if (m_neuralNet.m_hasFailed) {
            m_waypointsPast = 0;
            m_raycaster.ResetCrash ();
            m_carControl.ResetAxis ();

            if (loadBestGenome == false) {
                m_geneticAlg.SetGenomeFitness (m_geneticAlg.GetCurrentGenomeIndex (), m_currentFitness);
            }

            if (m_currentFitness > m_bestFitness && !loadBestGenome) {
                m_bestFitness = m_currentFitness;
                SaveLoad.SaveRun (m_geneticAlg.GetCurrentGenome ());
                Debug.Log ("Saved");
            }

            StartCoroutine (WaitGenomeStart ());
            if (m_geneticAlg.GetCurrentGenomeIndex () == m_populationCount - 1) {
                m_geneticAlg.BreedPopulation ();
                NextTestSubject ();
                m_currentGenerationIndex++;
                m_currentGenomeIndex = m_geneticAlg.GetCurrentGenomeIndex ();
                return;
            }
            NextTestSubject ();
            m_currentGenomeIndex = m_geneticAlg.GetCurrentGenomeIndex ();
        }

        UpdateUILayer ();
    }

    private float CalculateFitness ()
    {
        float fitness = m_waypointsPast;
        return fitness;
    }

    public void NextTestSubject ()
    {
        m_currentFitness = 0.0f;

        GeneticAlg.Genome genome = m_geneticAlg.GetNextGenome ();
        m_neuralNet.CreateNetFromGenome (genome, 8, 8, 2);
        m_neuralNet.ClearFailure ();

        transform.position = m_defaultPosition;
        transform.rotation = m_defaultRotation;

        foreach (Waypoint waypoint in m_waypoints) {
            waypoint.gameObject.SetActive (true);
        }
    }

    private void UpdateUILayer ()
    {
		m_mapToText["Generation Num"].text = m_currentGenerationIndex.ToString ();
		m_mapToText["Genome Num"].text = m_currentGenomeIndex.ToString ();
		m_mapToText["Best Fit Num"].text = m_bestFitness.ToString ();
		m_mapToText["Fitness Num"].text = m_currentFitness.ToString ();
        if (m_neuralNet.m_outputs.Count > 1) {
			m_mapToText["H Num"].text = m_carControl.H.ToString ();
			m_mapToText["V Num"].text = m_neuralNet.m_outputs[1].ToString ();
        }
		m_mapToText["Time Num"].text = m_neuralNet.m_spentTime.ToString();
    }
}

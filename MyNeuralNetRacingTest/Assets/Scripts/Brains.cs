using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Brains : MonoBehaviour {
	public static Brains instance;

    public bool loadBestGenome = false;

    private NeuralNetwork m_neuralNet;
    private GeneticAlgorithm m_geneticAlg;
    private Eyes m_raycaster;
    protected UnityStandardAssets.Vehicles.Car.CarUserControl m_carControl;

	public UnityEngine.Events.UnityAction ChangeSubject;
	    
	private int[] m_neuralTopology = { 9, 30, 30, 2 }; //First number - input neurons count, last one - output, in between - hidden neurons.

    protected int m_currentFitness = 0;
    protected int m_bestFitness = 0;

    private Vector3 m_defaultPosition;
    private Quaternion m_defaultRotation;

    private bool m_canGo = false;

    private int m_populationCount = 18;
    protected int m_currentGenomeIndex = 0;
    protected int m_currentGenerationIndex = 0;

    [HideInInspector]
    public List<Waypoint> m_waypoints = new List<Waypoint> ();
    [HideInInspector]
    public int m_waypointsPast = 0;

    private void Awake ()
    {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad (this.gameObject);
        } else {
            GameObject.Destroy (this.gameObject);
        }

    }

    private void Start ()
    {
        m_carControl = FindObjectOfType<UnityStandardAssets.Vehicles.Car.CarUserControl> ();
        m_waypoints = FindObjectsOfType<Waypoint> ().ToList ();
        m_raycaster = m_carControl.GetComponent<Eyes> ();

		SceneManager.sceneLoaded += OnSceneChanged;

        if (m_geneticAlg == null) {
            m_geneticAlg = new GeneticAlgorithm ();

            int totalWeights = 0;
			for (int i = 0; i < m_neuralTopology.Length - 1; i++) {
				totalWeights += m_neuralTopology [i] * m_neuralTopology [i + 1];
			}
            m_geneticAlg.GeneratePopulation (m_populationCount, totalWeights);

            m_neuralNet = new NeuralNetwork ();
            if (loadBestGenome == false) {
                GeneticAlgorithm.Genome genome = m_geneticAlg.GetNextGenome ();
                m_currentGenomeIndex = m_geneticAlg.GetCurrentGenomeIndex ();
				m_neuralNet.CreateNetFromGenome (genome, m_neuralTopology);
            } else {
                BestAIRun bestRun = SaveLoad.LoadRun ();
                GeneticAlgorithm.Genome genome = bestRun.genomeToSave;
                if (genome.weights.Count == 0) {
                    Debug.LogError ("There are nothing to load. Uncheck 'Load Best Genome' option.");
                    Debug.Break ();
                    return;
                }
				m_neuralNet.CreateNetFromGenome (genome, m_neuralTopology);

                m_currentGenerationIndex = bestRun.generationNumber;
                m_currentGenomeIndex = bestRun.genomeNumber;
                m_bestFitness = (int)genome.fitness;
            }

            m_defaultPosition = transform.position;
            m_defaultRotation = transform.rotation;
        }

        StartCoroutine (WaitGenomeStart ());
    }

    private IEnumerator WaitGenomeStart()
    {
        m_canGo = false;
        yield return new WaitForSeconds (0.3f);
        m_canGo = true;
    }

    private void ChangeScene ()
    {
		ChangeSubject ();
        m_canGo = false;
        m_carControl = null;
        m_raycaster = null;
        m_waypoints.Clear ();
        
        string currentSceneName = SceneManager.GetActiveScene ().name;
        SceneManager.LoadSceneAsync (currentSceneName, LoadSceneMode.Single);
    }

    private void OnSceneChanged (Scene scene, LoadSceneMode mode)
    {
        m_carControl = FindObjectOfType<UnityStandardAssets.Vehicles.Car.CarUserControl> ();
        m_waypoints = FindObjectsOfType<Waypoint> ().ToList ();
        m_raycaster = m_carControl.GetComponent<Eyes> ();

        StartCoroutine (WaitGenomeStart ());
    }

    private void Update()
    {
        if (m_canGo == false) {
            return;
        }

		m_neuralNet.MakeUpdate (m_raycaster, m_neuralTopology, m_waypointsPast, m_carControl.GetCurrentNormalizedSpeed());

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
                SaveLoad.SaveRun (m_geneticAlg.GetCurrentGenome (), m_currentGenerationIndex, m_currentGenomeIndex);
                Debug.Log ("Saved");
            }
            
            if (m_geneticAlg.GetCurrentGenomeIndex () == m_populationCount - 1) {
                m_geneticAlg.BreedPopulation ();
                NextTestSubject ();
                m_currentGenerationIndex++;
                m_currentGenomeIndex = m_geneticAlg.GetCurrentGenomeIndex ();
                ChangeScene ();
                return;
            }
            NextTestSubject ();
            m_currentGenomeIndex = m_geneticAlg.GetCurrentGenomeIndex ();
            ChangeScene ();
        }
    }

    private int CalculateFitness ()
    {
		return m_waypointsPast;
    }

    public void DeactivateWaypoint(Waypoint waypoint)
    {
        waypoint.gameObject.SetActive (false);
        m_waypointsPast++;

        if (m_waypointsPast % m_waypoints.Count == 0) {
            for (int i = 0; i < m_waypoints.Count; i++) {
                m_waypoints[i].gameObject.SetActive (true);
                m_raycaster.m_pastWaypointID = -1;
            }
        }
    }

    public void NextTestSubject ()
    {
        m_currentFitness = 0;

        GeneticAlgorithm.Genome genome = m_geneticAlg.GetNextGenome ();
		m_neuralNet.CreateNetFromGenome (genome, m_neuralTopology);
        m_neuralNet.ClearFailure ();

        transform.position = m_defaultPosition;
        transform.rotation = m_defaultRotation;

        foreach (Waypoint waypoint in m_waypoints) {
            waypoint.gameObject.SetActive (true);
        }
    }

	public Face.TextData RequestTextData()
	{
		Face.TextData textData = new Face.TextData (m_currentGenerationIndex, m_currentGenomeIndex, m_bestFitness, m_currentFitness,
			                         m_neuralNet.H, m_neuralNet.V, m_neuralNet.m_spentTime, m_carControl.GetCurrentSpeed ());

		return textData;

	}

    private void OnApplicationQuit()
    {
        if (m_currentFitness > m_bestFitness && !loadBestGenome) {
            m_bestFitness = m_currentFitness;
            SaveLoad.SaveRun (m_geneticAlg.GetCurrentGenome (), m_currentGenerationIndex, m_currentGenomeIndex);
            Debug.Log ("Saved On Exit");
        }
    }
}

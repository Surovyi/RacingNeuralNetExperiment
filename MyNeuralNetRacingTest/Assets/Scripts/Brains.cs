using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    private string[] uiNames = { "Generation Num", "Genome Num", "Best Fit Num", "Fitness Num", "H Num", "V Num", "Time Num" };

    protected float m_currentFitness = 0f;
    protected float m_bestFitness = 0f;

    private Vector3 m_defaultPosition;
    private Quaternion m_defaultRotation;

    private bool m_canGo = false;
    private bool m_textInitialized = false;

    private int m_populationCount = 15;
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
        m_raycaster = m_carControl.GetComponent<Raycast> ();

        InitializeText ();

        if (m_geneticAlg == null) {
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
        m_textInitialized = false;
        m_canGo = false;
        m_carControl = null;
        m_raycaster = null;
        m_waypoints.Clear ();
        
        SceneManager.sceneLoaded += OnSceneChanged;
        string currentSceneName = SceneManager.GetActiveScene ().name;
        SceneManager.LoadSceneAsync (currentSceneName, LoadSceneMode.Single);
    }

    private void OnSceneChanged (Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneChanged;
        InitializeText ();
        m_carControl = FindObjectOfType<UnityStandardAssets.Vehicles.Car.CarUserControl> ();
        m_waypoints = FindObjectsOfType<Waypoint> ().ToList ();
        m_raycaster = m_carControl.GetComponent<Raycast> ();

        StartCoroutine (WaitGenomeStart ());
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

        if (m_textInitialized) {
            UpdateUILayer ();
        }
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

    private void InitializeText ()
    {
        if (generation == null || genome == null || bestFitness == null || fitness == null || h == null || v == null || time == null) {
            Transform canvas = GameObject.FindGameObjectWithTag ("UICanvas").transform;
            generation = canvas.FindChild (uiNames[0]).GetComponent<Text> ();
            genome = canvas.FindChild (uiNames[1]).GetComponent<Text> ();
            bestFitness = canvas.FindChild (uiNames[2]).GetComponent<Text> ();
            fitness = canvas.FindChild (uiNames[3]).GetComponent<Text> ();
            h = canvas.FindChild (uiNames[4]).GetComponent<Text> ();
            v = canvas.FindChild (uiNames[5]).GetComponent<Text> ();
            time = canvas.FindChild (uiNames[6]).GetComponent<Text> ();
        }

        m_textInitialized = true;
    }

    private void UpdateUILayer ()
    {
		generation.text = m_currentGenerationIndex.ToString ();
		genome.text = m_currentGenomeIndex.ToString ();
		bestFitness.text = m_bestFitness.ToString ();
		fitness.text = m_currentFitness.ToString ();
        if (m_neuralNet.m_outputs.Count > 1) {
			h.text = m_carControl.H.ToString ();
			v.text = m_neuralNet.m_outputs[1].ToString ();
        }
		time.text = m_neuralNet.m_spentTime.ToString();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Face : MonoBehaviour {
	private Brains m_brains;

	private Text m_generation;
	private Text m_genome;
	private Text m_bestFitness;
	private Text m_fitness;
	private Text m_h;
	private Text m_v;
	private Text m_time;
	private Text m_speed;

	private bool m_textInitialized = false;

	private string[] m_uiNames = { "Generation Num", "Genome Num", "Best Fit Num", "Fitness Num", "H Num", "V Num", "Time Num", "Speed Num" };

	private void Start ()
	{
		m_brains = Brains.instance;
		m_brains.ChangeSubject += OnSubjectFailed;
		SceneManager.sceneLoaded += OnSceneChanged;
		InitializeText ();
	}

	private void InitializeText()
	{
		if (m_generation == null || m_genome == null || m_bestFitness == null || m_fitness == null || m_h == null || m_v == null || m_time == null) {
			Transform canvas = GameObject.FindGameObjectWithTag ("UICanvas").transform;
			m_generation = canvas.FindChild (m_uiNames[0]).GetComponent<Text> ();
			m_genome = canvas.FindChild (m_uiNames[1]).GetComponent<Text> ();
			m_bestFitness = canvas.FindChild (m_uiNames[2]).GetComponent<Text> ();
			m_fitness = canvas.FindChild (m_uiNames[3]).GetComponent<Text> ();
			m_h = canvas.FindChild (m_uiNames[4]).GetComponent<Text> ();
			m_v = canvas.FindChild (m_uiNames[5]).GetComponent<Text> ();
			m_time = canvas.FindChild (m_uiNames[6]).GetComponent<Text> ();
			m_speed = canvas.FindChild (m_uiNames[7]).GetComponent<Text> ();

			m_textInitialized = true;
		}
	}

	private void Update()
	{
		if (m_textInitialized) {
			UpdateText ();
		}
	}

	private void UpdateText()
	{
		TextData textData = m_brains.RequestTextData ();

		m_generation.text = textData.currentGenerationIndex.ToString ();
		m_genome.text = textData.currentGenomeIndex.ToString ();
		m_bestFitness.text = textData.bestFitness.ToString ();
		m_fitness.text = textData.currentFitness.ToString ();
		m_h.text = textData.h.ToString ();
		m_v.text = textData.v.ToString ();
		m_time.text = textData.spentTime.ToString("0.00");
		m_speed.text = textData.speed.ToString("0.00");
	}

	private void OnSubjectFailed ()
	{
		m_textInitialized = false;
	}

	private void OnSceneChanged (Scene scene, LoadSceneMode mode)
	{
		InitializeText ();
	}

	public struct TextData
	{
		public int currentGenerationIndex;
		public int currentGenomeIndex;
		public float bestFitness;
		public float currentFitness;
		public float h;
		public float v;
		public float spentTime;
		public float speed;

		public TextData (int currentGenerationIndex, int currentGenomeIndex, float bestFitness, float currentFitness, float h, float v, float spentTime, float speed)
		{
			this.currentGenerationIndex = currentGenerationIndex;
			this.currentGenomeIndex = currentGenomeIndex;
			this.bestFitness = bestFitness;
			this.currentFitness = currentFitness;
			this.h = h;
			this.v = v;
			this.spentTime = spentTime;
			this.speed = speed;
		}
	}
}

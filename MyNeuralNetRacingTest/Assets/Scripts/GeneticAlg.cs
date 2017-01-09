using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GeneticAlg
{

    public int m_currentGenome = -1;
    public int m_totalPopulation = 0;
    public int m_generationNumber = 1;

    public float m_mutationRate = 0.15f;
    public float m_maxPerbetuation = 0.3f;

    private int m_genomeID = 0;
    private int m_totalGenomeWeights;

    public List<Genome> m_population = new List<Genome> ();

    public void GeneratePopulation (int populationCount, int totalWeights)
    {
        m_population.Clear ();
        m_currentGenome = -1;
        m_generationNumber = 1;
        m_totalPopulation = populationCount;

        for (int i = 0; i < populationCount; i++) {
            Genome genome = CreateGenome (totalWeights);
            m_population.Add (genome);
        }
    }

    public Genome CreateGenome (int totalWeights)
    {
        Genome genome = new Genome ();
        genome.ID = m_genomeID;
        m_genomeID++;

        for (int i = 0; i < totalWeights; i++) {
            genome.weights.Add (RandomFloat());
        }

        return genome;
    }

    public Genome GetGenome (int id)
    {
        if (id >= m_totalPopulation)
            return null;
        return m_population[id];
    }

    public Genome GetNextGenome ()
    {
        m_currentGenome++;
        if (m_currentGenome >= m_population.Count)
            return null;

        return m_population[this.m_currentGenome];
    }

    public Genome GetCurrentGenome ()
    {
        return m_population[this.m_currentGenome];
    }

    public Genome GetWorstGenome ()
    {
        int worstGenome = -1;
        float fitness = 1000000.0f;
        for (int i = 0; i < m_population.Count; i++) {
            if (m_population[i].fitness < fitness) {
                fitness = m_population[i].fitness;
                worstGenome = i;
            }
        }

        return m_population[worstGenome];
    }

    public List <Genome> GetBestGenomes (int howMuch)
    {
        List<Genome> bestCases = new List<Genome> ();
        bestCases = m_population;
        bestCases = bestCases.OrderByDescending (x => x.fitness).Take (howMuch).ToList ();

        return bestCases;
    }

    public List<Genome> GetWorstGenomes (int howMuch)
    {
        List<Genome> bestCases = new List<Genome> ();
        bestCases = m_population;
        bestCases = bestCases.OrderBy (x => x.fitness).Take(howMuch).ToList ();

        return bestCases;
    }

    public int GetCurrentGenomeIndex ()
    {
        return m_currentGenome;
    }

    public void SetGenomeFitness (int index, float fitness)
    {
        if (index >= m_population.Count) {
            return;
        }

        m_population[index].fitness = fitness;
    }

    public List <Genome> CrossbreedGenomes (Genome g1, Genome g2)
    {
        List<Genome> children = new List<Genome> ();

        Genome childrenOne = new Genome ();
        Genome childrenTwo = new Genome ();

        childrenOne.ID = m_genomeID;
        childrenTwo.ID = m_genomeID + 1;
        m_genomeID += 2;

        int parentSynapsesCount = g1.weights.Count;
        int crossoverIndex = Random.Range (1, parentSynapsesCount);

        for (int i = 0; i < crossoverIndex; i++) {
            childrenOne.weights.Add (g1.weights[i]);
            childrenTwo.weights.Add (g2.weights[i]);
        }

        for (int i = crossoverIndex; i < parentSynapsesCount; i++) {
            childrenOne.weights.Add (g2.weights[i]);
            childrenTwo.weights.Add (g1.weights[i]);
        }

        children.Add (childrenOne);
        children.Add (childrenTwo);

        return children;
    }

    public void BreedPopulation()
    {
        List<Genome> bestGenomes = new List<Genome> ();
        bestGenomes = GetBestGenomes (4); //tweek this

        List<Genome> childrens = new List<Genome> ();
        childrens.Add (bestGenomes[0]);

        List<Genome> candidates = new List<Genome> ();
        candidates.AddRange (CrossbreedGenomes (bestGenomes[0], bestGenomes[1]));
        candidates[0] = Mutate (candidates[0]);
        candidates[1] = Mutate (candidates[1]);

        candidates.AddRange (CrossbreedGenomes (bestGenomes[0], bestGenomes[2]));
        candidates[2] = Mutate (candidates[2]);
        candidates[3] = Mutate (candidates[3]);

        candidates.AddRange (CrossbreedGenomes (bestGenomes[0], bestGenomes[3]));
        candidates[4] = Mutate (candidates[4]);
        candidates[5] = Mutate (candidates[5]);

        candidates.AddRange (CrossbreedGenomes (bestGenomes[1], bestGenomes[2]));
        candidates[6] = Mutate (candidates[6]);
        candidates[7] = Mutate (candidates[7]);

        candidates.AddRange (CrossbreedGenomes (bestGenomes[1], bestGenomes[3]));
        candidates[8] = Mutate (candidates[8]);
        candidates[9] = Mutate (candidates[9]);

        //candidates.AddRange (CrossbreedGenomes (bestGenomes[2], bestGenomes[3]));
        //candidates[10] = Mutate (candidates[10]);
        //candidates[11] = Mutate (candidates[11]);

        childrens.AddRange (candidates);

        int remainingCount = (m_totalPopulation - childrens.Count);
        for (int i = 0; i < remainingCount; i++) {
            childrens.Add (CreateGenome (bestGenomes[0].weights.Count));
        }

        m_population.Clear ();
        m_population = childrens;

        m_currentGenome = 0;
        m_generationNumber++;
    }

    public Genome Mutate (Genome genome)
    {
        for (int i = 0; i < genome.weights.Count; i++) {
            if (Random.Range (0f, 1f) < m_mutationRate) {
                genome.weights[i] += (RandomFloat() * m_maxPerbetuation);
            }
        }

        return genome;
    }

    /// <summary>
    /// Value between -1 and 1.
    /// </summary>
    /// <returns></returns>
    public float RandomFloat ()
    {
        float rand = Random.Range (0.1f, 15237.0f);
        rand = Mathf.Sin (rand);
        
        return rand;
    }


    [System.Serializable]
    public class Genome
    {
        public float fitness = 0.0f;
        public int ID;
        public List<float> weights = new List<float> ();

        public void SetFitness (float fitness)
        {
            this.fitness = fitness;
        }
    }
}

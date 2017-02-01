using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "BestAIRun", menuName = "BestAIRun")]
public class BestAIRun : ScriptableObject {

    public int generationNumber;
    public int genomeNumber;
    public GeneticAlgorithm.Genome genomeToSave;
}

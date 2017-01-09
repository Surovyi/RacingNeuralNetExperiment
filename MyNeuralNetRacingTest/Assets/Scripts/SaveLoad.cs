using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SaveLoad {

    private static BestAIRun bestAIRun;

    private static void FindScriptableObject ()
    {
        string name = "BestAIRun";
        bestAIRun = Resources.Load<BestAIRun> (name);
    }

    public static void SaveRun (GeneticAlg.Genome genome)
    {
        FindScriptableObject ();
        bestAIRun.genomeToSave = genome;
    }

    public static GeneticAlg.Genome LoadRun ()
    {
        FindScriptableObject ();
        if (bestAIRun.genomeToSave == null) {
            Debug.LogError ("Nothing to load");
            return null;
        }
        return bestAIRun.genomeToSave;
    }
}

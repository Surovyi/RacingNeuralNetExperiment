using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class SaveLoad
{

    private static BestAIRun bestAIRun;

    private static void FindScriptableObject ()
    {
        string levelId = UnityEngine.SceneManagement.SceneManager.GetActiveScene ().name;
        Regex regex = new Regex (@"\d+");
        Match match = regex.Match (levelId);
        levelId = match.Value;
        string name = "BestAIRun" + levelId;
        bestAIRun = Resources.Load<BestAIRun> (name);
    }

    public static void SaveRun (GeneticAlgorithm.Genome genome)
    {
        FindScriptableObject ();
        bestAIRun.genomeToSave = genome;
    }

    public static GeneticAlgorithm.Genome LoadRun ()
    {
        FindScriptableObject ();
        if (bestAIRun.genomeToSave == null) {
            Debug.LogError ("Nothing to load");
            return null;
        }
        return bestAIRun.genomeToSave;
    }
}
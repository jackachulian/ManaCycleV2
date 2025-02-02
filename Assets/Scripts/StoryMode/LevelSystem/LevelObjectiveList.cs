using System;
using System.Collections.Generic;
using LevelSystem.Objectives;
using UnityEngine;

[Serializable]
public class LevelObjectiveList
{
    [SerializeReference] public List<LevelObjective> objectives = new List<LevelObjective>();
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Condition {
    public ConditionID ID { get; set; }
    public string Name { get; set; }
    public string Abbreviation { get; set; }
    public string Description { get; set; }
    public string StartMessage { get; set; }
    public Func<Pokemon, bool> OnStart { get; set; }
    public Func<Pokemon, bool> OnBeforeMove { get; set; }
    public Action<Pokemon> OnAfterTurn { get; set; }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NopEvent : WorldEvent
{
    public override void ApplyEvent()
    {
        return;
    }
}


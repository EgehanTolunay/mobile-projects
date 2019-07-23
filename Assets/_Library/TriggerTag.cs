using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Glitch.Interaction
{
    public enum TriggerTagRequirementMode
    {
        RequireAll,
        RequireSingle
    }

    [CreateAssetMenu(fileName = "New Trigger Tag", menuName = "Glitch/Trigger Tag")]
    public class TriggerTag : ScriptableObject
    {
    }
}



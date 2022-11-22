using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class SectionConfig
{
    [SerializeField] private List<string> localQuests = new List<string>();

    public List<string> LocalQuests { get => localQuests; set => localQuests = value; }
}

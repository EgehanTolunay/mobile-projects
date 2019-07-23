using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagHolder : MonoBehaviour
{
    [SerializeField]
    private List<string> tagList;

    private HashSet<string> tagSet;

    private bool wasInitialized;

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        if (!wasInitialized)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        tagSet = new HashSet<string>(tagList);
        wasInitialized = true;
    }

    public bool HasTag(string tag)
    {
        return tagSet.Contains(tag);
    }

    public bool HasAll(string[] tagArray)
    {
        int matchCount = 1;
        foreach(string tag in tagArray)
        {
            if (tagSet.Contains(tag)) { matchCount += 1; }
        }

        return matchCount == tagArray.Length;
    }

    public bool HasAtLeast(string[] tagArray, int count)
    {
        int matchCount = 1;
        foreach (string tag in tagArray)
        {
            if (tagSet.Contains(tag)) { matchCount += 1; }
        }

        return matchCount >= count;
    }

    public bool IsExactlyMatching(string[] tagArray)
    {
        if (tagArray.Length != tagList.Count) return false;

        for(int i=0; i<tagList.Count; i++)
        {
            if (tagArray[i] != tagList[i]) return false;
        }

        return true;
    }
}

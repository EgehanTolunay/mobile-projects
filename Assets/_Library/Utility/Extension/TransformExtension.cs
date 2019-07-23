using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtension{

    public static List<Transform> GetSiblings(this Transform transform)
    {
        int siblingCounter = 0;
        List<Transform> siblingList = new List<Transform>();

        while (true)
        {
            Transform sibling;
            if (transform.parent!= null)
            {
                if(siblingCounter == transform.parent.childCount)
                {
                    break;
                }

                sibling = transform.parent.GetChild(siblingCounter);

                if (sibling.parent == transform.parent)
                {
                    if (sibling != transform)
                        siblingList.Add(sibling);
                }
                else
                {
                    break;
                }

                siblingCounter += 1;
            }
            else
            {
                foreach(GameObject go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    if(transform != go.transform)
                    {
                        siblingList.Add(go.transform);
                    }
                }
                break;
            }

            
        }

        return siblingList;
    } 

    public static List<Transform> GetFirstChilds(this Transform transform)
    {
        List<Transform> firstChilds = new List<Transform>();
        for (int i = 0; i<transform.childCount; i++)
        {
            firstChilds.Add(transform.GetChild(i));
        }

        return firstChilds;
    }

    /// <summary>
    /// Zero means first depth.
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="depth"></param>
    /// <returns></returns>
    public static List<Transform> GetChildsOnDepth(this Transform transform, int depth)
    {
        List<Transform> childs = transform.GetFirstChilds();
        int currentDepth = 0;
        Transform[] allChilds = transform.GetComponentsInChildren<Transform>();
        Transform lastChild = allChilds[allChilds.Length - 1]; 

        if (depth == 0)
        {
            return childs;
        }
        else
        {
            while (true)
            {
                currentDepth += 1;
                childs = GetFirstChildsOfTransforms(childs);
                if (currentDepth == depth || childs.Contains(lastChild))
                {
                    break;
                }
            }
        }

        return childs;
    }

    private static List<Transform> GetFirstChildsOfTransforms(List<Transform> transformList)
    {
        List<Transform> allChilds = new List<Transform>();
        foreach(Transform t in transformList)
        {
            allChilds.AddRange(t.GetFirstChilds());
        }

        return allChilds;
    }

    public static Vector3 DirectionTo(this Transform transform, Transform destination)
    {
        Vector3 direction = destination.position - transform.position;
        return Vector3.Normalize(direction);
    }

}

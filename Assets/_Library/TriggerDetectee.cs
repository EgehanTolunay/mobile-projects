using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDetectee : TriggerDetectionBase
{
    public void NotifyTriggerEnter(TriggerDetector detector, GameObject other)
    {
        TriggerData newData = new TriggerData();

        FillData(newData, detector, other);
        triggerEnter.Invoke(newData);
    }

    public void NotifyTriggerExit(TriggerDetector detector, GameObject other)
    {
        TriggerData newData = new TriggerData();

        FillData(newData, detector, other);
        triggerExit.Invoke(newData);
    }

    void FillData(TriggerData data, TriggerDetector detector, GameObject other)
    {
        data.triggerDetectee = this;
        data.triggerDetector = detector;
        data.otherTagHolder = detector.TagHolder;
        data.otherTransform = detector.transform;
        data.otherGameObject = detector.gameObject;
    }
}

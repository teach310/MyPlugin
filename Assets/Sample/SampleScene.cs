using System.Collections;
using System.Collections.Generic;
using Sample;
using UnityEngine;
using UnityEngine.UI;

public class SampleScene : MonoBehaviour
{
    [SerializeField] Text label;
    bool isAuthorized = false;
    bool isProcessing = false;

    public void OnClick()
    {
        if (isProcessing)
        {
            return;
        }

        isProcessing = true;

        if (isAuthorized)
        {
            SamplePlugin.GetStepsToday(steps =>
            {
                label.text = steps.ToString();
                isProcessing = false;
            });
        }
        else
        {
            SamplePlugin.Authorize(requestSuccess =>
            {
                isAuthorized = requestSuccess;
                if (isAuthorized)
                {
                    SamplePlugin.GetStepsToday(steps =>
                    {
                        label.text = steps.ToString();
                        isProcessing = false;
                    });
                }
                else
                {
                    label.text = "Not authorized";
                    isProcessing = false;
                }
            });
        }
    }
}

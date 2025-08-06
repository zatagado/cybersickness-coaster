using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class SendData
{
    public static void SendStart(MonoBehaviour monoBehaviour, string surveyURL)
    {
        // create function
        monoBehaviour.StartCoroutine(SendStartToQualtrics(surveyURL));
    }

    private static IEnumerator SendStartToQualtrics(string surveyURL)
    {
        // Form containing data
        WWWForm form = new WWWForm();
        form.AddField("pid", CybersickData.PID);
        form.AddField("session", CybersickData.Session);

        // Input the data into the site
        using (UnityWebRequest request = UnityWebRequest.Post(surveyURL, form))
        {
            yield return request.SendWebRequest();
        }
        Debug.Log("Sent");
    }

    public static void SendLevel(MonoBehaviour monoBehaviour, string surveyURL, int level, float balloonsPoppedRatio, float runTime, double moi, int[] surveyData)
    {
        monoBehaviour.StartCoroutine(SendLevelToQualtrics(surveyURL, level, balloonsPoppedRatio, runTime, moi, surveyData)); // maybe switch to async
    }

    private static IEnumerator SendLevelToQualtrics(string surveyURL, int level, float balloonsPoppedRatio, float runTime, double moi, int[] surveyData)
    {
        // Form containing data
        WWWForm form = new WWWForm();
        form.AddField("pid", CybersickData.PID);
        form.AddField("session", CybersickData.Session);
        form.AddField("level", level); // use numbers or words for the input?
        form.AddField("balloons popped ratio", balloonsPoppedRatio.ToString());
        form.AddField("moment of inertia", moi.ToString());
        form.AddField("general discomfort", ResultToString(surveyData[0]));
        form.AddField("fatigue", ResultToString(surveyData[1]));
        form.AddField("eyestrain", ResultToString(surveyData[2]));
        form.AddField("difficulty focusing", ResultToString(surveyData[3]));
        form.AddField("headache", ResultToString(surveyData[4]));
        form.AddField("fullness of head", ResultToString(surveyData[5]));
        form.AddField("blurred vision", ResultToString(surveyData[6]));
        form.AddField("dizzy", ResultToString(surveyData[7]));
        form.AddField("vertigo", ResultToString(surveyData[8]));
        form.AddField("running time", runTime.ToString());

        // Input the data into the site
        using (UnityWebRequest request = UnityWebRequest.Post(surveyURL, form))
        {
            yield return request.SendWebRequest();
        }
        Debug.Log("Sent");
    }

    private static string ResultToString(int i)
    {
        switch (i)
        {
            case 1:
                return "not at all";
            case 2:
                return "slightly";
            case 3:
                return "moderately";
            case 4:
                return "very";
            default:
                return null;
        }
    }
}

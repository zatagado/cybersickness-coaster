using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioAndSubtitles : MonoBehaviour
{
    [Serializable]
    public class Subtitle
    {
        [TextArea(1, 10)] [SerializeField] private string text;
        [SerializeField] private float time;

        public string Text { get => text; set => text = value; }
        public float Time { get => time; set => time = value; } // the time in the audio file where the subtitle should stop showing
    }

    [SerializeField] private AudioClip audioClip;
    [Range(0f, 1f)] [SerializeField] private float volume;
    [SerializeField] private AudioSource source;

    [SerializeField] private Subtitle[] subtitles;
    [SerializeField] private TextMeshProUGUI text;

    [SerializeField] private bool isSkippable;
    private bool continued;
    [SerializeField] private bool isReplayable;
    private bool replayed;

    public Action OnContinue;

    public void PromptContinue()
    {
        continued = true;
    }

    public void PromptReplay()
    {
        replayed = true;
    }

    public void Play()
    {
        StartCoroutine(PlayAudio());
    }

    public IEnumerator PlayAudio()
    {
        source.clip = audioClip;
        source.volume = volume;
        if (audioClip)
        {
            source?.Play();
        }

        float timer = 0f;
        int i = 0;
        while (i < subtitles.Length) // last one can be shorter
        {
            text.text = subtitles[i].Text;

            do
            {
                yield return new WaitForSeconds(0.05f);
                timer += 0.05f;

                if (continued && isSkippable)
                {
                    continued = false;
                    Continue();
                    OnContinue?.Invoke();
                }
                else if (replayed && isReplayable) // replay the audio and subtitles
                {
                    replayed = false;
                    source?.Stop();
                    Play();
                    yield break;
                }
            } while (timer < subtitles[i].Time);
            i++;
        }

        continued = false;
        replayed = false;
        do 
        {
            yield return new WaitForSeconds(0.1f);
            if (replayed)
            {
                replayed = false;
                Play();
                yield break;
            }
        } while (!continued);
        
        Continue();
        OnContinue?.Invoke();
    }

    private void Continue()
    {
        text.gameObject.SetActive(false);
        source?.Stop();
    }
}

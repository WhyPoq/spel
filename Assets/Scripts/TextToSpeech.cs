using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;


public class TextToSpeech : MonoBehaviour
{
	private const string googleURL = "https://translate.google.com/translate_tts?ie=UTF-8&total=1&idx=0&textlen=32&client=tw-ob&q=";

	[SerializeField] private AudioSource source;

	AudioClip curAudio;

	public void UpdateAndPlayAudio(string word)
    {
		DestroyCurAudio();
		StartCoroutine(UpdateCurAudio(word));
    }

	public void DestroyCurAudio()
    {
		if (curAudio != null)
		{
			AudioClip.Destroy(curAudio);
		}
	}

	public void PlayCurAudio()
    {
		if(curAudio == null)
			return;

		source.Play();
	}

	private IEnumerator UpdateCurAudio(string word)
	{
		string url = googleURL + word + "&tl=en";
		using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
		{
			yield return www.SendWebRequest();
			if (www.result == UnityWebRequest.Result.Success)
			{
				curAudio = DownloadHandlerAudioClip.GetContent(www);
				source.clip = curAudio;
				source.Play();
			}
			else
			{
				Debug.LogError("Error: " + www.error);
			}
		}
	}
}

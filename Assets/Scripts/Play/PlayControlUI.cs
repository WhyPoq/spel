using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayControlUI : MonoBehaviour
{
    [SerializeField]
    private PlayControl playControl;

    [SerializeField]
    private TextMeshProUGUI packNameText;
    [SerializeField]
    private TextMeshProUGUI cardsLeftText;
    [SerializeField]
    private GameObject nextButton;
    [SerializeField]
    private GameObject typoButton;
    [SerializeField]
    private TMP_InputField input;
    [SerializeField]
    private TextMeshProUGUI answerText;
    [SerializeField]
    private GameObject checkButton;
    [SerializeField]
    private GameObject completeBox;
    [SerializeField]
    private GameObject fade;

    [SerializeField]
    private Color wrongColor;
    [SerializeField]
    private Color rightColor;

    Word curWord;

    public void Start()
    {
        packNameText.text = PlayControl.openedPack;
    }

    public void DrawWord(Word word)
    {
        curWord = word;

        input.text = "";

        nextButton.SetActive(false);
        typoButton.SetActive(false);
        answerText.gameObject.SetActive(false);

        checkButton.SetActive(true);
    }

    public void DrawAnswerWord(bool right, string word)
    {
        answerText.text = word;

        nextButton.SetActive(true);
        answerText.gameObject.SetActive(true);

        checkButton.SetActive(false);

        if (!right)
        {
            typoButton.SetActive(true);
            //answerText.color = wrongColor;
        }
        else
        {
            //answerText.color = rightColor;
        }
    }

    public void Check()
    {
        playControl.CheckAnswer(input.text);
    }

    public void Typo()
    {
        playControl.ReplayWord();
    }

    public void Next()
    {
        playControl.NextWord();
    }

    public void SetWordsLeft(int count)
    {
        cardsLeftText.text = count.ToString();
    }

    public void ReplayAudio()
    {
        playControl.ReplayAudio();
    }

    public void WordsEnded()
    {
        fade.SetActive(true);
        completeBox.SetActive(true);
    }

    public void GoBack()
    {
        playControl.End();
    }

}

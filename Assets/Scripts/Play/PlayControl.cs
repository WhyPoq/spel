using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using System.Globalization;
using UnityEngine.SceneManagement;
using System.Linq;

public class Word
{
    public string word;
    public int level;
    public bool n;
    public DateTime date;

    public Word(string word, int level, bool n, DateTime date)
    {
        this.word = word;
        this.level = level;
        this.n = n;
        this.date = date;
    }
}

public class PlayControl : MonoBehaviour
{
    public static string openedPack = "AnotherTwo";

    [SerializeField]
    private string packSelectScene;

    [SerializeField]
    private PlayControlUI playControlUI;
    [SerializeField]
    private TextToSpeech textToSpeech;

    public List<Word> allWords = new List<Word>();

    public List<Word> wordsToLearn = new List<Word>();
    public List<Word> wordsToRepeat = new List<Word>();

    private Word savedData;

    private int curWord = 0;
    private bool repeating = false;

    void Awake()
    {
        DateTime nowDate = DateTime.Now;
        string fullPath = Path.Combine(Constants.packsDirectory, openedPack + Constants.packExtention);
        if (openedPack != "" && File.Exists(fullPath))
        {
            using (StreamReader sr = new StreamReader(fullPath))
            {
                string line = sr.ReadLine();
                while (line != null)
                {
                    string[] items = line.Split(' ');

                    if (items.Length < 4) continue;

                    int spacesCount = 0;
                    int wordLength = line.Length;
                    for (int i = line.Length - 1; i >= 0; i--)
                    {
                        if (line[i] == ' ') spacesCount++;
                        if (spacesCount == 4)
                        {
                            wordLength = i;
                            break;
                        }
                    }
                    StringBuilder sb = new StringBuilder("");
                    for (int i = 0; i < wordLength; i++)
                    {
                        sb.Append(line[i]);
                    }
                    string word = sb.ToString();

                    int level = 0;
                    Int32.TryParse(items[items.Length - 4], out level);

                    bool n = false;
                    if (items[items.Length - 3] == "n") n = true;

                    DateTime date = DateTime.Now;
                    DateTime.TryParseExact(items[items.Length - 2] + " " + items[items.Length - 1], "dd.MM.yyyy HH:mm",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

                    int daysPassed = (int)(nowDate - date).TotalDays;

                    Word curWord = new Word(word, level, n, date);

                    allWords.Add(curWord);

                    if ((Constants.levels.ContainsKey(level) && Constants.levels[level] != -1 && daysPassed >= Constants.levels[level])
                            || n)
                    {
                        wordsToLearn.Add(curWord);
                    }

                    line = sr.ReadLine();
                }
            }
        }
        System.Random rand = new System.Random();
        wordsToLearn = wordsToLearn.OrderBy(_ => rand.Next()).ToList();
    }

    private void Start()
    {
        repeating = false;
        curWord = 0;
        playControlUI.SetWordsLeft(wordsToLearn.Count);
        NextWord();
    }

    private void WordsEnded()
    {
        playControlUI.WordsEnded();
    }

    public void NextWord()
    {
        if (!repeating)
        {
            if (curWord >= wordsToLearn.Count)
            {
                curWord = 0;
                repeating = true;

                System.Random rand = new System.Random();
                wordsToRepeat = wordsToRepeat.OrderBy(_ => rand.Next()).ToList();

                NextWord();
            }
            else
            {
                textToSpeech.UpdateAndPlayAudio(wordsToLearn[curWord].word);

                playControlUI.DrawWord(wordsToLearn[curWord]);
                playControlUI.SetWordsLeft(wordsToLearn.Count - curWord);

                curWord++;
            }
        }
        else
        {
            if(curWord >= wordsToRepeat.Count)
            {
                WordsEnded();
            }
            else
            {
                textToSpeech.UpdateAndPlayAudio(wordsToRepeat[curWord].word);

                playControlUI.DrawWord(wordsToRepeat[curWord]);
                playControlUI.SetWordsLeft(0);

                curWord++;
            }
        }
    }

    public void ReplayAudio()
    {
        textToSpeech.PlayCurAudio();
    }

    public void ReplayWord()
    {
        curWord--;
        if (!repeating)
        {
            wordsToLearn[curWord].date = savedData.date;
            wordsToLearn[curWord].level = savedData.level;
            wordsToLearn[curWord].n = savedData.n;

            wordsToRepeat.RemoveAt(wordsToRepeat.Count - 1);
        }
        NextWord();
    }

    public void CheckAnswer(string wordWritten)
    {
        bool right = false;

        wordWritten = wordWritten.ToLower();
        string rightWord = "";

        if (!repeating) {
            rightWord = wordsToLearn[curWord - 1].word.ToLower();
        }
        else {
            rightWord = wordsToRepeat[curWord - 1].word.ToLower();
        }

        if (wordWritten == rightWord)
        {
            right = true;
        }

        if (!repeating)
        {
            Word modData = wordsToLearn[curWord - 1];
            savedData = new Word(modData.word, modData.level, modData.n, modData.date);

            modData.date = DateTime.Now;
            modData.n = false;

            if (right) {
                if (Constants.levels.ContainsKey(modData.level + 1))
                {
                    modData.level++;
                }
            }
            else
            {
                modData.level = 1;
                wordsToRepeat.Add(modData);
            }

            wordsToLearn[curWord - 1] = modData;
        }

        string toShow = rightWord;
        if (!right)
        {
            toShow = HighlightErrors(wordWritten, rightWord);
        }
        playControlUI.DrawAnswerWord(right, toShow);
    }

    private void SaveWords()
    {
        string curPack = Path.Combine(Constants.packsDirectory, openedPack + Constants.packExtention);
        using (StreamWriter sw = new StreamWriter(curPack, false))
        {
            foreach (Word word in allWords)
            {
                string line = word.word + " " + word.level + " ";
                if (word.n) line += "n ";
                else line += "o ";
                line += word.date.ToString("dd.MM.yyyy HH:mm");
                sw.WriteLine(line);
            }
        }
    }

    public void End()
    {
        textToSpeech.DestroyCurAudio();
        SaveWords();
        SceneManager.LoadScene(packSelectScene);
    }

    void OnApplicationQuit()
    {
        textToSpeech.DestroyCurAudio();
        SaveWords();
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveWords();
        }
    }

    string HighlightErrors(string writtenWord, string rightWord)
    {
        //"<color=#ff0000ff>colorfully</color> amused"

        int[,] d = new int[writtenWord.Length + 1, rightWord.Length + 1];

        // Initialising first column:
        for (int i = 0; i <= writtenWord.Length; i++)
            d[i, 0] = i;

        // Initialising first row:
        for (int j = 0; j <= rightWord.Length; j++)
            d[0, j] = j;

        // Applying the algorithm:
        int insertion, deletion, replacement;
        for (int i = 1; i <= writtenWord.Length; i++)
        {
            for (int j = 1; j <= rightWord.Length; j++)
            {
                if (writtenWord[i - 1] == (rightWord[j - 1]))
                    d[i, j] = d[i - 1, j - 1];
                else
                {
                    insertion = d[i, j - 1];
                    deletion = d[i - 1, j];
                    replacement = d[i - 1, j - 1];

                    // Using the sub-problems
                    d[i, j] = 1 + Mathf.Min(insertion, deletion, replacement);
                }
            }
        }

        if(d[writtenWord.Length, rightWord.Length] >= rightWord.Length / 2 + 1) 
            return "<color=#ee1c22>" + rightWord + "</color>";

        List<int> redInd = new List<int>();
        List<int> blueInd = new List<int>();

        string markedStr = "";

        int curInd = 0;

        int curI = writtenWord.Length;
        int curJ = rightWord.Length;
        while (curI > 0 || curJ > 0)
        {
            int curVal = 0;
            int maxDir = -1;
            if (curI > 0)
            {
                maxDir = 0;
                curVal = d[curI - 1, curJ];
            }
            if(curJ > 0 && (maxDir == -1 || curVal > d[curI, curJ - 1]))
            {
                maxDir = 1;
                curVal = d[curI, curJ - 1];
            }
            if(curI > 0 && curJ > 0 && curVal >= d[curI - 1, curJ - 1])
            {
                maxDir = 2;
                curVal = d[curI - 1, curJ - 1];
            }

            if(maxDir == 0) //delete curI
            {
                curI--;
                blueInd.Add(rightWord.Length - 1 - curInd);
            }
            else if(maxDir == 1) //insert
            {
                curJ--;
                markedStr += rightWord[curJ];
                redInd.Add(rightWord.Length - 1 - curInd);
                curInd++;
            }
            else if (maxDir == 2) //replace
            {
                curI--;
                curJ--;
                markedStr += rightWord[curJ];
                if(writtenWord[curI] != rightWord[curJ])
                {
                    redInd.Add(rightWord.Length - 1 - curInd);
                }
                curInd++;
            }
        }

        int[] col = new int[rightWord.Length];

        for (int i = 0; i < redInd.Count; i++)
        {
            int ind = redInd[i];
            col[ind] = 2;
        }

        for (int i = 0; i < blueInd.Count; i++)
        {
            int ind = blueInd[i];

            if (ind == -1) col[ind + 1] = 1;
            if (ind == rightWord.Length - 1) col[ind] = 1;

            if(ind >= 0 && ind < rightWord.Length - 1 && (col[ind] != 2 || col[ind + 1] != 2))
            {
                col[ind] = 1;
                col[ind + 1] = 1;
            }
        }

        string coloredStr = "";
        for(int i = 0; i < rightWord.Length; i++)
        {
            if(col[i] == 0)
            {
                coloredStr += rightWord[i];
            }
            else if(col[i] == 1)
            {
                coloredStr += "<color=#0096FF>" + rightWord[i]  + "</color>";
            }
            else if(col[i] == 2)
            {
                coloredStr += "<color=#ee1c22>" + rightWord[i] + "</color>";
            }
        }

        return coloredStr;
    }

}

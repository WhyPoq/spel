using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Globalization;
using UnityEngine.SceneManagement;
using System.Text;

public struct WordData
{
    public string word;
    public int level;
    public bool n;
    public DateTime date;
    public WordElement element;

    public WordData(string word, int level, bool n, DateTime date, WordElement element)
    {
        this.word = word;
        this.level = level;
        this.n = n;
        this.date = date;
        this.element = element;
    }
}

public class EditControl : MonoBehaviour
{
    public static string openedPack = "AnotherTwo";

    [SerializeField]
    private Transform content;
    [SerializeField]
    private GameObject wordElementPrefab;
    [SerializeField]
    private Transform spacer;
    [SerializeField]
    private EditControlUI editControlUI;

    [SerializeField]
    private string packSelectScene;

    private LinkedList<WordData> elements = new LinkedList<WordData>();
    public HashSet<string> hasNames = new HashSet<string>();

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
                    for(int i = line.Length - 1; i >= 0; i--)
                    {
                        if (line[i] == ' ') spacesCount++;
                        if (spacesCount == 4)
                        {
                            wordLength = i;
                            break;
                        }
                    }
                    StringBuilder sb = new StringBuilder("");
                    for(int i = 0; i < wordLength; i++)
                    {
                        sb.Append(line[i]);
                    }
                    string word = sb.ToString();

                    int level = 0;
                    Int32.TryParse(items[items.Length - 4], out level);

                    bool n = false;
                    if (items[items.Length - 3] == "n") n = true;

                    DateTime date = nowDate;
                    DateTime.TryParseExact(items[items.Length - 2] + " " + items[items.Length - 1], "dd.MM.yyyy HH:mm",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

                    GameObject wordElement = Instantiate(wordElementPrefab, content);
                    WordData wordData = new WordData(word, level, n, date, wordElement.GetComponent<WordElement>());
                    elements.AddLast(wordData);
                    hasNames.Add(word);
                    wordElement.GetComponent<WordElement>().SetUp(elements.Last, editControlUI);

                    line = sr.ReadLine();
                }
            }
        }
        spacer.SetSiblingIndex(elements.Count);
    }

    public void RenameCurPack(string newName)
    {
        string oldFile = Path.Combine(Constants.packsDirectory, openedPack + Constants.packExtention);
        string newFile = Path.Combine(Constants.packsDirectory, newName + Constants.packExtention);

        if (File.Exists(oldFile))
        {
            File.Move(oldFile, newFile);
        }

        openedPack = newName;
    }

    public void DeleteCurPack()
    {
        string curPack = Path.Combine(Constants.packsDirectory, openedPack + Constants.packExtention);
        if (File.Exists(curPack))
        {
            File.Delete(curPack);
        }
        CloseEditingScene();
    }

    public void DeleteWord(LinkedListNode<WordData> node)
    {
        Destroy(node.Value.element.gameObject);
        hasNames.Remove(node.Value.word);
        elements.Remove(node);
    }

    public void AddWord(string word)
    {
        GameObject wordElement = Instantiate(wordElementPrefab, content);
        WordData wordData = new WordData(word, 1, true, DateTime.Now, wordElement.GetComponent<WordElement>());
        elements.AddLast(wordData);
        hasNames.Add(word);
        wordElement.GetComponent<WordElement>().SetUp(elements.Last, editControlUI);

        spacer.SetSiblingIndex(elements.Count);
    }

    public void SaveWords()
    {
        string curPack = Path.Combine(Constants.packsDirectory, openedPack + Constants.packExtention);
        using (StreamWriter sw = new StreamWriter(curPack, false))
        {
            foreach (WordData wordData in elements)
            {
                string line = wordData.word + " " + wordData.level + " ";
                if (wordData.n) line += "n ";
                else line += "o ";
                line += wordData.date.ToString("dd.MM.yyyy HH:mm");
                sw.WriteLine(line);
            }
        }
    }

    public void CloseEditingScene()
    {
        SceneManager.LoadScene(packSelectScene);
    }

    public void EndEditing()
    {
        SaveWords();
        CloseEditingScene();
    }

    void OnApplicationQuit()
    {
        SaveWords();
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveWords();
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Globalization;
using UnityEngine.SceneManagement;


public class PackControl : MonoBehaviour
{
    struct PackInfo
    {
        public DateTime oldestDate;
        public int countToRepeat;

        public PackInfo(DateTime oldestDate, int countToRepeat)
        {
            this.oldestDate = oldestDate;
            this.countToRepeat = countToRepeat;
        }
    }

    struct SortStruct
    {
        public PackInfo packInfo;
        public PackElement packElement;

        public SortStruct(PackInfo packInfo, PackElement packElement)
        {
            this.packInfo = packInfo;
            this.packElement = packElement;
        }

        public int CompareToOldest(System.Object item)
        {
            SortStruct that = (SortStruct)item;

            int dateCompare = DateTime.Compare(this.packInfo.oldestDate, that.packInfo.oldestDate);
            if (dateCompare == 0)
            {
                if(this.packInfo.countToRepeat > that.packInfo.countToRepeat)
                {
                    return -1;
                }
                else if (this.packInfo.countToRepeat < that.packInfo.countToRepeat)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }

            return dateCompare;
        }

        public int CompareToNewest(System.Object item)
        {
            SortStruct that = (SortStruct)item;

            int dateCompare = -DateTime.Compare(this.packInfo.oldestDate, that.packInfo.oldestDate);
            if (dateCompare == 0)
            {
                if (this.packInfo.countToRepeat > that.packInfo.countToRepeat)
                {
                    return -1;
                }
                else if (this.packInfo.countToRepeat < that.packInfo.countToRepeat)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }

            return dateCompare;
        }
    }

    [SerializeField]
    private Transform content;
    [SerializeField]
    private GameObject packElementPrefab;

    [SerializeField]
    private string editScene;
    [SerializeField]
    private string playScene;
    [SerializeField]
    private string menuScene;

    private List<SortStruct> repeatElements = new List<SortStruct>();
    private List<SortStruct> otherElements = new List<SortStruct>();

    private int sortType = 0;

    private void Awake()
    {
        LoadAllPacks();
    }

    public void LoadAllPacks()
    {
        DateTime nowDate = DateTime.Now;

        if (!Directory.Exists(Constants.packsDirectory))
        {
            Directory.CreateDirectory(Constants.packsDirectory);
        }

        int from = Constants.packsDirectory.Length;
        string[] fileEntries = Directory.GetFiles(Constants.packsDirectory);
        for (int i = 0; i < fileEntries.Length; i++)
        {
            string fileName = fileEntries[i].Substring(from);
            if (fileName.Length <= Constants.packExtention.Length) continue;

            if (fileName.Substring(fileName.Length - Constants.packExtention.Length) == Constants.packExtention)
            {
                string packName = fileName.Substring(0, fileName.Length - Constants.packExtention.Length);
                PackInfo packInfo = getPackInfo(packName, nowDate);

                GameObject packElement = Instantiate(packElementPrefab, content);
                packElement.GetComponent<PackElement>().SetUp(packName, this, packInfo.countToRepeat);
                
                SortStruct sortStruct = new SortStruct(packInfo, packElement.GetComponent<PackElement>());
                if (packInfo.countToRepeat > 0)
                {
                    repeatElements.Add(sortStruct);
                }
                else
                {
                    otherElements.Add(sortStruct);
                }
            }       
        }

        SortElements();
    }

    private PackInfo getPackInfo(string packName, DateTime nowDate)
    {
        DateTime oldestDate = DateTime.Now;
        int countToRepeat = 0;

        string line;

        using (StreamReader sr = new StreamReader(Path.Combine(Constants.packsDirectory, packName + Constants.packExtention)))
        {
            line = sr.ReadLine();
            while (line != null)
            {
                string[] items = line.Split(' ');

                if (items.Length < 4) continue;

                DateTime wordDate;
                if (DateTime.TryParseExact(items[items.Length - 2] + " " + items[items.Length - 1], "dd.MM.yyyy HH:mm", 
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out wordDate))
                {
                    if (DateTime.Compare(oldestDate, wordDate) > 0)
                    {
                        oldestDate = wordDate;
                    }

                    int daysPassed = (int)(nowDate - wordDate).TotalDays;
                    string stringLevel = items[items.Length - 4];
                    int level;
                    if(Int32.TryParse(stringLevel, out level))
                    {
                        if((Constants.levels.ContainsKey(level) && Constants.levels[level] != -1 && daysPassed >= Constants.levels[level]) 
                            || items[items.Length - 3] == "n")
                        {
                            countToRepeat++;
                        }
                    }
                }

                line = sr.ReadLine();
            }
        }

        return new PackInfo(oldestDate, countToRepeat);
    }

    private void SortElements()
    {
        if (sortType == 0)
        {
            repeatElements.Sort((a, b) => a.CompareToOldest(b));
            otherElements.Sort((a, b) => a.CompareToOldest(b));
        }
        else if(sortType == 1)
        {
            repeatElements.Sort((a, b) => a.CompareToNewest(b));
            otherElements.Sort((a, b) => a.CompareToNewest(b));
        }

        int counter = 0;
        for(int i = 0; i < repeatElements.Count; i++)
        {
            repeatElements[i].packElement.transform.SetSiblingIndex(counter);
            counter++;
        }
        for (int i = 0; i < otherElements.Count; i++)
        {
            otherElements[i].packElement.transform.SetSiblingIndex(counter);
            counter++;
        }
    }

    public void OpenPack(string name, int countToRepeat)
    {
        if (countToRepeat > 0)
        {
            PlayControl.openedPack = name;
            SceneManager.LoadScene(playScene);
        }
    }

    public void EditPack(string name)
    {       
        EditControl.openedPack = name;
        SceneManager.LoadScene(editScene);
    }

    public void OpenMenu()
    {
        SceneManager.LoadScene(menuScene);
    }

    public void CreatePack(string name)
    {
        File.Create(Path.Combine(Constants.packsDirectory, name + Constants.packExtention));

        GameObject packElement = Instantiate(packElementPrefab, content);
        packElement.GetComponent<PackElement>().SetUp(name, this, 0);
        otherElements.Add(new SortStruct(new PackInfo(DateTime.Now, 0), packElement.GetComponent<PackElement>()));

        if (sortType == 0)
        {
            packElement.transform.SetSiblingIndex(repeatElements.Count + otherElements.Count - 1);
        }
        else if(sortType == 1)
        {
            otherElements.Sort((a, b) => a.CompareToNewest(b));
            for (int i = 0; i < otherElements.Count; i++)
            {
                otherElements[i].packElement.transform.SetSiblingIndex(repeatElements.Count + i);
            }
        }
    }
}

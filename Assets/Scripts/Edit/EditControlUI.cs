using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class EditControlUI : MonoBehaviour
{
    [SerializeField]
    private EditControl editControl;

    [SerializeField]
    private TextMeshProUGUI packNameText;
    [SerializeField]
    private TMP_InputField packNameInput;
    [SerializeField]
    private GameObject fade;
    [SerializeField]
    private GameObject editPackPanel;
    [SerializeField]
    private GameObject deleteConformation;
    [SerializeField]
    private GameObject wordEditPanel;
    [SerializeField]
    private TMP_InputField wordNameInput;
    [SerializeField]
    private TextMeshProUGUI wordLevel;
    [SerializeField]
    private GameObject deleteWordConformation;
    [SerializeField]
    private GameObject addWordPanel;
    [SerializeField]
    private GameObject addWordPanelClone;
    [SerializeField]
    private TMP_InputField addWordInput;
    [SerializeField]
    private TMP_InputField addWordCloneInput;

    [SerializeField]
    private float swipeTime = 0.3f;

    private LinkedListNode<WordData> curNode;
    private WordData curData;

    private float swipeProgress = -1;
    private float swipeCurTime = 0;

    private void Awake()
    {
        packNameText.text = EditControl.openedPack;
    }

    public void ClosePack()
    {
        editControl.EndEditing();
    }

    public void OpenEditPack()
    {
        packNameInput.text = EditControl.openedPack;

        fade.SetActive(true);
        editPackPanel.SetActive(true);
    }

    public void CloseEditPack()
    {
        fade.SetActive(false);
        editPackPanel.SetActive(false);
    }

    public void OpenDeletePack()
    {
        CloseEditPack();

        fade.SetActive(true);
        deleteConformation.SetActive(true);
    }

    public void CloseDeletePack()
    {
        fade.SetActive(false);
        deleteConformation.SetActive(false);

        OpenEditPack();
    }

    public void ConfirmPackEditing()
    {
        string filename = packNameInput.text + Constants.packExtention;
        if (packNameInput.text != "" && filename.IndexOfAny(Path.GetInvalidFileNameChars()) < 0)
        {
            editControl.RenameCurPack(packNameInput.text);
            packNameText.text = EditControl.openedPack;
            CloseEditPack();
        }
    }

    public void DeletePack()
    {
        editControl.DeleteCurPack();
        CloseDeletePack();
        CloseEditPack();
    }

    public void EditWord(LinkedListNode<WordData> node)
    {
        curNode = node;
        curData = node.Value;

        fade.SetActive(true);
        wordEditPanel.SetActive(true);

        wordNameInput.text = curData.word;
        if (Constants.levels.ContainsKey(curData.level) && Constants.levels[curData.level] == -1)
        {
            wordLevel.text = "Max";
        }
        else
        {
            wordLevel.text = curData.level.ToString();
        }
    }

    public void CloseEditWord()
    {
        fade.SetActive(false);
        wordEditPanel.SetActive(false);
    }

    public void ConfirmWordEdit()
    {
        if (wordNameInput.text != "" && !wordNameInput.text.Contains('\n') && 
            (!editControl.hasNames.Contains(wordNameInput.text) || curData.word == wordNameInput.text))
        {
            curData.word = wordNameInput.text;
            curNode.Value = curData;
            curNode.Value.element.SetUp(curNode, this);

            CloseEditWord();
        }
    }

    public void ChangeLevel(int amount)
    {
        if(Constants.levels.ContainsKey(curData.level + amount)) curData.level += amount;

        if (Constants.levels.ContainsKey(curData.level) && Constants.levels[curData.level] == -1)
        {
            wordLevel.text = "Max";
        }
        else
        {
            wordLevel.text = curData.level.ToString();
        }
    }

    public void OpenDeleteWord()
    {
        CloseEditWord();

        fade.SetActive(true);
        deleteWordConformation.SetActive(true);
    }

    public void CloseDeleteWord()
    {
        fade.SetActive(false);
        deleteWordConformation.SetActive(false);

        EditWord(curNode);
    }

    public void DeleteWord()
    {
        editControl.DeleteWord(curNode);
        CloseDeleteWord();
        CloseEditWord();
    }

    public void OpenAddWord()
    {
        addWordInput.text = "";

        fade.SetActive(true);
        addWordPanel.SetActive(true);

        addWordPanel.GetComponent<CanvasGroup>().interactable = true;
    }

    public void SwipeAddWord()
    {
        addWordPanelClone.SetActive(true);

        addWordCloneInput.text = addWordInput.text;
        addWordInput.text = "";

        addWordPanel.GetComponent<CanvasGroup>().interactable = false;

        swipeProgress = 0;
        swipeCurTime = 0;
    }

    public void ConfirmAddWord()
    {
        if (addWordInput.text != "" && !addWordInput.text.Contains('\n') && !editControl.hasNames.Contains(addWordInput.text))
        {
            editControl.AddWord(addWordInput.text);

            //maybe add card change animation
            SwipeAddWord();
            addWordInput.text = "";
        }
    }

    public void CloseAddWord()
    {
        fade.SetActive(false);
        addWordPanel.SetActive(false);
    }

    private void Update()
    {
        if (swipeProgress != -1)
        {
            swipeCurTime += Time.deltaTime;
            swipeProgress = swipeCurTime / swipeTime;

            if (swipeProgress < 1)
            {
                float panelWidth = addWordPanel.GetComponent<RectTransform>().rect.width;
                addWordPanel.GetComponent<RectTransform>().localPosition = new Vector3(-(Screen.width + panelWidth) / 2 * (1 - swipeProgress), 0, 0);

                float clonePanelWidth = addWordPanelClone.GetComponent<RectTransform>().rect.width;
                addWordPanelClone.GetComponent<RectTransform>().localPosition = new Vector3((Screen.width + clonePanelWidth) / 2 * swipeProgress, 0, 0);
            }
            else
            {
                swipeProgress = -1;
                addWordPanel.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);

                addWordPanelClone.SetActive(false);
                addWordPanel.GetComponent<CanvasGroup>().interactable = true;
            }

        }
    }

}

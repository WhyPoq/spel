using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WordElement : MonoBehaviour
{
    [SerializeField]
    private Button editButton;
    [SerializeField]
    private TextMeshProUGUI nameText;
    [SerializeField]
    private TextMeshProUGUI levelText;

    private LinkedListNode<WordData> node;

    public void SetUp(LinkedListNode<WordData> node, EditControlUI editControlUI)
    {
        this.node = node;

        nameText.text = node.Value.word;

        if (Constants.levels.ContainsKey(node.Value.level) && Constants.levels[node.Value.level] == -1)
        {
            levelText.text = "Max";
        }
        else
        {
            levelText.text = node.Value.level.ToString();
        }

        editButton.onClick.AddListener(delegate { editControlUI.EditWord(this.node); });
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class PackElement : MonoBehaviour
{
    [SerializeField]
    private Button openPackButton;
    [SerializeField]
    private Button editPackButton;
    [SerializeField]
    private TextMeshProUGUI nameText;
    [SerializeField]
    private TextMeshProUGUI countToRepeatText;

    public string elementName;
    public int countToRepeat;

    public void SetUp(string elementName, PackControl packControl, int countToRepeat)
    {
        this.elementName = elementName;
        nameText.text = elementName;

        this.countToRepeat = countToRepeat;
        countToRepeatText.text = countToRepeat.ToString();

        openPackButton.onClick.AddListener(delegate { packControl.OpenPack(this.elementName, this.countToRepeat); });
        editPackButton.onClick.AddListener(delegate { packControl.EditPack(this.elementName); });
    }
}

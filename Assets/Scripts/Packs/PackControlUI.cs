using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class PackControlUI : MonoBehaviour
{
    [SerializeField]
    private GameObject packAddPanel;
    [SerializeField]
    private GameObject fade;
    [SerializeField]
    private TMP_InputField inputField;

    [SerializeField]
    private PackControl packControl;

    public void OpenAddPackPanel()
    {
        inputField.text = "";
        packAddPanel.SetActive(true);
        fade.SetActive(true);
    }

    public void CloseAddPackPanel()
    {
        packAddPanel.SetActive(false);
        fade.SetActive(false);
    }

    public void CreatePack()
    {
        string filename = inputField.text + Constants.packExtention;
        if (inputField.text != "" && filename.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 && 
            !File.Exists(Path.Combine(Constants.packsDirectory, filename))){
            packControl.CreatePack(inputField.text);
            CloseAddPackPanel();
        }
    }

    public void OpenMenu()
    {
        packControl.OpenMenu();
    }
}

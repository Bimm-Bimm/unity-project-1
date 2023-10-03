using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;
public class MainMenuManager : MonoBehaviour
{
    public GameObject StartPanel;
    public GameObject sessionListPanel;
    public GameObject createSessionlPanel;
    public GameObject statusPanel;
    //public GameObject BG;
    public TMP_InputField playerName;
    void Start()
    {
        if (PlayerPrefs.HasKey("PlayerName")) 
        {
            playerName.text = PlayerPrefs.GetString("PlayerName");
        }
    }
    public void HideAllPanel() 
    {
        StartPanel.SetActive(false);
        sessionListPanel.SetActive(false);
        createSessionlPanel.SetActive(false);
        statusPanel.SetActive(false);
}

    public void OnPlayClick() 
    {
        PlayerPrefs.SetString("PlayerName",playerName.text);
        PlayerPrefs.Save();
        NetworkSetup networkSetup = FindObjectOfType<NetworkSetup>();
        networkSetup.OnJoinLobby();
        HideAllPanel();      
        sessionListPanel.SetActive(true);
        FindObjectOfType<SessionListUIHandler>(true).OnLookingForGameSession();

    }
    public void OnCreateNewGameClick() 
    {
        HideAllPanel();
        createSessionlPanel.SetActive(true);
    }
    public void OnStartGameClick() 
    {
        Debug.Log("on start click");

        NetworkSetup networkSetup = FindObjectOfType<NetworkSetup>();
        networkSetup.CreateGame(playerName.text,1);
     
        HideAllPanel();
        statusPanel.GetComponentInChildren<TMP_Text>().text = "ON JOINING GAME";
        statusPanel.SetActive(true);

    }
    public void OnJoiningServer()
    {
        HideAllPanel();
        statusPanel.GetComponentInChildren<TMP_Text>().text = "ON JOINING GAME";
        statusPanel.SetActive(true);

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
public class SessionListUIHandler : MonoBehaviour
{
    public TMP_Text statusText;
    public GameObject sessionInfoListItemPrefab;
    public VerticalLayoutGroup verticalLayoutGroup;
    private void Awake()
    {
        ClearList();
    }
    public void ClearList() 
    {
        foreach (Transform child in verticalLayoutGroup.transform) 
        {
            Destroy(child.gameObject);
        }
        statusText.gameObject.SetActive(false);
    }
    public void AddToList(SessionInfo sessionInfo) 
    {
        SessionInfoListItem addedsessionInfoListItem = Instantiate(sessionInfoListItemPrefab, verticalLayoutGroup.transform).GetComponent<SessionInfoListItem>();
        addedsessionInfoListItem.SetInfomation(sessionInfo);
        addedsessionInfoListItem.OnJoinSession += AddedSessionInfoListItem_OnJoinSession;
    }
    private void AddedSessionInfoListItem_OnJoinSession(SessionInfo sessionInfo) 
    {
        NetworkSetup networkSetup = FindObjectOfType<NetworkSetup>();
        networkSetup.JoinGame(sessionInfo);
        MainMenuManager mainMenu = FindObjectOfType<MainMenuManager>();
        mainMenu.OnJoiningServer();

    }
    public void OnNoSessionFound() 
    {
        ClearList();

        statusText.text = "No game session found (T.T)";
        statusText.gameObject.SetActive(true);
    }
    public void OnLookingForGameSession() 
    {
        ClearList();
        statusText.text = "On looking for game session (@-@)";
        statusText.gameObject.SetActive(true);
    }
    
}

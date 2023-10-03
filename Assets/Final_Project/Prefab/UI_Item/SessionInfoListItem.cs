using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using Fusion;
using System;

public class SessionInfoListItem : MonoBehaviour
{
    public TMP_Text sessionNameText;
    public TMP_Text playerCountText;
    public Button joinButton;
    SessionInfo sessionInfo;


    public event Action<SessionInfo> OnJoinSession;
    public void SetInfomation(SessionInfo sessionInfo) 
    {
        this.sessionInfo = sessionInfo;
        //sessionNameText.text = sessionInfo.Name;
        sessionNameText.text = sessionInfo.Name;
        playerCountText.text = $"{sessionInfo.PlayerCount.ToString()}/{sessionInfo.MaxPlayers.ToString()}";
        bool isJoinButtonActive = true;
        if (sessionInfo.PlayerCount >= sessionInfo.MaxPlayers)
            isJoinButtonActive = false;

        joinButton.gameObject.SetActive(isJoinButtonActive);
    }
    public void OnClick() 
    {
        OnJoinSession?.Invoke(sessionInfo);
    }
    public void OnJoinClick() 
    {
        NetworkSetup networkSetup = FindObjectOfType<NetworkSetup>();
        networkSetup.JoinGame(this.sessionInfo);
    }

}

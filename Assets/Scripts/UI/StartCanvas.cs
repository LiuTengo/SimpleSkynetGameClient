using System;
using System.Net;
using System.Net.Sockets;
using CardNetWork;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class StartCanvas : MonoBehaviour
{
    public TMP_InputField ipInputField;
    public TMP_InputField portInputField;
    public TMP_Text errorTextField;
    
    public TMP_InputField userNameInputField;
    public TMP_InputField userPasswordInputField;

    public UnityEvent OnConnectSuccessEvent;
    public UnityEvent OnConnectFailedEvent;

    public GameObject LoginSuccessPanel;
    public GameObject LoginFailPanel;


    public void SubmitIPAndPort()
    {
        string ip = ipInputField.text;
        string port = portInputField.text;
        int portNumber = int.Parse(port);
        
        try
        {
            CardNetWork.NetWorkManager.instance.ConnectToHost(ip, portNumber,OnConnectSuccessEvent,OnConnectFailedEvent);
        }
        catch (Exception e)
        {
            errorTextField.text = e.Message;
        }
    }

    private void Start()
    {
        NetWorkManager.instance.RegisterCmd("login0", OnLoginSuccess);
        NetWorkManager.instance.RegisterCmd("login1", OnLoginFailed);
        
        NetWorkManager.instance.RegisterCmd("enter0", OnEnterGameSuccess);
        NetWorkManager.instance.RegisterCmd("enter1", OnEnterGameFailed);
    }

    private void OnDisable()
    {
        NetWorkManager.instance.UnRegisterCmd("login0",OnLoginSuccess);
        NetWorkManager.instance.UnRegisterCmd("login1",OnLoginFailed);
        
        NetWorkManager.instance.UnRegisterCmd("enter0", OnEnterGameSuccess);
        NetWorkManager.instance.UnRegisterCmd("enter1", OnEnterGameFailed);
    }

    public void LoginGame()
    {
        string username = userNameInputField.text;
        string password = userPasswordInputField.text;
        
        NetWorkManager.instance.SetPlayerInfo(username, password);
        NetWorkManager.instance.SendMessageToServer("login", new [] {username,password});
    }

    private void OnLoginSuccess(ServerMessageEventArgs args)
    {
        NetWorkManager.instance.SendMessageToServer("enter");
    }

    private void OnLoginFailed(ServerMessageEventArgs args)
    {
        LoginFailPanel.SetActive(true);
    }
    
    private void OnEnterGameSuccess(ServerMessageEventArgs args)
    {
        SceneManager.LoadScene("GameLoby");
    }
    
    private void OnEnterGameFailed(ServerMessageEventArgs args)
    {
        Debug.LogError("Enter Game Failed");
    }
}

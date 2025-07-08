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
        NetWorkManager.instance.RegisterCmd("login", OnLogin);
        NetWorkManager.instance.RegisterCmd("enter", OnEnterGame);
    }

    private void OnDisable()
    {
        NetWorkManager.instance.UnRegisterCmd("login",OnLogin);
        NetWorkManager.instance.UnRegisterCmd("enter", OnEnterGame);
    }

    public void LoginGame()
    {
        string username = userNameInputField.text;
        string password = userPasswordInputField.text;
        
        NetWorkManager.instance.SetPlayerInfo(username, password);
        NetWorkManager.instance.SendMessageToServer("login", new [] {username,password});
    }

    private void OnLogin(ServerMessageEventArgs args)
    {
        if (args.RunResult == ServerRunResult.Success)
        {
            NetWorkManager.instance.SendMessageToServer("enter");
        }
        else
        {
            LoginFailPanel.SetActive(true);
        }
    }
    
    private void OnEnterGame(ServerMessageEventArgs args)
    {
        if (args.RunResult == ServerRunResult.Success)
        {
            SceneManager.LoadScene("GameLoby");
        }
        else
        {
            Debug.LogError("Enter Game Failed");
        }
    }
}

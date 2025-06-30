using System;
using System.Net;
using System.Net.Sockets;
using TMPro;
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
    
    public UnityAction OnLoginSuccessEvent;
    public UnityAction OnLoginFailedEvent;

    public void OnEnable()
    {
        OnLoginSuccessEvent += OnLoginSuccess;
        OnLoginFailedEvent += OnLoginFailed;
    }

    private void OnDisable()
    {
        OnLoginSuccessEvent -= OnLoginSuccess;
        OnLoginFailedEvent -= OnLoginFailed;
    }

    public void SubmitIPAndPort()
    {
        string ip = ipInputField.text;
        string port = portInputField.text;
        int portNumber = int.Parse(port);
        
        try
        {
            NetWorkManager.instance.ConnectToHost(ip, portNumber,OnConnectSuccessEvent,OnConnectFailedEvent);
        }
        catch (Exception e)
        {
            errorTextField.text = e.Message;
        }
    }

    public void LoginGame()
    {
        string username = userNameInputField.text;
        string password = userPasswordInputField.text;
        
        NetWorkManager.instance.RegisterCmd("login0",OnLoginSuccessEvent);
        NetWorkManager.instance.RegisterCmd("login1",OnLoginFailedEvent);
        
        string[] msg = { "login",username,password}; 
        NetWorkManager.instance.Login(msg);
        
        NetWorkManager.instance.UnRegisterCmd("login0",OnLoginSuccessEvent);
        NetWorkManager.instance.UnRegisterCmd("login1",OnLoginFailedEvent);
    }

    private void OnLoginSuccess()
    {
        SceneManager.LoadScene("GameLoby");
    }

    private void OnLoginFailed()
    {
        LoginFailPanel.SetActive(true);
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using General;
using UnityEngine;
using UnityEngine.Events;

public class NetWorkManager : SingletonMono<NetWorkManager>
{
    private Socket socket;
    private string ipAddress;
    private int serverPort;
    private IPEndPoint endPoint;
    private Dictionary<string, UnityEvent> cmdEvents= new Dictionary<string, UnityEvent>();

    #region Attribute
    public Socket Socket => socket;
    public string IP => ipAddress;
    public int Port => serverPort;
    public IPEndPoint EndPoint => endPoint;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        cmdEvents.Add("login0",new UnityEvent()); //登录成功
        cmdEvents.Add("login1",new UnityEvent()); //登录失败
    }

    /// <summary>
    /// 玩家链接到服务器
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    public void ConnectToHost(string ip, int port, UnityEvent onConnected = null, UnityEvent onConnectedFailed = null)
    {
        this.ipAddress = ip;
        this.serverPort = port;
        
        try
        {
            endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(endPoint);
            onConnected?.Invoke();
        }
        catch (Exception e)
        {
            onConnectedFailed?.Invoke();
            //Debug.LogError(e.Message);
            throw;
        }
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="loginInfo">登录信息（cmd,username,password）</param>
    /// <exception cref="Exception"></exception>
    public void Login(string[] loginInfo)
    {
        if (socket != null)
        {
            try
            {
                string msgToServer = string.Join(",", loginInfo) + "\r\n";
                Debug.Log(msgToServer);
                socket.Send(Encoding.UTF8.GetBytes(msgToServer));
                ReceiveMsgFromServer();
            }
            catch
            {
                throw;
            }
        }
        else
        {
            throw new Exception("You have to connect first!");
        }
    }

    /// <summary>
    /// 解码服务器信息
    /// </summary>
    /// <param name="msg"></param>
    public void EncodingLoginMsgFromServer(string msg)
    {
        //cmd,cmd_code,description
        //login,0,登陆成功
        //login,1,登陆失败(password error\)
        var splitMsg = msg.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        foreach (var m in splitMsg)
        {
            var splitM = m.Split(",", StringSplitOptions.RemoveEmptyEntries);
            string cmd = splitM[0]+splitM[1];
            if (cmdEvents.TryGetValue(cmd, out var handler))
            {
                handler.Invoke();
            }
            else
            {
                Debug.LogWarning($"未知命令: {cmd}");
            }
        }
    }

    /// <summary>
    /// 接收服务器信息
    /// </summary>
    public void ReceiveMsgFromServer()
    {
        byte[] msg = new byte[1024];
        int msgLength = socket.Receive(msg);
        string msgFromServer = Encoding.UTF8.GetString(msg, 0, msgLength);
        EncodingLoginMsgFromServer(msgFromServer);
    }

    public void RegisterCmd(string cmd, UnityAction cmdEvt)
    {
        if (!cmdEvents.ContainsKey(cmd))
        {
            if (cmdEvents.TryAdd(cmd, new UnityEvent()))
            {
                cmdEvents[cmd].AddListener(cmdEvt);
            }
            else
            {
                throw new Exception("Add CmdEvent Error!");
            }
        }
        else
        {
            cmdEvents[cmd].AddListener(cmdEvt);
        }
    }

    public void UnRegisterCmd(string cmd, UnityAction cmdEvt)
    {
        if (!cmdEvents.ContainsKey(cmd))
        {
            throw new Exception("CmdEvent does not exist!");
        }
        else
        {
            cmdEvents[cmd].RemoveListener(cmdEvt);
        }
    }
}
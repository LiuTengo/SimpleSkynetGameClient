using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
        EnableThread();
        
        cmdEvents.Add("login0",new UnityEvent()); //登录成功
        cmdEvents.Add("login1",new UnityEvent()); //登录失败
        
        cmdEvents.Add("bet0", new UnityEvent()); //下注成功
        cmdEvents.Add("bet1", new UnityEvent()); //下注失败
        cmdEvents.Add("start_sendcard0", new UnityEvent()); //开始发牌
        cmdEvents.Add("start_sendcard1", new UnityEvent()); //发牌失败
        cmdEvents.Add("hit0", new UnityEvent()); //抽牌
        cmdEvents.Add("hit0", new UnityEvent());
        cmdEvents.Add("stand", new UnityEvent());
        cmdEvents.Add("restart_game", new UnityEvent());
        cmdEvents.Add("leave", new UnityEvent());
        
        // 广播消息
        cmdEvents.Add("update_player_info", new UnityEvent());
        cmdEvents.Add("result", new UnityEvent());
        cmdEvents.Add("game_restart", new UnityEvent());
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
        Debug.Log(msg);
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
    
     // 新增游戏命令事件
    private Dictionary<string, UnityEvent<string[]>> gameCmdEvents = new();

    // 初始化游戏命令
    private void InitGameCommands()
    {
        gameCmdEvents.Add("enter", new UnityEvent<string[]>());
        gameCmdEvents.Add("bet", new UnityEvent<string[]>());
        gameCmdEvents.Add("start_sendcard", new UnityEvent<string[]>());
        gameCmdEvents.Add("hit", new UnityEvent<string[]>());
        gameCmdEvents.Add("stand", new UnityEvent<string[]>());
        gameCmdEvents.Add("restart_game", new UnityEvent<string[]>());
        gameCmdEvents.Add("leave", new UnityEvent<string[]>());
        
        // 广播消息
        gameCmdEvents.Add("update_player_info", new UnityEvent<string[]>());
        gameCmdEvents.Add("result", new UnityEvent<string[]>());
        gameCmdEvents.Add("game_restart", new UnityEvent<string[]>());
    }

    // 发送游戏命令
    public void SendGameCommand(string command, params object[] args)
    {
        if (socket == null || !socket.Connected)
        {
            Debug.LogError("未连接到服务器");
            return;
        }

        StringBuilder sb = new StringBuilder(command);
        foreach (var arg in args)
        {
            sb.Append(",");
            sb.Append(arg.ToString());
        }
        sb.Append("\r\n");

        byte[] data = Encoding.UTF8.GetBytes(sb.ToString());
        socket.Send(data);
        Debug.Log($"发送命令: {sb}");
    }

    // 注册/注销游戏命令处理器
    public void RegisterGameHandler(string cmd, UnityAction<string[]> action)
    {
        if (gameCmdEvents.TryGetValue(cmd, out var handler))
        {
            handler.AddListener(action);
        }
    }

    public void UnregisterGameHandler(string cmd, UnityAction<string[]> action)
    {
        if (gameCmdEvents.TryGetValue(cmd, out var handler))
        {
            handler.RemoveListener(action);
        }
    }
    
    Thread thread_GlobalInfo;

    //开启线程
    void EnableThread()
    {
        //首先要创建一个线程
        thread_GlobalInfo = new Thread(ReceiveInfo);

        //然后调用Start函数启动该线程
        thread_GlobalInfo.Start();
    }

    /// <summary>
    /// 每帧监听消息
    /// </summary>
    void ReceiveInfo()
    {
        while (true)
        {
            ReceiveMsgFromServer();
        }
    }
}
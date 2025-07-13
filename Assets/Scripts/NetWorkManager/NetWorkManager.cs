using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using General;
using UnityEngine;
using UnityEngine.Events;

namespace CardNetWork
{
    public enum ServerRunResult
    {
        Success = 0,
        Failed = 1,
    }
    
    public class ServerMessageEventArgs :EventArgs{
        public string Cmd { get; set; }        // 比如 login
        
        public ServerRunResult RunResult { get; set; }       // 比如 0 表示成功
        public string Parameters { get; set; } // 比如 “登录成功”
        public string Raw { get; set; }        // 原始消息
    }
    
    public delegate void ServerMessageHandler(ServerMessageEventArgs args);

    public class NetWorkManager : SingletonMono<NetWorkManager>
    {
        private Socket socket;
        private string ipAddress;
        private int serverPort;
        private string playerName;
        private string playerPassword;
        private string opponentName;
        private IPEndPoint endPoint;
        private Dictionary<string, ServerMessageHandler> cmdEvents= new Dictionary<string, ServerMessageHandler>();

        //Message Queue
        public bool isListening;
        private ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();
        
        #region Attribute
        public string PlayerName => playerName;
        public string OpponentName => opponentName;
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

        private void Update()
        {
            while (mainThreadActions.TryDequeue(out Action action))
            {
                //mainThreadActions.TryDequeue(out Action action);
                if (action != null)
                {
                    action?.Invoke();
                }
                
                //catch (SocketException ex)
                //{
                //    Debug.LogError("Socket 异常: " + ex.Message);
                //    isListening = false;
                //}
                //catch (Exception e)
                //{
                //    Debug.LogError("接收线程异常: " + e.Message);
                //    isListening = false;
                //}
            }
        }


        private void OnDisable()
        {
            StopListenThread();
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
                StartListenThread();
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
        /// <exception cref="Exception"></exception>
        public void SendMessageToServer(string cmd, string[] args = null)
        {
            if (socket != null)
            {
                try
                {
                    // 拼接命令和参数为字符串："hit,2,10\r\n"
                    string msgToServer = cmd + ((args != null && args.Length > 0) ? "," + string.Join(",", args) : "") + "\r\n";
            
                    socket.Send(Encoding.UTF8.GetBytes(msgToServer));
                    //string msg = ReceiveMsgFromServer();
                    //EncodingMsgFromServer(msg);
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

        public void SetPlayerInfo(string username, string password)
        {
            playerName = username;
            playerPassword = password;
        }

        /// <summary>
        /// 解码服务器信息
        /// </summary>
        /// <param name="msg"></param>
        private void EncodingMsgFromServer(string msg)
        {
            //cmd,cmd_code,description
            //login,0,登陆成功
            //login,1,登陆失败(password error\)
            var splitMsg = msg.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
            foreach (var m in splitMsg)
            {
                var splitM = m.Split(",", StringSplitOptions.RemoveEmptyEntries);
                if (splitM.Length < 3) continue;

                var args = new ServerMessageEventArgs
                {
                    Cmd = splitM[0],
                    RunResult = (ServerRunResult)int.Parse(splitM[1]),
                    Parameters = string.Join(",", splitM, 2, splitM.Length - 2),
                    Raw = m
                };

                string key = args.Cmd;
                if (cmdEvents.TryGetValue(key, out var handler))
                {
                    handler?.Invoke(args);
                }
                else
                {
                    Debug.LogWarning($"未知命令: {key}");
                }
            }
        }
    
        /// <summary>
        /// 接收服务器信息
        /// </summary>
        private string ReceiveMsgFromServer()
        {
            byte[] msg = new byte[1024];
            
            if(socket == null) return null;
            
            int msgLength = socket.Receive(msg);
            
            if(msgLength <= 0) return null;
            
            string msgFromServer  = Encoding.UTF8.GetString(msg, 0, msgLength);
            return msgFromServer;
        }

        public void RegisterCmd(string cmd, ServerMessageHandler handler)
        {
            if (cmdEvents.ContainsKey(cmd))
            {
                cmdEvents[cmd] += handler; // 多播支持
            }
            else
            {
                cmdEvents[cmd] = handler;
            }
        }

        public void UnRegisterCmd(string cmd, ServerMessageHandler handler)
        {
            if (cmdEvents.ContainsKey(cmd))
            {
                cmdEvents[cmd] -= handler;
                if (cmdEvents[cmd] == null)
                {
                    cmdEvents.Remove(cmd);
                }
            }
        }
        
        
        Thread thread_GlobalInfo;
        //开启线程
        private void StartListenThread()
        {
            isListening = true;
            thread_GlobalInfo = new Thread(ReceiveInfo);
            thread_GlobalInfo.IsBackground = true;
            thread_GlobalInfo.Start();
        }

        private void StopListenThread()
        {
            isListening = false;
            socket?.Shutdown(SocketShutdown.Both);
            socket?.Close();
        }
        
        /// <summary>
        /// 每帧监听消息
        /// </summary>
        private void ReceiveInfo()
        {
            //FIXME: 需要用消息队列完成线程间的通信
            while (true)
            {
                string msg = ReceiveMsgFromServer();
                if (msg != null)
                {
                    mainThreadActions.Enqueue(() =>
                    {
                        // 在主线程中处理消息（比如事件分发）
                        EncodingMsgFromServer(msg); // 原来的 EncodingLoginMsgFromServer
                    });
                }
            }
        }
    }
}
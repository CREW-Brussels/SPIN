using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Brussels.Crew.Spin.Spin
{
    [Serializable]
    public class HTTPFile
    {
        public string path;
        public string contentType;
        public TextAsset file;
    }

    public class HTTPInterface : MonoBehaviour
    {
        [FormerlySerializedAs("Port")] public int port = 8080;
        public List<HTTPFile> files;
        public TMP_Text iptext;
        private HttpListener _listener;
        private SpinConfigManager _configManager;
        private Queue<HttpListenerContext> _httpListenerContexts = new Queue<HttpListenerContext>();
        private int _ipIndex = 0;
        private float _timeElapsed = 0;
        
        void Start()
        {
            _configManager = SpinConfigManager.Instance;

            _listener = new HttpListener();
            _listener.Prefixes.Add("http://*:" + port.ToString() + "/");
            _listener.Start();

            Receive();
        }
        
        void UpdateIpText()
        {
            List<string> ips = GetLocalIPAddress();
            if (_ipIndex >= ips.Count)
            {
                iptext.text = "Spin " + Application.version;
                _ipIndex = 0;
            }
            else
            {
                iptext.text = ips[_ipIndex] + ":" + port.ToString();
                _ipIndex++;
            }
        }
        
        private List<string> GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            
            List<string> addresses = new List<string>();
            
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    addresses.Add(ip.ToString());
                }
            }
            return addresses;
        }

        private void Receive()
        {
            _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);
        }

        private void ListenerCallback(IAsyncResult result)
        {
            if (_listener.IsListening)
            {
                _httpListenerContexts.Enqueue(_listener.EndGetContext(result));
                Receive();
            }
        }
        
        private void Update() 
        {
            _timeElapsed += Time.deltaTime;
            if (_timeElapsed >= 5.0f)
            {
                UpdateIpText();
                _timeElapsed = 0;
            }

            if (_httpListenerContexts.TryDequeue(out var context))
            {
                HttpListenerRequest request = context.Request;

                if (request.Url.Segments.Length >= 2 && request.Url.Segments[1].Trim('/') == "settings")
                    ExecuteAPI(request.Url.AbsolutePath, context);
                else if (request.Url.Segments.Length >= 2 && request.Url.Segments[1].Trim('/') == "clear")
                    ClearConf(request.Url.AbsolutePath, context);
                else
                    SendFile(request.Url.AbsolutePath, context);
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void ClearConf(string urlAbsolutePath, HttpListenerContext context)
        {
            _configManager.ClearSpinConfig();
            context.Response.StatusCode = 200;
            context.Response.OutputStream.Close();
        }

        private void ExecuteAPI(string method, HttpListenerContext context)
        {
            HttpListenerResponse response = context.Response;
            HttpListenerRequest request = context.Request;

            response.ContentType = "application/json";
            response.StatusCode = 200;

            if (request.HasEntityBody)
            {
                if (request.ContentType != "application/json")
                {
                    response.StatusCode = 500;
                    response.StatusDescription = "Unexpected Content Type";
                }
                else
                {
                    StreamReader stream = new StreamReader(request.InputStream);
                    string data = stream.ReadToEnd();

                    OSCTrackerConfig newConfig = JsonUtility.FromJson<OSCTrackerConfig>(data);

                    if (newConfig == null)
                    {
                        response.StatusCode = 500;
                        response.StatusDescription = "Invalid Data";
                    }
                    else
                    {
                        _configManager.OSCTrackersConfig = newConfig;
                        _configManager.SaveSpinConfig();
                    }
                }
            }

            if (response.StatusCode == 200)
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(_configManager.OSCTrackersConfig));
                response.OutputStream.Write(data, 0, data.Length);
            }
            response.OutputStream.Close();
        }
        private void SendFile(string path, HttpListenerContext context)
        {
            HttpListenerResponse response = context.Response;
            response.StatusCode = 404;

            foreach (HTTPFile file in files)
            {
                if (file.path == path)
                {
                    response.StatusCode = 200;
                    response.ContentType = file.contentType;


                    byte[] data = file.file.bytes;
                    response.OutputStream.Write(data, 0, data.Length);
                }
            }

            response.OutputStream.Close();
        }
    }
}

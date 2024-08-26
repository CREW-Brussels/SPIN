using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Xml.Linq;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Brussels.Crew.Spin
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
        public int Port = 8080;
        public List<HTTPFile> files;

        private HttpListener _listener;
        private SpinConfigManager configManager;
        private Queue<HttpListenerContext> httpListenerContexts = new Queue<HttpListenerContext>();

        void Start()
        {
            configManager = SpinConfigManager.Instance;

            _listener = new HttpListener();
            _listener.Prefixes.Add("http://*:" + Port.ToString() + "/");
            _listener.Start();
            Receive();
        }

        private void Receive()
        {
            _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);
        }

        private void ListenerCallback(IAsyncResult result)
        {
            if (_listener.IsListening)
            {
                httpListenerContexts.Enqueue(_listener.EndGetContext(result));
                Receive();
            }
        }

        private void FixedUpdate()
        {
            HttpListenerContext context;
            if (httpListenerContexts.TryDequeue(out context))
            {
                HttpListenerRequest request = context.Request;

                if (request.Url.Segments.Length >= 2 && request.Url.Segments[1].Trim('/') == "settings")
                    ExecuteAPI(request.Url.AbsolutePath, context);
                else
                    SendFile(request.Url.AbsolutePath, context);
            }
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
                        configManager.OSCTrackersConfig = newConfig;
                        configManager.SaveSpinConfig();
                    }
                }
            }

            if (response.StatusCode == 200)
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(configManager.OSCTrackersConfig));
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

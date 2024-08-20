using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Brussels.Crew.Spin
{
    public class HTTPInterface : MonoBehaviour
    {
        public int Port = 8080;

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

        private void Update()
        {
            HttpListenerContext context;
            if (httpListenerContexts.TryDequeue(out context))
            {
                HttpListenerRequest request = context.Request;

                if (request.Url.Segments.Length == 3 && request.Url.Segments[1].Trim('/') == "api")
                    ExecuteAPI(request.Url.Segments[2], context);
                else
                    SendFile(request.Url.AbsolutePath, context);
            }
        }

        private void ExecuteAPI(string method, HttpListenerContext context)
        {
            HttpListenerResponse response = context.Response;

            if (method == "read")
            {
                response.StatusCode = 200;
                response.ContentType = "application/json";

                byte[] data = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(configManager.OSCTrackersConfig));

                response.OutputStream.Write(data, 0, data.Length);
                response.OutputStream.Close();
            }
            else if (method == "write")
            {

            }
            else if (method == "clear")
            {
                configManager.ClearSpinConfig();
                response.StatusCode = 200;
                response.OutputStream.Close();
            }
            else
            {
                response.StatusCode = 404;
                response.OutputStream.Close();
            }
        }
        private void SendFile(string path, HttpListenerContext context)
        {
            Debug.Log("httpInterface file " + path);
        }
    }
}

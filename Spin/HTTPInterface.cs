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
    /// <summary>
    /// Represents an HTTP file that can be served by the HTTP server.
    /// </summary>
    [Serializable]
    public class HTTPFile
    {
        /// <summary>
        /// Represents the file path associated with an HTTP request or response.
        /// </summary>
        public string path;

        /// <summary>
        /// Represents the MIME type of the content associated with an HTTP file.
        /// </summary>
        /// <remarks>
        /// The contentType variable is used to specify the media type of the
        /// file being sent over HTTP, which informs the client about how to
        /// process the received file. Examples include "text/plain", "image/png",
        /// "application/json", etc.
        /// </remarks>
        public string contentType;

        /// <summary>
        /// Represents a file used within the HTTP interface, containing information
        /// about the file path, content type, and actual file data.
        /// </summary>
        /// <remarks>
        /// This file is utilized for sending HTTP responses with the correct
        /// file data when the path matches a request made to the HTTP interface.
        /// </remarks>
        public TextAsset file;
    }

    /// <summary>
    /// Represents an HTTP interface for managing server-related functionalities within the Unity environment.
    /// </summary>
    public class HTTPInterface : MonoBehaviour
    {
        /// <summary>
        /// Specifies the port number on which the HTTP server listens for incoming requests.
        /// </summary>
        [FormerlySerializedAs("Port")] public int port = 8080;

        /// <summary>
        /// A list of HTTPFile objects representing files that can be served
        /// over an HTTP interface. Each HTTPFile contains information about
        /// the file path, content type, and the file itself.
        /// </summary>
        public List<HTTPFile> files;

        /// <summary>
        /// A reference to a TextMeshPro text component used to display the IP address and port number.
        /// </summary>
        public TMP_Text iptext;

        /// <summary>
        /// Represents the HTTP listener that handles incoming HTTP requests
        /// within the HTTPInterface class. It is responsible for initializing
        /// the HTTP listener, adding prefixes for listening to specific
        /// network interfaces, and starting the listener to receive
        /// incoming web requests.
        /// </summary>
        private HttpListener _listener;

        /// <summary>
        /// Represents the configuration manager used for managing and accessing configuration settings
        /// specific to the Spin application, such as tracker configurations, default server details,
        /// and device information. It facilitates saving and clearing configuration settings
        /// and is intended to be instantiated as a singleton via <see cref="SpinConfigManager.Instance"/>.
        /// </summary>
        private SpinConfigManager _configManager;

        /// <summary>
        /// A queue for storing and managing <see cref="HttpListenerContext"/> instances.
        /// This queue holds the incoming HTTP request contexts that are processed
        /// asynchronously by the HTTP server. It allows for sequential handling
        /// of HTTP requests received by the <see cref="HttpListener"/>.
        /// </summary>
        private Queue<HttpListenerContext> _httpListenerContexts = new Queue<HttpListenerContext>();

        /// <summary>
        /// Represents the index of the current IP address being displayed from a list of local IP addresses.
        /// Used to cycle through available IP addresses and update the displayed IP address accordingly.
        /// </summary>
        private int _ipIndex = 0;

        /// <summary>
        /// Represents the cumulative time in seconds that has elapsed since the last IP text update.
        /// This value is used to determine when the IP address should be refreshed
        /// and displayed again, occurring every 5 seconds.
        /// </summary>
        private float _timeElapsed = 0;

        /// <summary>
        /// Initializes the HTTP listener on the specified port and begins listening for incoming HTTP requests.
        /// The method also sets up the SpinConfigManager instance used for configuration management.
        /// </summary>
        void Start()
        {
            _configManager = SpinConfigManager.Instance;

            _listener = new HttpListener();
            _listener.Prefixes.Add("http://*:" + port.ToString() + "/");
            _listener.Start();

            Receive();
        }

        /// <summary>
        /// Updates the text displayed on the IP address text field with the
        /// current local IP address and port number. If all local IP addresses
        /// have been displayed, it resets and starts over from the first address.
        /// Additionally, if no local IP addresses are present, it displays the
        /// application version.
        /// </summary>
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

        /// <summary>
        /// Retrieves a list of local IP addresses associated with the host machine
        /// that are using the InterNetwork address family (IPv4).
        /// </summary>
        /// <returns>
        /// A list of strings, where each string is a local IPv4 address of the host.
        /// </returns>
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

        /// <summary>
        /// Initiates asynchronous listening for incoming HTTP requests.
        /// </summary>
        /// <remarks>
        /// This method begins the asynchronous operation to obtain an incoming
        /// request context from the HTTP listener. It uses a callback method
        /// to handle the completion of the operation. When a request is received,
        /// the callback method adds the request to the internal queue for processing.
        /// </remarks>
        private void Receive()
        {
            _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);
        }

        /// <summary>
        /// Callback method for handling HTTP listener context asynchronously.
        /// </summary>
        /// <param name="result">An IAsyncResult that represents the asynchronous operation begun by calling BeginGetContext.</param>
        private void ListenerCallback(IAsyncResult result)
        {
            if (_listener.IsListening)
            {
                _httpListenerContexts.Enqueue(_listener.EndGetContext(result));
                Receive();
            }
        }

        /// Updates the HTTP interface state by processing HTTP requests from the queue
        /// and updating the IP text display at regular intervals.
        /// This method is responsible for dequeuing HTTP requests and handling them
        /// according to their URL segments. It executes different processing methods
        /// (ExecuteAPI, ClearConf, SendFile) depending on the request path.
        /// It also manages the regular updating of IP text, cycling through available
        /// IP addresses and displaying them every five seconds.
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
        /// <summary>
        /// Clears the current spin configuration settings and sends a successful HTTP response.
        /// </summary>
        /// <param name="urlAbsolutePath">The absolute path of the request URL.</param>
        /// <param name="context">The HttpListenerContext object that provides access to the request and response.</param>
        private void ClearConf(string urlAbsolutePath, HttpListenerContext context)
        {
            _configManager.ClearSpinConfig();
            context.Response.StatusCode = 200;
            context.Response.OutputStream.Close();
        }

        /// <summary>
        /// Executes the API call by handling HTTP requests directed to the specified method path, processes the
        /// associated data if present, and updates the configuration accordingly.
        /// </summary>
        /// <param name="method">The HTTP request method path specifying the target API endpoint.</param>
        /// <param name="context">The HttpListenerContext object representing the current HTTP request and response.</param>
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

        /// Sends a file to the specified HTTP listener context if the file path is matched in the available list of files.
        /// <param name="path">The requested file path from the HTTP request.</param>
        /// <param name="context">The HTTP listener context that provides the response object for sending the file.</param>
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

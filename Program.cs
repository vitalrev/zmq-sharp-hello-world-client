using System;
using System.Reflection;
using System.Runtime.InteropServices;
using NativeLibraryManager;

namespace client
{

    class Program
    {
        private const int ZMQ_SOCKS_PROXY = 68;
        private const int ZMQ_SOCKS_USERNAME = 99;
        private const int ZMQ_SOCKS_PASSWORD = 100;

        [DllImport("libzmq")]
        public static extern IntPtr zmq_ctx_new();
        [DllImport("libzmq")]
        public static extern IntPtr zmq_socket(IntPtr context, int type);
        [DllImport("libzmq")]
        public static extern void zmq_connect(IntPtr socket, string url);
        [DllImport("libzmq")]
        public static extern void zmq_send(IntPtr requester, string message, int len, int flags);
        [DllImport("libzmq")]
        public static extern void zmq_recv(IntPtr requester, byte[] response, int len, int flags);
        [DllImport("libzmq")]
        public static extern void zmq_close(IntPtr requester);
        [DllImport("libzmq")]
        public static extern void zmq_ctx_destroy(IntPtr context);
        [DllImport("libzmq")]
        public static extern void zmq_setsockopt(IntPtr socket, int option_name, string option_value, int len);

        [Obsolete]
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World Client starting...");

            CheckEnvVar("ZMQ_SERVER_HOST", "lissi-cloud-dev.westeurope.cloudapp.azure.com");

            var accessor = new ResourceAccessor(Assembly.GetExecutingAssembly());

            LibraryManager libManager;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                libManager = new LibraryManager(
                    Assembly.GetExecutingAssembly(),
                    new LibraryItem(Platform.Linux, Bitness.x64,
                        new LibraryFile("libzmq.so", accessor.Binary("libzmq.so")))
                );
                Console.WriteLine("Running on Linux");
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                libManager = new LibraryManager(
                    Assembly.GetExecutingAssembly(),
                    new LibraryItem(Platform.MacOs, Bitness.x64,
                        new LibraryFile("libzmq.dylib", accessor.Binary("libzmq.dylib")))
                );
                Console.WriteLine("Running on MacOS");
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                Console.WriteLine("Windows is not supported");
                return;
            } else {
                Console.WriteLine("Unsupported platform");
                return;
            }

            libManager.LoadNativeLibrary();

            var context = zmq_ctx_new();
            var socket = zmq_socket(context, 3); // 3 = ZMQ_REQ

            var server_host = Environment.GetEnvironmentVariable("ZMQ_SERVER_HOST");
            Console.WriteLine("ZMQ_SERVER_HOST: {0}", server_host);

            var socks_proxy = Environment.GetEnvironmentVariable("ZMQ_SOCKS_PROXY");
            Console.WriteLine("ZMQ_SOCKS_PROXY: {0}", socks_proxy);
            if (socks_proxy != null)
            {
                zmq_setsockopt(socket, ZMQ_SOCKS_PROXY, socks_proxy, socks_proxy.Length);
            }

            var proxy_user = Environment.GetEnvironmentVariable("ZMQ_PROXY_USER");
            Console.WriteLine("ZMQ_PROXY_USER: {0}", proxy_user);
            if (proxy_user != null)
            {
                zmq_setsockopt(socket, ZMQ_SOCKS_USERNAME, proxy_user, proxy_user.Length);
            }

            var proxy_password = Environment.GetEnvironmentVariable("ZMQ_PROXY_PASSWORD");
            Console.WriteLine("ZMQ_PROXY_PASSWORD: {0}", proxy_password);
            if (proxy_password != null)
            {
                zmq_setsockopt(socket, ZMQ_SOCKS_PASSWORD, proxy_password, proxy_password.Length);
            }

            zmq_connect(socket, String.Format("tcp://{0}:9702", server_host));

            int request_nbr;
            for (request_nbr = 0; request_nbr < 10; request_nbr++)
            {
                byte[] response = new byte[32];
                String message = String.Format("Hello {0}", request_nbr);
                Console.WriteLine("Sending {0}", message);
                zmq_send(socket, message, 7, 0);
                zmq_recv(socket, response, 30, 0);  // Hello, world!
                string charsStr = System.Text.Encoding.Default.GetString(response);
                Console.WriteLine("Received: {0}", charsStr);
            }
            zmq_close(socket);
            zmq_ctx_destroy(context);
        }

        private static void CheckEnvVar(string envVar, string defaultValue)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(envVar)))
            {
                Environment.SetEnvironmentVariable(envVar, defaultValue);
            }
        }

    }
}

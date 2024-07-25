using System;
using System.Reflection;
using System.Security.Cryptography;
using Rcl;
using Rcl.Logging;
using Rcl.Parameters;
using Rosidl.Messages.Geometry;
using Rosidl.Runtime;

namespace AllianceDM.IO
{
    internal sealed class IOManager : Component
    {
        public IOManager()
        {
            InitComponents(0, [], []);
        }
        struct MessageContainer(IMessage msg, string name)
        {
            internal IMessage Message = msg;
            internal string Name = name;
        }

        public delegate void MessageHandler<T>(T msg) where T : IMessage;
        public delegate void BufferHandler(RosMessageBuffer msg);
        public override void Start()
        {

        }
        /// <summary>
        /// Recive Msg
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="handler"></param>
        public static void RegistrySubscription<T>(string name, MessageHandler<T> handler) where T : IMessage
        {
            Task.Run(() => CommonRecieveTask(name, handler));
        }
        public static void RegistrySubscription<T>(string name, BufferHandler handler) where T : IMessage
        {
            Task.Run(() => NativeRecieveTask<T>(name, handler));
        }

        public static async void CommonRecieveTask<T>(string name, MessageHandler<T> handler) where T : IMessage
        {
            using var subscription = Ros2Def.node.CreateSubscription<T>(name);
            bool l = false;
            await foreach (var i in subscription.ReadAllAsync())
            {
                if (l)
                    continue;
                l = true;
                handler(i);
                l = false;
            }
            Console.WriteLine(name + ":Registry");
        }
        public static async void NativeRecieveTask<T>(string name, BufferHandler handler) where T : IMessage
        {
            using var subscription = Ros2Def.node.CreateNativeSubscription<T>(name);
            bool l = false;
            await foreach (var i in subscription.ReadAllAsync())
            {
                if (l)
                    continue;
                l = true;
                _ = i.AsRef<Rosidl.Messages.Geometry.Pose2D.Priv>();
                handler(i);
                l = false;
                i.Dispose();
            }
            Console.WriteLine(name + ":Registry");
        }

        internal static void ArgsAppend(string[] args)
        {

        }

        public static void Input() => ROS2Msgs.TlarcMsgs.InputData();
        public override void Update()
        {
            // _update = true;
        }

    }
}
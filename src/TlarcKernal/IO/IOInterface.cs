using System;
using System.Reflection;
using System.Security.Cryptography;
using Rcl;
using Rcl.Logging;
using Rcl.Parameters;
using Rosidl.Messages.Geometry;
using Rosidl.Runtime;
using Tlarc.IO.TlarcMsgs;

namespace Tlarc.IO
{
    public sealed class IOManager : Component
    {
        public List<(object src, CopyWithExpressions)> CopyForOut = new();
        public List<(object dst, CopyWithExpressions)> CopyForIn = new();
        internal ROS2Msgs.TlarcRosMsgs TlarcRosMsgs = new();
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

        /// <summary>
        /// Receive Msg
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="handler"></param>
        public void RegistrySubscription<T>(string name, MessageHandler<T> handler) where T : IMessage
        {
            Task.Run(() => CommonReceiveTask(name, handler));
        }
        public void RegistrySubscription<T>(string name, BufferHandler handler) where T : IMessage
        {
            Task.Run(() => NativeReceiveTask<T>(name, handler));
        }

        public async void CommonReceiveTask<T>(string name, MessageHandler<T> handler) where T : IMessage
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
        public async void NativeReceiveTask<T>(string name, BufferHandler handler) where T : IMessage
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

        public void Input()
        {
            TlarcRosMsgs.InputData();
            foreach (var k in CopyForIn)
                k.Item2.CopyTo(k.dst);
        }
        public void Output()
        {
            foreach (var k in CopyForOut)
                k.Item2.CopyFrom(k.src);
        }
    }
}
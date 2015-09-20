﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Pipes.Models.Pipes;
using SharedResources.SharedResources;

namespace Pipes.Models.Lets
{
    public class Inlet<TMessage> : Let<TMessage>
    {
        internal Outlet<TMessage> ConnectedOutlet { get; set; }

        private readonly IList<WaitingSender<TMessage>> waitingSenders;

        internal Inlet(IPipe<TMessage> pipe, SharedResource resource) : base(pipe, resource)
        {
            waitingSenders = new List<WaitingSender<TMessage>>();
            ConnectedOutlet = null;
        }

        /// <summary>
        /// Send a message down the pipe. If the pipe system has insufficient capacity to accept the message, this will block until the message can be sent.
        /// </summary>
        public void Send(TMessage message)
        {
            Lock();
            if (ConnectedOutlet != null)
            {
                Unlock();
                throw new InvalidOperationException("You cannot send through a connected inlet.");
            }

            if (!HasWaitingSender())
            {
                var receiver = Pipe.FindReceiver();
                if (receiver != null)
                {
                    receiver(message);
                    Unlock();
                    return;
                }
            }
            var waitingSender = new WaitingSender<TMessage>(message);
            waitingSenders.Add(waitingSender);
            Unlock();
            WaitToSendMessage(waitingSender, s => s.WaitOne(), new ThreadInterruptedException("The message could not be sent as the thread was interrupted"));
        }

        private void WaitToSendMessage(WaitingSender<TMessage> waitingSender, Func<Semaphore, bool> waitFunction, Exception failureException)
        {
            try
            {
                var messageSent = waitFunction(waitingSender.WaitSemaphore);
                if (messageSent) return;
                Lock();
                if (!waitingSender.MessageSent)
                {
                    waitingSenders.Remove(waitingSender);
                    Unlock();
                    throw failureException;
                } // else the message has actually been sent (a moment later)
                Unlock();
            }
            catch
            {
                Lock();
                if (!waitingSender.MessageSent) waitingSenders.Remove(waitingSender);
                Unlock();
                throw;
            }
        }

        /// <summary>
        /// Connect this outlet to an inlet. This helps you to build up a pipe system!
        /// By default, the method will also check to see if you would create a cycle by doing this. If so, it will refuse to connect to the given inlet and throw
        /// an InvalidOperationException. This is quite an expensive check for large pipe systems however, so if you're confident you are not creating cycles, you
        /// can turn it off.
        /// </summary>
        public void ConnectTo(Outlet<TMessage> outlet, bool checkForCycles = true)
        {
            LockWith(outlet);
            Connect(this, outlet, checkForCycles);
            Unlock();
        }

        /// <summary>
        /// Disconnect this outlet from the given inlet. This breaks a pipe system apart.
        /// </summary>
        public void DisconnectFrom(Outlet<TMessage> outlet)
        {
            LockWith(outlet);
            Disconnect(this, outlet);
            Unlock();
        }

        protected override bool ReadyToConnect()
        {
            return !waitingSenders.Any() && ConnectedOutlet == null;
        }

        public Func<TMessage> FindSender()
        {
            if (ConnectedOutlet != null) return ConnectedOutlet.Pipe.FindSender();
            if (HasWaitingSender()) return UseWaitingSender;
            return null;
        }

        private bool HasWaitingSender()
        {
            return waitingSenders.Any();
        }

        private TMessage UseWaitingSender()
        {
            var waitingSender = waitingSenders.First();
            waitingSenders.Remove(waitingSender);
            waitingSender.RecordMessageSent();
            waitingSender.WaitSemaphore.Release();
            return waitingSender.Message;
        }
    }
}
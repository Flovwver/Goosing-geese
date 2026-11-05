using System;
using UnityEngine;

namespace GoosingGeese.Infrastructure
{
    public sealed class EventPublisher
    {
        public void Publish(Action handlers, UnityEngine.Object context)
        {
            if (handlers == null)
                return;

            foreach (Action handler in handlers.GetInvocationList())
            {
                try
                {
                    handler();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, context);
                }
            }
        }

        public void Publish<T>(Action<T> handlers, T value, UnityEngine.Object context)
        {
            if (handlers == null)
                return;

            foreach (Action<T> handler in handlers.GetInvocationList())
            {
                try
                {
                    handler(value);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, context);
                }
            }
        }

        public void Publish<TFirst, TSecond>(Action<TFirst, TSecond> handlers, TFirst first, TSecond second, UnityEngine.Object context)
        {
            if (handlers == null)
                return;

            foreach (Action<TFirst, TSecond> handler in handlers.GetInvocationList())
            {
                try
                {
                    handler(first, second);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, context);
                }
            }
        }
    }
}

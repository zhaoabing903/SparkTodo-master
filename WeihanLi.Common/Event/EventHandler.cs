﻿using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using WeihanLi.Extensions;

namespace WeihanLi.Common.Event
{
    public interface IEventHandler
    {
        Task Handle(object eventData);
    }

    public interface IEventHandler<in TEvent> : IEventHandler where TEvent : class, IEventBase
    {
        /// <summary>
        /// Handler event
        /// </summary>
        /// <param name="event">event</param>
        Task Handle(TEvent @event);
    }

    public abstract class EventHandlerBase<TEvent> : IEventHandler<TEvent> where TEvent : class, IEventBase
    {
        public abstract Task Handle(TEvent @event);

        public virtual Task Handle(object eventData)
        {
            if (null == eventData)
            {
                throw new ArgumentNullException(nameof(eventData));
            }

            if (eventData is TEvent data)
            {
                return Handle(data);
            }
            if (eventData is JObject jObject)
            {
                return Handle(jObject.ToObject<TEvent>());
            }
            if (eventData is string eventDataJson)
            {
                return Handle(eventDataJson.JsonToObject<TEvent>());
            }

            throw new ArgumentException($"unsupported eventDataType:{eventData.GetType()}", nameof(eventData));
        }
    }
}

using System;

namespace BuildMon
{
    public static class EventHelper
    {
        public static void Raise<T>(this EventHandler<T> eventHandler, object sender, T args)
        {
            if (eventHandler == null)
                return;
            eventHandler(sender, args);
        }
    }
}
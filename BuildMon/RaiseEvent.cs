﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

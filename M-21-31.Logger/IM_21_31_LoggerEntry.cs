﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace M_21_31.Logger
{
    public interface IM_21_31_LoggerEntry
    {
        Dictionary<string, object> CreateEntry(EventType eventType,
            EventStatus eventStatus,
            object? message,
            Exception? exception);
    }
}
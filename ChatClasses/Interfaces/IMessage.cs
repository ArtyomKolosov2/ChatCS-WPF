﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ChatClasses.Interfaces
{
    public interface IMessage
    {
        string Message { get; set; }
        string FormatedMessage { get; }
        string IP { get; }
        DateTime SendTime { get; set; }
    }
    
}

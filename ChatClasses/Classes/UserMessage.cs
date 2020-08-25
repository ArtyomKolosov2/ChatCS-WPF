using ChatClasses.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ChatClasses.Classes
{
    public class UserMessage : IMessage
    {
        
        public string Message { get; set; }
        public string IP { get; set; }


        
    }
}

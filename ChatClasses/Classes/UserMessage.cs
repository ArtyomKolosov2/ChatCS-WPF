using ChatClasses.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ChatClasses.Classes
{
    public class UserMessage : IMessage, IClientInfo
    {
        public string Message { get; set; }
        public string NickName { get; }
        public int Id { get; set; }
        public string FormatedMessage 
        {
            get
            {
                string result = $"[{SendTime.ToString("HH:mm:ss")}]: {Message}";
                return result;
            }
        }
        public string IP { get; }
        public DateTime SendTime { get; set; } = DateTime.Now;

        public override string ToString()
        {
            return FormatedMessage;
        }
    }
}

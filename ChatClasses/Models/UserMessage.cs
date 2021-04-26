using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Common.Models
{
    public class UserMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Message { get; set; }
        public string Nickname { get; set; }
        public string FormattedMessage => $"[{SendTime:HH:mm:ss}]:{Nickname ?? string.Empty} {Message}";
        public DateTime SendTime { get; set; } = DateTime.Now;
        public override string ToString() => FormattedMessage;
    }
}
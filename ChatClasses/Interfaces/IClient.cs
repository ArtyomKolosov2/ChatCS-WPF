using System;
using System.Collections.Generic;
using System.Text;

namespace ChatClasses.Interfaces
{
    interface IClient
    {
        string IP { get; set; }
        string NickName { get; set; }
    }
}

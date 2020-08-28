using System;
using System.Collections.Generic;
using System.Text;

namespace ChatClasses.Interfaces
{
    interface IClientInfo
    {
        string NickName { get; }
        int Id { get; set; }
    }
}

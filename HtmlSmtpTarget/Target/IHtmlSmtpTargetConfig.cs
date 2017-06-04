using NLog.Layouts;
using System;
using System.Net.Mail;

namespace NLog.HtmlSmtpTarget.Target
{
    public interface IHtmlSmtpTargetConfig
    {
       
        string Transport { get;  }

        Layout To { get;  }
        Layout From { get;  }
        Layout ReplyTo { get;  }
        Layout Subject { get; }
        MailPriority Priority { get;  }

 
        TimeSpan HolddownPeriod { get;  }

     
        int EventBacklog { get; }

       
        int MaximumEventsPerMessage { get;  }

   
        int PreTriggerMessages { get;  }

   
        string TriggerLevel { get;  }
    }
}
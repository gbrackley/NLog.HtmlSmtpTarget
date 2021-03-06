# HtmlSmtpAppender

A SMTP appender for NLog that will send a small history of messages when a threshold is reached.

The appender works by keeping a ring buffer of the last few messages. When a triggering log
message is received (e.g. warning or more severe), then the entire buffer including the triggering
message is emailed to the recipients. This means the previous messages with context information 
will be received.

So as to not flood recipients with emails, a hysteresis is introduced so that subsequent triggering
events are batched up into the next delivery window.  

# Configuration

Add an appender to you nlog configuration:

 ```
   <extensions>
     <add assembly="HtmlSmtpTarget"/>
   </extensions>

   <target
      name="HtmlSmtp"
      xsi:type="HtmlSmtp"
      to="user@example.com"
      from="smtp-appender@example.com"/>

  <rules>
    <logger name="*" minlevel="Trace" writeTo="HtmlSmtp" />
  </rules>
 ```
 
 The transport setting is a URI of the form `smtp://username:password@hostname:port`

 # Links


 https://github.com/nlog/nlog/wiki/How-to-write-a-custom-target
/*using System.Linq;
using System.Text;
using NLog.Common;
using NLog.Config;
using NLog.LayoutRenderers;

namespace NLog.HtmlSmtpTarget.Target
{
    [LayoutRenderer("HtmlSmtp")]
    public class HtmlSmtpLayoutRenderer : LayoutRenderer
    {
        private HtmlSmtpTarget _target;

        protected override void InitializeLayoutRenderer()
        {
            base.InitializeLayoutRenderer();
            if (!string.IsNullOrEmpty(Target))
            {
                _target = LoggingConfiguration.FindTargetByName<HtmlSmtpTarget>(Target);
                if (_target == null)
                {
                    InternalLogger.Error(
                        "Failed to initialise HTML SMTP layout renderer, as there is no Html SMTP target");
                }
            }
            else
            {
                _target = LoggingConfiguration.AllTargets.FirstOrDefault(t => t is HtmlSmtpTarget) as HtmlSmtpTarget;
                if (_target == null)
                {
                    InternalLogger.Error(
                        "Failed to initialise HTML SMTP layout renderer, as there is no Html SMTP target");
                }
            }
        }

        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            builder.Append("hello universe!");
        }

        /// <summary>
        ///     The name of the variable to render.
        /// </summary>
        [DefaultParameter]
        [RequiredParameter]
        public bool Name { get; set; }

        /// <summary>
        ///     The name of the target to use for rendering variable. By default
        ///     this can be left empty and the first <see cref="HtmlSmtpTarget" /> will be used.
        /// </summary>
        public string Target { get; set; }
    }
}*/
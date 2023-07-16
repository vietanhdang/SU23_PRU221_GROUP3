using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.CustomException
{
    public class ExceptionHandling : ApplicationException
    {
        private string messageDetails = String.Empty;
        public DateTime ErrorTimeStamp { get; set; } = DateTime.Now;
        public string CauseOfError { get; set; }
        public string Line { get; set; }

        public ExceptionHandling()
        {
        }
        public ExceptionHandling(string message, string cause, DateTime time, string line)
        {
            messageDetails = message;
            CauseOfError = cause;
            ErrorTimeStamp = time;
            this.Line = line;
        }

        public ExceptionHandling(string message, DateTime now) : base(message)
        {
        }

        // Override the Exception.Message property.
        public override string Message => "Custom Error Message: " + messageDetails;

        public void Handle()
        {
            Debug.LogError(Message + " - " + ErrorTimeStamp.ToString("dd/MM/yyy HH:MM:ss") + " - " + (!string.IsNullOrEmpty(Line) ? ("At line :" + Line) : "Unknown")
                + "\n" + (!string.IsNullOrEmpty(CauseOfError) ? CauseOfError : ""));
        }
    }



}

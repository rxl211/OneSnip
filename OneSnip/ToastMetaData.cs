using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OneSnip
{
    class ToastMetadata
    {
        private string title;
        private string text;

        public ToastMetadata()
        {
            title = "";
            text = "";
        }

        public void setupToast(string _title, string _text)
        {
            title = _title;
            text = _text;
        }

        public void ShowToast(NotifyIcon notifyIcon)
        {
            notifyIcon.BalloonTipTitle = title;
            notifyIcon.BalloonTipText = text;
            notifyIcon.Icon = OneSnip.Properties.Resources.onesnip64x64;
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(1);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OneSnip
{
    public class Snip
    {
        public event EventHandler<SnipEventArgs> SnipCreated;
        private Bitmap image;
        private Screen screenUsed;

        protected virtual void OnSnipCreated()
        {
            if ((this.SnipCreated != null))
            {
                SnipEventArgs e = new SnipEventArgs(image, screenUsed);
                this.SnipCreated(this, e);
            }
        }

        public void setImage(Bitmap _image, Screen _screenUsed)
        {
            image = _image;
            screenUsed = _screenUsed;
            this.OnSnipCreated();
        }
    }

    public class SnipEventArgs : EventArgs
    {
        public Bitmap image;
        public Screen screenUsed;

        public SnipEventArgs(Bitmap _image, Screen _screenUsed)
        {
            image = _image;
            screenUsed = _screenUsed;
        }
    }


}

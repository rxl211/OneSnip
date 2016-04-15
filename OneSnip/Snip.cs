using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneSnip
{
    public class Snip
    {
        public event EventHandler<SnipEventArgs> SnipCreated;
        private Bitmap image;
        protected virtual void OnSnipCreated()
        {
            if ((this.SnipCreated != null))
            {
                SnipEventArgs e = new SnipEventArgs(image);
                this.SnipCreated(this, e);
            }
        }

        public void setImage(Bitmap _image)
        {
            image = _image;
            this.OnSnipCreated();
        }
    }

    public class SnipEventArgs : EventArgs
    {
        public Bitmap image;

        public SnipEventArgs(Bitmap _image)
        {
            image = _image;
        }
    }


}

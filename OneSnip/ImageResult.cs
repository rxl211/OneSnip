using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneSnip
{
    class ImageResult
    {
        public string link;
        public Image image;
        public string cloudName;

        public ImageResult(string _cloudName, Image _image, string _link = null)
        {
            link = _link;
            image = _image;
            cloudName = _cloudName;
        }
    }
}

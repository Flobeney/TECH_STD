using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ex_01_format_bmp {
    class Program {
        static void Main(string[] args) {
            //Variables
            Bitmap imgBase = new Bitmap("../../../logo.bmp");
            Bitmap imgRes = new Bitmap(imgBase.Width, imgBase.Height);

            //Parcourir l'image
            for (int i = 0; i < imgBase.Width; i++) {
                for (int j = 0; j < imgBase.Height; j++) {
                    //Copier le pixel inversé à la même place
                    imgRes.SetPixel(i, j, Color.FromArgb(255 - imgBase.GetPixel(i, j).ToArgb()));
                }
            }

            //Sauver l'image
            imgRes.Save("res.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

            Console.WriteLine("Traitement terminé");
        }
    }
}

using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ex_01_format_bmp {
    class Program {
        static void Main(string[] args) {
            //Constantes 
            const string PATH_IMG_BASE = "../../../logo.bmp";
            const string IMG_RES = "res.bmp";

            //Variables
            BinaryReader reader;
            Bitmap imgBase = new Bitmap(PATH_IMG_BASE);
            Bitmap imgRes = new Bitmap(imgBase.Width, imgBase.Height);

            //Parcourir l'image
            for (int i = 0; i < imgBase.Width; i++) {
                for (int j = 0; j < imgBase.Height; j++) {
                    //Copier le pixel inversé à la même place
                    imgRes.SetPixel(i, j, Color.FromArgb(255 - imgBase.GetPixel(i, j).ToArgb()));
                }
            }

            //Sauver l'image
            imgRes.Save(IMG_RES, System.Drawing.Imaging.ImageFormat.Bmp);

            //Fermer les images
            imgBase.Dispose();
            imgRes.Dispose();

            Console.WriteLine("Traitement de l'image terminé");

            //Lire les infos du header
            reader = new BinaryReader(File.Open(PATH_IMG_BASE, FileMode.Open));

            string bfType;
            int bfSize;
            Byte[] currentBytes;

            currentBytes = reader.ReadBytes(2);
            bfType = Encoding.ASCII.GetString(currentBytes);

            currentBytes = reader.ReadBytes(4);
            bfSize = BitConverter.ToInt32(currentBytes, 0);

            Console.WriteLine(bfType);
            Console.WriteLine(bfSize);

            //Fermer le reader
            reader.Close();
        }
    }
}

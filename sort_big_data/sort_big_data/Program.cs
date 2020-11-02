using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace sort_big_data {
    class Program {
        //Constantes
        const string FILENAME = "file.txt";

        //Champs
        static SortBigData sort;
        static StreamReader file;
        static string currentLine;

        static void Main(string[] args) {
            //Vérification de l'existence du fichier
            if (!File.Exists(FILENAME)) {
                throw new FileNotFoundException();
            }

            //Classe pour trier le fichier
            sort = new SortBigData();

            //Lecture du fichier
            using(file = new StreamReader(FILENAME)) {
                //Lecture d'une ligne
                while ((currentLine = file.ReadLine()) != null) {
                    sort.AddLine(currentLine);
                }
            }

            sort.SortAllFiles();
        }
    }
}

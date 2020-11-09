using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace sort_big_data {
    class Program {
        //Constantes
        const string FILENAME = "file200mb.txt";
        //const string FILENAME = "lorem5.txt";
        //const int NB_LINES_TO_READ = 100_000;
        const int NB_LINES_TO_READ = 10_000;

        //Champs
        static SortBigData sort;
        static StreamReader file;
        static string currentLine;
        static string[] linesRead;
        static int lines;
        static List<Task> tasks;
        static Stopwatch sw;

        static void Main(string[] args) {
            sw = new Stopwatch();
            sw.Start();
            Console.WriteLine(DateTime.Now);
            //Vérification de l'existence du fichier
            if (!File.Exists(FILENAME)) {
                throw new FileNotFoundException();
            }

            //Classe pour trier le fichier
            sort = new SortBigData();
            //Lines lues
            linesRead = new string[NB_LINES_TO_READ];
            currentLine = "";
            //Tâches pour l'asynchrone
            tasks = new List<Task>();

            //Lecture du fichier
            using (file = new StreamReader(FILENAME)) {
                while (currentLine != null) {
                    //Lire NB_LINES_TO_READ
                    int i;
                    for (i = 0; i < NB_LINES_TO_READ && (currentLine = file.ReadLine()) != null; i++) {
                        linesRead[i] = currentLine;
                        lines++;
                    }
                    //S'il n'y avait plus de lignes à lire, couper le tableau où ça c'est arrêté
                    if(i < NB_LINES_TO_READ) {
                        linesRead = linesRead.Take(i).ToArray();
                    }
                    tasks.Add(sort.AddLines(linesRead));
                }
            }

            //Attendre que toutes les lignes soient ajoutées dans leur fichier respectif
            Task.WaitAll(tasks.ToArray());

            Console.WriteLine($"File readed : {lines} lignes");
            Console.WriteLine($"Start merging files at {DateTime.Now}");

            //Fusionner les fichiers
            sort.MergeFiles();

            Console.WriteLine("End merging files");
            sw.Start();
            Console.WriteLine(DateTime.Now);
            Console.WriteLine($"Traitement effectué en {sw.ElapsedMilliseconds}ms");
            Console.ReadKey();
        }

    }
}

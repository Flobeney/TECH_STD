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
        //const string FILENAME = "lorem5.txt";
        const int NB_LINES_TO_READ = 100000;

        //Champs
        static SortBigData sort;
        static StreamReader file;
        static string currentLine;
        static string[] linesRead;
        static int lines;
        static List<Task> tasks;

        static void Main(string[] args) {
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
                    Console.WriteLine($"Begin read {NB_LINES_TO_READ} lines");
                    //Lire NB_LINES_TO_READ
                    for (int i = 0; i < NB_LINES_TO_READ && (currentLine = file.ReadLine()) != null; i++) {
                        linesRead[i] = currentLine;
                        lines++;
                    }
                    tasks.Add(sort.AddLines(linesRead));
                }
                 
                //Lecture d'une ligne
                //while ((currentLine = file.ReadLine()) != null) {
                //    sort.addline(currentline);
                //}
            }

            //Attendre que toutes les lignes soient ajoutées dans leur fichier respectif
            Task.WaitAll(tasks.ToArray());
            //while (tasks.Count > 0) {
            //    Task finished = await Task.WhenAny(tasks);
            //    Console.WriteLine("finished");
            //    tasks.Remove(finished);

            //}

            Console.WriteLine($"File readed : {lines} lignes");

            //Trier les mots dans les fichiers
            sort.SortAllFiles();

            //Écrire dans le fichier de résultat
            sort.WriteResFile();
            Console.WriteLine(DateTime.Now);
        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sort_big_data {
    class SortBigData {
        //Constantes
        private const string FILE_RES = "res.txt";
        private readonly string FOLDER_DATA = "data_" + DateTime.Now.Ticks.ToString() + "/";
        private const char WORD_SEPARATOR = ' ';

        //Champs
        private char[] alphabet;
        private List<FileStream> files;
        private List<StreamReader> readers;
        private FileStream fileRes;
        private UTF8Encoding encoding;
        private object objLock;

        /// <summary>
        /// Constructeur
        /// </summary>
        public SortBigData() {
            //Supprimer le fichier de résultat s'il existe déjà
            if (File.Exists(FILE_RES)) {
                File.Delete(FILE_RES);
            }
            //Dossier des fichiers de données
            if (Directory.Exists(FOLDER_DATA)) {
                throw new Exception($"Le dossier {FOLDER_DATA} existe déjà");
            }
            Directory.CreateDirectory(FOLDER_DATA);
            //Initialisation
            alphabet = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
            files = new List<FileStream>();
            readers = new List<StreamReader>();
            fileRes = File.Create(FILE_RES);
            encoding = new UTF8Encoding();
            objLock = new object();

            Console.WriteLine("Create");
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        //~SortBigData() {
        //    Console.WriteLine("Delete");
        //    //Pour chaque lettres
        //    for (int nb = 0; nb < files.Count; nb++) {
        //        Console.WriteLine(nb);
        //        //Fermer les Streams
        //        files[nb].Close();
        //        //Supprimer le fichier
        //        File.Delete(GetFilename(nb));
        //    }
        //    //Dossier des fichiers de données
        //    Directory.Delete(FOLDER_DATA);
        //}

        /// <summary>
        /// Ajouter plusieurs lignes dans un fichier
        /// </summary>
        /// <param name="lines"></param>
        public async Task AddLines(string[] lines) {
            Console.WriteLine($"Begin add lines to files {files.Count}.txt");
            List<string>[] words = new List<string>[lines.Length];
            string[] res = new string[lines.Length];
            //Créer le fichier
            files.Add(
                File.Create(
                    GetFilename(files.Count)
                )
            );
            int cpt = files.Count - 1;
            readers.Add(new StreamReader(files[cpt]));
            //Lancer l'exécution de la lecture du mot
            await Task.Run(() => {
                Console.WriteLine($"start task on {cpt}.txt");
                //Parcourir les lignes
                Parallel.For(0, lines.Length, i => {
                    words[i] = new List<string>();
                    res[i] = string.Empty;
                    //Séparer la ligne en mots
                    words[i] = lines[i].Split(WORD_SEPARATOR).ToList();

                    //Trier les mots
                    words[i].Sort();

                    //Ajouter les mots dans le résultat
                    foreach (string word in words[i]) {
                        res[i] += word + Environment.NewLine;
                    }

                    lock (objLock) {
                        //Ajouter les mots au fichier correspondant
                        Write(files[cpt], res[i]);
                    }
                });
                Console.WriteLine($"end task on {cpt}.txt");
            });
            Console.WriteLine($"Finish adding lines on {cpt}.txt");
        }

        /// <summary>
        /// Ajout d'une ligne dans le(s) fichier(s) correspondant(s)
        /// </summary>
        /// <param name="line">La ligne à ajouter</param>
        public void AddLine(string line) {
            //Séparer la ligne en mots
            string[] words = line.Split(WORD_SEPARATOR);

            //Parcourir les mots de la ligne
            foreach (string word in words) {
                //Ajouter le mot au fichier correspondant
                Write(files[word.ToLower()[0]], word + Environment.NewLine);
            }
        }

        /// <summary>
        /// Trier tous les fichiers
        /// </summary>
        public void SortAllFiles() {
            Console.WriteLine("start sort");
            Parallel.For(0, files.Count, i => {
                SortFile(i);
            });
            Console.WriteLine("end sort");
        }

        /// <summary>
        /// Écrire le contenu des fichiers de données dans le fichier de résultat
        /// </summary>
        public void WriteResFile() {
            for (int i = 0; i < readers.Count; i++) {
                WriteFile(i);
            }
        }

        /// <summary>
        /// Écrire le contenu du fichier donné dans le fichier de résultat
        /// </summary>
        /// <param name="nb">Le chiffre du fichier de donnée</param>
        private void WriteFile(int nb) {
            string currentLine;

            //Lire à partir du début
            readers[nb].BaseStream.Position = 0;
            readers[nb].DiscardBufferedData();

            //Lire ligne par ligne
            while ((currentLine = readers[nb].ReadLine()) != null) {
                //Écrire dans le fichier de résultat
                Write(fileRes, currentLine + Environment.NewLine);
            }
        }

        /// <summary>
        /// Trier le fichier donné
        /// </summary>
        /// <param name="nb">Le chiffre du fichier de donnée</param>
        private void SortFile(int nb) {
            Console.WriteLine($"start sort on {nb}.txt");
            //Lecture des lignes
            List<string> lines = ReadAllLines(nb);

            //Tri des lignes
            lines.Sort();

            //Écrire par dessus le fichier
            files[nb].Position = 0;
            Write(files[nb], string.Join(Environment.NewLine, lines));

            //Écrire dans le fichier de résultat
            //Write(fileRes, string.Join(Environment.NewLine, lines));
            Console.WriteLine($"end sort on {nb}.txt");
        }

        /// <summary>
        /// Méthode d'écriture d'un FileStream
        /// </summary>
        /// <param name="fs">FileStream du fichier</param>
        /// <param name="text">Texte à écrire</param>
        private void Write(FileStream fs, string text) {
            fs.Write(
                encoding.GetBytes(text),
                0,
                encoding.GetByteCount(text)
            );
        }

        /// <summary>
        /// Lecture de toutes les lignes d'un fichier
        /// </summary>
        /// <param name="nb">Le chiffre du fichier de donnée</param>
        /// <returns>Le contenu d'un fichier</returns>
        private List<string> ReadAllLines(int nb) {
            List<string> res = new List<string>();
            string currentLine;

            //Lire à partir du début
            readers[nb].BaseStream.Position = 0;
            readers[nb].DiscardBufferedData();

            //Lire ligne par ligne
            while ((currentLine = readers[nb].ReadLine()) != null) {
                res.Add(currentLine);
            }

            return res;
        }

        /// <summary>
        /// Récupérer le nom complet d'un fichier de données
        /// </summary>
        /// <param name="c">La lettre du fichier</param>
        /// <returns>Le nom complet du fichier de données</returns>
        private string GetFilename(char c) {
            return $"{FOLDER_DATA}{c}.txt";
        }

        /// <summary>
        /// Récupérer le nom complet d'un fichier de données
        /// </summary>
        /// <param name="nb">Le numéro du fichier</param>
        /// <returns>Le nom complet du fichier de données</returns>
        private string GetFilename(int nb) {
            return $"{FOLDER_DATA}{nb}.txt";
        }
    }
}

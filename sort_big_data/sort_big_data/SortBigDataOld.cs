using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sort_big_data {
    class SortBigDataOld {
        //Constantes
        private const string FILE_RES = "res.txt";
        private const string FOLDER_DATA = "data/";
        private const char WORD_SEPARATOR = ' ';

        //Champs
        char[] alphabet;
        Dictionary<char, FileStream> files;
        Dictionary<char, StreamReader> readers;
        FileStream fileRes;
        UTF8Encoding encoding;

        /// <summary>
        /// Constructeur
        /// </summary>
        public SortBigDataOld() {
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
            files = new Dictionary<char, FileStream>();
            readers = new Dictionary<char, StreamReader>();
            fileRes = File.Create(FILE_RES);
            encoding = new UTF8Encoding();

            Console.WriteLine("Create");
            foreach (char c in alphabet) {
                Console.WriteLine(c);
                //Créer le fichier pour la lettre correspondante
                files.Add(
                    c,
                    File.Create(GetFilename(c))
                );
                //Créer le Reader pour ce fichier
                readers.Add(
                    c,
                    new StreamReader(files[c])
                );
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~SortBigDataOld() {
            Console.WriteLine("Delete");
            //Pour chaque lettres
            foreach (char c in alphabet) {
                Console.WriteLine(c);
                //Fermer les Streams
                files[c].Close();
                readers[c].Close();
                //Supprimer le fichier
                File.Delete(GetFilename(c));
            }
            //Dossier des fichiers de données
            Directory.Delete(FOLDER_DATA);
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
            Parallel.ForEach(alphabet, c => {
                Console.WriteLine(c);
                SortFile(c);
            });
            Console.WriteLine("end sort");
        }

        /// <summary>
        /// Écrire le contenu des fichiers de données dans le fichier de résultat
        /// </summary>
        public void WriteResFile() {
            foreach (char c in alphabet) {
                WriteFile(c);
            }
        }

        /// <summary>
        /// Écrire le contenu du fichier donné dans le fichier de résultat
        /// </summary>
        /// <param name="c">La lettre du fichier de donnée</param>
        private void WriteFile(char c) {
            //Lire à partir du début
            readers[c].BaseStream.Position = 0;
            readers[c].DiscardBufferedData();

            string currentLine;
            //Lire ligne par ligne
            while ((currentLine = readers[c].ReadLine()) != null) {
                //Écrire dans le fichier de résultat
                Write(fileRes, currentLine + Environment.NewLine);
            }
        }

        /// <summary>
        /// Trier le fichier donné
        /// </summary>
        /// <param name="c">La lettre du fichier de donnée</param>
        private void SortFile(char c) {
            //Lecture des lignes
            List<string> lines = ReadAllLines(c);

            //Tri des lignes
            lines.Sort();

            //Écrire par dessus le fichier
            files[c].Position = 0;
            Write(files[c], string.Join(Environment.NewLine, lines));

            //Écrire dans le fichier de résultat
            //Write(fileRes, string.Join(Environment.NewLine, lines));
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
        /// <param name="c">La lettre du fichier de donnée</param>
        /// <returns>Le contenu d'un fichier</returns>
        private List<string> ReadAllLines(char c) {
            List<string> res = new List<string>();
            string currentLine;

            //Lire à partir du début
            readers[c].BaseStream.Position = 0;
            readers[c].DiscardBufferedData();

            //Lire ligne par ligne
            while ((currentLine = readers[c].ReadLine()) != null) {
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
    }
}

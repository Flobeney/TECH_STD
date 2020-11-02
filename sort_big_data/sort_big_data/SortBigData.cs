using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sort_big_data {
    class SortBigData {
        //Champs
        char[] alphabet;
        Dictionary<char, FileStream> files;
        UTF8Encoding encoding;

        /// <summary>
        /// Constructeur
        /// </summary>
        public SortBigData() {
            alphabet = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
            files = new Dictionary<char, FileStream>();
            encoding = new UTF8Encoding();

            Console.WriteLine("Create");
            foreach (char c in alphabet) {
                Console.WriteLine(c);
                //Créer le fichier pour la lettre correspondante
                files.Add(
                    c,
                    File.Create($"{c}.txt")
                );
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~SortBigData() {
            Console.WriteLine("Delete");
            //Pour chaque lettres
            foreach (char c in alphabet) {
                Console.WriteLine(c);
                //Supprimer le fichier
                File.Delete($"{c}.txt");
            }
        }

        /// <summary>
        /// Ajout d'une ligne dans le(s) fichier(s) correspondant(s)
        /// </summary>
        /// <param name="line">La ligne à ajouter</param>
        public void AddLine(string line) {
            //Séparer la ligne en mots
            string[] words = line.Split(' ');

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
            foreach (char c in alphabet) {
                SortFile(files[c]);
            }
        }

        /// <summary>
        /// Trier le fichier donné
        /// </summary>
        /// <param name="fs">FileStream du fichier</param>
        private void SortFile(FileStream fs) {
            //Lecture des lignes
            List<string> lines = ReadAllLines(fs);

            //Tri des lignes
            lines.Sort();

            //Écrire par dessus le fichier
            fs.Position = 0;
            Write(fs, string.Join(Environment.NewLine, lines));
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
        /// <param name="fs">FileStream du fichier</param>
        /// <returns>Le contenu d'un fichier</returns>
        private List<string> ReadAllLines(FileStream fs) {
            //Lire à partir du début
            fs.Position = 0;
            StreamReader reader = new StreamReader(fs);
            List<string> res = new List<string>();

            string currentLine;
            //Lire ligne par ligne
            while ((currentLine = reader.ReadLine()) != null) {
                res.Add(currentLine);
            }

            return res;
        }
    }
}

using System;
using System.Collections.Concurrent;
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
        private ConcurrentDictionary<int, SortFile> sortedFiles;
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
            sortedFiles = new ConcurrentDictionary<int, SortFile>();
            encoding = new UTF8Encoding();
            objLock = new object();
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~SortBigData() {
            //Dossier des fichiers de données
            Directory.Delete(FOLDER_DATA);
        }

        /// <summary>
        /// Ajouter plusieurs lignes dans un fichier
        /// </summary>
        /// <param name="lines"></param>
        public async Task AddLines(string[] lines) {
            //Console.WriteLine($"Begin add lines to files {files.Count}.txt");
            string[][] words = new string[lines.Length][];
            string[] res = new string[lines.Length];
            int key = sortedFiles.Count;

            //Créer le fichier
            AddFile(key);

            //Lancer l'exécution de la lecture des lignes
            await Task.Run(() => {
                //Console.WriteLine($"start task on {key}.txt");
                //Parcourir les lignes
                Parallel.For(0, lines.Length, i => {
                    res[i] = string.Empty;
                    //Séparer la ligne en mots
                    words[i] = lines[i].Split(WORD_SEPARATOR);

                    //Ajouter les mots dans le résultat
                    foreach (string word in words[i]) {
                        res[i] += word + Environment.NewLine;
                    }

                    //Empêcher l'écriture simultanée
                    lock (objLock) {
                        //Ajouter les mots au fichier correspondant
                        Write(sortedFiles[key].fs, res[i]);
                    }
                });
                //Console.WriteLine($"Finish adding lines on {key}.txt");
                //Tri du fichier
                SortFile(key);
                //Indiquer que toutes les données ont été ajoutées au fichier
                sortedFiles[key].AllDataAdded = true;
                //Console.WriteLine($"end task on {key}.txt");
            });
        }

        /// <summary>
        /// Fusion des fichiers
        /// </summary>
        public void MergeFiles() {
            List<Task> merged = new List<Task>();
            //Récupérer les clés
            int[] keys = sortedFiles.Keys.ToArray();

            //Si un seul fichier, pas besoin de merge, terminé
            if (keys.Length != 1) {
                int i, j;
                for (i = 0, j = 0; i < keys.Length - 1; i += 2, j++) {
                    merged.Add(MergeSortAsync(keys[i], keys[i + 1], keys.Length + j));
                }
                //Tant qu'il y a des tâches en attente
                Task.WaitAll(merged.ToArray());
                //Console.WriteLine("Finished waiting merge");
                //Continuer à fusionner les fichiers
                MergeFiles();
            } else {
                //Fermer les streams du fichier
                sortedFiles[keys[0]].CloseFile();
                //Le déplacer comme fichier de résultat
                File.Move(GetFilename(keys[0]), FILE_RES);
            }
        }

        /// <summary>
        /// Tri fusion asynchrone de deux fichiers
        /// </summary>
        /// <param name="keyFirstFile">Clé du premier fichier</param>
        /// <param name="keySecondFile">Clé du second fichier</param>
        /// <param name="key">Clé du fichier résultant</param>
        /// <returns></returns>
        private async Task MergeSortAsync(int keyFirstFile, int keySecondFile, int key) {
            string[] currentLine = new string[2];

            //Créer un nouveau fichier temporaire
            AddFile(key);

            //Lire à partir du début
            sortedFiles[keyFirstFile].sr.BaseStream.Position = 0;
            sortedFiles[keySecondFile].sr.BaseStream.Position = 0;
            sortedFiles[keyFirstFile].sr.DiscardBufferedData();
            sortedFiles[keySecondFile].sr.DiscardBufferedData();

            //Lancer l'exécution de la fusion des fichiers
            await Task.Run(() => {
                //Console.WriteLine($"Start merge file {key}.txt with {keyFirstFile} and {keySecondFile}");
                //Lire la 1ère ligne
                currentLine[0] = sortedFiles[keyFirstFile].sr.ReadLine();
                currentLine[1] = sortedFiles[keySecondFile].sr.ReadLine();
                //Lire ligne par ligne
                while (
                    (currentLine[0] != null) &&
                    (currentLine[1] != null)
                    ) {

                    //Savoir quel élément vient en 1er
                    if(currentLine[0].CompareTo(currentLine[1]) < 0) {
                        //currentLine[0] vient avant currentLine[1]
                        //Écrire dans le fichier de résultat
                        Write(sortedFiles[key].fs, currentLine[0] + Environment.NewLine);
                        currentLine[0] = sortedFiles[keyFirstFile].sr.ReadLine();
                    } else {
                        //currentLine[0] vient après currentLine[1]
                        //Écrire dans le fichier de résultat
                        Write(sortedFiles[key].fs, currentLine[1] + Environment.NewLine);
                        currentLine[1] = sortedFiles[keySecondFile].sr.ReadLine();
                    }
                }

                //Écrire la fin du fichier qui n'a pas été lu
                if (currentLine[0] == null) {
                    if(currentLine[1] != null) {
                        //Écrire l'élément déjà lu
                        Write(sortedFiles[key].fs, currentLine[1] + Environment.NewLine);
                        //Écrire le reste des éléments
                        while ((currentLine[1] = sortedFiles[keySecondFile].sr.ReadLine()) != null) {
                            Write(sortedFiles[key].fs, currentLine[1] + Environment.NewLine);
                        }
                    }
                } else {
                    //Écrire l'élément déjà lu
                    Write(sortedFiles[key].fs, currentLine[0] + Environment.NewLine);
                    //Écrire le reste des éléments
                    while ((currentLine[0] = sortedFiles[keyFirstFile].sr.ReadLine()) != null) {
                        Write(sortedFiles[key].fs, currentLine[0] + Environment.NewLine);
                    }
                }

                //Supprimer les fichiers
                DeleteFile(keyFirstFile);
                DeleteFile(keySecondFile);

                //Console.WriteLine($"End merge file {key}.txt");
            });
        }

        /// <summary>
        /// Trier le fichier donné
        /// </summary>
        /// <param name="keyFile">La clé du fichier de donnée</param>
        private void SortFile(int keyFile) {
            //Console.WriteLine($"start sort on {keyFile}.txt");
            //Lecture des lignes
            List<string> lines = ReadAllLines(keyFile);

            //Tri des lignes
            lines.Sort();

            //Écrire par dessus le fichier
            sortedFiles[keyFile].fs.Position = 0;
            Write(sortedFiles[keyFile].fs, string.Join(Environment.NewLine, lines));

            //Console.WriteLine($"end sort on {keyFile}.txt");
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
        /// <param name="keyFile">La clé du fichier de donnée</param>
        /// <returns>Le contenu d'un fichier</returns>
        private List<string> ReadAllLines(int keyFile) {
            List<string> res = new List<string>();
            string currentLine;

            //Lire à partir du début
            sortedFiles[keyFile].sr.BaseStream.Position = 0;
            sortedFiles[keyFile].sr.DiscardBufferedData();

            //Lire ligne par ligne
            while ((currentLine = sortedFiles[keyFile].sr.ReadLine()) != null) {
                res.Add(currentLine);
            }

            return res;
        }

        /// <summary>
        /// Récupérer le nom complet d'un fichier de données
        /// </summary>
        /// <param name="key">La clé du fichier</param>
        /// <returns>Le nom complet du fichier de données</returns>
        private string GetFilename(int key) {
            return $"{FOLDER_DATA}{key}.txt";
        }

        /// <summary>
        /// Créer un fichier et crée les streams associés (FileStream, StreamReader)
        /// </summary>
        /// <param name="key">La clé du fichier</param>
        private void AddFile(int key) {
            sortedFiles.TryAdd(key, new SortFile(GetFilename(key)));
        }

        /// <summary>
        /// Supprimer un fichier et ses streams associés (FileStream, StreamReader)
        /// </summary>
        /// <param name="key">La clé du fichier</param>
        private void DeleteFile(int key) {
            //Fermer les Streams
            sortedFiles[key].CloseFile();
            //Enlever l'instance
            SortFile removed;
            sortedFiles.TryRemove(key, out removed);
            //Supprimer le fichier
            File.Delete(GetFilename(key));
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sort_big_data {
    class SortFile {
        //Propriétés
        public FileStream fs { get; set; }
        public StreamReader sr { get; set; }
        public bool SortingInProgress { get; set; }
        public bool AllDataAdded { get; set; }

        public SortFile(string filename) {
            //Créer le fichier
            fs = File.Create(filename);
            //Créer le reader
            sr = new StreamReader(fs);
            //Initialisation
            SortingInProgress = false;
            AllDataAdded = false;
        }

        /// <summary>
        /// Fermer les streams d'un fichier
        /// </summary>
        public void CloseFile() {
            fs.Close();
            sr.Close();
        }
    }
}

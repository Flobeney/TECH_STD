# Imports
import os
import time
from multiprocessing import Pool

# Constantes
FILENAME = 'file.txt'
FILENAME = 'file100mb.txt'
FILENAME_RES = 'res.txt'
DATA_FOLDER = 'data_' + str(time.time_ns()) + '/'
BYTES_LIMIT = 5000

# Fonctions

# Récupérer le temps en ms
def time_ms():
    return int(round(time.time() * 1000))

# Récupérer le nom complet du fichier avec le dossier temporaire
def getFilename(num):
    return DATA_FOLDER + str(num) + '.txt'

# Ajouter des lignes dans un fichier
def addLines(lines, filename):
    file = open(filename, 'a')

    # Parcourir les lignes
    for line in lines:
        # Séparer les mots
        words = line.split()

        # Parcourir les mots
        for word in words:
            # Les écrire dans le fichier
            file.write(word + '\n')

    # Fermeture du fichier
    file.close()

# Trier le fichier
def sortFile(filename):
    # Lire les données
    file = open(filename, 'r')
    words = file.readlines()
    file.close()

    # Tri des mots
    words.sort(key=lambda word: word.lower())

    # Ecrire les données triées
    file = open(filename, 'w')
    file.writelines(words)
    file.close()

# Fusion des fichiers
def mergeFiles(files):
    # Lister les clés
    keys = list(files.keys())
    max = len(keys)
    currentKey = 0
    currentFilename = ''

    # Parcourir les fichiers 2 par 2
    for i in range(0, max - 1, 2):
        currentKey = max + i
        currentFilename = getFilename(currentKey)
        mergeFileDisk(
                [
                    files[keys[i]], 
                    files[keys[i + 1]]
                ], 
                currentFilename
            )
        files[currentKey] = currentFilename
        
        # Effacer les clés et les fichiers fusionnés
        os.remove(files[keys[i]])
        os.remove(files[keys[i + 1]])
        del files[keys[i]]
        del files[keys[i + 1]]

    # Continuer le merge s'il reste des fichiers
    if len(files) > 1:
        mergeFiles(files)
    else:
        # Récupérer la clé restante
        key = list(files.keys())[0]
        # Déplacer le fichier
        os.rename(files[key], FILENAME_RES)

# Fusionner 2 fichiers
def mergeFileDisk(filenames, filenameRes):
    # Variables
    files = []
    lines = []

    # Fichier de résultat
    fileRes = open(filenameRes, 'w')

    # Lire les fichiers
    for filename in filenames:
        files.append(open(filename, 'r'))
        # Lire les premières lignes
        lines.append(files[-1].readline())

    # Tant qu'il y a des lignes à lire
    while lines[0] and lines[1]:
        # Savoir quel élément vient en premier
        if lines[0].lower() < lines[1].lower():
            # lines[0] vient avant
            fileRes.write(lines[0])
            # Lire la ligne suivante
            lines[0] = files[0].readline()
        else:
            fileRes.write(lines[1])
            # Lire la ligne suivante
            lines[1] = files[1].readline()

    # Lire la fin du fichier
    if lines[0]:
        # Ecrire la ligne déjà lue
        fileRes.write(lines[0])
        # Lire la fin des lignes
        fileRes.writelines(files[0].readlines())
    elif lines[1]:
        # Ecrire la ligne déjà lue
        fileRes.write(lines[1])
        # Lire la fin des lignes
        fileRes.writelines(files[1].readlines())

    # Fermer le fichier de résultat
    fileRes.close()

    return 

# Fonction principale
def main():
    # Variables
    nbFiles = 0
    allLinesRead = False
    files = {}
    lines = []
    startTime = time_ms()

    # Début du programme
    print('Start time : ' + str(startTime) + 'ms')

    # Créer le dossier où seront stockées les données
    os.mkdir(DATA_FOLDER)

    # Ouverture du fichier
    file = open(FILENAME, 'r')

    # Lecture des lignes
    while not allLinesRead:
        lines = file.readlines(BYTES_LIMIT)
        # Si lines est nul, le fichier a été totalement lu
        if lines:
            # Nom du fichier
            currentFile = getFilename(nbFiles)
            # Ajouter les lignes au fichier
            addLines(lines, currentFile)
            # Trier le fichier
            sortFile(currentFile)
            # Enregistrer le fichier dans le dictionnaire
            files[nbFiles] = currentFile
            # Passer au fichier suivant
            nbFiles+=1
            # Trier le fichier
        else:
            allLinesRead = True
    
    print('everything read')

    # Lorsque toutes les lignes ont été lues, fusionner les fichiers
    mergeFiles(files)

    # Supprimer le dossier de données
    os.rmdir(DATA_FOLDER)

    # Fermeture du fichier
    file.close()

    # Fin du programme
    endTime = time_ms()
    print('End time : ' + str(endTime) + 'ms')
    print('Elapsed : ' + str(endTime - startTime) + 'ms')

# Lancer le programme principal
main()
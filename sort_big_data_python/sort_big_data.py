# Import
import time

# Constantes
FILENAME = 'lorem5.txt'
FILENAME_RES = 'res.txt'
NS_TO_MS = (10**6)

# Fonctions

# Retourne le temps actuel en ms
def time_ms():
    return round(time.time_ns() / NS_TO_MS)

# Fonction principale
def main():
    # Variables
    startTime = time_ms()
    endTime = 0
    words = {}
    wordsRead = []
    sortedKeysWords = []

    # Début du programme
    print('Start at : ' + str(startTime) + 'ms')

    # Ouvrir les fichiers
    file = open(FILENAME, 'r')
    fileRes = open(FILENAME_RES, 'w')

    # Lire toutes les lignes du fichier
    for line in file:
        # Séparer la ligne en mots
        wordsRead = line.split()

        # Parcourir les mots lus
        for word in wordsRead:
            # Si le mot est déjà dans le dictionnaire
            if word in words:
                # Augmenter le compte du mot
                words[word] += 1
            else:
                # Ajouter le mot
                words[word] = 1

    # Fermeture du fichier
    file.close()

    # Lister et trier les mots
    sortedKeysWords = list(words.keys())
    sortedKeysWords.sort(key=lambda word: word.lower())

    # Parcourir les mots
    for key in sortedKeysWords:
        # Ecrire les mots dans le fichier résultant
        fileRes.write((key + '\n') * words[key])

    # Fermeture du fichier
    fileRes.close()

    # Fin du programme
    endTime = time_ms()
    print('End at : ' + str(endTime) + 'ms')
    print('Done in : ' + str(endTime - startTime) + 'ms')

# Lancer la fonction principale
main()
import os
import struct

# Constante
FILE_PATH = 'file.mp3'

# Ouvrir le fichier et le lire
file = open(FILE_PATH, 'rb')
data = file.read()

# Sortir les informations
end_file = 24
test = struct.unpack('3s2b11s2i', data[slice(0, end_file)])

# Récupérer les infos dans l'ordre
res = {
    'fileID': test[0].decode('utf-8'),
    'version': str(test[1]) + '.' + str(test[2]),
    'flags': test[3].decode('utf-8'),
    'size': test[4]
}

# Afficher les infos
print(res)
print(test)

# Fermer le fichier
file.close()
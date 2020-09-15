# Librairies
import os
import struct

# Fonctions
def unsynchsafe(num):
    out = 0
    mask = 0x7f000000
    for i in range(4):
        out >>= 1
        out |= num & mask
        mask >>= 8
    return out

# Extraire les données d'une frame
def get_frame(data, count):
    # Header de la frame
    count = {
        'start': count['end'],
        'end': count['end'] + 10
    }
    # Sortir les données
    data_res = struct.unpack('4s4b2s', data[count['start']:count['end']])
    # Les trier
    res = {
        'id': data_res[0].decode('utf-8'),
        'size': unsynchsafe(int.from_bytes([data_res[1], data_res[2], data_res[3], data_res[4]], byteorder='big')),
        'flags': data_res[5].decode('utf-8')
    }

    # Sortir les informations suivantes
    count = {
        'start': count['end'],
        'end': count['end'] + res['size']
    }
    # Sortir les données
    data_res = struct.unpack('b' + str(res['size'] - 1) + 's', data[count['start']:count['end']])

    # Encodage
    res['encoding'] = data_res[0]

    # Si la taille n'est pas trop grande
    if res['size'] < 1000:
        # En fonction de l'encodage
        if res['encoding'] == 3:
            res['tag'] = data_res[1].decode('utf-8')
        else:
            res['tag'] = data_res[1].decode('latin_1')


    return {
        'count': count,
        'res': res
    }

# Constante
FILE_PATH = 'song.mp3'

# Ouvrir le fichier et le lire
file = open(FILE_PATH, 'rb')
data = file.read()

# Sortir les informations
count = {
    'start': 0,
    'end': 10
}
data_res = struct.unpack('3s7b', data[count['start']:count['end']])

# Récupérer les infos dans l'ordre
# Header
header = {
    'fileID': data_res[0].decode('utf-8'),
    'version': str(data_res[1]) + '.' + str(data_res[2]),
    'flags': str(data_res[3]),
    'size': unsynchsafe(int.from_bytes([data_res[4], data_res[5], data_res[6], data_res[7]], byteorder='big'))
}

# Variables
frames = []
all_tags_size = 0

# Sortir les informations suivantes
for i in range(18):
    res = get_frame(data, count)
    frames.append(res['res'])
    # Compter la taille des tags extraits
    all_tags_size += res['res']['size']
    # Garder en mémoire où on en est dans les données
    count = res['count']

# Afficher les infos
print('Header: ', header)
print('Frames: ', frames)
print('Taille des tags: ', all_tags_size)

# Fermer le fichier
file.close()
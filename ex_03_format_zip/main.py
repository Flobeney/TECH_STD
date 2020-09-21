# Librairies
import os
import struct

# Fonctions

# Extraire les données d'un fichier
def get_filename(data, count):
    # Header
    count = {
        'start': count['end'],
        'end': count['end'] + 30
    }
    # Sortir les données
    data_res = struct.unpack('4s2s2s2s2s2s4s4s4s2s2s', data[count['start']:count['end']])
    # Les trier
    res = {
        'localFileHeader': hex(int.from_bytes(data_res[0], 'little')),
        'versionNeeded': int.from_bytes(data_res[1], 'little'),
        'bitFlag': int.from_bytes(data_res[2], 'little'),
        'compressionMethod': int.from_bytes(data_res[3], 'little'),
        'fileLastModifTime': int.from_bytes(data_res[4], 'little'),
        'fileLastModifDate': int.from_bytes(data_res[5], 'little'),
        'crc': int.from_bytes(data_res[6], 'little'),
        'sizeCompressed': int.from_bytes(data_res[7], 'little'),
        'sizeUncompressed': int.from_bytes(data_res[8], 'little'),
        'fileLength': int.from_bytes(data_res[9], 'little'),
        'extraFieldLength': int.from_bytes(data_res[10], 'little'),
    }

    # Sortir les informations suivantes
    count = {
        'start': count['end'],
        'end': count['end'] + res['fileLength'] + res['extraFieldLength']
    }
    # Sortir les données
    data_res = struct.unpack(str(res['fileLength']) + 's' + str(res['extraFieldLength']) + 's', data[count['start']:count['end']])
    # Les trier
    res['filename'] = data_res[0].decode('utf-8')
    res['extraField'] = data_res[1]

    return {
        'count': count,
        'data': res
    }

# Constante
FILE_PATH = 'file.zip'

# Ouvrir le fichier et le lire
file = open(FILE_PATH, 'rb')
data = file.read()
count = {
    'start': 0,
    'end': 0
}
filenames = []
next_file = 0

# Sortir les informations
while next_file != -1:
    res = get_filename(data, count)
    filenames.append(res['data']['filename'])
    count = res['count']
    next_file = data.find(b'PK\x03\x04', count['start'])
    count = {
        'start': next_file,
        'end': next_file
    }

print('filenames: ', filenames)

# Fermer le fichier
file.close()
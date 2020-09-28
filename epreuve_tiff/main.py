# Librairies
import os
import struct

# Constantes
FILE_PATH = 'Lenna.tiff'
VALID_TIFF_FILE = 42 # Page 13 TIFF6.pdf

# Fonctions
# Extraction d'un IFD
def extractIFD(data, count):
    # Tag, type, count
    count = {
        'start': count['end'],
        'end': count['end'] + 8
    }
    # Sortir les informations
    data_res = struct.unpack('>2hi', data[count['start']:count['end']])
    res = {
        'tag': data_res[0],
        'type': data_res[1],
        'count': data_res[2],
    }
    # Value
    count = {
        'start': count['end'],
        'end': count['end'] + 4
    }
    # String qui sera utilisé pour le unpack, dépend du type de données
    str_unpack = '>'
    # En fonction du type de données
    # 3 => short
    if res['type'] == 3:
        # 1er h => valeur, 2ème => padding
        str_unpack += 'hh'
    # 4 => long
    if res['type'] == 4:
        # 1er i => valeur
        str_unpack += 'i'
        
    # Sortir les informations
    data_res = struct.unpack(str_unpack, data[count['start']:count['end']])

    # Valeur
    res['value'] = data_res[0]

    # Retourner le résultat ainsi que le count actuel pour pouvoir le réutiliser
    return {
        'count': count,
        'res': res
    }

# Affichage des IFD
def printIFD(res):
    # Parcourir les IFD
    for currentItem in res:
        # Tag
        tag = hex(res[currentItem]['tag'])
        # Valeur
        value = str(res[currentItem]['value'])
        # Affichage
        print(currentItem + ' ' + tag + ' --> ' + value)

# Affichage du header
def printHeader(header):
    # Parcourir les items du header
    for currentItem in header:
        # Valeur
        value = str(header[currentItem])
        # Affichage
        print(currentItem + ' --> ' + value)

# Ouvrir le fichier et le lire
file = open(FILE_PATH, 'rb')
data = file.read()

# Tableau des IFDs
ifds = {}
listNamesIFD = [
    'ImageWidth',
    'ÌmageLength',
    'BitsPerSample',
    'Compression',
    'PhotometricInterpretation',
    'FillOrder',
    'StripOffsets',
    'Orientation',
    'SamplesPerPixel',
    'RowsPerStrip',
    'StripByteCounts',
    'PlanarConfiguration',
    'ResolutionUnit',
    'ExtraSamples',
    'SampleFormat',
]

# Sortir les informations
count = {
    'start': 0,
    'end': 8
}
data_res = struct.unpack('>2s2bi', data[count['start']:count['end']])

# Récupérer les infos dans l'ordre
byteOrder = data_res[0].decode('utf-8')
# Header
header = {
    'byteOrder': byteOrder + (' (Big-endian, Motorola)' if byteOrder == 'MM' else ' (Little-endian, Intel)'),
    'validTiffFile': data_res[2] == VALID_TIFF_FILE,
    'offset': data_res[3]
}

# Sortir les informations
count = {
    'start': header['offset'],
    'end': header['offset'] + 2
}
data_res = struct.unpack('>h', data[count['start']:count['end']])
# Nombre de IFDs
header['nbIFDs'] = data_res[0]

# Récupérer tous les IFDs (-1 parce quel le nombre d'IFD compte comme un)
for i in range(header['nbIFDs'] - 1):
    # Extraire l'IFD
    res = extractIFD(data, count)
    # Garder le count en mémoire
    count = res['count']
    # Récupérer l'IFD avec le bon nom pour indiquer le type de valeur
    ifds[listNamesIFD[i]] = res['res']

# Affichage du header et des IFDs
printHeader(header)
printIFD(ifds)
# Journal de bord

## 05.10.2020 - début du projet

* Choix du langage : C#
* Création de la classe "Magasin", qui contient l'affichage des éléments (clients, caisses, magasin)
* Déplacements des clients, avec gestion des rebonds
* Création des caisses
* Déclenchement d'un événement lorsque le temps restant (de shopping) est écoulé

## 12.10.2020

* Levage d'un événement lorsque le temps de shopping est écoulé
* Lorsque c'est le cas, passage d'une caisse à un client pour qu'il se dirige vers cette caisse
* Arrêt du client lorsqu'il a atteint la caisse
* Empilage des clients l'un après l'autre (file d'attente)

## 16.10.2020

* Ajout d'une "file d'attente" pour l'emplacement des clients lorsqu'ils attendent à la caisse
* Les clients se dirigent donc au bout de la file d'attente, et s'empilent ensuite
* Le client le plus proche de la caisse a un temps d'attente avant d'avoir fini à la caisse
* Ensuite, il disparaît, et les clients du dessus descendent d'un cran (s'il en reste)
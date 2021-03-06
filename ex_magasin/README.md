# Journal de bord

[Lien du GitHub](https://github.com/Flobeney/TECH_STD/tree/master/ex_magasin)

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

## 23.10.2020

* Affichage du temps d'attente restant sur le client lorsqu'il est à la caisse
* Ajout d'un client après un certain temps
* Indication qu'une caisse est pleine et ne peut accueillir plus de clients
  * Les clients deviennent rouges et reprennent leurs vitesses d'avant
  * Une fois qu'une caisse est de nouveau disponible, les clients se dirigent à nouveau vers la caisse

## 24.10.2020

* Lorsqu'un client se dirige vers une caisse et que celle-ci est pleine, le client va essayer de se diriger vers une autre caisse (s'il en reste de disponible)
* Après un certain temps d'attente avec aucune caisse disponible, une nouvelle caisse s'ouvre
* Fermeture d'une caisse si elle reste ouverte sans client trop longtemps
* Les clients ayant besoin d'une caisse se dirigent vers la caisse ayant le moins de clients dans sa file d'attente

## 26.10.2020

* Refactorisation, commentaire du code

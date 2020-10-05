using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ex_magasin {
    /// <summary>
    /// Représentation d'un client
    /// </summary>
    class Customer {
        //Constantes
        const int SIZE_SPRITE = 25;
        const int OFFSET_CLIENT = 5;

        //Champs
        PointF startLocation;
        PointF speed;
        SizeF size;
        Stopwatch sw;
        Brush color;
        long timeLeft;

        //Propriétés
        PointF currentLocation {
            get {
                //Position qui sera retournée
                PointF res = new PointF(
                    (speed.X / 1000 * (float)sw.Elapsed.TotalMilliseconds) + startLocation.X,
                    (speed.Y / 1000 * (float)sw.Elapsed.TotalMilliseconds) + startLocation.Y
                );

                //Gestion des rebonds
                HandleBounce(res);

                return res;
            }
        }

        /// <summary>
        /// Constructeur désigné
        /// </summary>
        /// <param name="pStartLocation">Position de départ</param>
        /// <param name="pSpeed">Vitesse</param>
        public Customer(PointF pStartLocation, PointF pSpeed, int pTimeLeft) {
            //Nouveau Stopwatch
            sw = new Stopwatch();
            //Démarrer le Stopwatch
            sw.Start();
            //Initialiser les valeurs
            startLocation = pStartLocation;
            speed = pSpeed;
            timeLeft = pTimeLeft * 1000; // Convertir le temps en ms
            color = Brushes.Gray;
            size = new SizeF(SIZE_SPRITE, SIZE_SPRITE);
        }

        /// <summary>
        /// Dessin du sprite
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Paint(object sender, PaintEventArgs e) {
            e.Graphics.FillEllipse(color, new RectangleF(currentLocation, size));
            Console.WriteLine($"timeLeft: {timeLeft}");
        }

        /// <summary>
        /// Gérer les rebonds
        /// </summary>
        /// <param name="newLocation">Nouvelle postion qui sera donnée</param>
        private void HandleBounce(PointF newLocation) {
            //Droite
            if (newLocation.X + size.Width > Properties.Settings.Default.SIZE_FORM.Width) {
                //Éviter les blocages contre le bord
                newLocation.X -= OFFSET_CLIENT;
                //Faire le rebond
                Bounce(newLocation);
            }
            //Gauche
            if (newLocation.X < 0) {
                //Éviter les blocages contre le bord
                newLocation.X += OFFSET_CLIENT;
                //Faire le rebond
                Bounce(newLocation);
            }
            //Bas
            if (newLocation.Y + size.Height > Properties.Settings.Default.SIZE_FORM.Height) {
                //Éviter les blocages contre le bord
                newLocation.Y -= OFFSET_CLIENT;
                //Faire le rebond
                Bounce(newLocation, false);
            }
            //Haut
            if (newLocation.Y < 0) {
                //Éviter les blocages contre le bord
                newLocation.Y += OFFSET_CLIENT;
                //Faire le rebond
                Bounce(newLocation, false);
            }
        }

        /// <summary>
        /// Faire rebondir le client
        /// </summary>
        /// <param name="newLocation">Nouvelle postion qui sera donnée</param>
        /// <param name="hOrV">Rebond horizontal ou vertical</param>
        private void Bounce(PointF newLocation, bool hOrV = true) {
            //Sauver la position actuelle comme étant la nouvelle position de départ
            startLocation = newLocation;
            //Définir la nouvelle vitesse
            //Rebond horizontal ou vertical
            if (hOrV) { 
                speed.X = -speed.X;
            } else {
                speed.Y = -speed.Y;
            }
            //Stopper et remettre à 0 le timer
            sw.Restart();
        }
    }
}

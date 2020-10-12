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
        const int SPEED_GOTO_CHECKOUT = 5;
        readonly Brush BASE_COLOR = Brushes.Gray;
        readonly Brush TIME_END_COLOR = Brushes.Blue;

        //Gestion d'événement
        public event EventHandler TimeEnded;

        //Champs
        PointF startLocation;
        PointF speed;
        SizeF size;
        Stopwatch sw;
        Brush color;
        Timer timeLeft;
        Checkout checkoutToGo;

        //Propriétés
        PointF NewLocation {
            get {
                return new PointF(
                    (speed.X / 1000 * (float)sw.Elapsed.TotalMilliseconds) + startLocation.X,
                    (speed.Y / 1000 * (float)sw.Elapsed.TotalMilliseconds) + startLocation.Y
                );
            }
        }
        PointF CurrentLocation {
            get {
                //Position qui sera retournée
                PointF res = NewLocation;

                //Gestion des rebonds
                HandleBounce(res);

                //Si la caisse est atteinte
                if (IsCheckoutReached(res)) {
                    Console.WriteLine("reached");
                    checkoutToGo.AddCustomer(this);
                }

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
            color = BASE_COLOR;
            size = new SizeF(SIZE_SPRITE, SIZE_SPRITE);
            //Timer pour le temps restant
            timeLeft = new Timer();
            timeLeft.Interval = pTimeLeft * 1000;
            timeLeft.Enabled = true;
            timeLeft.Tick += new EventHandler(OnTick);
        }

        /// <summary>
        /// Dirige le client avec la nouvelle position et vitesse
        /// </summary>
        /// <param name="newLocation">Nouvelle position qui sera donnée</param>
        /// <param name="newSpeed">Nouvelle vitesse qui sera donnée</param>
        public void GoTo(PointF newLocation, PointF newSpeed) {
            //Nouvelle valeur
            startLocation = newLocation;
            speed = newSpeed;
            //Relancer le timer
            sw.Restart();
        }

        /// <summary>
        /// Dirige le client vers une caisse
        /// </summary>
        /// <param name="checkout">La caisse vers laquelle se diriger</param>
        public void GoTo(Checkout checkout) {
            checkoutToGo = checkout;

            //Calculer la nouvelle position
            PointF newLocation = NewLocation;
            GoTo(newLocation, new PointF(
                (checkout.Location.X - newLocation.X) / SPEED_GOTO_CHECKOUT,
                (checkout.Location.Y - newLocation.Y) / SPEED_GOTO_CHECKOUT
            ));
        }

        /// <summary>
        /// Si la caisse est atteinte
        /// </summary>
        /// <param name="newLocation">Nouvelle position qui sera donnée</param>
        /// <returns>true si la caisse est atteinte, false sinon</returns>
        private bool IsCheckoutReached(PointF newLocation) {
            bool res = false;

            if(checkoutToGo != null) {
                RectangleF customer = new RectangleF(newLocation, size);
                //Déterminer si le client entre en contact avec la caisse
                res = checkoutToGo.RectToDraw.IntersectsWith(customer);
            }

            return res;
        }

        /// <summary>
        /// Dessin du sprite
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Paint(object sender, PaintEventArgs e) {
            e.Graphics.FillEllipse(color, new RectangleF(CurrentLocation, size));
        }

        /// <summary>
        /// Gérer les rebonds
        /// </summary>
        /// <param name="newLocation">Nouvelle position qui sera donnée</param>
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
        /// <param name="newLocation">Nouvelle position qui sera donnée</param>
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

        /// <summary>
        /// Pour chaque Tick du timer
        /// </summary>
        protected void OnTick(object sender, EventArgs e) {
            Console.WriteLine("time end");
            color = TIME_END_COLOR;
            timeLeft.Enabled = false;
            //Appeler l'event pour dire que le client a fini ses courses
            OnTimeEnded(EventArgs.Empty);
        }

        /// <summary>
        /// Invocation d'event
        /// </summary>
        /// <param name="e">Argument des events</param>
        protected virtual void OnTimeEnded(EventArgs e) {
            //Invoquer l'event si TimeEnded n'est pas null
            TimeEnded?.Invoke(this, e);
        }
    }
}

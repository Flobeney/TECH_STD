﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace ex_magasin {
    /// <summary>
    /// Enumération du statut possible pour le client
    /// </summary>
    public enum Status {
        SHOPPING,
        GOING_TO_CHECKOUT,
        WAITING_FOR_ANOTHER_CHECKOUT,
        WAITING_AT_CHECKOUT,
        AT_CHECKOUT,
    };

    /// <summary>
    /// Représentation d'un client
    /// </summary>
    class Customer {
        //Constantes
        private const int OFFSET_CLIENT = 5;
        private const int SPEED_GOTO_CHECKOUT = 5;
        private readonly Brush BASE_COLOR = Brushes.Gray;
        private readonly Brush TIME_END_COLOR = Brushes.Blue;
        private readonly Brush WAITING_ANOTHER_CHECKOUT_COLOR = Brushes.Red;
        private readonly Brush COLOR_TEXT = Brushes.Black;
        private readonly Font FONT_TEXT = new Font(FontFamily.GenericSansSerif, 12);

        //Gestionnaire d'événement
        public event EventHandler TimeEnded;

        //Champs
        private PointF startLocation;
        private PointF speed;
        private PointF baseSpeed;
        private SizeF size;
        private Stopwatch sw;
        private Brush color;
        private Timer tmr;
        private int timeToWait;

        //Propriétés
        public int TimeToWaitAtCheckout { get; private set; }
        public Checkout CheckoutToGo { get; private set; }
        public Status StatusCustomer { get; private set; }

        //Propriétés calculées
        /// <summary>
        /// Position du client en fonction du temps écoulé
        /// </summary>
        private PointF NewLocation {
            get {
                return new PointF(
                    (speed.X / 1000 * (float)sw.Elapsed.TotalMilliseconds) + startLocation.X,
                    (speed.Y / 1000 * (float)sw.Elapsed.TotalMilliseconds) + startLocation.Y
                );
            }
        }
        /// <summary>
        /// Position du client en tenant compte des rebonds sur les bords
        /// </summary>
        private PointF CurrentLocation {
            get {
                //Position qui sera retournée
                PointF res;
                if (StatusCustomer == Status.WAITING_AT_CHECKOUT || StatusCustomer == Status.AT_CHECKOUT) {
                    res = CheckoutToGo.GetNextWaitingLocation(this);
                } else {
                    res = NewLocation;

                    //Si le client fait son shopping ou attend l'ouverture d'une autre caisse
                    if(StatusCustomer == Status.SHOPPING || StatusCustomer == Status.WAITING_FOR_ANOTHER_CHECKOUT) {
                        //Gestion des rebonds
                        HandleBounce(res);
                    }

                    //Si la caisse est atteinte
                    if (StatusCustomer == Status.GOING_TO_CHECKOUT && IsCheckoutReached(res)) {
                        StatusCustomer = Status.WAITING_AT_CHECKOUT;
                        CheckoutToGo.AddCustomer(this);
                    }
                }

                return res;
            }
        }

        /// <summary>
        /// Constructeur désigné
        /// </summary>
        /// <param name="pStartLocation">Position de départ</param>
        /// <param name="pSpeed">Vitesse</param>
        public Customer(PointF pStartLocation, PointF pSpeed, int pTmr) {
            //Nouveau Stopwatch
            sw = new Stopwatch();
            //Démarrer le Stopwatch
            sw.Start();
            //Initialiser les valeurs
            startLocation = pStartLocation;
            speed = pSpeed;
            baseSpeed = pSpeed;
            color = BASE_COLOR;
            size = Properties.Settings.Default.SIZE_CHECKOUT_CUSTOMER;
            StatusCustomer = Status.SHOPPING;
            //Timer pour le temps à faire du shopping
            timeToWait = pTmr;
            TimeToWaitAtCheckout = pTmr / 2;
            tmr = new Timer();
            //timeToWait est en secondes, le convertir en ms
            tmr.Interval = timeToWait * 1000;
            tmr.Enabled = true;
            tmr.Tick += new EventHandler(OnTick);
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
            CheckoutToGo = checkout;
            StatusCustomer = Status.GOING_TO_CHECKOUT;
            color = TIME_END_COLOR;

            //Calculer la nouvelle position
            PointF newLocation = NewLocation;
            GoTo(newLocation, new PointF(
                (checkout.LocationWaitingQueue.X - newLocation.X) / SPEED_GOTO_CHECKOUT,
                (checkout.LocationWaitingQueue.Y - newLocation.Y) / SPEED_GOTO_CHECKOUT
            ));
        }

        /// <summary>
        /// Si la caisse est atteinte
        /// </summary>
        /// <param name="newLocation">Nouvelle position qui sera donnée</param>
        /// <returns>true si la caisse est atteinte, false sinon</returns>
        private bool IsCheckoutReached(PointF newLocation) {
            bool res = false;

            if(CheckoutToGo != null) {
                Rectangle customer = new Rectangle(
                    new Point((int)newLocation.X, (int)newLocation.Y), 
                    new Size((int)size.Width, (int)size.Height)
                );
                //Déterminer si le client entre en contact avec la caisse
                res = CheckoutToGo.WaitingQueueToDraw.IntersectsWith(customer);
            }

            return res;
        }

        /// <summary>
        /// Dessin du client
        /// </summary>
        public void Paint(object sender, PaintEventArgs e) {
            //Dessiner le client
            e.Graphics.FillEllipse(color, new RectangleF(CurrentLocation, size));
            //Si le client est à la caisse, indiquer le temps d'attente restant
            if(StatusCustomer == Status.AT_CHECKOUT) {
                e.Graphics.DrawString(
                    $"{TimeToWaitAtCheckout - sw.Elapsed.Seconds}", 
                    FONT_TEXT, COLOR_TEXT, 
                    CurrentLocation.X + (size.Width / 4), CurrentLocation.Y + (size.Height / 4)
                );
            }
        }

        /// <summary>
        /// Le client attend à la caisse
        /// </summary>
        public void CustomerWaitAtCheckout() {
            StatusCustomer = Status.AT_CHECKOUT;
            //Timer pour le temps d'attente en caisse
            tmr.Interval = 1000;
            tmr.Enabled = true;
            sw.Restart();
        }

        /// <summary>
        /// Aucune caisse disponible pour le client
        /// </summary>
        public void CheckoutFull() {
            //La caisse n'est pas disponible
            CheckoutToGo = null;
            //Couleur
            color = WAITING_ANOTHER_CHECKOUT_COLOR;
            //Status
            StatusCustomer = Status.WAITING_FOR_ANOTHER_CHECKOUT;
            //Vitesse et direction
            startLocation = NewLocation;
            speed = baseSpeed;
            sw.Restart();
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
            if (newLocation.Y + size.Height > Properties.Settings.Default.SIZE_FORM.Height - Properties.Settings.Default.SIZE_WAITING_QUEUE.Height - Properties.Settings.Default.SIZE_CHECKOUT_CUSTOMER.Height) {
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
            if(StatusCustomer != Status.AT_CHECKOUT) {
                tmr.Enabled = false;
                //Appeler l'event pour dire que le client a fini ses courses
                OnTimeEnded(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Invocation d'event lorsque le temps de shopping est fini
        /// </summary>
        /// <param name="e">Argument de l'event</param>
        protected void OnTimeEnded(EventArgs e) {
            //Invoquer l'event si TimeEnded n'est pas null
            TimeEnded?.Invoke(this, e);
        }
    }
}

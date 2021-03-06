﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ex_magasin {
    /// <summary>
    /// Représentation d'une caisse
    /// </summary>
    class Checkout : IComparable {
        //Constantes
        private const int OFFEST_WAITING = 10;
        private const int NB_MAX_CUSTOMER = 3;
        private readonly Brush COLOR_CLOSE = Brushes.Red;
        private readonly Brush COLOR_OPEN = Brushes.Green;
        private readonly Pen COLOR_WAITING_QUEUE = Pens.Black;

        //Gestionnaire d'événement
        public event EventHandler<CustomerDoneAtCheckoutEventArgs> CustomerDoneAtCheckout;
        public event EventHandler CheckoutFull;
        public event EventHandler CheckoutAvailable;

        //Champs
        private PointF location;
        private RectangleF checkoutToDraw;
        private Timer tmrWait;

        //Propriétés
        public Point LocationWaitingQueue { get; set; }
        public bool IsOpen { get; set; }
        public Rectangle WaitingQueueToDraw { get; private set; }
        public List<Customer> CustomersWaiting { get; set; }
        //Propriétés calculées
        public bool IsAtMax {
            get {
                return CustomersWaiting.Count >= NB_MAX_CUSTOMER;
            }
        }
        public bool IsEmpty {
            get {
                return CustomersWaiting.Count == 0;
            }
        }

        /// <summary>
        /// Constructeur désigné
        /// </summary>
        /// <param name="pLocation">Position</param>
        public Checkout(PointF pLocation, bool pIsOpen = false) {
            CustomersWaiting = new List<Customer>();
            location = pLocation;
            IsOpen = pIsOpen;
            //Dessin de la caisse
            checkoutToDraw = new RectangleF(location, Properties.Settings.Default.SIZE_CHECKOUT_CUSTOMER);
            //Emplacement de la file d'attente
            LocationWaitingQueue = new Point(
                (int)location.X,
                (int)location.Y - Properties.Settings.Default.SIZE_WAITING_QUEUE.Height
            );
            //Dessin de la file d'attente
            WaitingQueueToDraw = new Rectangle(
                LocationWaitingQueue, 
                Properties.Settings.Default.SIZE_WAITING_QUEUE
            );
            //Timer pour le temps d'attente
            tmrWait = new Timer();
            tmrWait.Tick += new EventHandler(OnTick);
            tmrWait.Enabled = false;
        }

        /// <summary>
        /// Ajouter un client à la file d'attente de la caisse
        /// </summary>
        /// <param name="customer">Le client</param>
        public void AddCustomer(Customer customer) {
            CustomersWaiting.Add(customer);
            //Premier client
            if (CustomersWaiting.Count == 1) {
                SetTimer();
            }
            //Caisse pleine
            if(CustomersWaiting.Count >= NB_MAX_CUSTOMER) {
                OnCheckoutFull(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Mettre en route le timer pour le client en train d'attendre
        /// </summary>
        private void SetTimer() {
            CustomersWaiting[0].CustomerWaitAtCheckout();
            tmrWait.Interval = CustomersWaiting[0].TimeToWaitAtCheckout * 1000;
            tmrWait.Enabled = true;
        }

        /// <summary>
        /// Retourne l'emplacement dans la file d'attente en fonction de la position dans la file d'attente
        /// </summary>
        /// <param name="c">CLient qui demande sa position dans la file d'attente</param>
        /// <returns>Emplacement dans la file d'attente</returns>
        public PointF GetNextWaitingLocation(Customer c) {
            return new PointF(
                location.X,
                location.Y - Properties.Settings.Default.SIZE_CHECKOUT_CUSTOMER.Height - (CustomersWaiting.IndexOf(c) * (OFFEST_WAITING + Properties.Settings.Default.SIZE_CHECKOUT_CUSTOMER.Height))
            );
        }

        /// <summary>
        /// Dessin du sprite
        /// </summary>
        public void Paint(object sender, PaintEventArgs e) {
            //Caisse
            e.Graphics.FillRectangle(IsOpen ? COLOR_OPEN : COLOR_CLOSE, checkoutToDraw);
            //File d'attente
            e.Graphics.DrawRectangle(COLOR_WAITING_QUEUE, WaitingQueueToDraw);
        }

        /// <summary>
        /// Pour chaque Tick du timer
        /// </summary>
        protected void OnTick(object sender, EventArgs e) {
            //Savoir si actuellement la file d'attente de la caisse est pleine
            bool isWaitingQueueFull = CustomersWaiting.Count == NB_MAX_CUSTOMER;
            //Indiquer au magasin que ce client a terminé
            OnCustomerDoneAtCheckout(new CustomerDoneAtCheckoutEventArgs(CustomersWaiting[0]));
            //Enlever le client de la liste
            CustomersWaiting.RemoveAt(0);
            //Lancer le prochain timer pour le client suivant, sinon le couper
            if(CustomersWaiting.Count > 0) {
                SetTimer();
            } else {
                tmrWait.Enabled = false;
            }
            //Si la file d'attente était pleine, indiquer que ce n'est plus le cas
            if (isWaitingQueueFull) {
                //Indiquer que la caisse a une/des place(s) disponible(s)
                OnCheckoutAvailable(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Invocation d'event lorsque le client a fini de payer
        /// </summary>
        /// <param name="e">Argument de l'event</param>
        protected void OnCustomerDoneAtCheckout(CustomerDoneAtCheckoutEventArgs e) {
            //Invoquer l'event si CustomerDoneAtCheckout n'est pas null
            CustomerDoneAtCheckout?.Invoke(this, e);
        }

        /// <summary>
        /// Invocation d'event lorsque la caisse est pleine
        /// </summary>
        /// <param name="e">Argument de l'event</param>
        protected void OnCheckoutFull(EventArgs e) {
            //Invoquer l'event si CheckoutFull n'est pas null
            CheckoutFull?.Invoke(this, e);
        }

        /// <summary>
        /// Invocation d'event lorsque une/des place(s) dans la file d'attente de la caisse est/sont disponible(s)
        /// </summary>
        /// <param name="e">Argument de l'event</param>
        protected void OnCheckoutAvailable(EventArgs e) {
            //Invoquer l'event si CheckoutAvailable n'est pas null
            CheckoutAvailable?.Invoke(this, e);
        }

        /// <summary>
        /// Méthode de comparaison
        /// </summary>
        /// <param name="obj">Élément à comparer</param>
        /// <returns>L'ordre des 2 éléments</returns>
        public int CompareTo(object obj) {
            Checkout other = obj as Checkout;

            return CustomersWaiting.Count.CompareTo(other.CustomersWaiting.Count);
        }
    }
}

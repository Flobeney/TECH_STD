using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ex_magasin {
    /// <summary>
    /// Représentation d'une caisse
    /// </summary>
    class Checkout {
        //Constantes
        const int OFFEST_WAITING = 10;
        readonly Brush COLOR_CLOSE = Brushes.Red;
        readonly Brush COLOR_OPEN = Brushes.Green;
        readonly Pen COLOR_WAITING_QUEUE = Pens.Black;

        //Gestionnaire d'événement
        public event EventHandler<CustomerDoneAtCheckoutEventArgs> CustomerDoneAtCheckout;

        //Propriétés
        private PointF Location { get; set; }
        public Point LocationWaitingQueue { get; set; }
        public bool IsOpen { get; set; }
        private RectangleF CheckoutToDraw { get; set; }
        public Rectangle WaitingQueueToDraw { get; private set; }
        private Timer TmrWait { get; set; }
        private List<Customer> CustomersWaiting { get; set; }

        /// <summary>
        /// Constructeur désigné
        /// </summary>
        /// <param name="pLocation">Position</param>
        public Checkout(PointF pLocation, bool pIsOpen = false) {
            CustomersWaiting = new List<Customer>();
            Location = pLocation;
            IsOpen = pIsOpen;
            //Dessin de la caisse
            CheckoutToDraw = new RectangleF(Location, Properties.Settings.Default.SIZE_CHECKOUT_CUSTOMER);
            //Emplacement de la file d'attente
            LocationWaitingQueue = new Point(
                (int)Location.X,
                (int)Location.Y - Properties.Settings.Default.SIZE_WAITING_QUEUE.Height
            );
            //Dessin de la file d'attente
            WaitingQueueToDraw = new Rectangle(
                LocationWaitingQueue, 
                Properties.Settings.Default.SIZE_WAITING_QUEUE
            );
            //Timer pour le temps d'attente
            TmrWait = new Timer();
            TmrWait.Tick += new EventHandler(OnTick);
            TmrWait.Enabled = false;
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
        }

        /// <summary>
        /// Mettre en route le timer pour le client en train d'attendre
        /// </summary>
        private void SetTimer() {
            CustomersWaiting[0].CustomerWaitAtCheckout();
            TmrWait.Interval = CustomersWaiting[0].TimeToWaitAtCheckout * 1000;
            TmrWait.Enabled = true;
        }

        /// <summary>
        /// Retourne l'emplacement dans la file d'attente en fonction de la position dans la file d'attente
        /// </summary>
        /// <param name="place">Position dans la file d'attente</param>
        /// <returns>Emplacement dans la file d'attente</returns>
        public PointF GetNextWaitingLocation(Customer c) {
            return new PointF(
                Location.X,
                Location.Y - Properties.Settings.Default.SIZE_CHECKOUT_CUSTOMER.Height - (CustomersWaiting.IndexOf(c) * (OFFEST_WAITING + Properties.Settings.Default.SIZE_CHECKOUT_CUSTOMER.Height))
            );
        }

        /// <summary>
        /// Dessin du sprite
        /// </summary>
        public void Paint(object sender, PaintEventArgs e) {
            //Caisse
            e.Graphics.FillRectangle(IsOpen ? COLOR_OPEN : COLOR_CLOSE, CheckoutToDraw);
            //File d'attente
            e.Graphics.DrawRectangle(COLOR_WAITING_QUEUE, WaitingQueueToDraw);
        }

        /// <summary>
        /// Pour chaque Tick du timer
        /// </summary>
        protected void OnTick(object sender, EventArgs e) {
            //Indiquer au magasin que ce client a terminé
            OnCustomerDoneAtCheckout(new CustomerDoneAtCheckoutEventArgs(CustomersWaiting[0]));
            //Enlever le client de la liste
            CustomersWaiting.RemoveAt(0);

            if(CustomersWaiting.Count > 0) {
                SetTimer();
            } else {
                TmrWait.Enabled = false;
            }
        }

        /// <summary>
        /// Invocation d'event
        /// </summary>
        /// <param name="e">Argument de l'event</param>
        protected virtual void OnCustomerDoneAtCheckout(CustomerDoneAtCheckoutEventArgs e) {
            //Invoquer l'event si TimeEnded n'est pas null
            CustomerDoneAtCheckout?.Invoke(this, e);
        }
    }
}

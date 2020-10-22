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
    /// Représentation d'un magasin
    /// </summary>
    class Magasin : Control {
        //Constantes
        private const int FPS_INTERVAL = 8;
        private const int NB_CUSTOMER = 15;
        private const int V_CUSTOMER = 200;
        private const int SPACE_BETWEEN_CHECKOUT = 15;
        private const int TIME_MIN_TO_STAY = 5;
        private const int TIME_MAX_TO_STAY = 20;
        private const int TIME_SECOND_BEFORE_ADD_CUSTOMER = 5;

        //Champs
        Bitmap bmp = null;
        Graphics g = null;
        Timer fps;
        Stopwatch newClient;
        Random rnd;
        List<Customer> customers;
        List<Customer> customersWaitingForAnotherCheckout;
        List<Checkout> checkouts;

        /// <summary>
        /// Constructeur
        /// </summary>
        public Magasin() : base() {
            customers = new List<Customer>();
            customersWaitingForAnotherCheckout = new List<Customer>();
            checkouts = new List<Checkout>();
            //Random
            rnd = new Random();
            //Stopwatch
            newClient = new Stopwatch();
            newClient.Start();
            //Timer
            fps = new Timer();
            fps.Interval = FPS_INTERVAL;
            fps.Enabled = true;
            fps.Tick += new EventHandler(OnTick);
            //Pour la fluidité de l'image
            DoubleBuffered = true;

            //Clients
            for (int i = 0; i < NB_CUSTOMER; i++) {
                AddCustomer();
            }
            //Caisses
            //Position Y des caisses
            int checkoutY = Properties.Settings.Default.SIZE_FORM.Height - Properties.Settings.Default.SIZE_CHECKOUT_CUSTOMER.Height;
            //Ajouter autant de caisse que l'espace disponible permet
            for (int i = 0; i < (Properties.Settings.Default.SIZE_FORM.Width - Properties.Settings.Default.SIZE_CHECKOUT_CUSTOMER.Width) / Properties.Settings.Default.SIZE_CHECKOUT_CUSTOMER.Width; i++) {
                Checkout currentCheckout = new Checkout(
                    new PointF(
                        10 + (Properties.Settings.Default.SIZE_CHECKOUT_CUSTOMER.Width * i) + (SPACE_BETWEEN_CHECKOUT * i), 
                        checkoutY
                    ),
                    //Ouvrir la 1ère caisse
                    i == 0
                );
                //Affichage
                Paint += currentCheckout.Paint;
                //Handler des events
                currentCheckout.CustomerDoneAtCheckout += HandlerCustomerDoneAtCheckout;
                currentCheckout.CheckoutFull += HandlerCheckoutFull;
                currentCheckout.CheckoutAvailable += HandlerCheckoutAvailable;
                //L'ajouter à la liste des caisses
                checkouts.Add(currentCheckout);
            }
        }

        /// <summary>
        /// Ajouter un client
        /// </summary>
        private void AddCustomer() {
            Customer currentCustomer = new Customer(
                //Position de départ centrée
                new PointF(Properties.Settings.Default.SIZE_FORM.Width / 2, Properties.Settings.Default.SIZE_FORM.Height / 2),
                //Vitesse aléatoire
                new PointF(rnd.Next(-V_CUSTOMER, V_CUSTOMER), rnd.Next(-V_CUSTOMER, V_CUSTOMER)),
                //Temps de shopping
                rnd.Next(TIME_MIN_TO_STAY, TIME_MAX_TO_STAY)
            );
            //Affichage
            Paint += currentCustomer.Paint;
            //Handler de l'event
            currentCustomer.TimeEnded += HandlerTimeEnded;
            //L'ajouter à la liste des clients
            customers.Add(currentCustomer);
        }

        /// <summary>
        /// Évènement appelé lors du dessinage de la Scene
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e) {
            //Initialiser les variables si elles sont nulles (notation C# 8.0, https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-8#null-coalescing-assignment)
            bmp ??= new Bitmap(Size.Width, Size.Height);
            g ??= Graphics.FromImage(bmp);
            //Pour la netteté de l'image
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            PaintEventArgs p = new PaintEventArgs(g, e.ClipRectangle);

            //Nettoyer l'écran
            g.Clear(BackColor);
            //Appeler l'event de base
            base.OnPaint(p);
            //Dessiner la nouvelle image sur l'écran
            e.Graphics.DrawImage(bmp, new Point(0, 0));
        }

        /// <summary>
        /// Pour chaque Tick du timer
        /// </summary>
        protected void OnTick(object sender, EventArgs e) {
            Invalidate(true);
            //Si le temps avant d'ajouter un client est écoulé
            if(newClient.Elapsed.TotalSeconds / TIME_SECOND_BEFORE_ADD_CUSTOMER > 1) {
                AddCustomer();
                newClient.Restart();
            }
        }

        /// <summary>
        /// Le client n'a pas pu être ajouté à une caisse
        /// </summary>
        /// <param name="currentCustomer">Le client en question</param>
        private void CheckoutFull(Customer currentCustomer) {
            currentCustomer.CheckoutFull();
            //Ajouter le client à la liste des clients en attente d'une autre caisse ouverte
            if (!customersWaitingForAnotherCheckout.Contains(currentCustomer)) {
                customersWaitingForAnotherCheckout.Add(currentCustomer);
            }
        }

        /// <summary>
        /// Event lorsque le client a fini ses courses
        /// </summary>
        /// <param name="e">Argument</param>
        private void HandlerTimeEnded(object sender, EventArgs e) {
            //Récupérer le client
            Customer currentCustomer = sender as Customer;
            //Récupérer une caisse ouverte
            Checkout currentCheckout = checkouts.Find(checkout => checkout.IsOpen && !checkout.IsAtMax);
            //Pas de caisse disponible
            if(currentCheckout == null) {
                CheckoutFull(currentCustomer);
            } else {
                //Faire aller le client vers la caisse ouverte
                currentCustomer.GoTo(currentCheckout);
            }
        }

        /// <summary>
        /// Event lorsque le client a fini ses courses
        /// </summary>
        /// <param name="e">Argument</param>
        private void HandlerCustomerDoneAtCheckout(object sender, CustomerDoneAtCheckoutEventArgs e) {
            Paint -= e.customer.Paint;
            customers.Remove(e.customer);
        }

        /// <summary>
        /// Event lorsque la caisse est pleine
        /// </summary>
        /// <param name="e">Argument</param>
        private void HandlerCheckoutFull(object sender, EventArgs e) {
            Checkout currentCheckout = sender as Checkout;

            //Récupérer les clients qui se dirigent vers cette caisse
            List<Customer> customersGoingToThatCheckout = customers.FindAll(customer => customer.CheckoutToGo == currentCheckout && customer.StatusCustomer == Status.GOING_TO_CHECKOUT);

            //Parcourir les clients récupérés
            foreach (Customer currentCustomer in customersGoingToThatCheckout) {
                CheckoutFull(currentCustomer);
            }
        }

        /// <summary>
        /// Invocation d'event lorsque une/des place(s) dans la file d'attente d'une caisse est/sont disponible(s)
        /// </summary>
        /// <param name="e">Argument</param>
        private void HandlerCheckoutAvailable(object sender, EventArgs e) {
            Checkout currentCheckout = sender as Checkout;

            foreach (Customer currentCustomer in customersWaitingForAnotherCheckout) {
                //Faire aller le client vers la caisse ouverte
                currentCustomer.GoTo(currentCheckout);
            }
        }
    }
}

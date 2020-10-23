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
        //Clients, caisses
        private const int SPACE_BETWEEN_CHECKOUT = 15;
        private const int NB_CHECKOUT_OPEN_AT_START = 2;
        private const int NB_CUSTOMER_AT_START = 30;
        private const int V_CUSTOMER = 200;
        //Durées
        private const int TIME_MIN_TO_STAY = 5;
        private const int TIME_MAX_TO_STAY = 20;
        private const int TIME_SECOND_BEFORE_ADD_CUSTOMER = 5;
        private const int TIME_SECOND_WAITING_WITHOUT_CHECKOUT_TOO_LONG = 5;
        private const int TIME_SECOND_CHECKOUT_OPEN_WITHOUT_CUSTOMER = 5;

        //Champs
        Bitmap bmp = null;
        Graphics g = null;
        Timer fps;
        Stopwatch newClient;
        Stopwatch waitingWithoutCheckoutOpen;
        Stopwatch checkoutOpenWithoutCustomer;
        Random rnd;
        List<Customer> customers;
        List<Checkout> checkouts;

        //Propriétés calculées
        private bool AreCheckoutOpenWithoutCustomer {
            get {
                return checkouts.Find(checkout => checkout.IsOpen && checkout.IsEmpty) != null;
            }
        }
        private bool AreCustomersShoppingOrWaiting {
            get {
                return customers.Find(customer => customer.StatusCustomer == Status.GOING_TO_CHECKOUT || customer.StatusCustomer == Status.WAITING_FOR_ANOTHER_CHECKOUT) != null;
            }
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        public Magasin() : base() {
            //Clients, caisses
            customers = new List<Customer>();
            checkouts = new List<Checkout>();
            //Random
            rnd = new Random();
            //Stopwatch
            newClient = new Stopwatch();
            newClient.Start();
            waitingWithoutCheckoutOpen = new Stopwatch();
            checkoutOpenWithoutCustomer = new Stopwatch();
            //Timer
            fps = new Timer();
            fps.Interval = FPS_INTERVAL;
            fps.Enabled = true;
            fps.Tick += new EventHandler(OnTick);
            //Pour la fluidité de l'image
            DoubleBuffered = true;

            //Clients
            for (int i = 0; i < NB_CUSTOMER_AT_START; i++) {
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
                    //Ouvrir des caisses
                    i < NB_CHECKOUT_OPEN_AT_START
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
            //Handler des events
            currentCustomer.TimeEnded += HandlerTimeEnded;
            currentCustomer.CheckoutReached += HandlerCheckoutReached;
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
            //Si les clients sans caisse attendent depuis trop longtemps
            if(waitingWithoutCheckoutOpen.Elapsed.TotalSeconds >= TIME_SECOND_WAITING_WITHOUT_CHECKOUT_TOO_LONG) {
                //Ouvrir la prochaine caisse
                Checkout currentCheckout = checkouts.Find(checkout => !checkout.IsOpen);
                currentCheckout.IsOpen = true;
                //Indiquer qu'une nouvelle caisse est disponible
                CheckoutAvailable(currentCheckout);
                //Remettre le compteur à zéro
                waitingWithoutCheckoutOpen.Reset();
            }
            //S'il y a au moins une caisse ouverte sans client depuis trop longtemps
            if(checkoutOpenWithoutCustomer.Elapsed.TotalSeconds >= TIME_SECOND_CHECKOUT_OPEN_WITHOUT_CUSTOMER) {
                //Vérifier qu'il y a plus que 1 caisse actuellement ouverte (pour qu'il reste une caisse ouverte après fermeture de l'autre)
                if(checkouts.FindAll(checkout => checkout.IsOpen).Count > 1) {
                    //Fermer la dernière caisse vide
                    Checkout currentCheckout = checkouts.FindLast(checkout => checkout.IsOpen && checkout.IsEmpty);
                    if (currentCheckout != null) {
                        currentCheckout.IsOpen = false;
                        //Indiquer que la caisse n'est plus disponible
                        CheckoutUnavailable(currentCheckout);
                        //Remettre le compteur à zéro
                        checkoutOpenWithoutCustomer.Reset();
                    }
                }
            }
        }

        /// <summary>
        /// Faire aller le client vers une caisse
        /// </summary>
        /// <param name="currentCustomer">Le client</param>
        private void GoToCheckout(Customer currentCustomer) {
            //Récupérer une caisse ouverte et avec de la place
            Checkout currentCheckout = checkouts.Find(checkout => checkout.IsOpen && !checkout.IsAtMax);
            //Pas de caisse disponible
            if (currentCheckout == null) {
                currentCustomer.CheckoutFull();
                //Commencer à mesurer la durée avec des clients sans caisse
                waitingWithoutCheckoutOpen.Start();
            } else {
                //Faire aller le client vers la caisse ouverte
                currentCustomer.GoTo(currentCheckout);
            }
        }

        /// <summary>
        /// Indiquer qu'une caisse est disponible
        /// </summary>
        /// <param name="currentCheckout">La caisse disponible</param>
        private void CheckoutAvailable(Checkout currentCheckout) {
            //Récupérer les clients qui attendent une caisse
            List<Customer> customersWaitingForAnotherCheckout = customers.FindAll(customer => customer.StatusCustomer == Status.WAITING_FOR_ANOTHER_CHECKOUT);

            //Parcourir les clients récupérés
            foreach (Customer currentCustomer in customersWaitingForAnotherCheckout) {
                //Faire aller le client vers la caisse ouverte
                currentCustomer.GoTo(currentCheckout);
            }
        }

        /// <summary>
        /// Inidquer aux clients se dirigeant vers une caisse qu'elle est indisponible (pleine / fermée)
        /// </summary>
        /// <param name="currentCheckout">La caisse indisponible</param>
        private void CheckoutUnavailable(Checkout currentCheckout) {
            //Récupérer les clients qui se dirigent vers cette caisse
            List<Customer> customersGoingToThatCheckout = customers.FindAll(customer => customer.CheckoutToGo == currentCheckout && customer.StatusCustomer == Status.GOING_TO_CHECKOUT);

            //Parcourir les clients récupérés
            foreach (Customer currentCustomer in customersGoingToThatCheckout) {
                //Le faire aller vers une autre caisse
                GoToCheckout(currentCustomer);
            }
        }

        /// <summary>
        /// Event lorsque le client a fini ses courses
        /// </summary>
        /// <param name="e">Argument</param>
        private void HandlerTimeEnded(object sender, EventArgs e) {
            //Récupérer le client
            Customer currentCustomer = sender as Customer;
            //Le faire aller vers une caisse
            GoToCheckout(currentCustomer);
        }

        /// <summary>
        /// Event lorsque le client a atteint une caisse
        /// </summary>
        /// <param name="e">Argument</param>
        private void HandlerCheckoutReached(object sender, EventArgs e) {
            //S'il y a aucune caisse ouverte sans client
            if (!AreCheckoutOpenWithoutCustomer) {
                checkoutOpenWithoutCustomer.Stop();
            }
        }

        /// <summary>
        /// Event lorsque le client a fini ses courses
        /// </summary>
        /// <param name="e">Argument</param>
        private void HandlerCustomerDoneAtCheckout(object sender, CustomerDoneAtCheckoutEventArgs e) {
            Paint -= e.customer.Paint;
            customers.Remove(e.customer);

            //S'il y a au moins une caisse ouverte sans client, et pas de clients voulant une caisse
            if (AreCheckoutOpenWithoutCustomer && !AreCustomersShoppingOrWaiting) {
                checkoutOpenWithoutCustomer.Start();
            }
        }

        /// <summary>
        /// Event lorsque la caisse est pleine
        /// </summary>
        /// <param name="e">Argument</param>
        private void HandlerCheckoutFull(object sender, EventArgs e) {
            Checkout currentCheckout = sender as Checkout;

            //Indiquer que la caisse n'est plus disponible
            CheckoutUnavailable(currentCheckout);
        }

        /// <summary>
        /// Invocation d'event lorsque une/des place(s) dans la file d'attente d'une caisse est/sont disponible(s)
        /// </summary>
        /// <param name="e">Argument</param>
        private void HandlerCheckoutAvailable(object sender, EventArgs e) {
            //Stopper la mesure de temps
            waitingWithoutCheckoutOpen.Stop();

            Checkout currentCheckout = sender as Checkout;

            //Indiquer que la caisse est disponible
            CheckoutAvailable(currentCheckout);
        }
    }
}

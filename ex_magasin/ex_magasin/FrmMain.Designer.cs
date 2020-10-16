namespace ex_magasin {
    partial class FrmMain {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent() {
            this.magasin = new ex_magasin.Magasin();
            this.SuspendLayout();
            // 
            // magasin
            // 
            this.magasin.BackColor = System.Drawing.Color.White;
            this.magasin.Location = new System.Drawing.Point(0, 0);
            this.magasin.Name = "magasin";
            this.magasin.Size = global::ex_magasin.Properties.Settings.Default.SIZE_FORM;
            this.magasin.TabIndex = 0;
            this.magasin.Text = "scene1";
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = global::ex_magasin.Properties.Settings.Default.SIZE_FORM;
            this.Controls.Add(this.magasin);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Magasin";
            this.ResumeLayout(false);

        }

        #endregion

        private Magasin magasin;
    }
}


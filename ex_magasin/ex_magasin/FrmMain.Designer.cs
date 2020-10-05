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
            this.scene1 = new ex_magasin.Magasin();
            this.SuspendLayout();
            // 
            // scene1
            // 
            this.scene1.BackColor = System.Drawing.Color.White;
            this.scene1.Location = new System.Drawing.Point(0, 0);
            this.scene1.Name = "scene1";
            this.scene1.Size = global::ex_magasin.Properties.Settings.Default.SIZE_FORM;
            this.scene1.TabIndex = 0;
            this.scene1.Text = "scene1";
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = global::ex_magasin.Properties.Settings.Default.SIZE_FORM;
            this.Controls.Add(this.scene1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Magasin";
            this.ResumeLayout(false);

        }

        #endregion

        private Magasin scene1;
    }
}


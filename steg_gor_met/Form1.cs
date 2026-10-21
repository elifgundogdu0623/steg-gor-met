using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace steg_gor_met
{
    public partial class Form1 : Form
    {
        // Temel Değişkenler
        public Bitmap rsm = null;

        public Form1()
        {
            InitializeComponent();
        }

        // 1. RESİM YÜKLE
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog open_dialog = new OpenFileDialog();
            open_dialog.Filter = "Resim Dosyaları (*.png; *.jpg; *.bmp)|*.png; *.jpg; *.bmp";

            if (open_dialog.ShowDialog() == DialogResult.OK)
            {
                rsm = new Bitmap(open_dialog.FileName);
                pictureBox2.Image = rsm;

                // NOT: Eğer txtDosya ve txtBoyut kutularını sildiysen 
                // aşağıdaki iki satırın başına // koyarak iptal edebilirsin.
                // txtDosya.Text = open_dialog.FileName;
                // txtBoyut.Text = new FileInfo(open_dialog.FileName).Length.ToString() + " Byte";
            }
        }

        // 2. TEMİZLE
        private void button2_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
            txtMetin.Clear();
            txtbX.Clear();
            txtbY.Clear();
            pictureBox2.Image = null;
            pictureBox3.Image = null;
        }

        // 3. ">" BUTONU (Pikselleri Listele)
        private void button3_Click(object sender, EventArgs e)
        {
            if (rsm == null) return;

            // Koordinatları ve seçilen formatı (DEC, BIN, HEX) alıyoruz
            int basX = int.Parse(txtbX.Text);
            int basY = int.Parse(txtbY.Text);
            string format = cboDTuru.Text;

            dataGridView1.Rows.Clear();

            for (int i = 0; i < 8; i++) // 8 satır
            {
                for (int j = 0; j < 8; j++) // 8 sütun
                {
                    Color p = rsm.GetPixel(basX + j, basY + i);
                    string r, g, b;

                    // --- FORMAT KONTROLÜ (İşte hocanın istediği kısım) ---
                    if (format == "BIN")
                    {
                        // Sayıyı 2'lik sisteme çevir ve 8 haneye tamamla
                        r = Convert.ToString(p.R, 2).PadLeft(8, '0');
                        g = Convert.ToString(p.G, 2).PadLeft(8, '0');
                        b = Convert.ToString(p.B, 2).PadLeft(8, '0');
                    }
                    else if (format == "HEX")
                    {
                        // 16'lık sistem (On altılık)
                        r = p.R.ToString("X2");
                        g = p.G.ToString("X2");
                        b = p.B.ToString("X2");
                    }
                    else
                    {
                        // Varsayılan: DEC (Onluk sistem - 255 gibi)
                        r = p.R.ToString();
                        g = p.G.ToString();
                        b = p.B.ToString();
                    }

                    dataGridView1.Rows.Add(r, g, b);
                }
            }
        }

        // 4. OLUŞTUR (Mesajı Gizle)
        private void button4_Click(object sender, EventArgs e)
        {
            if (rsm == null) return;

            Bitmap sirliResim = new Bitmap(rsm);
            string mesaj = txtMetin.Text + "\0";
            byte[] mesajByte = Encoding.ASCII.GetBytes(mesaj);

            int mesajIdx = 0;
            int bitIdx = 0;

            for (int y = 0; y < sirliResim.Height; y++)
            {
                for (int x = 0; x < sirliResim.Width; x++)
                {
                    if (mesajIdx < mesajByte.Length)
                    {
                        Color p = sirliResim.GetPixel(x, y);
                        int bit = (mesajByte[mesajIdx] >> (7 - bitIdx)) & 1;

                        int yeniR = p.R;

                        // Mod 2 Mantığı (Tek-Çift)
                        if (bit == 1) { if (yeniR % 2 == 0) yeniR++; }
                        else { if (yeniR % 2 != 0) yeniR--; }

                        sirliResim.SetPixel(x, y, Color.FromArgb(yeniR, p.G, p.B));

                        bitIdx++;
                        if (bitIdx == 8) { bitIdx = 0; mesajIdx++; }
                    }
                }
            }
            pictureBox3.Image = sirliResim;

            // Alttaki tabloyu (Şifreli Pikseller) doldurma
            dataGridView2.Rows.Clear();
            int bX2 = int.Parse(txtbX.Text);
            int bY2 = int.Parse(txtbY.Text);
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Color pS = sirliResim.GetPixel(bX2 + j, bY2 + i);
                    dataGridView2.Rows.Add(pS.R.ToString(), pS.G.ToString(), pS.B.ToString());
                }
            }
            MessageBox.Show("Mesaj Gizlendi!");
        }

        // 5. RESMİ SAKLA
        private void button5_Click(object sender, EventArgs e)
        {
            if (pictureBox3.Image == null) return;

            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "PNG Image|*.png";
            if (save.ShowDialog() == DialogResult.OK)
            {
                pictureBox3.Image.Save(save.FileName, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        // 6. MESAJI ÇÖZ
        private void button6_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image == null) return;

            Bitmap oku = new Bitmap(pictureBox2.Image);
            string sonuc = "";
            byte kr = 0;
            int sayac = 0;

            for (int y = 0; y < oku.Height; y++)
            {
                for (int x = 0; x < oku.Width; x++)
                {
                    Color p = oku.GetPixel(x, y);
                    kr = (byte)((kr << 1) | (p.R % 2));
                    sayac++;

                    if (sayac == 8)
                    {
                        if (kr == 0) goto bitti;
                        sonuc += (char)kr;
                        kr = 0; sayac = 0;
                    }
                }
            }
        bitti: txtMetin.Text = sonuc;
        }

        // Boş kalan event handler (Sildiğin bir nesneye bağlı olabilir, hata vermesin diye dursun)
        private void cboDTuru_SelectedIndexChanged(object sender, EventArgs e) { }
    }
}
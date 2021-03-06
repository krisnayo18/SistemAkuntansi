﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ClassLibraryTransaksi;
using System.IO;

namespace ClassLibraryTransaksi
{
    public class NotaPenjualan
    {

        #region Data Member
        private string noNotaPenjualan, status, keterangan;
        private double diskon;
        private int totalHarga;
        private DateTime tglBatasPelunasan, tglBatasDiskon, tglJual;
        private Pelanggan pelanggan; //aggregation
        private List<DetilNotaJual> listNotaJualDetil; //composition

        #endregion

        #region Constructor 
        public NotaPenjualan(string noNotaPenjualan, string status, string keterangan, double diskon, int totalHarga, DateTime tglBatasPelunasan, DateTime tglBatasDiskon,  DateTime pTglJual, Pelanggan pelanggan)
        {
            this.noNotaPenjualan = noNotaPenjualan;
            this.status = status;
            this.keterangan = keterangan;
            this.diskon = diskon;
            this.totalHarga = totalHarga;
            this.tglBatasPelunasan = tglBatasPelunasan;
            this.tglBatasDiskon = tglBatasDiskon;
            this.tglJual = pTglJual;
            this.pelanggan = pelanggan;
            ListNotaJualDetil = new List<DetilNotaJual>();
        }
        public NotaPenjualan()
        {
            NoNotaPenjualan = "";
            Status = "";
            Keterangan = "";
            Diskon = 0;
            TotalHarga = 0;
            TglBatasPelunasan = DateTime.Now;
            TglBatasDiskon = DateTime.Now;
            TglJual = DateTime.Now;
            ListNotaJualDetil = new List<DetilNotaJual>();
            //pelangggan aggregation, tidak bole di new didalam class
        }
        #endregion

        #region Properties
        public string NoNotaPenjualan
        {
            get
            {
                return noNotaPenjualan;
            }

            set
            {
                noNotaPenjualan = value;
            }
        }

        public string Status
        {
            get
            {
                return status;
            }

            set
            {
                status = value;
            }
        }

        public string Keterangan
        {
            get
            {
                return keterangan;
            }

            set
            {
                keterangan = value;
            }
        }

        public double Diskon
        {
            get
            {
                return diskon;
            }

            set
            {
                diskon = value;
            }
        }

        public int TotalHarga
        {
            get
            {
                return totalHarga;
            }

            set
            {
                totalHarga = value;
            }
        }

        public DateTime TglBatasPelunasan
        {
            get
            {
                return tglBatasPelunasan;
            }

            set
            {
                tglBatasPelunasan = value;
            }
        }

        public DateTime TglBatasDiskon
        {
            get
            {
                return tglBatasDiskon;
            }

            set
            {
                tglBatasDiskon = value;
            }
        }


        public Pelanggan Pelanggan
        {
            get
            {
                return pelanggan;
            }

            set
            {
                pelanggan = value;
            }
        }

        public List<DetilNotaJual> ListNotaJualDetil
        {
            get
            {
                return listNotaJualDetil;
            }

           private set
            {
                listNotaJualDetil = value;
                //list nota jual detil composition jadi tidak boleh diset dari luar
            }
        }

        public DateTime TglJual
        {
            get
            {
                return tglJual;
            }

            set
            {
                tglJual = value;
            }
        }



        #endregion

        #region Method
        public void TambahDetilBarang(Barang pBarang, int pJumlah, int pHargaJual)
        {
            DetilNotaJual dnj = new DetilNotaJual(pBarang, pJumlah, pHargaJual);
            ListNotaJualDetil.Add(dnj);


        }
        public static string TambahData(NotaPenjualan pNotaJual)
        {
            using (var tranScope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                // perintah sql 1 = untuk menambahkan data ke tabel nota penjualan  
                string sql1 = "INSERT INTO notapenjualan(noNotaPenjualan, diskon,  totalHarga, tglBatasPelunasan, tglBatasDiskon, tglJual, status, keterangan, idPelanggan) VALUES ('" + 
                    pNotaJual.NoNotaPenjualan + "', " +
                    pNotaJual.Diskon + ", " + 
                    pNotaJual.TotalHarga + ", '" +
                    pNotaJual.TglBatasPelunasan.ToString("yyyy-MM-dd ") + "', '" +
                    pNotaJual.TglBatasDiskon.ToString("yyyy-MM-dd ") + "', '" +
                    pNotaJual.TglJual.ToString("yyyy-MM-dd ") + "', '" +
                    pNotaJual.Status + "', '"+
                    pNotaJual.Keterangan + "', " +
                    pNotaJual.Pelanggan.IdPelanggan + ")";

                try
                {
                    //jalankan perintah untuk menambahkan  ke tabel notapenjualan
                    Koneksi.JalankanPerintahDML(sql1);
                    //menambahkan semua barang yang terjual ke dalam detilnotajual
                    for (int i = 0; i < pNotaJual.ListNotaJualDetil.Count; i++)
                    {
                        //perintah sql2 = untuk menambahkan barang barang yang terjual ke tabel detilnotajual
                        string sql2 = "INSERT INTO detilnotajual(noNotaPenjualan, kodeBarang, jumlah, hargaJual) VALUES ('" + 
                            pNotaJual.NoNotaPenjualan + "', '" + 
                            pNotaJual.ListNotaJualDetil[i].Barang.KodeBarang + "', " + 
                            pNotaJual.ListNotaJualDetil[i].Jumlah + ", " + 
                            pNotaJual.ListNotaJualDetil[i].HargaJual + ")";

                        //menjalankan perintah untuk menambahkan  ke tabel notajualdetil
                        Koneksi.JalankanPerintahDML(sql2);

                        //panggil method untuk mengurang stok/quantity barang yang terjual
                        string hasilUpdateBrng = Barang.UbahStokTerjual(pNotaJual.ListNotaJualDetil[i].Barang.KodeBarang, pNotaJual.ListNotaJualDetil[i].Jumlah);
                    }
                    //jika semua perinth dml berhasil dijalankan
                    tranScope.Complete();
                    return "1";
                }
                catch (Exception e)
                {
                    //jika ada kegagalan perintah dml
                    tranScope.Dispose();
                    return e.Message;
                }
            }

        }
        public static string GenerateNoNota(out string pHasilNoNota)
        {
            //perintah sql = mendapatkan nourut nota terakhir ditanggal hari ini(tanggal komputer)
            string sql = "SELECT SUBSTRING(noNotaPenjualan, 9, 3) AS noUrutTransaksi " +
                         "FROM notaPenjualan WHERE Date(tglJual) = Date(CURRENT_DATE) " +
                         "ORDER BY noNotaPenjualan DESC LIMIT 1";
            pHasilNoNota = "";
            try
            {
                MySqlDataReader hasilData = Koneksi.JalankanPerintahQuery(sql);

                string noUrutTransTerbaru = "";
                //cek apakah sudah ada transaksi  pada tanggal  hari ini (data reader dari sql  diatas bisa terbca atau tidak )
                if (hasilData.Read())//jika berhasil membaca data (sudah ada transaksi pada hari ini)
                {
                    int noUrutTrans = int.Parse(hasilData.GetValue(0).ToString()) + 1; //dapatkan no urut transaksi terbaru 
                    noUrutTransTerbaru = noUrutTrans.ToString().PadLeft(3, '0'); // jika nourutTransaksi pada hari ini 
                }
                else //jika belum ada transaksi hari ini
                {
                    noUrutTransTerbaru = "001";
                }
                //generate nomor nota terbaru dengan format yyyymmddxxx (y tahun, m bulan, d hari , dan xxx no urut transaksi tgl tsb)
                string tahun = DateTime.Now.Year.ToString();//dapatkan tahun dari tanggal kompter
                string bulan = DateTime.Now.Month.ToString().PadLeft(2, '0');//dapatkan bulan
                string tanggal = DateTime.Now.Day.ToString().PadLeft(2, '0');//dapatkan hari

                //generate nomor nota terbaru sesuai format terbaru
                pHasilNoNota = tahun + bulan + tanggal + noUrutTransTerbaru.ToString();
                return "1";
            }
            catch (Exception e)
            {
                return e.Message;
            }

        }
        public static string BacaData(string kriteria, string nilaiKriteria, List<NotaPenjualan> listNotaJual)
        {
            string sql1 = "";

            if (kriteria == "")
            {
                //tuliskan perintah sql1 = untuk menampilkan semua data  ditabel notapenjualan 
                sql1 = "SELECT * FROM vnotapenjualan";
            }
            else
            {
                sql1 = "SELECT * FROM vnotapenjualan WHERE "   + kriteria + " LIKE '%" + nilaiKriteria + "%'";
            }

            try
            {
                //data reader 1 = memperoleh semua data di tabel notaPenjualan
                MySqlDataReader hasilData1 = Koneksi.JalankanPerintahQuery(sql1);
                listNotaJual.Clear();//kosongi isi list terlebih dahulu

                while (hasilData1.Read())
                {

                    //mendapatkan  nomornota, status ,dll
                    string nomorNota = hasilData1.GetValue(0).ToString();
                    double diskon = double.Parse(hasilData1.GetValue(4).ToString());
                    int totalHarga = int.Parse(hasilData1.GetValue(5).ToString());
                    DateTime tglBatasPelunasan = DateTime.Parse(hasilData1.GetValue(6).ToString());
                    DateTime tglBatasDiskon = DateTime.Parse(hasilData1.GetValue(7).ToString());
                    DateTime tglJual = DateTime.Parse(hasilData1.GetValue(8).ToString());
                    string status = hasilData1.GetValue(9).ToString();
                    string keterangan = hasilData1.GetValue(10).ToString();
                    
                    //pelanggan yang melakukan transaksi
                    //mendapatkan idpelanggan, nama, dan alamat 
                    int idPlg = int.Parse(hasilData1.GetValue(1).ToString());
                    string namaPlg = hasilData1.GetValue(2).ToString();
                    string alamatPlg = hasilData1.GetValue(3).ToString();

                    //buat object bertipe pelanggan 
                    Pelanggan plg = new Pelanggan();
                    //tambahkan 3 data dibawah
                    plg.IdPelanggan = idPlg;
                    plg.Nama = namaPlg;
                    plg.Alamat = alamatPlg;

                    //nota jual
                    //buat object notapenjualan dan tambahkan data
                    NotaPenjualan nota = new NotaPenjualan( nomorNota,  status,  keterangan,  diskon, totalHarga, 
                                                            tglBatasPelunasan,tglBatasDiskon, tglJual, plg);

                    //DETAIL nota jual
                    //query utk detail nota jual dati tiap nota jual
                    //sql2 untuk mendapatkan barang yang ada di nota  (dar tabel detilnotajual)
                    string sql2 =  "SELECT DNJ.kodeBarang, B.Nama, DNJ.Jumlah , DNJ.HargaJual FROM NotaPenjualan N INNER JOIN " +
                                   "detilNotaJual DNJ ON N.noNotaPenjualan = DNJ.noNotaPenjualan INNER JOIN Barang B ON " +
                                   "DNJ.KodeBarang = B.KodeBarang WHERE N.noNotaPenjualan = '" + nomorNota + "'";

                    //memperoleh semua data barang nota ditabel detilnotajual
                    MySqlDataReader hasilData2 = Koneksi.JalankanPerintahQuery(sql2);

                    while (hasilData2.Read())
                    {
                        //mendapatkan  kode dan nama barang yang terjual 
                        string kodeBrg = hasilData2.GetValue(0).ToString();
                        string namaBrg = hasilData2.GetValue(1).ToString();
                        //buat object barang dan tambahkan
                        Barang brg = new Barang();
                        brg.KodeBarang = kodeBrg;
                        brg.Nama = namaBrg;

                        //mendapatkan harga jual dan jumlah transaksi 
                        int hargaJual = int.Parse(hasilData2.GetValue(3).ToString());
                        int jumlah = int.Parse(hasilData2.GetValue(2).ToString());

                        //buat object bertipe detilnotajual dan tambahkan 
                        //ingat baik baik agar fk tidak duplicate
                        DetilNotaJual detilNota = new DetilNotaJual(brg, jumlah, hargaJual);

                        //simpan detil barang ke nota
                        nota.TambahDetilBarang(brg, jumlah, hargaJual);
                    }
                    //simpan ke list
                    listNotaJual.Add(nota);
                }
                return "1";
            }
            catch (MySqlException ex)
            {
                return ex.Message;
            }
        }

        public static  string BacaDataPelunasan(string pKriteria, string pNilaiKriteria, List<NotaPenjualan> listNotaJual)
        {
            string sql = "";
            if (pKriteria == "")
            {
                sql = "select * from notapenjualan where status ='P' ";


            }
            else
            {
                sql = " select * from notapenjualan where status ='P' and "
                      + pKriteria + " LIKE '%" +
                       pNilaiKriteria + "%'";

            }
            try
            {

                MySqlDataReader hasilData = Koneksi.JalankanPerintahQuery(sql);
                listNotaJual.Clear();//kosongi isi list terlebih dahulu

                while (hasilData.Read())
                {

                    string nomornota = hasilData.GetValue(0).ToString();
                    int nominal =int.Parse( hasilData.GetValue(2).ToString());
                    string status = hasilData.GetValue(6).ToString();
                    double disc = double.Parse(hasilData.GetValue(1).ToString());
                    DateTime btsDisc = DateTime.Parse(hasilData.GetValue(4).ToString());

                    NotaPenjualan nota = new NotaPenjualan();
                    nota.NoNotaPenjualan = nomornota;
                    nota.Status = status;
                    nota.TotalHarga = nominal;
                    nota.Diskon = disc;
                    nota.TglBatasDiskon = btsDisc;

                    listNotaJual.Add(nota);
                }
                    return "1";
            }
            catch(MySqlException ex)
            {
                return ex.Message;
            }
            
        }

        public static string CetakNota(string pKriteria, string pNilaiKriteria, string pNamaFile)
        {
            try
            {
                List<NotaPenjualan> listNotaJual = new List<NotaPenjualan>();

                string hasilBaca = NotaPenjualan.BacaData(pKriteria, pNilaiKriteria, listNotaJual);

                StreamWriter file = new StreamWriter(pNamaFile);

                for (int i = 0; i < listNotaJual.Count; i++)
                {
                    file.WriteLine("");
                    file.WriteLine("TOKO MAJU MAKMUR UNTUNG SELALU");
                    file.WriteLine("Jl. Raya Kalirungkut Surabaya");
                    file.WriteLine("Telp. (031) - 12345678");
                    file.WriteLine("=".PadRight(50, '='));

                    //Header
                    file.WriteLine("No.Nota : " + listNotaJual[i].NoNotaPenjualan);
                    file.WriteLine("Tanggal : " + listNotaJual[i].TglJual);
                    file.WriteLine("");
                    file.WriteLine("Pelanggan : " + listNotaJual[i].Pelanggan.Nama);
                    file.WriteLine("Alamat   : " + listNotaJual[i].Pelanggan.Alamat);
                    file.WriteLine("");
                    //file.WriteLine("Pegawai Pembelian : " + listNotaBeli[i]..Nama);
                    file.WriteLine("=".PadRight(50, '='));

                    //Tampilkan barang yg terjual
                    long grandTotal = 0;
                    for (int j = 0; j < listNotaJual[i].ListNotaJualDetil.Count; j++)
                    {
                        string nama = listNotaJual[i].ListNotaJualDetil[j].Barang.Nama;

                        if (nama.Length > 30)
                        {
                            nama = nama.Substring(0, 30);
                        }

                        int jmlh = listNotaJual[i].ListNotaJualDetil[j].Jumlah;
                        long harga = listNotaJual[i].ListNotaJualDetil[j].HargaJual;
                        long subTotal = jmlh * harga;

                        file.Write(nama.PadRight(30, ' '));
                        file.Write(jmlh.ToString().PadRight(3, ' '));
                        file.Write(harga.ToString("0,###").PadLeft(7, ' '));
                        file.Write(subTotal.ToString("0,###").PadLeft(10, ' '));
                        file.WriteLine("");
                        //hitung grand total
                        grandTotal = grandTotal + (jmlh * harga);
                    }
                    file.WriteLine("=".PadRight(50, '='));
                    file.WriteLine("TOTAL : " + grandTotal.ToString("0,###"));
                    file.WriteLine("=".PadRight(50, '='));
                    file.WriteLine("");
                }
                file.Close();

                //cetak ke printer
                Cetak c = new Cetak(pNamaFile, "Courier New", 9, 10, 10, 10, 10);
                c.CetakKePrinter("tulisan");
                return "1";
            }
            catch (MySqlException e)
            {
                return e.Message;
            }
        }

        #endregion
    }
}

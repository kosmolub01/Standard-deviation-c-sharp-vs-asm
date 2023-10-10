namespace OdchylenieStandardowe
{
    internal class DataGridItem
    {
        public DataGridItem(int lp, string tryb, double czas, float wynik, string plik)
        {
            Lp = lp;
            Tryb = tryb;
            Czas = czas;
            Wynik = wynik;
            Plik = plik;
        }

        public int Lp { get; set; }
        public string Tryb { get; set; }
        public double Czas { get; set; }

        public float Wynik { get; set; }
        public string Plik { get; set; }
    }
}
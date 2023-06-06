using System.Globalization;
using System.Text;

string file = "C:\\Meine\\C#\\SearchInThisArea\\SearchInThisArea\\ukraine_poi.csv";
string[] dic = File.ReadAllLines(file);

List<Leaf> leafs = new List<Leaf>();
foreach (string point in dic)
{
leafs.Add(new Leaf(point));
}
Console.WriteLine("input longitude:");
string longitude = Console.ReadLine()!;
Console.WriteLine("Input latitude:");
string latitude = Console.ReadLine()!;
Console.WriteLine("Input Radius:");
double R = double.Parse(Console.ReadLine()!);
Leaf userPoint = new Leaf(longitude, latitude);
foreach (Leaf _leaf in leafs)
{
if (_leaf.IsInCircle(userPoint, R))
{
Console.WriteLine(_leaf);
}
}

class Leaf
{
    public double longitude;
    public double latitude;
    private string description;

    public Leaf(string str)
    {
        string[] loc = str.Split(';');
        double.TryParse(loc[0], out longitude);
        double.TryParse(loc[1], out latitude);
        description = "";
        for (int i = loc.Length - 1; i > 1; i--)
            description += loc[i];
    }

    public Leaf(string longitude, string latitude)
    {
        double.TryParse(longitude, out this.longitude);
        double.TryParse(latitude, out this.latitude);
        description = "";
    }

    public static double Distance(Leaf l1, Leaf l2)
    {
        double earthRadius = 6371;
        double deltaLatitude = DegreeToRadian(l1.latitude - l2.latitude);
        double deltaLongitude = DegreeToRadian(l1.longitude - l2.longitude);

        double a = Math.Sin(deltaLatitude / 2) * Math.Sin(deltaLatitude / 2) +
                   Math.Cos(DegreeToRadian(l2.latitude)) * Math.Cos(DegreeToRadian(l1.latitude)) *
                   Math.Sin(deltaLongitude / 2) * Math.Sin(deltaLongitude / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        double distance = earthRadius * c;
        return distance;
    }
    private static double DegreeToRadian(double partsLatitude)
    {
        return partsLatitude * 180 / Math.PI;
    }

    public bool IsInCircle(Leaf lc, double R)
    {
        return Distance(this, lc) <= R;
    }

    public override string ToString()
    {
        return $"{longitude}" + ';' + $" {latitude}" + ';' + description;
    }

}

class Rectangle
{
    private Leaf LBC;
    private Leaf RTC;
    private Rectangle LCh;
    private Rectangle RCh;
    private bool div;
    private List<Leaf> Points;

    public static double MedianDL(List<Leaf> sample, bool dl)
    {
        int dim = sample.Count;
        int i = 0;
        double[] dovg = new double[dim];
        foreach (var leaf in sample)
        {
            dovg[i] = (dl) ? leaf.longitude : leaf.latitude;
        }
        Array.Sort(dovg);
        return (dim % 2 == 0) ? (dovg[dim / 2 - 1] + dovg[dim / 2]) / 2 : dovg[dim / 2 - 1];
    }

    public Rectangle(Leaf LBC, Leaf RTC, bool div)
    {
        this.LBC = LBC;
        this.RTC = RTC;
        this.div = div;
        double LBCLlong = (div) ? LBC.longitude : MedianDL(Points, !div);
        double LBCLlatit = (!div) ? LBC.latitude : MedianDL(Points, div);
        double LBCRlong = (div) ? MedianDL(Points, !div) : RTC.longitude;
        double LBCRlatit = (!div) ? MedianDL(Points, div) : RTC.latitude;

        Leaf LBCL = new Leaf(LBCLlong.ToString(), LBCLlong.ToString());
        Leaf LBCR = new Leaf(LBCRlong.ToString(), LBCLlatit.ToString());

    }
}

using System.Diagnostics;
using System.Globalization;

Console.WriteLine("Input latitude:");
double userLatitude = Convert.ToDouble(Console.ReadLine(), CultureInfo.InvariantCulture);
Console.WriteLine("Input longitude:");
double userLongitude = Convert.ToDouble(Console.ReadLine(), CultureInfo.InvariantCulture);
Console.WriteLine("Input radius:");
double radius = Convert.ToDouble(Console.ReadLine(), CultureInfo.InvariantCulture);

Stopwatch swBuild = new();
Stopwatch swSearch = new();
swBuild.Start();
RTree tree = new();
tree.BuildTree();
swBuild.Stop();

swSearch.Start();
List<Point> result = tree.SearchInThisArea(userLatitude, userLongitude, radius);
Console.OutputEncoding = System.Text.Encoding.UTF8;
for (int i = 0; i < result.Count; i++)
{
    Console.WriteLine($"Latitude: {result[i].latitude}, longitude: {result[i].longitude}, {result[i].type1}, {result[i].type2}, {result[i].name1}, {result[i].name2}");
}
swSearch.Stop();
Console.WriteLine($"Building elapsed time: {swBuild.Elapsed}");
Console.WriteLine($"Searching elapsed time: {swSearch.Elapsed}");
class Leaf
{
    public double latitude;
    public double longitude;
    public Leaf(double latitude, double longitude)
    {
        this.latitude = latitude;
        this.longitude = longitude;
    }
}
class RTree
{
    private RectNode rootNode;

    public void BuildTree()
    {
        string fileName = "C:\\Meine\\C#\\SearchInThisArea\\SearchInThisArea\\ukraine_poi.csv";
        string[] lines = File.ReadAllLines(fileName);
        double minLatitude = double.MaxValue;
        double maxLatitude = double.MinValue;
        double minLongitude = double.MaxValue;
        double maxLongitude = double.MinValue;

        for (int i = 0; i < lines.Length; i++)
        {
            string[] data = lines[i].Split(';');
            if (data[0] == "" || data[1] == "")
            {
                continue;
            }
            double latitude = Convert.ToDouble(data[0], CultureInfo.InvariantCulture);
            double longitude = Convert.ToDouble(data[1], CultureInfo.InvariantCulture);

            minLatitude = latitude < minLatitude ? latitude : minLatitude;
            maxLatitude = latitude > maxLatitude ? latitude : maxLatitude;
            minLongitude = longitude < minLongitude ? longitude : minLongitude;
            maxLongitude = longitude > maxLongitude ? longitude : maxLongitude;
        }

        rootNode = new RectNode(minLatitude, maxLatitude, minLongitude, maxLongitude);

        for (int i = 0; i < lines.Length; i++)
        {
            string[] data = lines[i].Split(';');
            if (data[0] == "" || data[1] == "")
            {
                continue;
            }
            double latitude = Convert.ToDouble(data[0], CultureInfo.InvariantCulture);
            double longitude = Convert.ToDouble(data[1], CultureInfo.InvariantCulture);
            string type1 = data[2];
            string type2 = data[3];
            string name1 = data[4];
            string name2 = data[5];

            rootNode.Add(new Point(latitude, longitude, type1, type2, name1, name2));
        }

        rootNode.RectangleSplit();
    }

    public List<Point> SearchInThisArea(double latitude, double longitude, double radius)
    {
        RectNode rect = new RectNode(latitude - radius, latitude + radius, longitude - radius, longitude + radius);
        List<Point> result = new List<Point>();
        SearchInNode(rootNode, rect, result, radius, latitude, longitude);
        return result;
    }

    private void SearchInNode(RectNode currentNode, RectNode searchingNode, List<Point> result, double radius, double x, double y)
    {
        if (!Overlap(currentNode, searchingNode))
        {
            return;
        }
        if ((currentNode.rightChild == null) && (currentNode.leftChild == null))
        {
            for (int i = 0; i < currentNode.points.Count; i++)
            {
                if (currentNode.points[i].Distance(x, y) <= radius)
                {
                    result.Add(currentNode.points[i]);
                }
            }
            return;
        }
        SearchInNode(currentNode.leftChild, searchingNode, result, radius, x, y);
        SearchInNode(currentNode.rightChild, searchingNode, result, radius, x, y);
    }

    private bool Overlap(RectNode first, RectNode second)
    {
        Leaf leftTop1 = new Leaf(first.minLatitude, first.maxLongitude);
        Leaf rightBottom1 = new Leaf(first.maxLatitude, first.minLongitude);
        Leaf leftTop2 = new Leaf(second.minLatitude, second.maxLongitude);
        Leaf rightBottom2 = new Leaf(second.maxLatitude, second.minLongitude);

        if (rightBottom1.longitude > leftTop2.longitude || rightBottom2.longitude > leftTop1.longitude)
        {
            return false;
        }
        else if (leftTop1.latitude > rightBottom2.latitude || leftTop2.latitude > rightBottom1.latitude)
        {
            return false;
        }
        return true;
    }
}

class Point
{
    public double latitude;
    public double longitude;
    public string type1;
    public string type2;
    public string name1;
    public string name2;

    public Point(double latitude, double longitude, string type1, string type2, string name1, string name2)
    {
        this.latitude = latitude;
        this.longitude = longitude;
        this.type1 = type1;
        this.type2 = type2;
        this.name1 = name1;
        this.name2 = name2;
    }

    public double Distance(double latitude, double longitude)
    {
        return HaversineDistance(this.latitude, this.longitude, latitude, longitude);
    }

    private static double HaversineDistance(double latitude1, double longitude1, double latitude2, double longitude2)
    {
        double earthRadius = 6371;
        double radianLatitude = ToRadians(latitude2 - latitude1);
        double radianLongitude = ToRadians(longitude2 - longitude1);
        latitude1 = ToRadians(latitude1);
        latitude2 = ToRadians(latitude2);

        double firstCalc = Math.Sin(radianLatitude / 2) * Math.Sin(radianLatitude / 2) + Math.Sin(radianLongitude / 2) * Math.Sin(radianLongitude / 2) * Math.Cos(latitude1) * Math.Cos(latitude2);
        double result = 2 * earthRadius * Math.Asin(Math.Sqrt(firstCalc));
        return result;
    }

    private static double ToRadians(double angle)
    {
        return Math.PI * angle / 180.0;
    }
}

class RectNode
{
    public double minLatitude;
    public double maxLatitude;
    public double minLongitude;
    public double maxLongitude;
    public List<Point> points;
    public RectNode leftChild;
    public RectNode rightChild;

    public RectNode(double minLatitude, double maxLatitude, double minLongitude, double maxLongitude)
    {
        this.minLatitude = minLatitude;
        this.maxLatitude = maxLatitude;
        this.minLongitude = minLongitude;
        this.maxLongitude = maxLongitude;
        points = new List<Point>();
    }

    public void Add(Point p)
    {
        points.Add(p);
    }

    private double MedianDL(List<Point> points, Func<Point, double> selector)
    {
        List<double> values = new List<double>();
        for (int i = 0; i < points.Count; i++)
        {
            values.Add(selector(points[i]));
        }

        values.Sort();
        int count = values.Count;
        if (count % 2 == 0)
        {
            int midIndex1 = count / 2 - 1;
            int midIndex2 = count / 2;
            return (values[midIndex1] + values[midIndex2]) / 2;
        }
        else
        {
            int midIndex = count / 2;
            return values[midIndex];
        }
    }

    public void RectangleSplit()
    {
        if (points.Count <= 100)
        {
            return;
        }

        RectNode left;
        RectNode right;
        if (maxLatitude - minLatitude >= maxLongitude - minLongitude)
        {
            (left, right) = LatitudeSplit();
        }
        else
        {
            (left, right) = LongitudeSplit();
        }
        leftChild = left;
        rightChild = right;
        leftChild.RectangleSplit();
        rightChild.RectangleSplit();
        points = null;
    }

    private (RectNode left, RectNode right) LatitudeSplit()
    {
        double medianLatitude = MedianDL(points, point => point.latitude);
        var left = new RectNode(minLatitude, medianLatitude, minLongitude, maxLongitude);
        var right = new RectNode(medianLatitude, maxLatitude, minLongitude, maxLongitude);

        for (int i = 0; i < points.Count; i++)
        {
            if (points[i].latitude <= medianLatitude)
            {
                left.Add(points[i]);
            }
            else
            {
                right.Add(points[i]);
            }
        }
        return (left, right);
    }

    private (RectNode left, RectNode right) LongitudeSplit()
    {
        double medianLongitude = MedianDL(points, point => point.longitude);
        var left = new RectNode(minLatitude, maxLatitude, minLongitude, medianLongitude);
        var right = new RectNode(minLatitude, maxLatitude, medianLongitude, maxLongitude);
        for (int i = 0; i < points.Count; i++)
        {
            if (points[i].longitude <= medianLongitude)
            {
                left.Add(points[i]);
            }
            else
            {
                right.Add(points[i]);
            }
        }
        return (left, right);
    }
}

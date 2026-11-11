namespace LabShop;

// Prints the LabShop daily report: dotnet run --project LabShop -- ../data/orders.csv
public static class Report
{
    private const string Header = "order  customer     subtotal  discount       tax  shipping       total";

    public static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("usage: dotnet run --project LabShop -- <orders.csv>");
            return 2;
        }
        var orders = ReadOrders(args[0]);
        Console.WriteLine("LabShop daily report");
        Console.WriteLine(Header);
        var invalid = 0;
        foreach (var order in orders)
        {
            var bill = Pricing.PriceOrder(order);
            Console.WriteLine($"{order.Id,5}  {order.Customer,-10} {bill.Subtotal,10} {bill.Discount,9} " +
                              $"{bill.Tax,9} {bill.Shipping,9} {bill.Total,11}");
            if (order.CouponCode != "" && Pricing.FindCoupon(order.CouponCode.Trim().ToUpper()) == null)
            {
                invalid++;
            }
        }
        Console.WriteLine($"orders: {orders.Count}");
        Console.WriteLine($"invalid coupons: {invalid}");
        Console.WriteLine($"grand total: {GrandTotal(orders)}");
        return 0;
    }

    public static long GrandTotal(List<Order> orders)
    {
        long sum = 0;
        foreach (var order in orders)
        {
            sum += Pricing.PriceOrder(order).Total;
        }
        return sum;
    }

    public static List<Order> ReadOrders(string path)
    {
        var orders = new List<Order>();
        foreach (var line in File.ReadLines(path).Skip(1))
        {
            if (line == "")
            {
                continue;
            }
            var fields = line.Split(',');
            var order = new Order(int.Parse(fields[0]), fields[1], fields[2]);
            foreach (var part in fields[3].Split('|'))
            {
                var p = part.Split(':');
                order.Items.Add(new Item(p[0], long.Parse(p[1]), long.Parse(p[2])));
            }
            orders.Add(order);
        }
        return orders;
    }
}

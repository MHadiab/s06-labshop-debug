namespace LabShop.Tests;

public class PricingTests
{
    private static Order MakeOrder(string couponCode, params Item[] items)
    {
        var order = new Order(1, "test", couponCode);
        order.Items.AddRange(items);
        return order;
    }

    [Fact]
    public void Order_without_coupon()
    {
        var bill = Pricing.PriceOrder(MakeOrder("", new Item("A01", 100_000, 2), new Item("B01", 50_000, 1)));
        Assert.Equal(new Bill(250_000, 0, 22_500, 40_000, 312_500), bill);
    }

    [Fact]
    public void Fixed_coupon()
    {
        var bill = Pricing.PriceOrder(MakeOrder("FIX50", new Item("A01", 300_000, 1)));
        Assert.Equal(50_000, bill.Discount);
        Assert.Equal(312_500, bill.Total);
    }

    [Fact]
    public void Percent_coupon()
    {
        var bill = Pricing.PriceOrder(MakeOrder("OFF10", new Item("A01", 200_000, 1)));
        Assert.Equal(20_000, bill.Discount);
        Assert.Equal(236_200, bill.Total);
    }

    [Fact]
    public void Free_shipping_from_one_million()
    {
        Assert.Equal(40_000, Pricing.ShippingFor(999_990));
        Assert.Equal(0, Pricing.ShippingFor(1_000_000));
    }
    
    [Fact]
    public void Bulk_discount_combines_same_sku_quantities()
    {
        var order = MakeOrder("",
            new Item("K07", 38_500, 6),
            new Item("M21", 12_990, 1),
            new Item("K07", 38_500, 6));
        var bill = Pricing.PriceOrder(order);
        Assert.Equal(46_200, bill.Discount);
    }
}

using System.Runtime.InteropServices;

namespace LabShop;

// Pricing rules for LabShop orders. All amounts are in toman.
public static class Pricing
{
    public const long BulkMinQty = 10;
    public const long BulkPercent = 10;
    public const long TaxPercent = 9;
    public const long ShippingFee = 40_000;
    public const long FreeShippingFrom = 1_000_000;

    private static readonly Dictionary<string, Coupon> Coupons = new()
    {
        ["OFF10"] = new Coupon("percent", 10),
        ["OFF20"] = new Coupon("percent", 20),
        ["FIX50"] = new Coupon("fixed", 50_000),
    };

    public static Coupon FindCoupon(string code) => Coupons.GetValueOrDefault(code);

    public static Bill PriceOrder(Order order)
    {
        long subtotal = 0;
        foreach (var item in order.Items)
        {
            subtotal += item.UnitPrice * item.Qty;
        }
        var bulk = BulkDiscount(order.Items);
        var coupon = CouponDiscount(order, subtotal - bulk);
        var taxable = subtotal - bulk - coupon;
        var tax = TaxFor(taxable);
        var shipping = ShippingFor(taxable);
        return new Bill(subtotal, bulk + coupon, tax, shipping, taxable + tax + shipping);
    }

    // 10% off every line that has BulkMinQty units or more
    public static long BulkDiscount(List<Item> items)
    {
        long discount = 0;
        var quantitiesBySku = items
            .GroupBy(item => item.Sku)
            .ToDictionary(group => group.Key, group => group.Sum(item => item.Qty));

        foreach (var item in items)
        {
            if (quantitiesBySku[item.Sku] >= BulkMinQty)
            {
                discount += item.UnitPrice * item.Qty * BulkPercent / 100;
            }
        }
        return discount;
    }

    public static long CouponDiscount(Order order, long amount)
    {
        if (order.CouponCode == "")
        {
            return 0;
        }
        var cop = order.CouponCode.ToUpper().Trim();   
        
        var coupon = FindCoupon(cop) ;
        if (coupon == null)
        {
            coupon = new Coupon("fixed", 0);
        }
        
        if (coupon.Kind == "fixed")
        {
            return Math.Min(coupon.Value, amount);
        }
        return ApplyPercent(order.Items, coupon.Value);
    }

    // lower each unit price so the invoice shows the discounted prices
    private static long ApplyPercent(List<Item> items, long percent)
    {
        long discount = 0;
        foreach (var item in items)
        {
            var cut = item.UnitPrice * percent / 100;
            item.UnitPrice -= cut;
            discount += cut * item.Qty;
        }
        return discount;
    }

    // VAT, rounded to the nearest toman
    public static long TaxFor(long amount) => (amount * TaxPercent + 50) / 100;

    public static long ShippingFor(long amount)
    {
        if (amount >= FreeShippingFrom)
        {
            return 0;
        }
        return ShippingFee;
    }
}

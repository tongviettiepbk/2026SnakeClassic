using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Purchasing;

public class ProductIAP
{
    public string productId;
    public ProductType productType;

    public ProductIAP(string id, ProductType type)
    {
        this.productId = id;
        this.productType = type;
    }
}
using UnityEngine;
using BloodLuck;

public class ShopOpen : MonoBehaviour
{
    [SerializeField] private BloodLuckShop shop;

    void Start()
    {
        shop.OpenShop();
    }
}
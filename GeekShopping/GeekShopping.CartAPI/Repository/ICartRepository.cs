using GeekShopping.CartAPI.Data.ValueObjects;

namespace GeekShopping.CartAPI.Repository
{
    public interface ICartRepository
    {
        Task<CartVO> FindCartByUserID(string userID);
        Task<CartVO> SaveOrUpdateCart(CartVO cart);
        Task<bool> RemoveFromCart(long cartDetailsID);
        Task<bool> ApplyCoupon(string userID, string couponCode);
        Task<bool> RemoveCoupon(string userID);
        Task<bool> ClearCart(string userID);
    }
}

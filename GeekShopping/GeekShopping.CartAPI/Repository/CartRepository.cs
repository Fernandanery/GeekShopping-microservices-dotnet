using AutoMapper;
using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Model;
using GeekShopping.CartAPI.Model.Context;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.CartAPI.Repository
{
    public class CartRepository : ICartRepository
    {
        private readonly MySQLContext _context;
        private IMapper _mapper;

        public CartRepository(MySQLContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<bool> ApplyCoupon(string userID, string couponCode)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ClearCart(string userID)
        {
            var cartHeader = await _context.CartHeaders.FirstOrDefaultAsync(c => c.UserId == userID);

            if (cartHeader != null)
            {
                _context.CartDetails.RemoveRange(_context.CartDetails.Where(c => c.CartHeaderId == cartHeader.Id));

                _context.CartHeaders.Remove(cartHeader); 
                
                await _context.SaveChangesAsync();

                return true;

            }

            return false;

        }

        public async Task<CartVO> FindCartByUserID(string userID)
        {
            Cart cart = new()
            {
                CartHeader = await _context.CartHeaders.FirstOrDefaultAsync(c => c.UserId == userID),
            };
            cart.CartDetails = _context.CartDetails.Where(c => c.CartHeaderId == cart.CartHeader.Id)
                .Include(c => c.Product);

            return _mapper.Map<CartVO>(cart);
        }

        public async Task<bool> RemoveCoupon(string userID)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RemoveFromCart(long cartDetailsID)
        {
            try
            {
                CartDetail cartDetail = await _context.CartDetails.FirstOrDefaultAsync(c => c.Id == cartDetailsID);

                int total = _context.CartDetails.Where(c => c.CartHeaderId == cartDetail.CartHeaderId).Count();

                _context.CartDetails.Remove(cartDetail);

                if (total == 1)
                {
                    var cartHeaderToRemove = await _context.CartHeaders.FirstOrDefaultAsync(c => c.Id == cartDetail.CartHeaderId);

                    _context.CartHeaders.Remove(cartHeaderToRemove);

                }

                _context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public async Task<CartVO> SaveOrUpdateCart(CartVO vo)
        {
            Cart cart = _mapper.Map<Cart>(vo);

            //Anotação: Verificar se o produto já consta salvo na base de dados
            //Se não existir, salvar
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == vo.CartDetails.FirstOrDefault().ProductId);
            if (product != null)
            {
                _context.Products.Add(cart.CartDetails.FirstOrDefault().Product);
                await _context.SaveChangesAsync();
            }

            //Anotação: verificar se o cartHeader é null, se for, adicionar
            var cartHeader = await _context.CartHeaders.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == cart.CartHeader.UserId);

            if (cartHeader == null) 
            {
                _context.CartHeaders.Add(cart.CartHeader);
                await _context.SaveChangesAsync();

                cart.CartDetails.FirstOrDefault().CartHeaderId = cart.CartHeader.Id;
                cart.CartDetails.FirstOrDefault().Product = null;
                _context.CartDetails.Add(cart.CartDetails.FirstOrDefault());
                await _context.SaveChangesAsync();
            }
            else
            {
                //Anotação: Se CartHeader não for nullo, verifica se o cartdetails é do mesmo produto
                var cartDetail = await _context.CartDetails.AsNoTracking().
                    FirstOrDefaultAsync(p => p.ProductId == cart.CartDetails.FirstOrDefault().ProductId && p.CartHeaderId == cartHeader.Id);

                if (cartDetail == null)
                {
                    //Cria o cardDetails
                    cart.CartDetails.FirstOrDefault().CartHeaderId = cartHeader.Id;
                    cart.CartDetails.FirstOrDefault().Product = null;
                    _context.CartDetails.Add(cart.CartDetails.FirstOrDefault());
                    await _context.SaveChangesAsync();

                }
                else
                {
                    //Save or update
                    cart.CartDetails.FirstOrDefault().Product = null;
                    cart.CartDetails.FirstOrDefault().Count += cartDetail.Count;
                    cart.CartDetails.FirstOrDefault().Id = cartDetail.Id;
                    cart.CartDetails.FirstOrDefault().CartHeaderId = cartDetail.CartHeaderId;

                    _context.CartDetails.Update(cart.CartDetails.FirstOrDefault());
                    await _context.SaveChangesAsync();

                }
            }

            return _mapper.Map<CartVO>(cart);
        }
    }
}

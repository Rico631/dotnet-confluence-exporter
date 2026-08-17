using FluentValidation;
using MediatR;
using WebApp4.Application.Entities;
using WebApp4.Application.Infrastructure;
using WebApp4.Shared.Dpv;

namespace WebApp4.Application.Features.Dpv;

/// <summary>
/// Добавление товара в корзину
/// </summary>
/// <param name="Request">Реквест</param>
public record AddCartItem(AddCartItemRequest Request) : IRequest<AddCartItemResponse>
{
    public sealed class Validation : AbstractValidator<AddCartItem>
    {
        public Validation()
        {
            RuleFor(x => x.Request.UserId).CheckGuid();
            RuleFor(x => x.Request.Name).NotEmpty().WithMessage("Name is required.");
            RuleFor(x => x.Request.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0.");
            RuleFor(x => x.Request.Price).GreaterThan(0).WithMessage("Price must be greater than 0.");
        }
    }

    public sealed class Handler(ICartRepository cartRepository, IUserRepository userRepository, IUnitOfWork unitOfWork) : IRequestHandler<AddCartItem, AddCartItemResponse>
    {
        public async Task<AddCartItemResponse> Handle(AddCartItem request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetUserByIdAsync(request.Request.UserId, cancellationToken)
                ?? throw new ArgumentNullException(nameof(request.Request.UserId));

            var cartItem = new CartItem(request.Request.Name, request.Request.Quantity, request.Request.Price);

            var cart = await cartRepository.AddItemToCart(user.Id, cartItem, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new AddCartItemResponse
            {
                CartId = cart.Id,
                CartItemId = cartItem.Id,
                Name = cartItem.Name,
                Price = cartItem.Price,
                Quantity = cartItem.Quantity,
            };
        }
    }
}

// Top-level static extension class
public static class CommonValidation
{
    public static IRuleBuilderOptions<T, Guid> CheckGuid<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder.NotEmpty();
    }
}

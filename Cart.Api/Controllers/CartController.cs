using Cart.Api.DTOs;
using Cart.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cart.Api.Controllers;

[ApiController]
[Route("api/v1/cart")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CartResponse>> Get(
        string userId,
        CancellationToken cancellationToken)
    {
        var cart = await _cartService.GetAsync(userId, cancellationToken);

        return Ok(cart.ToResponse());
    }

    [HttpPost("{userId}/items")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CartResponse>> AddItem(
        string userId,
        [FromBody] AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var cart = await _cartService.AddItemAsync(userId, request, cancellationToken);

        return Ok(cart.ToResponse());
    }

    [HttpPut("{userId}/items/{productId:int}")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartResponse>> UpdateItem(
        string userId,
        int productId,
        [FromBody] UpdateCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var cart = await _cartService.UpdateItemAsync(
            userId,
            productId,
            request,
            cancellationToken);

        return cart is null ? NotFound() : Ok(cart.ToResponse());
    }

    [HttpDelete("{userId}/items/{productId:int}")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartResponse>> RemoveItem(
        string userId,
        int productId,
        CancellationToken cancellationToken)
    {
        var cart = await _cartService.RemoveItemAsync(
            userId,
            productId,
            cancellationToken);

        return cart is null ? NotFound() : Ok(cart.ToResponse());
    }

    [HttpDelete("{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        string userId,
        CancellationToken cancellationToken)
    {
        await _cartService.DeleteAsync(userId, cancellationToken);

        return NoContent();
    }
}

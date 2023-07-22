using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WalletApi.Models;
using WalletApi.Services;
using Transaction = WalletApi.Models.Transaction;

namespace WalletApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    /// <summary>
    /// Deposit or withdraw an amount for a user.
    /// </summary>
    /// <param name="request">The transaction request.</param>
    /// <returns>The updated balance.</returns>
    [HttpPost("transaction")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<decimal>> Transaction(
        [FromBody][Required] TransactionRequest request)
    {
        var balance = await _walletService.DepositOrWithdraw(request);
        return Ok(balance);
    }

    /// <summary>
    /// Locks an amount in a user's wallet.
    /// </summary>
    /// <param name="request">The lock funds transaction request.</param>
    /// <returns>The updated total locked amount.</returns>
    [HttpPost("transaction/lockfunds")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> LockFundsTransaction(
        [FromBody][Required] LockFundsTransactionRequest request)
    {
        var totalLockedAmount = await _walletService.LockFunds(request);
        return Ok(totalLockedAmount);
    }
    
    /// <summary>
    /// Get a list of transactions for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>The list of transactions.</returns>
    [HttpGet("transactions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Transaction>>> Transactions(
        [FromQuery][Required] TransactionFilter filter)
    {
        var transactions = await _walletService.GetTransactions(filter);
        return Ok(transactions);
    }
}
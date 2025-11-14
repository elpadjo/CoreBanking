using CoreBanking.Application.Accounts.Commands.CreateAccount;
using CoreBanking.Application.Accounts.DTOs;
using CoreBanking.Application.Common.Models;
using CoreBanking.Core.Entities;
using CoreBanking.Core.Enums;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using MediatR;

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, Result<CreateAccountResponse>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAccountCommandHandler(
        IAccountRepository accountRepository,
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateAccountResponse>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        // Validate customer exists
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
        if (customer == null)
            return Result<CreateAccountResponse>.Failure("Customer not found");

        // Validate initial deposit
        if (request.InitialDeposit < 0)
            return Result<CreateAccountResponse>.Failure("Initial deposit cannot be negative");

        if (request.InitialDeposit > 1000000)
            return Result<CreateAccountResponse>.Failure("Initial deposit too large");

        try
        {
            // Generate guaranteed unique account number from sequence
            var accountNumber = await _accountRepository.GenerateAccountNumberAsync();

            // Parse account type
            if (!Enum.TryParse<AccountType>(request.AccountType, out var accountType))
                return Result<CreateAccountResponse>.Failure("Invalid account type");

            // Create account with initial deposit
            var account = Account.Create(
                customerId: request.CustomerId,
                accountNumber: accountNumber,
                accountType: accountType,
                initialBalance: new Money(request.InitialDeposit, request.Currency)
            );

            // Add to repository and save
            await _accountRepository.AddAsync(account);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Return complete response
            return Result<CreateAccountResponse>.Success(new CreateAccountResponse
            {
                AccountId = account.Id.Value,
                AccountNumber = account.AccountNumber.Value,
                CustomerName = customer.GetFullName(),
                AccountType = account.AccountType.ToString(),
                CurrentBalance = account.CurrentBalance.Amount,
                Currency = account.CurrentBalance.Currency,
                DateOpened = account.DateOpened
            });
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result<CreateAccountResponse>.Failure(ex.Message);
        }
    }
}
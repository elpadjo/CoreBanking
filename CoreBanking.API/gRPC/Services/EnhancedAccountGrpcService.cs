using AutoMapper;
using CoreBanking.API.gRPC;
using CoreBanking.Application.Accounts.Commands.TransferMoney;
using CoreBanking.Application.Accounts.Queries.GetTransactionHistory;
using CoreBanking.Core.ValueObjects;
using Grpc.Core;
using MediatR;

namespace CoreBanking.API.gRPC.Services
{
    public class EnhancedAccountGrpcService : EnhancedAccountService.EnhancedAccountServiceBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<EnhancedAccountGrpcService> _logger;

        public EnhancedAccountGrpcService(IMediator mediator, IMapper mapper,
            ILogger<EnhancedAccountGrpcService> logger)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
        }

        // Server streaming for real-time transaction monitoring
        public override async Task StreamTransactions(StreamTransactionsRequest request,
            IServerStreamWriter<TransactionResponse> responseStream, ServerCallContext context)
        {
            _logger.LogInformation("Starting transaction stream for account {AccountNumber}",
                request.AccountNumber);

            var accountNumber = AccountNumber.Create(request.AccountNumber);
            var cancellationToken = context.CancellationToken;

            try
            {
                // Initial batch of recent transactions
                var historyQuery = new GetTransactionHistoryQuery
                {
                    AccountNumber = accountNumber,
                    PageSize = request.InitialBatchSize
                };

                var historyResult = await _mediator.Send(historyQuery, cancellationToken);

                if (historyResult.IsSuccess)
                {
                    foreach (var transaction in historyResult.Data!.Transactions)
                    {
                        if (cancellationToken.IsCancellationRequested) break;

                        await responseStream.WriteAsync(_mapper.Map<TransactionResponse>(transaction),
                            cancellationToken);
                        await Task.Delay(50, cancellationToken); // Stagger sends
                    }
                }

                // Simulate real-time updates (in real system, this would connect to event stream)
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // Check for new transactions (polling simulation)
                        // In production, this would hook into a real event stream
                        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

                        // For demo purposes, occasionally send a simulated transaction
                        if (DateTime.UtcNow.Second % 15 == 0) // Every 15 seconds for demo
                        {
                            var simulatedTransaction = new TransactionResponse
                            {
                                TransactionId = Guid.NewGuid().ToString(),
                                Type = "Deposit",
                                Amount = new Random().Next(100, 500),
                                Currency = "USD",
                                Description = "Simulated real-time transaction",
                                Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow)
                            };

                            await responseStream.WriteAsync(simulatedTransaction, cancellationToken);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error in transaction stream for account {AccountNumber}",
                    request.AccountNumber);
                throw new RpcException(new Status(StatusCode.Internal, "Streaming error occurred"));
            }

            _logger.LogInformation("Transaction stream ended for account {AccountNumber}",
                request.AccountNumber);
        }

        // Client streaming for batch operations
        public override async Task<BatchTransferResponse> BatchTransfer(
            IAsyncStreamReader<TransferMoneyRequest> requestStream, ServerCallContext context)
        {
            _logger.LogInformation("Starting batch transfer processing");

            var results = new List<BatchTransferResult>();
            var successfulTransfers = 0;
            var failedTransfers = 0;

            try
            {
                await foreach (var transferRequest in requestStream.ReadAllAsync(context.CancellationToken))
                {
                    try
                    {
                        var command = new TransferMoneyCommand
                        {
                            SourceAccountNumber = AccountNumber.Create(transferRequest.SourceAccountNumber),
                            DestinationAccountNumber = AccountNumber.Create(transferRequest.DestinationAccountNumber),
                            Amount = new Core.ValueObjects.Money((decimal)transferRequest.Amount, transferRequest.Currency),
                            Reference = transferRequest.Reference,
                            Description = transferRequest.Description
                        };

                        var result = await _mediator.Send(command, context.CancellationToken);

                        results.Add(new BatchTransferResult
                        {
                            Reference = transferRequest.Reference,
                            Success = result.IsSuccess,
                            Message = result.IsSuccess ? "Transfer completed" : string.Join("; ", result.Errors)
                        });

                        if (result.IsSuccess) successfulTransfers++;
                        else failedTransfers++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing batch transfer with reference {Reference}",
                            transferRequest.Reference);

                        results.Add(new BatchTransferResult
                        {
                            Reference = transferRequest.Reference,
                            Success = false,
                            Message = "Processing error"
                        });
                        failedTransfers++;
                    }
                }

                return new BatchTransferResponse
                {
                    TotalProcessed = results.Count,
                    Successful = successfulTransfers,
                    Failed = failedTransfers
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in batch transfer processing");
                throw new RpcException(new Status(StatusCode.Internal, "Batch processing failed"));
            }
        }
    }
}

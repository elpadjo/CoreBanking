using CoreBanking.API.gRPC.Interceptors;
using CoreBanking.API.gRPC.Mappings;
using CoreBanking.API.gRPC.Services;
using CoreBanking.API.Hubs;
using CoreBanking.API.Hubs.EventHandlers;
using CoreBanking.API.Middleware;
using CoreBanking.Application.Accounts.Commands.CreateAccount;
using CoreBanking.Application.Accounts.EventHandlers;
using CoreBanking.Application.Common.Behaviors;
using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Application.Common.Mappings;
using CoreBanking.Core.Events;
using CoreBanking.Core.Interfaces;
using CoreBanking.Infrastructure.Data;
using CoreBanking.Infrastructure.Repositories;
using CoreBanking.Infrastructure.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Reflection;

namespace CoreBanking.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<BankingDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add Application and Infrastructure Services
            //builder.Services.AddApplicationServices();
            //builder.Services.AddInfrastructureServices(builder.Configuration);

            builder.WebHost.ConfigureKestrel(options =>
            {
                // HTTP (for Swagger, REST, etc.)
                options.ListenLocalhost(5037, o =>
                {
                    o.Protocols = HttpProtocols.Http1;
                });

                // HTTPS (for gRPC, requires HTTP/2)
                options.ListenLocalhost(7288, o =>
                {
                    o.UseHttps(); // uses developer cert
                    o.Protocols = HttpProtocols.Http2;
                });
            });

            // Register dependencies (DI)

            // Register Repositories
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<IAccountRepository, AccountRepository>();
            builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            // Register domain event handlers
            builder.Services.AddTransient<INotificationHandler<AccountCreatedEvent>, AccountCreatedEventHandler>();
            builder.Services.AddTransient<INotificationHandler<MoneyTransferedEvent>, MoneyTransferedEventHandler>();
            builder.Services.AddTransient<INotificationHandler<InsufficientFundsEvent>, InsufficientFundsEventHandler>();

            // Register API event handlers (real-time signalR push)
            builder.Services.AddScoped<INotificationHandler<MoneyTransferedEvent>, RealTimeNotificationEventHandler>();

            // Register pipeline behaviors
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(DomainEventsBehavior<,>));
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

            // Add gRPC services to the container.
            builder.Services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = true;
                options.Interceptors.Add<ExceptionInterceptor>();
                options.MaxReceiveMessageSize = 16 * 1024 * 1024; // 16MB
                options.MaxSendMessageSize = 16 * 1024 * 1024; // 16MB
            });
            builder.Services.AddGrpcReflection();

            // Add SignalR
            builder.Services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            })
            .AddMessagePackProtocol(); // For smaller message sizes

            // Register hub filters
            builder.Services.AddSingleton<ErrorHandlingHubFilter>();

            // Add MediatR with behaviours
            builder.Services.AddMediatR(cfg =>
            {
                // Note: Registering one command is enough per Layer—MediatR scans the entire Application assembly (all Commands & Queries).
                cfg.RegisterServicesFromAssembly(typeof(CreateAccountCommand).Assembly);

                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
                cfg.AddOpenBehavior(typeof(DomainEventsBehavior<,>));

                cfg.Lifetime = ServiceLifetime.Scoped;
            });

            // Add Validators and AutoMapper
            builder.Services.AddValidatorsFromAssembly(typeof(CreateAccountCommandValidator).Assembly);
            builder.Services.AddAutoMapper(cfg => { }, typeof(AccountProfile).Assembly);
            builder.Services.AddAutoMapper(cfg => { }, typeof(AccountGrpcProfile).Assembly);

            // Register outbox and Background services
            builder.Services.AddScoped<IOutboxMessageProcessor, OutboxMessageProcessor>();
            builder.Services.AddHostedService<OutboxBackgroundService>();

            // Add controllers and swagger
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // Enriched swaggerGen with XML comments and authentication
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "CoreBanking API",
                    Version = "v1",
                    Description = "A modern banking API built with Clean Architecture, DDD and CQRS",
                    Contact = new OpenApiContact
                    {
                        Name = "CoreBanking Team",
                        Email = "support@corebanking.com"
                    }
                });

                // Include XML comments
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                // Add authentication support in Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
            });


            var app = builder.Build();

            
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger(options => options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0);

                // Enriched Swagger UI
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CoreBanking API v1");
                    c.RoutePrefix = "swagger"; // Access at /swagger
                    c.DocumentTitle = "CoreBanking API Documentation";
                    c.EnableDeepLinking();
                    c.DisplayOperationId();
                });
            }

            app.UseHttpsRedirection();           

            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

            app.UseAuthorization();

            app.MapControllers();

            // Configure SignalR hubs
            app.MapHub<NotificationHub>("/hubs/notifications");
            app.MapHub<TransactionHub>("/hubs/transactions");
            app.MapHub<EnhancedNotificationHub>("/hubs/enhanced-notifications");

            // Configure gRPC services            
            // Use grpc Endpoints
            app.MapGrpcService<AccountGrpcService>();
            app.MapGrpcService<EnhancedAccountGrpcService>();
            app.MapGrpcService<TradingGrpcService>();
            app.MapGet("/", () => "CoreBanking API is running. Use /swagger for REST or a gRPC client for gRPC calls.");
            if (app.Environment.IsDevelopment())
            {
                app.MapGrpcReflectionService();
            }

            app.Run();
        }
    }
}

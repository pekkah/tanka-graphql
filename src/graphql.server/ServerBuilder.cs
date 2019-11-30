using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tanka.GraphQL.Server.Links.DTOs;
using Tanka.GraphQL.Server.WebSockets;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.Validation;

namespace Tanka.GraphQL.Server
{
    public class ServerBuilder
    {
        public ServerBuilder(IServiceCollection services, Action<ServerOptions> configure = null)
        {
            Services = services;
            Initialize(configure);
        }

        public IServiceCollection Services { get; }

        public ServerBuilder WithExtension<TExtension>()
            where TExtension : class, IExecutorExtension
        {
            Services.TryAddEnumerable(ServiceDescriptor.Singleton<IExecutorExtension, TExtension>());

            return this;
        }

        public ServerBuilder WithContextExtension<TContext>()
        {
            return WithExtension<ContextExtension<TContext>>();
        }


        public ServerBuilder WithWebSockets(Action<WebSockets.WebSocketServerOptions> configure = null)
        {
            Services.TryAddSingleton<WebSocketServer>();
            Services.TryAddScoped<IProtocolHandler, GraphQLWSProtocol>();
            Services.TryAddScoped<IMessageContextAccessor, MessageContextAccessor>();

            if (configure != null)
                Services.Configure(configure);

            return this;
        }

        public ServerBuilder WithSchema(Func<ValueTask<ISchema>> schemaFactory)
        {
            if (schemaFactory == null) throw new ArgumentNullException(nameof(schemaFactory));

            var builder = Services.AddOptions<ServerOptions>();
            builder.Configure(options => options.GetSchema = _ => schemaFactory());
            return this;
        }

        public ServerBuilder WithSchema<TDep>(Func<TDep, ValueTask<ISchema>> schemaFactory) where TDep : class
        {
            if (schemaFactory == null) throw new ArgumentNullException(nameof(schemaFactory));

            var builder = Services.AddOptions<ServerOptions>();
            builder.Configure<TDep>((options, dep) => options.GetSchema = _ => schemaFactory(dep));

            return this;
        }

        public ServerBuilder WithSchema<TDep, TDep1>(Func<TDep, TDep1, ValueTask<ISchema>> schemaFactory)
            where TDep : class where TDep1 : class
        {
            if (schemaFactory == null) throw new ArgumentNullException(nameof(schemaFactory));

            var builder = Services.AddOptions<ServerOptions>();
            builder.Configure<TDep, TDep1>((options, dep, dep1) => options.GetSchema = _ => schemaFactory(dep, dep1));

            return this;
        }

        public ServerBuilder WithSchema<TDep, TDep1, TDep2>(Func<TDep, TDep1, TDep2, ValueTask<ISchema>> schemaFactory)
            where TDep : class where TDep1 : class where TDep2 : class
        {
            if (schemaFactory == null) throw new ArgumentNullException(nameof(schemaFactory));

            var builder = Services.AddOptions<ServerOptions>();
            builder.Configure<TDep, TDep1, TDep2>((options, dep, dep1, dep2) =>
                options.GetSchema = _ => schemaFactory(dep, dep1, dep2));

            return this;
        }

        public ServerBuilder WithRules(Func<CombineRule[], CombineRule[]> configureRules)
        {
            if (configureRules == null) throw new ArgumentNullException(nameof(configureRules));

            var builder = Services.AddOptions<ServerOptions>();
            builder.Configure(options => options.ValidationRules = configureRules(options.ValidationRules));
            return this;
        }

        public ServerBuilder WithRules<TDep>(Func<CombineRule[], TDep, CombineRule[]> configureRules) where TDep : class
        {
            if (configureRules == null) throw new ArgumentNullException(nameof(configureRules));

            var builder = Services.AddOptions<ServerOptions>();
            builder.Configure<TDep>((options, dep) => options.ValidationRules = configureRules(options.ValidationRules, dep));
            return this;
        }

        public ServerBuilder WithRules<TDep, TDep1>(Func<CombineRule[], TDep, TDep1, CombineRule[]> configureRules)
            where TDep : class where TDep1 : class
        {
            if (configureRules == null) throw new ArgumentNullException(nameof(configureRules));

            var builder = Services.AddOptions<ServerOptions>();
            builder.Configure<TDep, TDep1>((options, dep, dep1) => options.ValidationRules = configureRules(options.ValidationRules, dep, dep1));
            return this;
        }

        public ServerBuilder WithRules<TDep, TDep1, TDep2>(Func<CombineRule[], TDep, TDep1, TDep2, CombineRule[]> configureRules)
            where TDep : class where TDep1 : class where TDep2 : class
        {
            if (configureRules == null) throw new ArgumentNullException(nameof(configureRules));

            var builder = Services.AddOptions<ServerOptions>();
            builder.Configure<TDep, TDep1, TDep2>((options, dep, dep1, dep2) => options.ValidationRules = configureRules(options.ValidationRules, dep, dep1, dep2));
            return this;
        }

        private void Initialize(Action<ServerOptions> configure = null)
        {
            Services.TryAddScoped<IQueryStreamService, QueryStreamService>();
            WithExtension<ExecutionServiceScopeExtension>();

            if (configure != null)
                Services.Configure(configure);
        }
    }
}
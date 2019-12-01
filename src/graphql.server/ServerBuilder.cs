using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
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

        public ServerBuilder AddExtension<TExtension>()
            where TExtension : class, IExecutorExtension
        {
            Services.TryAddEnumerable(ServiceDescriptor.Singleton<IExecutorExtension, TExtension>());

            return this;
        }

        public ServerBuilder AddContextExtension<TContext>()
        {
            return AddExtension<ContextExtension<TContext>>();
        }

        public ServerBuilder ConfigureWebSockets()
        {
            Services.TryAddSingleton<WebSocketServer>();
            Services.TryAddScoped<IProtocolHandler, GraphQLWSProtocol>();
            Services.TryAddScoped<IMessageContextAccessor, MessageContextAccessor>();

            Services.AddOptions<WebSocketServerOptions>();

            return this;
        }

        public ServerBuilder ConfigureWebSockets(Func<MessageContext, Task> accept)
        {
            if (accept == null) throw new ArgumentNullException(nameof(accept));

            Services.TryAddSingleton<WebSocketServer>();
            Services.TryAddScoped<IProtocolHandler, GraphQLWSProtocol>();
            Services.TryAddScoped<IMessageContextAccessor, MessageContextAccessor>();

            var builder = Services.AddOptions<WebSocketServerOptions>();
            builder.Configure(options => options.AcceptAsync = accept);

            return this;
        }

        public ServerBuilder ConfigureWebSockets<TDep>(Func<MessageContext, TDep, Task> accept) where TDep : class
        {
            if (accept == null) throw new ArgumentNullException(nameof(accept));

            Services.TryAddSingleton<WebSocketServer>();
            Services.TryAddScoped<IProtocolHandler, GraphQLWSProtocol>();
            Services.TryAddScoped<IMessageContextAccessor, MessageContextAccessor>();

            var builder = Services.AddOptions<WebSocketServerOptions>();
            builder.Configure<TDep>((options, dep) => options.AcceptAsync = context => accept(context, dep));

            return this;
        }

        public ServerBuilder ConfigureWebSockets<TDep, TDep1>(Func<MessageContext, TDep, TDep1, Task> accept)
            where TDep : class where TDep1 : class
        {
            if (accept == null) throw new ArgumentNullException(nameof(accept));

            Services.TryAddSingleton<WebSocketServer>();
            Services.TryAddScoped<IProtocolHandler, GraphQLWSProtocol>();
            Services.TryAddScoped<IMessageContextAccessor, MessageContextAccessor>();

            var builder = Services.AddOptions<WebSocketServerOptions>();
            builder.Configure<TDep, TDep1>((options, dep, dep1) =>
                options.AcceptAsync = context => accept(context, dep, dep1));

            return this;
        }

        public ServerBuilder ConfigureWebSockets<TDep, TDep1, TDep2>(Func<MessageContext, TDep, TDep1, TDep2, Task> accept)
            where TDep : class where TDep1 : class where TDep2 : class
        {
            if (accept == null) throw new ArgumentNullException(nameof(accept));

            Services.TryAddSingleton<WebSocketServer>();
            Services.TryAddScoped<IProtocolHandler, GraphQLWSProtocol>();
            Services.TryAddScoped<IMessageContextAccessor, MessageContextAccessor>();

            var builder = Services.AddOptions<WebSocketServerOptions>();
            builder.Configure<TDep, TDep1, TDep2>((options, dep, dep1, dep2) =>
                options.AcceptAsync = context => accept(context, dep, dep1, dep2));

            return this;
        }

        public ServerBuilder ConfigureSchema(Func<ValueTask<ISchema>> schemaFactory)
        {
            if (schemaFactory == null) throw new ArgumentNullException(nameof(schemaFactory));

            var builder = Services.AddOptions<ServerOptions>();
            builder.Configure(options => options.GetSchema = _ => schemaFactory());
            return this;
        }

        public ServerBuilder ConfigureSchema<TDep>(Func<TDep, ValueTask<ISchema>> schemaFactory) where TDep : class
        {
            if (schemaFactory == null) throw new ArgumentNullException(nameof(schemaFactory));

            var builder = Services.AddOptions<ServerOptions>();
            builder.Configure<TDep>((options, dep) => options.GetSchema = _ => schemaFactory(dep));

            return this;
        }

        public ServerBuilder ConfigureSchema<TDep, TDep1>(Func<TDep, TDep1, ValueTask<ISchema>> schemaFactory)
            where TDep : class where TDep1 : class
        {
            if (schemaFactory == null) throw new ArgumentNullException(nameof(schemaFactory));

            var builder = Services.AddOptions<ServerOptions>();
            builder.Configure<TDep, TDep1>((options, dep, dep1) => options.GetSchema = _ => schemaFactory(dep, dep1));

            return this;
        }

        public ServerBuilder ConfigureSchema<TDep, TDep1, TDep2>(Func<TDep, TDep1, TDep2, ValueTask<ISchema>> schemaFactory)
            where TDep : class where TDep1 : class where TDep2 : class
        {
            if (schemaFactory == null) throw new ArgumentNullException(nameof(schemaFactory));

            var builder = Services.AddOptions<ServerOptions>();
            builder.Configure<TDep, TDep1, TDep2>((options, dep, dep1, dep2) =>
                options.GetSchema = _ => schemaFactory(dep, dep1, dep2));

            return this;
        }

        public ServerBuilder ConfigureRules(Func<CombineRule[], CombineRule[]> configureRules)
        {
            if (configureRules == null) throw new ArgumentNullException(nameof(configureRules));

            var builder = Services.AddOptions<ServerOptions>();
            builder.Configure(options => options.ValidationRules = configureRules(options.ValidationRules));
            return this;
        }

        public ServerBuilder ConfigureRules<TDep>(Func<CombineRule[], TDep, CombineRule[]> configureRules) where TDep : class
        {
            if (configureRules == null) throw new ArgumentNullException(nameof(configureRules));

            var builder = Services.AddOptions<ServerOptions>();
            builder.Configure<TDep>((options, dep) =>
                options.ValidationRules = configureRules(options.ValidationRules, dep));
            return this;
        }

        public ServerBuilder ConfigureRules<TDep, TDep1>(Func<CombineRule[], TDep, TDep1, CombineRule[]> configureRules)
            where TDep : class where TDep1 : class
        {
            if (configureRules == null) throw new ArgumentNullException(nameof(configureRules));

            var builder = Services.AddOptions<ServerOptions>();
            builder.Configure<TDep, TDep1>((options, dep, dep1) =>
                options.ValidationRules = configureRules(options.ValidationRules, dep, dep1));
            return this;
        }

        public ServerBuilder ConfigureRules<TDep, TDep1, TDep2>(
            Func<CombineRule[], TDep, TDep1, TDep2, CombineRule[]> configureRules)
            where TDep : class where TDep1 : class where TDep2 : class
        {
            if (configureRules == null) throw new ArgumentNullException(nameof(configureRules));

            var builder = Services.AddOptions<ServerOptions>();
            builder.Configure<TDep, TDep1, TDep2>((options, dep, dep1, dep2) =>
                options.ValidationRules = configureRules(options.ValidationRules, dep, dep1, dep2));
            return this;
        }

        private OptionsBuilder<ServerOptions> Initialize(Action<ServerOptions> configure = null)
        {
            Services.TryAddScoped<IQueryStreamService, QueryStreamService>();
            AddExtension<ExecutionServiceScopeExtension>();

            return Services.AddOptions<ServerOptions>()
                .ValidateDataAnnotations();
        }
    }
}
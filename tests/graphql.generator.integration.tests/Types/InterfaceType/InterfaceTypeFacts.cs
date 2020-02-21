using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace Tanka.GraphQL.Generator.Integration.Tests.Types.InterfaceType
{
    public class ObjectTypeController : ObjectTypeControllerBase<ObjectType>
    {
        public override ValueTask<IInterfaceType?> Property(ObjectType objectValue, IResolverContext context)
        {
            return new ValueTask<IInterfaceType?>(new FieldType());
        }

        public override ValueTask<IEnumerable<IInterfaceType?>?> List(ObjectType objectValue, IResolverContext context)
        {
            return new ValueTask<IEnumerable<IInterfaceType?>?>(new []
            {
                new FieldType(), 
                new FieldType(), 
            });
        }
    }

    public class InterfaceTypeFacts
    {
        public InterfaceTypeFacts()
        {
            _sut = Substitute.ForPartsOf<ObjectTypeController>();
            _schema = Substitute.For<ISchema>();
            var scope = Substitute.For<IServiceScope>();
            _provider = Substitute.For<IServiceProvider>();
            scope.ServiceProvider.Returns(_provider);

            _executorContext = Substitute.For<IExecutorContext>();
            _executorContext.Schema.Returns(_schema);
            _executorContext.ExtensionsRunner.Returns(new ExtensionsRunner(new[]
            {
                new ContextExtensionScope<IServiceScope>(scope)
            }));
        }

        private readonly ObjectTypeController _sut;
        private readonly IExecutorContext _executorContext;
        private readonly IServiceProvider _provider;
        private readonly ISchema _schema;

        private IResolverContext CreateContext(
            object? objectValue
        )
        {
            var context = Substitute.For<IResolverContext>();
            context.ObjectValue.Returns(objectValue);
            context.ExecutionContext.Returns(_executorContext);

            return context;
        }

        [Fact]
        public async Task Should_Use_IsTypeOf_from_interface_controller()
        {
            /* Given */
            var objectValue = new ObjectType();
            var context = CreateContext(objectValue);
            var interfaceTypeController = Substitute.For<IInterfaceTypeController>();
            _provider.GetService(typeof(IInterfaceTypeController))
                .Returns(interfaceTypeController);

            /* When */
            await _sut.Property(context);

            /* Then */
            interfaceTypeController.Received().IsTypeOf(
                Arg.Any<FieldType>(), 
                _schema);
        }

        [Fact(Skip = "For lists the IsTypeOf call is made during value completion")]
        public async Task Should_Use_IsTypeOf_from_interface_controller_for_list()
        {
            /* Given */
            var objectValue = new ObjectType();
            var context = CreateContext(objectValue);
            var interfaceTypeController = Substitute.For<IInterfaceTypeController>();
            _provider.GetService(typeof(IInterfaceTypeController))
                .Returns(interfaceTypeController);

            /* When */
            await _sut.List(context);

            /* Then */
            interfaceTypeController.Received().IsTypeOf(
                Arg.Any<FieldType>(), 
                _schema);
        }

        [Fact]
        public async Task Should_call_resolver_with_args()
        {
            /* Given */
            var objectValue = new ObjectType();
            var context = CreateContext(objectValue);
            var interfaceTypeController = Substitute.For<IInterfaceTypeController>();
            _provider.GetService(typeof(IInterfaceTypeController))
                .Returns(interfaceTypeController);

            /* When */
            await _sut.Property(context);

            /* Then */
            await _sut.Received().Property(objectValue, context);
        }

        [Fact]
        public async Task Should_call_list_resolver_with_args()
        {
            /* Given */
            var objectValue = new ObjectType();
            var context = CreateContext(objectValue);
            var interfaceTypeController = Substitute.For<IInterfaceTypeController>();
            _provider.GetService(typeof(IInterfaceTypeController))
                .Returns(interfaceTypeController);

            /* When */
            await _sut.List(context);

            /* Then */
            await _sut.Received().List(objectValue, context);
        }

        [Fact()]
        public void Default_IsTypeOf_uses__Typename_and_schema()
        {
            /* Given */
            var value = new FieldType();
            var controller = new InterfaceTypeController();

            /* When */
            controller.IsTypeOf(value, _schema);

            /* Then */
            _schema.Received().GetNamedType(value.__Typename);
        }
    }
}
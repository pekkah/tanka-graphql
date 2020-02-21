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

namespace Tanka.GraphQL.Generator.Integration.Tests.Types.UnionType
{
    public class ObjectTypeController : ObjectTypeControllerBase<ObjectType>
    {
        public override ValueTask<IFieldType?> Property(ObjectType objectValue, IResolverContext context)
        {
            return new ValueTask<IFieldType?>(new FieldValue1());
        }

        public override ValueTask<IEnumerable<IFieldType?>?> List(ObjectType objectValue, IResolverContext context)
        {
            return new ValueTask<IEnumerable<IFieldType?>?>(new IFieldType[]
            {
                new FieldValue1(), 
                new FieldValue2()
            });
        }
    }

    public class UnionTypeFacts
    {
        public UnionTypeFacts()
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
        public async Task Should_Use_IsTypeOf_from_union_controller()
        {
            /* Given */
            var objectValue = new ObjectType();
            var context = CreateContext(objectValue);
            var unionTypeController = Substitute.For<IFieldTypeController>();
            _provider.GetService(typeof(IFieldTypeController))
                .Returns(unionTypeController);

            /* When */
            await _sut.Property(context);

            /* Then */
            unionTypeController.Received().IsTypeOf(
                Arg.Any<IFieldType>(), 
                _schema);
        }

        [Fact(Skip = "For lists the IsTypeOf call is made during value completion")]
        public async Task Should_Use_IsTypeOf_from_union_controller_for_list()
        {
            /* Given */
            var objectValue = new ObjectType();
            var context = CreateContext(objectValue);
            var unionTypeController = Substitute.For<IFieldTypeController>();
            _provider.GetService(typeof(IFieldTypeController))
                .Returns(unionTypeController);

            /* When */
            await _sut.List(context);

            /* Then */
            unionTypeController.Received().IsTypeOf(
                Arg.Any<IFieldType>(), 
                _schema);
        }

        [Fact]
        public async Task Should_call_resolver_with_args()
        {
            /* Given */
            var objectValue = new ObjectType();
            var context = CreateContext(objectValue);
            var unionTypeController = Substitute.For<IFieldTypeController>();
            _provider.GetService(typeof(IFieldTypeController))
                .Returns(unionTypeController);

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
            var unionTypeController = Substitute.For<IFieldTypeController>();
            _provider.GetService(typeof(IFieldTypeController))
                .Returns(unionTypeController);

            /* When */
            await _sut.List(context);

            /* Then */
            await _sut.Received().List(objectValue, context);
        }

        [Fact()]
        public void Default_IsTypeOf_uses__Typename_and_schema()
        {
            /* Given */
            var value = new FieldValue1();
            var controller = new FieldTypeController();

            /* When */
            controller.IsTypeOf(value, _schema);

            /* Then */
            _schema.Received().GetNamedType(value.__Typename);
        }

        [Fact()]
        public void Default_IsTypeOf_uses__Typename_and_schema2()
        {
            /* Given */
            var value = new FieldValue2();
            var controller = new FieldTypeController();

            /* When */
            controller.IsTypeOf(value, _schema);

            /* Then */
            _schema.Received().GetNamedType(value.__Typename);
        }
    }
}
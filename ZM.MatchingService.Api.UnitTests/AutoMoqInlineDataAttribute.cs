using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using AutoFixture.Xunit2;
using FluentValidation;
using MediatR;
using System.Reflection;

namespace ZM.MatchingService.Api.UnitTests
{
    public class AutoMoqInlineDataAttribute : InlineAutoDataAttribute
    {
        public AutoMoqInlineDataAttribute(params object[] values) : base(new AutoMoqAttribute(), values)
        {

        }

        private class AutoMoqAttribute : AutoDataAttribute
        {
            public AutoMoqAttribute() : base(FixtureFactory)
            {

            }

            private static IFixture FixtureFactory()
            {
                var fixture = new Fixture();

                fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true });
                fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                    .ForEach(b => fixture.Behaviors.Remove(b));
                fixture.Behaviors.Add(new OmitOnRecursionBehavior());

                fixture.Register(() => new DateTime(2023, 10, 10, 10, 10, 10));

                fixture.Customizations.Add(new Omitter(new BasePropertySpecification<AbstractValidator<IRequest>>("ClassLevelCascadeMode")));
                fixture.Customizations.Add(new Omitter(new BasePropertySpecification<AbstractValidator<IRequest>>("RuleLevelCascadeMode")));
                return fixture;
            }
        }

        private class BasePropertySpecification<T> : IRequestSpecification
        {
            private readonly Type _declaringType;
            private readonly string _name;

            public BasePropertySpecification(string name)
            {
                _declaringType = typeof(T);
                _name = name;
            }

            public bool IsSatisfiedBy(object request)
            {
                if (request is not PropertyInfo pi)
                {
                    return false;
                }

                if (_declaringType.IsGenericType)
                {
                    return pi.DeclaringType != null
                           && _declaringType.GetGenericTypeDefinition() ==
                           (pi.DeclaringType.IsGenericType
                               ? pi.DeclaringType.GetGenericTypeDefinition()
                               : pi.DeclaringType)
                           && pi.Name.Equals(_name, StringComparison.Ordinal);
                }

                return pi.DeclaringType == _declaringType &&
                       pi.Name.Equals(_name, StringComparison.Ordinal);

            }
        }

    }
}

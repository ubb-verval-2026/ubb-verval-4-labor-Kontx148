using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using Moq;

namespace DatesAndStuff.Tests
{
    internal class CustomPersonCreationAutodataAttribute : AutoDataAttribute
    {
        public CustomPersonCreationAutodataAttribute()
        : base(() =>
        {
            var fixture = new Fixture();

            fixture.Customize(new AutoMoqCustomization());

            var paymentSequence = new MockSequence();
            var paymentService = new Mock<IPaymentService>();
            paymentService.InSequence(paymentSequence).Setup(m => m.StartPayment());
            paymentService.InSequence(paymentSequence).Setup(m => m.SpecifyAmount(Person.SubscriptionFee));
            paymentService.InSequence(paymentSequence).Setup(m => m.ConfirmPayment());
            fixture.Inject(paymentService);

            //fixture.Register<IPaymentService>(() => new TestPaymentService());

            double top = 20;
            double bottom = -10;
            fixture.Customize<double>(c => c.FromFactory(() => new Random().NextDouble() * (top - (bottom)) + bottom));
            return fixture;
        })
        { }
    }
}

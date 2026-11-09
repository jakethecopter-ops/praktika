using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BookingSystem;

namespace BookingSystem.Tests
{

    internal class FakeEventRepository : IEventRepository
    {
        public bool ExistsResult { get; set; } = true;
        public int AvailableSeatsResult { get; set; } = 10;
        public int ReserveSeatsCallCount { get; private set; }
        public int ReleaseSeatsCallCount { get; private set; }

        public bool Exists(int eventId) => ExistsResult;
        public int GetAvailableSeats(int eventId) => AvailableSeatsResult;
        
        public void ReserveSeats(int eventId, int seatsCount) => ReserveSeatsCallCount++;
        public void ReleaseSeats(int eventId, int seatsCount) => ReleaseSeatsCallCount++;
    }

    internal class FakeBookingRepository : IBookingRepository
    {
        public int SaveCallCount { get; private set; }
        public Booking? SavedBooking { get; private set; }

        public void Save(Booking booking)
        {
            SaveCallCount++;
            SavedBooking = booking;
        }
    }

    internal class FakePaymentService : IPaymentService
    {
        public bool ChargeResult { get; set; } = true;
        public Exception? ChargeException { get; set; }
        public int ChargeCallCount { get; private set; }
        public decimal LastChargedAmount { get; private set; }

        public bool Charge(string customerEmail, decimal amount)
        {
            ChargeCallCount++;
            LastChargedAmount = amount;

            if (ChargeException != null)
            {
                throw ChargeException;
            }

            return ChargeResult;
        }
    }

    internal class FakeClock : IClock
    {
        public DateTimeOffset UtcNowResult { get; set; } = new DateTimeOffset(2026, 10, 25, 12, 0, 0, TimeSpan.Zero);
        public DateTimeOffset UtcNow => UtcNowResult;
    }

    internal class FakeBookingIdGenerator : IBookingIdGenerator
    {
        public Guid GeneratedId { get; set; } = Guid.NewGuid();
        public Guid Create() => GeneratedId;
    }

    [TestClass]
    public class BookingServiceTests
    {
        private FakeEventRepository _fakeEventRepository = null!;
        private FakeBookingRepository _fakeBookingRepository = null!;
        private FakePaymentService _fakePaymentService = null!;
        private FakeClock _fakeClock = null!;
        private FakeBookingIdGenerator _fakeIdGenerator = null!;
        private BookingService _bookingService = null!;

        [TestInitialize]
        public void Setup()
        {
            _fakeEventRepository = new FakeEventRepository();
            _fakeBookingRepository = new FakeBookingRepository();
            _fakePaymentService = new FakePaymentService();
            _fakeClock = new FakeClock();
            _fakeIdGenerator = new FakeBookingIdGenerator();

            _bookingService = new BookingService(
                _fakeEventRepository,
                _fakeBookingRepository,
                _fakePaymentService,
                _fakeClock,
                _fakeIdGenerator);
        }

        [TestMethod]
        public void CreateBooking_ValidRequest_ReturnsConfirmedBooking()
        {
            // Arrange
            var request = new BookingRequest(1, "test@example.com", 2, 500m);
            var expectedId = _fakeIdGenerator.GeneratedId;
            var expectedTime = _fakeClock.UtcNowResult;

            // Act
            var result = _bookingService.CreateBooking(request);

            // Assert
            Assert.AreEqual(expectedId, result.Id);
            Assert.AreEqual(1, result.EventId);
            Assert.AreEqual("test@example.com", result.CustomerEmail);
            Assert.AreEqual(2, result.SeatsCount);
            Assert.AreEqual(1000m, result.TotalAmount);
            Assert.AreEqual(expectedTime, result.CreatedAt);
            Assert.AreEqual(BookingStatus.Confirmed, result.Status);

            Assert.AreEqual(1, _fakeEventRepository.ReserveSeatsCallCount);
            Assert.AreEqual(0, _fakeEventRepository.ReleaseSeatsCallCount);
            Assert.AreEqual(1, _fakePaymentService.ChargeCallCount);
            Assert.AreEqual(1000m, _fakePaymentService.LastChargedAmount);
            Assert.AreEqual(1, _fakeBookingRepository.SaveCallCount);
        }

        [TestMethod]
        public void CreateBooking_PaymentFails_ReturnsPaymentFailedBookingAndReleasesSeats()
        {
            // Arrange
            _fakePaymentService.ChargeResult = false;
            var request = new BookingRequest(1, "test@example.com", 2, 500m);

            // Act
            var result = _bookingService.CreateBooking(request);

            // Assert
            Assert.AreEqual(BookingStatus.PaymentFailed, result.Status);
            Assert.AreEqual(1, _fakeEventRepository.ReserveSeatsCallCount);
            Assert.AreEqual(1, _fakeEventRepository.ReleaseSeatsCallCount);
            Assert.AreEqual(1, _fakeBookingRepository.SaveCallCount);
        }

        [TestMethod]
        public void CreateBooking_PaymentThrowsException_ReleasesSeatsAndDoesNotSave()
        {
            // Arrange
            _fakePaymentService.ChargeException = new InvalidOperationException("Сбой шлюза");
            var request = new BookingRequest(1, "test@example.com", 2, 500m);

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() => _bookingService.CreateBooking(request));

            Assert.AreEqual(1, _fakeEventRepository.ReserveSeatsCallCount);
            Assert.AreEqual(1, _fakeEventRepository.ReleaseSeatsCallCount);
            Assert.AreEqual(0, _fakeBookingRepository.SaveCallCount);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(11)]
        [DataRow(100)]
        public void CreateBooking_InvalidSeatsCount_ThrowsArgumentOutOfRangeException(int seatsCount)
        {
            // Arrange
            var request = new BookingRequest(1, "test@example.com", seatsCount, 500m);

            // Act & Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _bookingService.CreateBooking(request));

            // Проверка отсутствия побочных эффектов
            Assert.AreEqual(0, _fakeEventRepository.ReserveSeatsCallCount);
            Assert.AreEqual(0, _fakePaymentService.ChargeCallCount);
            Assert.AreEqual(0, _fakeBookingRepository.SaveCallCount);
        }

        [TestMethod]
        public void CreateBooking_EventDoesNotExist_ThrowsInvalidOperationException()
        {
            // Arrange
            _fakeEventRepository.ExistsResult = false;
            var request = new BookingRequest(1, "test@example.com", 2, 500m);

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() => _bookingService.CreateBooking(request));

            Assert.AreEqual(0, _fakeEventRepository.ReserveSeatsCallCount);
            Assert.AreEqual(0, _fakePaymentService.ChargeCallCount);
            Assert.AreEqual(0, _fakeBookingRepository.SaveCallCount);
        }

        [TestMethod]
        public void CreateBooking_NotEnoughSeats_ThrowsInvalidOperationException()
        {
            // Arrange
            _fakeEventRepository.AvailableSeatsResult = 1; // Запрашиваем 2, доступно 1
            var request = new BookingRequest(1, "test@example.com", 2, 500m);

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() => _bookingService.CreateBooking(request));

            Assert.AreEqual(0, _fakeEventRepository.ReserveSeatsCallCount);
            Assert.AreEqual(0, _fakePaymentService.ChargeCallCount);
            Assert.AreEqual(0, _fakeBookingRepository.SaveCallCount);
        }

        [DataTestMethod]
        [DataRow(1, 100, 100)]
        [DataRow(2, 500, 1000)]
        [DataRow(3, 1250, 3750)]
        public void CreateBooking_ValidRequest_CalculatesCorrectTotalAmount(int seats, int price, int expectedTotal)
        {
            // Arrange
            var request = new BookingRequest(1, "test@example.com", seats, (decimal)price);

            // Act
            var result = _bookingService.CreateBooking(request);

            // Assert
            Assert.AreEqual((decimal)expectedTotal, result.TotalAmount);
        }
    }
}
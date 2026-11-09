using System;

namespace BookingSystem
{
    public enum BookingStatus
    {
        Confirmed,
        PaymentFailed
    }

    public sealed record BookingRequest(
        int EventId,
        string CustomerEmail,
        int SeatsCount,
        decimal PricePerSeat);

    public sealed record Booking(
        Guid Id,
        int EventId,
        string CustomerEmail,
        int SeatsCount,
        decimal TotalAmount,
        DateTimeOffset CreatedAt,
        BookingStatus Status);

    public interface IEventRepository
    {
        bool Exists(int eventId);
        int GetAvailableSeats(int eventId);
        void ReserveSeats(int eventId, int seatsCount);
        void ReleaseSeats(int eventId, int seatsCount);
    }

    public interface IBookingRepository
    {
        void Save(Booking booking);
    }

    public interface IPaymentService
    {
        bool Charge(string customerEmail, decimal amount);
    }

    public interface IClock
    {
        DateTimeOffset UtcNow { get; }
    }

    public interface IBookingIdGenerator
    {
        Guid Create();
    }

    /// Сервис для обработки логики бронирования мест на мероприятия
    public sealed class BookingService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IPaymentService _paymentService;
        private readonly IClock _clock;
        private readonly IBookingIdGenerator _idGenerator;

        public BookingService(
            IEventRepository eventRepository,
            IBookingRepository bookingRepository,
            IPaymentService paymentService,
            IClock clock,
            IBookingIdGenerator idGenerator)
        {
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
        }

        public Booking CreateBooking(BookingRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.EventId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(request.EventId), "Идентификатор мероприятия должен быть больше нуля");
            }

            if (string.IsNullOrWhiteSpace(request.CustomerEmail) || !request.CustomerEmail.Contains("@"))
            {
                throw new ArgumentException("Некорректный email клиента", nameof(request.CustomerEmail));
            }

            if (request.SeatsCount < 1 || request.SeatsCount > 10)
            {
                throw new ArgumentOutOfRangeException(nameof(request.SeatsCount), "Количество мест должно быть от 1 до 10");
            }

            if (request.PricePerSeat <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(request.PricePerSeat), "Цена за место должна быть больше нуля");
            }

            if (!_eventRepository.Exists(request.EventId))
            {
                throw new InvalidOperationException("Мероприятие не существует");
            }

            if (_eventRepository.GetAvailableSeats(request.EventId) < request.SeatsCount)
            {
                throw new InvalidOperationException("Недостаточно свободных мест");
            }

            var totalAmount = request.SeatsCount * request.PricePerSeat;
            var bookingId = _idGenerator.Create();
            var createdAt = _clock.UtcNow;

            _eventRepository.ReserveSeats(request.EventId, request.SeatsCount);

            try
            {
                var paymentSuccess = _paymentService.Charge(request.CustomerEmail, totalAmount);

                if (paymentSuccess)
                {
                    var booking = new Booking(
                        bookingId,
                        request.EventId,
                        request.CustomerEmail,
                        request.SeatsCount,
                        totalAmount,
                        createdAt,
                        BookingStatus.Confirmed);

                    _bookingRepository.Save(booking);
                    return booking;
                }
                else
                {
                    _eventRepository.ReleaseSeats(request.EventId, request.SeatsCount);

                    var booking = new Booking(
                        bookingId,
                        request.EventId,
                        request.CustomerEmail,
                        request.SeatsCount,
                        totalAmount,
                        createdAt,
                        BookingStatus.PaymentFailed);

                    _bookingRepository.Save(booking);
                    return booking;
                }
            }
            catch
            {
                _eventRepository.ReleaseSeats(request.EventId, request.SeatsCount);
                throw;
            }
        }
    }
}
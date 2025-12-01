using System;
using System.Collections.Generic;
using System.Text;

namespace BookingService.Domain.ValueObjects
{
    public sealed record TimeSlot
    {
        public DateTime Start { get; init; }
        public DateTime End { get; init; }

        public TimeSlot(DateTime start, DateTime end)
        {
            if (end <= start)
                throw new ArgumentException("End time must be after start time.", nameof(end));

            // ممكن تضيف تحقق آخر لاحقاً (عدم الحجز في الماضي مثلاً)
            Start = start;
            End = end;
        }

        public TimeSpan Duration => End - Start;
    }
}

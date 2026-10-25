using System;
using DictionaryTool.Core.Models;

namespace DictionaryTool.Core.Srs
{
    /// <summary>
    /// Lịch ôn tập giãn cách (spaced repetition) rút gọn theo kiểu SM-2/Anki.
    /// Hàm thuần, không I/O — dễ test và tái dùng. Trạng thái của 1 thẻ gồm:
    /// IntervalDays (khoảng cách hiện tại), Ease (hệ số dễ), Reps (số lần nhớ liên tiếp).
    /// </summary>
    public static class SrsScheduler
    {
        public const double DefaultEase = 2.5;
        private const double MinEase = 1.3;
        private const double MaxEase = 3.0;

        public static ReviewSchedule Next(int intervalDays, double ease, int reps, ReviewGrade grade, DateTime nowUtc)
        {
            if (ease < MinEase) ease = DefaultEase; // dữ liệu cũ chưa có ease hợp lệ

            switch (grade)
            {
                case ReviewGrade.Again:
                    // Quên: đặt lại chuỗi, hẹn ôn lại sau ~10 phút (trong cùng phiên).
                    return new ReviewSchedule
                    {
                        Reps = 0,
                        Ease = Clamp(ease - 0.2),
                        IntervalDays = 0,
                        NextReviewUtc = nowUtc.AddMinutes(10)
                    };

                case ReviewGrade.Hard:
                {
                    var newReps = reps + 1;
                    var newInterval = reps == 0 ? 1 : Math.Max(1, (int)Math.Round(intervalDays * 1.2));
                    var newEase = Clamp(ease - 0.15);
                    return Build(newReps, newEase, newInterval, nowUtc);
                }

                case ReviewGrade.Easy:
                {
                    var newReps = reps + 1;
                    var newInterval = reps == 0 ? 3 : (int)Math.Round(intervalDays * ease * 1.3);
                    var newEase = Clamp(ease + 0.15);
                    return Build(newReps, newEase, Math.Max(1, newInterval), nowUtc);
                }

                case ReviewGrade.Good:
                default:
                {
                    var newReps = reps + 1;
                    int newInterval;
                    if (reps == 0) newInterval = 1;
                    else if (reps == 1) newInterval = 3;
                    else newInterval = (int)Math.Round(intervalDays * ease);
                    return Build(newReps, ease, Math.Max(1, newInterval), nowUtc);
                }
            }
        }

        private static ReviewSchedule Build(int reps, double ease, int intervalDays, DateTime nowUtc)
        {
            return new ReviewSchedule
            {
                Reps = reps,
                Ease = ease,
                IntervalDays = intervalDays,
                NextReviewUtc = nowUtc.AddDays(intervalDays)
            };
        }

        private static double Clamp(double ease)
        {
            if (ease < MinEase) return MinEase;
            if (ease > MaxEase) return MaxEase;
            return ease;
        }
    }

    public struct ReviewSchedule
    {
        public int IntervalDays;
        public double Ease;
        public int Reps;
        public DateTime NextReviewUtc;
    }
}

using Domain.Enums;

namespace Domain.ValueObjects;

public class PlanLimits
{
    public int Boards { get; set; }
    public int Members { get; set; }
    public int FileUpload { get; set; } // MB
    public bool Automation { get; set; }
    public bool CalendarView { get; set; }
    public bool TimelineView { get; set; }
    public bool Reporting { get; set; }

    public static Dictionary<SubscriptionPlan, PlanLimits> GetPlanLimits()
    {
        return new Dictionary<SubscriptionPlan, PlanLimits>
        {
            {
                SubscriptionPlan.Free, new PlanLimits
                {
                    Boards = 10,
                    Members = 10,
                    FileUpload = 10,
                    Automation = false,
                    CalendarView = false,
                    TimelineView = false,
                    Reporting = false
                }
            },
            {
                SubscriptionPlan.Plus, new PlanLimits
                {
                    Boards = 50,
                    Members = 50,
                    FileUpload = 100,
                    Automation = true,
                    CalendarView = false,
                    TimelineView = false,
                    Reporting = false
                }
            },
            {
                SubscriptionPlan.Pro, new PlanLimits
                {
                    Boards = 100,
                    Members = 100,
                    FileUpload = 250,
                    Automation = true,
                    CalendarView = true,
                    TimelineView = true,
                    Reporting = true
                }
            },
            {
                SubscriptionPlan.Premium, new PlanLimits
                {
                    Boards = -1, // unlimited
                    Members = -1, // unlimited
                    FileUpload = 1000,
                    Automation = true,
                    CalendarView = true,
                    TimelineView = true,
                    Reporting = true
                }
            }
        };
    }
}
using JupiterNeoServiceClient.classes;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JupiterNeoServiceClient.Models
{
    public class SModel
    {
        public string? BascId { get; set; }
        public string? BascDate { get; set; }
        public string? BascTime { get; set; }
        public int BascStarted { get; set; }
        public int BascCompleted { get; set; }
        public int BascScanned { get; set; }
    }

    public class SchedulesModel : BaseModel
    {
        public enum Fields
        {
            ID,
            DATE,
            TIME,
            STARTED,
            COMPLETED
        }

        public SchedulesModel()
        {
            TableName = "backup_schedule";
            fields = new Dictionary<Enum, string>
            {
                [Fields.ID] = "basc_id",
                [Fields.DATE] = "basc_date",
                [Fields.TIME] = "basc_time",
                [Fields.STARTED] = "basc_started",
                [Fields.COMPLETED] = "basc_completed"
            };
        }

        public void DeleteCompletedBackups()
        {
            string today = Helpers.Today();
            query()
                .Where(fields[Fields.DATE], "<>", today)
                .WhereNotNull(fields[Fields.COMPLETED])
                .Delete();
        }

        public SModel? GetSchedule(string time)
        {
            string today = Helpers.Today();
            return query()
                .Where(fields[Fields.DATE], today)
                .Where(fields[Fields.TIME], time)
                .Get<SModel>()
                .FirstOrDefault();
        }

        public SModel? GetScheduleById(string scheduleId) =>
            query()
                .Where(fields[Fields.ID], scheduleId)
                .Get<SModel>()
                .FirstOrDefault();

        public SModel? GetUncompletedBackup() =>
            query()
                .Where(fields[Fields.STARTED], 1)
                .Where(fields[Fields.COMPLETED], 0)
                .Get<SModel>()
                .FirstOrDefault();

        public SModel? GetUnstartedBackup()
        {
            string today = Helpers.Today();
            return query()
                .Where(fields[Fields.STARTED], 0)
                .Where(fields[Fields.COMPLETED], 0)
                .Where(fields[Fields.DATE], today)
                .Get<SModel>()
                .FirstOrDefault();
        }

        public IEnumerable<SModel> GetAllUnstartedBackups()
        {
            string today = Helpers.Today();
            return query()
                .Where(fields[Fields.STARTED], 0)
                .Where(fields[Fields.COMPLETED], 0)
                .Where(fields[Fields.DATE], today)
                .Get<SModel>();
        }

        public void MarkScheduleAsStarted(SModel model)
        {
            model.BascStarted = 1;
            query()
                .Where(fields[Fields.ID], model.BascId)
                .Update(model);
        }

        public void MarkScheduleAsCompleted(SModel model)
        {
            model.BascCompleted = 1;
            query()
                .Where(fields[Fields.ID], model.BascId)
                .Update(model);
        }

        public int InsertSchedule(string time)
        {
            var model = new SModel
            {
                BascDate = Helpers.Today(),
                BascTime = time,
                BascStarted = 0,
                BascCompleted = 0,
                BascScanned = 0
            };

            return insert(model);
        }

        public void MarkScheduleAsScanned(SModel schedule)
        {
            schedule.BascScanned = 1;
            query()
                .Where(fields[Fields.ID], schedule.BascId)
                .Update(schedule);
        }
    }
}

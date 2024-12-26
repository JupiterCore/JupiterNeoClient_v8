using JupiterNeoServiceClient.classes;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JupiterNeoServiceClient.Models
{
    public class SModel
    {
        public string? basc_id { get; set; }
        public string? basc_date { get; set; }
        public string? basc_time { get; set; }
        public int basc_started { get; set; }
        public int basc_completed { get; set; }
        public int basc_scanned { get; set; }
    }

    public class SchedulesModel : BaseModel
    {
        public enum FD { ID, DATE, TIME, STARTED, COMPLETED };

        public SchedulesModel()
        {
            this.TableName = "backup_schedule";
            this.fields = new Dictionary<Enum, string>
            {
                [FD.ID] = "basc_id",
                [FD.DATE] = "basc_date",
                [FD.TIME] = "basc_time",
                [FD.STARTED] = "basc_started",
                [FD.COMPLETED] = "basc_completed"
            };
        }

        public void deleteCompletedBackups()
        {
            if (this.fields == null)
                throw new Exception("[] fields is null");

            string today = Helpers.today();
            this.query()
                .Where(fields[FD.DATE], "<>", today)
                .WhereNotNull(fields[FD.COMPLETED])
                .Delete();
        }

        public SModel? getSchedule(string time)
        {
            if (this.fields == null)
                throw new Exception("[] fields is null");

            string today = Helpers.today();
            return this.query()
                .Where(fields[FD.DATE], today)
                .Where(fields[FD.TIME], time)
                .FirstOrDefault<SModel>();
        }

        public SModel? getSchedue(string scheduleId)
        {
            if (this.fields == null)
                throw new Exception("[] fields is null");

            return this.query()
                .Where(fields[FD.ID], scheduleId)
                .FirstOrDefault<SModel>();
        }

        public SModel? getUncompletedBackup()
        {
            if (this.fields == null)
                throw new Exception("[getUncompletedBackup] fields is null");

            return this.query()
                .Where(fields[FD.STARTED], 1)
                .Where(fields[FD.COMPLETED], 0)
                .FirstOrDefault<SModel>();
        }

        public SModel? getUnstartedBackup()
        {
            if (this.fields == null)
                throw new Exception("[] fields is null");

            string today = Helpers.today();
            return this.query()
                .Where(fields[FD.STARTED], 0)
                .Where(fields[FD.COMPLETED], 0)
                .Where(fields[FD.DATE], today)
                .FirstOrDefault<SModel>();
        }

        public IEnumerable<SModel> getAllUnstartedBackups()
        {
            if (this.fields == null)
                throw new Exception("[] fields is null");

            string today = Helpers.today();
            return this.query()
                .Where(fields[FD.STARTED], 0)
                .Where(fields[FD.COMPLETED], 0)
                .Where(fields[FD.DATE], today)
                .Get<SModel>();
        }

        public void markScheduleAsStarted(SModel m)
        {
            if (this.fields == null)
                throw new Exception("[] fields is null");

            m.basc_started = 1;
            this.query()
                .Where(fields[FD.ID], m.basc_id)
                .Update(m);
        }

        public void markScheduleAsCompleted(SModel m)
        {
            if (this.fields == null)
                throw new Exception("[] fields is null");

            m.basc_completed = 1;
            this.query()
                .Where(fields[FD.ID], m.basc_id)
                .Update(m);
        }

        public int insertSchedule(string time)
        {
            var model = new SModel
            {
                basc_date = Helpers.today(),
                basc_time = time,
                basc_started = 0,
                basc_completed = 0,
                basc_scanned = 0
            };

            return this.insert(model);
        }

        public void markScheduleAsScanned(SModel schedule)
        {
            if (this.fields == null)
                throw new Exception("[] fields is null");

            schedule.basc_scanned = 1;
            this.query()
                .Where(fields[FD.ID], schedule.basc_id)
                .Update(schedule);
        }
    }
}
